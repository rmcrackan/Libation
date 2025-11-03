#!/bin/bash

BIN_DIR=$1; shift
VERSION=$1; shift
ARCH=$1; shift

if [ -z "$BIN_DIR" ]
then
  echo "This script must be called with a the Libation Linux bins directory as an argument."
  exit
fi

if [ ! -d "$BIN_DIR" ]
then
  echo "The directory \"$BIN_DIR\" does not exist."
  exit
fi

if [ -z "$VERSION" ]
then
  echo "This script must be called with the Libation version number as an argument."
  exit
fi

if [ -z "$ARCH" ]
then
  echo "This script must be called with the Libation cpu architecture as an argument."
  exit
fi

contains() { case "$1" in *"$2"*) true ;; *) false ;; esac }

if ! contains "$BIN_DIR" "$ARCH"
then
  echo "This script must be called with a Libation binaries for ${ARCH}."
  exit
fi

ARCH=$(echo $ARCH | sed 's/x64/amd64/')

DEB_DIR=./deb

FOLDER_EXEC=$DEB_DIR/usr/lib/libation
echo "Exec dir: $FOLDER_EXEC"
mkdir -p $FOLDER_EXEC

echo "Moving bins from $BIN_DIR to $FOLDER_EXEC"
mv "${BIN_DIR}/"* $FOLDER_EXEC

if [ $? -ne 0 ]
 then echo "Error moving ${BIN_DIR} files"
 exit
fi


delfiles=('LinuxConfigApp' 'LinuxConfigApp.deps.json' 'LinuxConfigApp.runtimeconfig.json')

for n in "${delfiles[@]}"
do
  echo "Deleting $n"
  rm $FOLDER_EXEC/$n
done

FOLDER_ICON=$DEB_DIR/usr/share/icons/hicolor/scalable/apps/
echo "Icon dir: $FOLDER_ICON"

FOLDER_DESKTOP=$DEB_DIR/usr/share/applications
echo "Desktop dir: $FOLDER_DESKTOP"

FOLDER_DEBIAN=$DEB_DIR/DEBIAN
echo "Debian dir: $FOLDER_DEBIAN"

mkdir -p $FOLDER_ICON
mkdir -p $FOLDER_DESKTOP
mkdir -p $FOLDER_DEBIAN

echo "Copying icon..."
cp $FOLDER_EXEC/libation_glass.svg $FOLDER_ICON/libation.svg

echo "Copying desktop file..."
cp $FOLDER_EXEC/Libation.desktop $FOLDER_DESKTOP/Libation.desktop

echo "Creating pre-install file..."
echo "#!/bin/bash
# Pre-install script, removes previous installation program files and sym links
echo \"Removing previously created symlinks...\"
rm /usr/bin/libation
rm /usr/bin/hangover
rm /usr/bin/libationcli
echo \"Removing previously installed Libation files...\"
rm -r /usr/lib/libation
# making sure it won't stop installation
exit 0
" >> $FOLDER_DEBIAN/preinst

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
" >> $FOLDER_DEBIAN/postinst

echo "Creating control file..."
echo "Package: Libation
Version: $VERSION
Architecture: $ARCH
Essential: no
Priority: optional
Maintainer: github.com/rmcrackan
Description: liberate your audiobooks
Recommends: libgtk-3-0, libwebkit2gtk-4.1-0
" >> $FOLDER_DEBIAN/control

echo "Changing permissions for pre- and post-install files..."
chmod +x "$FOLDER_DEBIAN/preinst"
chmod +x "$FOLDER_DEBIAN/postinst"

if [ "$(uname -s)" == "Darwin" ]; then
  echo "macOS detected, installing dpkg"
  brew install dpkg
fi

DEB_FILE=Libation.${VERSION}-linux-chardonnay-${ARCH}.deb
echo "Creating $DEB_FILE"
dpkg-deb -Zxz --build $DEB_DIR ./$DEB_FILE

echo "moving to ./bundle/$DEB_FILE"
mkdir bundle
mv $DEB_FILE ./bundle/$DEB_FILE

rm -r "$BIN_DIR"

echo "Done!"