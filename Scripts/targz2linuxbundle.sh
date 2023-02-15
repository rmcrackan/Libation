#!/bin/bash

FILE=$1; shift
VERSION=$1; shift

if [ -z "$FILE" ]
then
  echo "This script must be called with a the Libation Linux bin zip file as an argument."
  exit
fi

if [ ! -f "$FILE" ]
then
  echo "The file \"$FILE\" does not exist."
  exit
fi

if [ -z "$VERSION" ]
then
  echo "This script must be called with the Libation version number as an argument."
  exit
fi

contains() { case "$1" in *"$2"*) true ;; *) false ;; esac }

if ! contains "$FILE" "$VERSION"
then
  echo "This script must be called with a Libation version number that is present in the filename passed."
  exit
fi

# remove trailing ".tar.gz"
FOLDER_MAIN=${FILE::-7}
echo "Working dir: $FOLDER_MAIN"

if [[ -d "$FOLDER_MAIN" ]]
then
  echo "$FOLDER_MAIN directory already exists, aborting."
  exit
fi

FOLDER_EXEC="$FOLDER_MAIN/usr/lib/libation"
echo "Exec dir: $FOLDER_EXEC"

FOLDER_ICON="$FOLDER_MAIN/usr/share/icons/hicolor/scalable/apps/"
echo "Icon dir: $FOLDER_ICON"

FOLDER_DESKTOP="$FOLDER_MAIN/usr/share/applications"
echo "Desktop dir: $FOLDER_DESKTOP"

FOLDER_DEBIAN="$FOLDER_MAIN/DEBIAN"
echo "Debian dir: $FOLDER_DEBIAN"

mkdir -p "$FOLDER_EXEC"
mkdir -p "$FOLDER_ICON"
mkdir -p "$FOLDER_DESKTOP"
mkdir -p "$FOLDER_DEBIAN"

echo "Extracting $FILE to $FOLDER_EXEC..."
tar -xzf ${FILE} -C ${FOLDER_EXEC}

if [ $? -ne 0 ]
 then echo "Error extracting ${FILE}"
 exit
fi

echo "Copying icon..."
cp "$FOLDER_EXEC/glass-with-glow_256.svg" "$FOLDER_ICON/libation.svg"

echo "Copying desktop file..."
cp "$FOLDER_EXEC/Libation.desktop" "$FOLDER_DESKTOP/Libation.desktop"

echo "Workaround for desktop file..."
sed -i '/^Exec=Libation/c\Exec=/usr/bin/libation' "$FOLDER_DESKTOP/Libation.desktop"

echo "Creating pre-install file..."
echo "#!/bin/bash

# Pre-install script, removes previous installation program files and sym links

echo \"Removing previously created symlinks...\"

rm /usr/bin/libation
rm /usr/bin/Libation
rm /usr/bin/hangover
rm /usr/bin/Hangover
rm /usr/bin/libationcli
rm /usr/bin/LibationCli

echo \"Removing previously installed Libation files...\"

rm -r /usr/lib/libation
rm -r /usr/lib/Libation

# making sure it won't stop installation
exit 0
" >> "$FOLDER_DEBIAN/preinst"

echo "Creating post-install file..."
echo "#!/bin/bash

gtk-update-icon-cache -f /usr/share/icons/hicolor/

ln -s /usr/lib/libation/Libation /usr/bin/libation
ln -s /usr/lib/libation/Hangover /usr/bin/hangover
ln -s /usr/lib/libation/LibationCli /usr/bin/libationcli

# Increase the maximum number of inotify instances

if ! grep -q 'fs.inotify.max_user_instances=524288' /etc/sysctl.conf; then  
  echo fs.inotify.max_user_instances=524288 | tee -a /etc/sysctl.conf && sysctl -p
fi

# workaround until this file is moved to the user's home directory
touch /usr/lib/libation/appsettings.json
chmod 666 /usr/lib/libation/appsettings.json
" >> "$FOLDER_DEBIAN/postinst"

echo "Creating control file..."
echo "Package: Libation
Version: $VERSION
Architecture: all
Essential: no
Priority: optional
Maintainer: github.com/rmcrackan
Description: liberate your audiobooks
" >> "$FOLDER_DEBIAN/control"

echo "Changing permissions for pre- and post-install files..."
chmod +x "$FOLDER_DEBIAN/preinst"
chmod +x "$FOLDER_DEBIAN/postinst"

echo "Creating .deb file..."
dpkg-deb -Zxz --build $FOLDER_MAIN

mkdir bundle
echo "moving to ./bundle/$FOLDER_MAIN.deb"
mv "$FOLDER_MAIN.deb" "./bundle/$FOLDER_MAIN.deb"

rm -r "$FOLDER_MAIN"

echo "Done!"
