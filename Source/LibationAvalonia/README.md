# Run Libation on Ubuntu
This walkthrough should get you up and running with Libation on your Ubuntu machine.

Some limitations of the linux release are:
- Cannot customize how illegial filename characters are replaced.
- The Auto-update function is unavailable

## Dependencies

### FFMpeg (Optional)
If you want to convert your audiobooks to mp3, install FFMpeg using the following command:

```console
sudo apt-get install -y ffmpeg
```

## Install Libation

Download the most recent linux-64 binaries zip file and save it as `libation-linux-bin.zip`. Save the 'install-libation.sh' bash script to a file. From the terminal make the script file executable:

<details>
  <summary>install-libation.sh</summary>
  
  ```BASH
  #!/bin/bash

  FILE=$1

  if [ -z "$FILE" ]
   then echo "This script must be called with a the Libation Linux bin zip file as an argument."
   exit
  fi

  if [[ "$EUID" -ne 0 ]]
    then echo "Please run as root"
    exit
  fi

  if [ ! -f "$FILE" ]
   then echo "The file \"$FILE\" does not exist."
   exit
  fi

  echo "Extracting $FILE"

  FOLDER="$(dirname "$FILE")/libation_src"
  echo "$FOLDER"

  sudo -u $SUDO_USER unzip -q -o ${FILE} -d ${FOLDER}

  if [ $? -ne 0 ]
   then echo "Error unzipping ${FILE}"
   exit
  fi

  sudo -u $SUDO_USER chmod +700 ${FOLDER}/Libation
  sudo -u $SUDO_USER chmod +700 ${FOLDER}/Hangover
  sudo -u $SUDO_USER chmod +700 ${FOLDER}/LibationCli

  #Remove previous installation program files and sym link
  rm /usr/bin/Libation
  rm /usr/bin/Hangover
  rm /usr/bin/LibationCli
  rm /usr/bin/libationcli
  rm /usr/lib/libation -r

  #Copy install files, icon and desktop file
  cp ${FOLDER}/glass-with-glow_256.svg /usr/share/icons/hicolor/scalable/apps/libation.svg
  cp ${FOLDER}/Libation.desktop /usr/share/applications/Libation.desktop
  mv ${FOLDER}/ /usr/lib/libation

  chmod +666 /usr/share/icons/hicolor/scalable/apps/libation.svg
  gtk-update-icon-cache -f /usr/share/icons/hicolor/
  ln -s /usr/lib/libation/Libation /usr/bin/Libation
  ln -s /usr/lib/libation/Hangover /usr/bin/Hangover
  ln -s /usr/lib/libation/LibationCli /usr/bin/LibationCli
  ln -s /usr/lib/libation/LibationCli /usr/bin/libationcli

  echo "Done!"
  ```
</details>

```console
chmod +700 install-libation.sh
```
Then run the script with the libation binaries zipfile as an argument.
```console
sudo ./install-libation.sh libation-linux-bin.zip
```

You should now see Libation among your applications.

Report bugs to https://github.com/rmcrackan/Libation/issues
