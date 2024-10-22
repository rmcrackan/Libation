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
    warn "${FULLPATH} not found, using defaults"
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
  if mount | grep -q "${DIR}";
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
    warn "database not found, creating one at ${DBFILE}"
    if ! touch "${DBFILE}"; then
      error "unable to create database, check permissions on host"
      exit 1
    fi
    return 1
  fi
}

run() {
  info "scanning accounts"
  /libation/LibationCli scan
  info "liberating books"
  /libation/LibationCli liberate
}

main() {
  # TERM isn't set by default in docker images
  if [[ -z ${TERM} || ${TERM} = "dumb" ]]; then
    TERM=xterm-256color
  fi

  info ${TERM}

  info "initializing libation"
  init_config_file AccountsSettings.json
  init_config_file Settings.json
  
  info "loading settings"
  update_settings Settings.json Books /data
  update_settings Settings.json InProgress /tmp

  info "loading database"
  FILE=LibationContext.db
  # If user provides a separate database mount, use that
  if is_mounted ${LIBATION_DB_DIR};
  then
    debug "using database directory ${LIBATION_DB_DIR}"
    if [ -f "${LIBATION_DB_DIR}/${FILE}" ]; then
      info "database found in ${LIBATION_DB_DIR}"
    else
      create_db ${LIBATION_DB_DIR}/${FILE}
    fi
    ln -s /${LIBATION_DB_DIR}/${FILE} ${LIBATION_CONFIG_INTERNAL}/${FILE}
  # Otherwise, use the config directory
  else
    debug "using config directory ${LIBATION_CONFIG_DIR}"
    if [ -f "${LIBATION_CONFIG_DIR}/${FILE}" ]; then
      info "database found in ${LIBATION_CONFIG_DIR}"
    else
      create_db ${LIBATION_CONFIG_DIR}/${FILE}
    fi
    ln -s /${LIBATION_CONFIG_DIR}/${FILE} ${LIBATION_CONFIG_INTERNAL}/${FILE}
  fi

  # Try to warn if books dir wasn't mounted in
  if ! is_mounted ${LIBATION_BOOKS_DIR};
  then
    warn "${LIBATION_BOOKS_DIR} does not appear to be mounted, books will not be saved"
  fi

  # Let the user know what the run type will be
  if [[ -z "${SLEEP_TIME}" ]]; then
    SLEEP_TIME=-1
  fi

  if [ "${SLEEP_TIME}" = -1 ]; then
    info "running once"
  else
    info "running every ${SLEEP_TIME}"
  fi

  # loop
  while true
  do
    run
    
    # Liberate only once if SLEEP_TIME was set to -1
    if [ "${SLEEP_TIME}" = -1 ]; then
      break  
    fi

    sleep "${SLEEP_TIME}"
  done

  info "exiting"
}

main
