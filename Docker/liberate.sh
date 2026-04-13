#!/bin/bash

error() {
  log "ERROR" "$1"
}

warn() {
  log "WARNING" "$1"
}

info() {
  log "info" "$1"
}

debug() {
  if [ "${LOG_LEVEL}" = "debug" ]; then
    log "debug" "$1"
  fi
}

log() {
  LEVEL=$1
  MESSAGE=$2
  printf "$(date '+%F %T') %s: %s\n" "${LEVEL}" "${MESSAGE}"
}

init_config_file() {
  FILE=$1
  FULLPATH=${LIBATION_CONFIG_DIR}/${FILE}
  if [ -f ${FULLPATH} ]; then
    info "loading ${FILE}"
    cp ${FULLPATH} ${LIBATION_CONFIG_INTERNAL}/
    return 0
  else
    warn "${FULLPATH} not found, creating empty file"
    echo "{}" > ${LIBATION_CONFIG_INTERNAL}/${FILE}
    return 1
  fi
}

update_settings() {
  FILE=$1
  KEY=$2
  VALUE=$3
  info "setting ${KEY} to ${VALUE}"
  echo $(jq --arg k "${KEY}" --arg v "${VALUE}" '.[$k] = $v' ${LIBATION_CONFIG_INTERNAL}/${FILE}) > ${LIBATION_CONFIG_INTERNAL}/${FILE}.tmp
  mv ${LIBATION_CONFIG_INTERNAL}/${FILE}.tmp ${LIBATION_CONFIG_INTERNAL}/${FILE}
}

is_mounted() {
  DIR=$1
  if grep -qs "${DIR} " /proc/mounts;
  then
    return 0
  else
    return 1
  fi
}

create_db() {
  DBFILE=$1
  if [ -f "${DBFILE}" ]; then
    warn "prexisting database found when creating"
    return 0
  else
    if ! touch "${DBFILE}"; then
      error "unable to create database, check permissions on host"
      exit 1
    fi
    return 1
  fi
}

setup_db() {
  local DBPATH=$1
  local dbpattern="*.db"
  local FILE LINK LINK_ORIGIN
  local -a files

  debug "using database directory ${DBPATH}"

  # Figure out the right databse file
  if [[ -z $LIBATION_DB_FILE ]]
  then
    shopt -s nullglob     # files shall be empty if no match is found
    files=( "${DBPATH}"/${dbpattern} )
    shopt -u nullglob

    case ${#files[@]} in
      0)
        FILE="${DBPATH}/LibationContext.db"
        ;;
      1)
        FILE="${files[0]}"
        ;;
      *)
        debug "found ${#files[@]} database files matching '${dbpattern}'"
        error "too many database files found, set LIBATION_DB_FILE to the filename you wish to use"
        exit 1
        ;;
    esac
  else
    FILE="${DBPATH}/${LIBATION_DB_FILE}"
  fi

  debug "planning to use database '${FILE}'"

  if [[ -f $FILE ]]
  then
    info "database found at ${FILE}"
  elif [[ $LIBATION_CREATE_DB == "true" ]]
  then
    warn "database not found, creating one at ${FILE}"
    create_db "${FILE}"
  else
    error "database not found and creation is disabled"
    exit 1
  fi

  LINK="${LIBATION_CONFIG_INTERNAL}/LibationContext.db"
  if [[ -L $LINK ]]
  then
    # check if the link is correct
    LINK_ORIGIN=$(readlink -fn "$LINK")
    if [[ $LINK_ORIGIN == $(readlink -fn "$FILE") ]]
    then
      warn "symlink '${LINK_ORIGIN}' to '${LINK}' already established"
      return 0
    fi
    warn "removing existing symlink '${LINK_ORIGIN}' to '${LINK}'"
    rm -f "$LINK"
  elif [[ -e $LINK ]]
  then
    error "found blocking file at '${LINK}' - can't create symlink"
    exit 1
  fi

  ln -s "${FILE}" "${LINK}"
}

run() {
  info "scanning accounts"
  /libation/LibationCli scan
  info "liberating books"
  /libation/LibationCli liberate
}

main() {
  info "initializing libation"
  init_config_file AccountsSettings.json
  init_config_file Settings.json
  
  info "loading settings"
  update_settings Settings.json Books "${LIBATION_BOOKS_DIR:-/data}"
  update_settings Settings.json InProgress /tmp

  info "loading database"
  # If user provides a separate database mount, use that
  if is_mounted "${LIBATION_DB_DIR}";
  then
    DB_LOCATION=${LIBATION_DB_DIR}
  # Otherwise, use the config directory
  else
    DB_LOCATION=${LIBATION_CONFIG_DIR}
  fi
  setup_db ${DB_LOCATION}

  # Try to warn if books dir wasn't mounted in
  if ! is_mounted "${LIBATION_BOOKS_DIR}";
  then
    warn "${LIBATION_BOOKS_DIR} does not appear to be mounted, books will not be saved"
  fi

  # Let the user know what the run type will be
  if [[ -z "${SLEEP_TIME}" ]]; then
    SLEEP_TIME=-1
  fi

  if [ "${SLEEP_TIME}" == -1 ]; then
    info "running once"
  else
    info "running every ${SLEEP_TIME}"
  fi

  # loop
  while true
  do
    run
    
    # Liberate only once if SLEEP_TIME was set to -1
    if [ "${SLEEP_TIME}" == -1 ]; then
      break  
    fi

    sleep "${SLEEP_TIME}"
  done

  info "exiting"
}

main
