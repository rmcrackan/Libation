# Run Libation on Ubuntu
This walkthrough should get you up and running with Libation on your Ubuntu machine.

Some limitations of the linux release are:
- Cannot customize how illegial filename characters are replaced.
- The Auto-update function is unavailable
- The "Hangover" app for debugging is not yet available.

## Dependencies
### Dotnet Runtime
You must install the dotnet 6.0 runtime on your machine.

First, add the Microsoft package signing key to your list of trusted keys and add the package repository.

<details>
  <summary>Ubuntu 22.04</summary>
  
  ```console
  wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
  sudo dpkg -i packages-microsoft-prod.deb
  rm packages-microsoft-prod.deb
  ```
</details>

<details>
  <summary>Ubuntu 21.10</summary>
  
  ```console
  wget https://packages.microsoft.com/config/ubuntu/21.10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
  sudo dpkg -i packages-microsoft-prod.deb
  rm packages-microsoft-prod.deb
  ```
</details>

<details>
  <summary>Ubuntu 20.04</summary>
  
  ```console
  wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
  sudo dpkg -i packages-microsoft-prod.deb
  rm packages-microsoft-prod.deb
  ```
</details>

For other distributions, see [Microsoft's instructions for installing .NET on Linux](https://docs.microsoft.com/en-us/dotnet/core/install/linux).

Then install the dotnet 6.0 runtime

```console
sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y dotnet-runtime-6.0
```
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

if [ "$EUID" -ne 0 ]
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

  #Remove previous installation program files and sym link
  rm /usr/bin/Libation
  rm /usr/lib/libation -r

  #Copy install files, icon and desktop file
  cp ${FOLDER}/glass-with-glow_256.svg /usr/share/icons/hicolor/scalable/apps/libation.svg
  cp ${FOLDER}/Libation.desktop /usr/share/applications/Libation.desktop
  mv ${FOLDER}/ /usr/lib/libation

  chmod +666 /usr/share/icons/hicolor/scalable/apps/libation.svg
  gtk-update-icon-cache -f /usr/share/icons/hicolor/
  ln -s /usr/lib/libation/Libation /usr/bin/Libation

  echo "Done!"

  ```
</details>

```console
chmod +700 install-libation.sh
```
Then run the script with the libation binaries zipfile as an argument.
```console
./install-libation.sh libation-linux-bin.zip
```

Report bugs to https://github.com/rmcrackan/Libation/issues
