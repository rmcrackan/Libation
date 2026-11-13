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
  if [ -f "${FULLPATH}" ]; then
    info "loading ${FILE}"
    if ! cp "${FULLPATH}" "${LIBATION_CONFIG_INTERNAL}/"; then
      error "failed to copy ${FULLPATH} to ${LIBATION_CONFIG_INTERNAL}/"
      exit 1
    fi
    return 0
  else
    warn "${FULLPATH} not found, creating empty file"
    if ! echo "{}" > "${LIBATION_CONFIG_INTERNAL}/${FILE}"; then
      error "failed to create ${LIBATION_CONFIG_INTERNAL}/${FILE}"
      exit 1
    fi
    return 1
  fi
}

init_master_key() {
  FILE=libation-master.key
  FULLPATH=${LIBATION_CONFIG_DIR}/${FILE}
  if [ -n "${LIBATION_MASTER_KEY_FILE}" ]; then
    debug "LIBATION_MASTER_KEY_FILE is already set, not looking for ${FILE}"
    return 0
  fi
  if [ -n "${LIBATION_MASTER_KEY}" ]; then
    debug "LIBATION_MASTER_KEY is already set, not looking for ${FILE}"
    return 0
  fi
  if [ -f "${FULLPATH}" ]; then
    info "loading ${FILE}"
    if ! cp "${FULLPATH}" "${LIBATION_CONFIG_INTERNAL}/"; then
      error "failed to copy ${FULLPATH} to ${LIBATION_CONFIG_INTERNAL}/"
      exit 1
    fi
  else
    debug "${FULLPATH} not found, assuming accounts don't use encrypted tokens"
  fi
}

update_settings() {
  shift
  if ! /libation/LibationCli set-setting --libationFiles "${LIBATION_CONFIG_INTERNAL}" "$@"; then
    error "failed to update settings in ${LIBATION_CONFIG_INTERNAL}/Settings.json"
    exit 1
  fi
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
    if ! rm -f "$LINK"; then
      error "failed to remove existing symlink '${LINK}'"
      exit 1
    fi
  elif [[ -e $LINK ]]
  then
    error "found blocking file at '${LINK}' - can't create symlink"
    exit 1
  fi

  if ! ln -s "${FILE}" "${LINK}"; then
    error "failed to symlink '${FILE}' to '${LINK}'"
    exit 1
  fi
}

# Run a command in the background and wait for it. bash only runs a trap once the
# foreground command exits, so `docker stop` would otherwise wait out a whole scan,
# liberate or sleep.
run_child() {
  "$@" &
  CHILD_PID=$!
  wait "${CHILD_PID}"
  local child_exit=$?
  CHILD_PID=
  return "${child_exit}"
}

# Docker sends SIGTERM to PID 1 only, and bash as PID 1 ignores it unless it is trapped.
shutdown() {
  info "received ${1}, shutting down"
  if [ -n "${CHILD_PID}" ]; then
    # Always forward TERM: background children ignore SIGINT
    kill -TERM "${CHILD_PID}" 2>/dev/null
    wait "${CHILD_PID}"
  fi
  info "exiting"
  exit "${2}"
}

run() {
  info "scanning accounts"
  run_child /libation/LibationCli scan
  local scan_exit=$?
  if [ "${scan_exit}" -ne 0 ]; then
    error "scan failed (exit ${scan_exit}); skipping liberate. If the log shows Failed to decrypt ExistingAccessToken, see https://getlibation.com/docs/frequently-asked-questions#docker-finds-no-new-books-failed-to-decrypt-existingaccesstoken"
    exit "${scan_exit}"
  fi
  info "liberating books"
  run_child /libation/LibationCli liberate
  local liberate_exit=$?
  if [ "${liberate_exit}" -ne 0 ]; then
    error "liberate failed (exit ${liberate_exit})"
  fi
}

main() {
  info "initializing libation"
  init_config_file AccountsSettings.json
  init_config_file Settings.json
  init_master_key

  info "loading settings"
  info "setting Books to ${LIBATION_BOOKS_DIR:-/data}"
  info "setting InProgress to /tmp"
  update_settings Settings.json "Books=${LIBATION_BOOKS_DIR:-/data}" "InProgress=/tmp"

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

  # If a command was passed to the container (e.g. `docker run -it libation bash`),
  # run that instead of the liberate loop so the environment is still set up
  if [ "$#" -gt 0 ]; then
    info "executing '$*' instead of the liberate loop"
    exec "$@"
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

  trap 'shutdown SIGTERM 143' TERM
  trap 'shutdown SIGINT 130' INT

  # loop
  while true
  do
    run
    
    # Liberate only once if SLEEP_TIME was set to -1
    if [ "${SLEEP_TIME}" == -1 ]; then
      break  
    fi

    run_child sleep "${SLEEP_TIME}"
  done

  info "exiting"
}

main "$@"
