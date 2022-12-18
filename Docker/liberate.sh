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

# Check if the config directory is passed in, and there is no link to it then create the link.
if [ -d "/config" ] && [ ! -d "/root/Libation" ]; then
    echo "Linking config directory to the Libation config directory"
    ln -s /config/ /root/Libation
fi

# If no config error and exit
if [ ! -d "/config" ]; then
    echo "ERROR: No /config directory. You must pass in a volume containing your config mapped to /config"
    exit 1
fi

# If user passes in db from a /db/ folder and a db does not already exist / is not already linked
FILE=/db/LibationContext.db
if [ -f "${FILE}" ] && [ ! -f "/config/LibationContext.db" ]; then
    echo "Linking passed in Libation database from /db/ to the Libation config directory"
    ln -s $FILE /config/LibationContext.db
fi

# Confirm we have a db in the config direcotry.
if [ ! -f "/config/LibationContext.db" ]; then
    echo "ERROR: No Libation database detected, exiting."
    exit 1
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