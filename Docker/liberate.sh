#!/bin/bash

# Rewire echo to print date time
echo() {
    if [[ -n $1  ]]; then
        printf "$(date '+%F %T'): %s\n" "$1"
    fi
}

# ################################
# Setup
# ################################
echo "Starting"
if [[ -z "${SLEEP_TIME}" ]]; then
  echo "No sleep time passed in. Will run once and exit."
else
  echo "Sleep time is set to ${SLEEP_TIME}"
fi

echo ""

# If user passes in db from a /db/ folder and a db does not already exist / is not already linked
FILE=/db/LibationContext.db
if [ -f "${FILE}" ] && [ ! -f "/config/LibationContext.db" ]; then
    echo "Linking passed in Libation database from /db/ to the Libation config directory"
    ln -s $FILE /config/LibationContext.db
fi

# ################################
# Loop and liberate
# ################################
while true
do
    echo ""
    echo "Scanning accounts"
    /libation/LibationCli scan
    echo "Liberating books"
    /libation/LibationCli liberate
    echo ""
    
    # Liberate only once if SLEEP_TIME was set to -1
    if [ "${SLEEP_TIME}" = -1 ]; then
      break  
    fi

    echo "Sleeping for ${SLEEP_TIME}"
    sleep "${SLEEP_TIME}"
done

echo "Exiting"
