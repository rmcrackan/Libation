#!/bin/bash
set -euo pipefail

BIN_DIR=$1; shift
VERSION=$1; shift
ARCH=$1; shift
SIGN_WITH_KEY=$1; shift

if [ -z "$BIN_DIR" ]
then
  echo "This script must be called with a the Libation macos bins directory as an argument."
  exit 1
fi

if [ ! -d "$BIN_DIR" ]
then
  echo "The directory \"$BIN_DIR\" does not exist."
  exit 1
fi

if [ -z "${VERSION:-}" ]
then
  echo "This script must be called with the Libation version number as an argument."
  exit 1
fi

if [ -z "${ARCH:-}" ]
then
  echo "This script must be called with the Libation cpu architecture as an argument."
  exit 1
fi

if [ "$SIGN_WITH_KEY" != "true" ]
then
  echo "::warning:: App will fail Gatekeeper verification without valid Apple Team information."
fi

BUNDLE=./Libation.app
echo "Bundle dir: $BUNDLE"

if [[ -d $BUNDLE ]]
then
  echo "$BUNDLE directory already exists, aborting."
  exit 1
fi

# hdiutil create intermittently fails on CI with "Resource busy" (macOS background scanning).
create_dmg_with_retry() {
  local src_folder="$1"
  local output_path="$2"
  local max_attempts=10
  local attempt=1
  local delay=3

  while [ "$attempt" -le "$max_attempts" ]; do
    echo "Creating disk image (attempt ${attempt}/${max_attempts}): ${output_path}"
    rm -f "${output_path}"
    sync
    sleep 1

    if hdiutil create -srcFolder "${src_folder}" -ov -format UDZO -o "${output_path}"; then
      if [ -f "${output_path}" ]; then
        return 0
      fi
    fi

    echo "hdiutil create failed on attempt ${attempt}"
    hdiutil detach -force /Volumes/Libation 2>/dev/null || true
    rm -f "${output_path}" "${output_path}.temp.dmg" 2>/dev/null || true
    sync

    if [ "$attempt" -lt "$max_attempts" ]; then
      echo "Waiting ${delay}s before retry..."
      sleep "${delay}"
      delay=$((delay + 2))
    fi
    attempt=$((attempt + 1))
  done

  echo "Error: hdiutil create failed after ${max_attempts} attempts"
  return 1
}

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

if [ $? -ne 0 ]; then
  echo "Error moving ${BIN_DIR} files"
  exit 1
fi

echo "Make fileicon executable..."
chmod +x $BUNDLE_MACOS/fileicon

echo "Moving icon..."
mv $BUNDLE_MACOS/libation.icns $BUNDLE_RESOURCES/libation.icns

echo "Moving Info.plist file..."
mv $BUNDLE_MACOS/Info.plist $BUNDLE_CONTENTS/Info.plist

echo "Moving Libation_DS_Store file..."
mv $BUNDLE_MACOS/Libation_DS_Store ./Libation_DS_Store

echo "Moving background.png file..."
mv $BUNDLE_MACOS/background.png ./background.png

echo "Moving background.png file..."
mv $BUNDLE_MACOS/Libation.entitlements ./Libation.entitlements

PLIST_ARCH=$(echo $ARCH | sed 's/x64/x86_64/')
echo "Set LSArchitecturePriority to $PLIST_ARCH"
# Portable sed -i (BSD sed on macOS requires backup arg; use .bak then remove)
sed -i.bak "s/ARCHITECTURE_STRING/$PLIST_ARCH/" $BUNDLE_CONTENTS/Info.plist && rm -f $BUNDLE_CONTENTS/Info.plist.bak

echo "Set CFBundleVersion to $VERSION"
sed -i.bak "s/VERSION_STRING/$VERSION/" $BUNDLE_CONTENTS/Info.plist && rm -f $BUNDLE_CONTENTS/Info.plist.bak

delfiles=('MacOSConfigApp' 'MacOSConfigApp.deps.json' 'MacOSConfigApp.runtimeconfig.json')
for n in "${delfiles[@]}"
do
  echo "Deleting $n"
  rm $BUNDLE_MACOS/$n
done

DMG_FILE="Libation.${VERSION}-macOS-chardonnay-${ARCH}.dmg"

all_identities=$(security find-identity -v -p codesigning)
identity=$(echo ${all_identities} | sed -n 's/.*"\(.*\)".*/\1/p')

if [ "$SIGN_WITH_KEY" == "true" ]; then
  echo "Signing executables in: $BUNDLE"
  codesign --force --deep --timestamp --options=runtime --entitlements "./Libation.entitlements" --sign "${identity}" "$BUNDLE"
  codesign --verify --verbose "$BUNDLE"
else
  echo "Signing with empty key: $BUNDLE"
  codesign --force --deep -s - $BUNDLE
fi

echo "Creating app disk image: $DMG_FILE"
mkdir Libation
mv $BUNDLE ./Libation/$BUNDLE
mv Libation_DS_Store Libation/.DS_Store
mkdir Libation/.background
mv background.png Libation/.background/
ln -s /Applications "./Libation/ "
mkdir ./bundle
create_dmg_with_retry ./Libation "./bundle/$DMG_FILE"
# Create a .DS_Store by:
#  - mounting an existing image in shadow mode (hdiutil attach Libation.dmg -shadow junk.dmg)
#  - Open the folder and edit it to your liking.
#  - Copy the .DS_Store from the directory and save it to Libation_DS_Store


if [ "$SIGN_WITH_KEY" == "true" ]; then
  echo "Signing $DMG_FILE"
  codesign --deep --sign "${identity}" "./bundle/$DMG_FILE"
fi

echo "Done!"
