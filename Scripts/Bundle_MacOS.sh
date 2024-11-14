#!/bin/bash

BIN_DIR=$1; shift
VERSION=$1; shift
ARCH=$1; shift

if [ -z "$BIN_DIR" ]
then
  echo "This script must be called with a the Libation macos bins directory as an argument."
  exit
fi

if [ ! -d "$BIN_DIR" ]
then
  echo "The directory \"$BIN_DIR\" does not exist."
  exit
fi

if [ -z $VERSION ]
then
  echo "This script must be called with the Libation version number as an argument."
  exit
fi

if [ -z $ARCH ]
then
  echo "This script must be called with the Libation cpu architecture as an argument."
  exit
fi

contains() { case "$1" in *"$2"*) true ;; *) false ;; esac }

if ! contains "$BIN_DIR" $ARCH
then
  echo "This script must be called with a Libation binaries for ${ARCH}."
  exit
fi

BUNDLE=./Libation.app
echo "Bundle dir: $BUNDLE"

if [[ -d $BUNDLE ]]
then
  echo "$BUNDLE directory already exists, aborting."
  exit
fi

BUNDLE_CONTENTS=$BUNDLE/Contents
echo "Bundle Contents dir: $BUNDLE_CONTENTS"

BUNDLE_RESOURCES=$BUNDLE_CONTENTS/Resources
echo "Resources dir: $BUNDLE_RESOURCES"

BUNDLE_MACOS=$BUNDLE_CONTENTS/MacOS
echo "MacOS dir: $BUNDLE_MACOS"

mkdir -p $BUNDLE_CONTENTS
mkdir -p $BUNDLE_RESOURCES
mkdir -p $BUNDLE_MACOS

mv "${BIN_DIR}/"*  $BUNDLE_MACOS

if [ $? -ne 0 ]
 then echo "Error moving ${BIN_DIR} files"
 exit
fi

echo "Make fileicon executable..."
chmod +x $BUNDLE_MACOS/fileicon

echo "Moving icon..."
mv $BUNDLE_MACOS/libation.icns $BUNDLE_RESOURCES/libation.icns

echo "Moving Info.plist file..."
mv $BUNDLE_MACOS/Info.plist $BUNDLE_CONTENTS/Info.plist

PLIST_ARCH=$(echo $ARCH | sed 's/x64/x86_64/')
echo "Set LSArchitecturePriority to $PLIST_ARCH"
sed -i -e "s/ARCHITECTURE_STRING/$PLIST_ARCH/" $BUNDLE_CONTENTS/Info.plist

echo "Set CFBundleVersion to $VERSION"
sed -i -e "s/VERSION_STRING/$VERSION/" $BUNDLE_CONTENTS/Info.plist


delfiles=( 'libmp3lame.arm64.so' 'libmp3lame.x64.so' 'libmp3lame.x64.dll' 'libmp3lame.x86.dll' 'ffmpegaac.arm64.so' 'ffmpegaac.x64.so' 'ffmpegaac.x64.dll' 'ffmpegaac.x86.dll' 'MacOSConfigApp' 'MacOSConfigApp.deps.json' 'MacOSConfigApp.runtimeconfig.json')
if [[ "$ARCH" == "arm64" ]]
then
  delfiles+=('libmp3lame.x64.dylib' 'ffmpegaac.x64.dylib')
  mv $BUNDLE_MACOS/ffmpegaac.arm64.dylib  $BUNDLE_MACOS/ffmpegaac.dylib
  mv $BUNDLE_MACOS/libmp3lame.arm64.dylib  $BUNDLE_MACOS/libmp3lame.dylib
else
  delfiles+=('libmp3lame.arm64.dylib' 'ffmpegaac.arm64.dylib')
  mv $BUNDLE_MACOS/ffmpegaac.x64.dylib  $BUNDLE_MACOS/ffmpegaac.dylib
  mv $BUNDLE_MACOS/libmp3lame.x64.dylib  $BUNDLE_MACOS/libmp3lame.dylib
fi


for n in "${delfiles[@]}"
do
  echo "Deleting $n"
  rm $BUNDLE_MACOS/$n
done

APP_FILE=Libation.${VERSION}-macOS-chardonnay-${ARCH}.tgz

echo "Signing executables in: $BUNDLE"
codesign --force --deep -s - $BUNDLE

echo "Creating app bundle: $APP_FILE"
tar -czvf $APP_FILE $BUNDLE

mkdir bundle
echo "moving to ./bundle/$APP_FILE"
mv $APP_FILE ./bundle/$APP_FILE

rm -r $BUNDLE

echo "Done!"
