#!/bin/bash

FILE=$1; shift
VERSION=$1; shift

if [ -z "$FILE" ]
then
  echo "This script must be called with a the Libation macos bin zip file as an argument."
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

BUNDLE="Libation.app"
echo "Bundle dir: $BUNDLE"

if [[ -d "$BUNDLE" ]]
then
  echo "$BUNDLE directory already exists, aborting."
  exit
fi

BUNDLE_CONTENTS="$BUNDLE/Contents"
echo "Bundle Contents dir: $BUNDLE_CONTENTS"

BUNDLE_RESOURCES="$BUNDLE_CONTENTS/Resources"
echo "Resources dir: $BUNDLE_RESOURCES"

BUNDLE_MACOS="$BUNDLE_CONTENTS/MacOS"
echo "MacOS dir: $BUNDLE_MACOS"

mkdir -p "$BUNDLE_CONTENTS"
mkdir -p "$BUNDLE_RESOURCES"
mkdir -p "$BUNDLE_MACOS"

echo "Extracting $FILE to $BUNDLE_MACOS..."
tar -xzf ${FILE} -C ${BUNDLE_MACOS}

if [ $? -ne 0 ]
 then echo "Error extracting ${FILE}"
 exit
fi

echo "Copying icon..."
cp "$BUNDLE_MACOS/libation.icns" "$BUNDLE_RESOURCES/libation.icns"

echo "Copying Info.plist file..."
cp "$BUNDLE_MACOS/Info.plist" "$BUNDLE_CONTENTS/Info.plist"

echo "Set Libation version number..."
sed -i -e "s/VERSION_STRING/$VERSION/" "$BUNDLE_CONTENTS/Info.plist"

echo "deleting unneeded files.."
delfiles=("libmp3lame.x64.so" "ffmpegaac.x64.so" "libation.icns" "Info.plist")
for n in "${delfiles[@]}"; do rm "$BUNDLE_MACOS/$n"; done

echo "Creating app bundle: $BUNDLE-$VERSION.tar.gz"
tar -czvf "$BUNDLE-$VERSION.tar.gz" "$BUNDLE"

mkdir bundle
echo "moving to ./bundle/$BUNDLE-$VERSION.tar.gz"
mv "$BUNDLE-$VERSION.tar.gz" "./bundle/$BUNDLE-x64-$VERSION.tgz"

rm -r "$BUNDLE"

echo "Done!"
