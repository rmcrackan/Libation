version=$1
release_name="chardonnay"
profile="Properties/PublishProfiles/MacOSArmProfile.pubxml"

if [ -z "$version" ]
then
  echo "This script must be called with the Libation version number as an argument."
  exit
fi

# Publish
cd ../Source
dotnet publish -c Release LibationAvalonia/LibationAvalonia.csproj "-p:PublishProfile=LibationAvalonia/${profile}"
dotnet publish -c Release LoadByOS/MacOSConfigApp/MacOSConfigApp.csproj "-p:PublishProfile=LoadByOS/MacOSConfigApp/${profile}"
dotnet publish -c Release LibationCli/LibationCli.csproj "-p:PublishProfile=LibationCli/${profile}"
dotnet publish -c Release HangoverAvalonia/HangoverAvalonia.csproj "-p:PublishProfile=HangoverAvalonia/${profile}"

# Remove x64 and x86 libraries, add arm64 libs
rm "bin/Publish/MacOS-${release_name}/"ffmpegaac.*
rm "bin/Publish/MacOS-${release_name}/"libmp3lame.*
cp ../ffmpegaac.arm64.dylib "bin/Publish/MacOS-${release_name}/"
cp ../libmp3lame.arm64.dylib "bin/Publish/MacOS-${release_name}/"

# Create release artefact
cd "bin/Publish/MacOS-${release_name}/"
artefact="Libation.${version}-macOS-ARM64-${release_name}"
tar -zcvf "../${artefact}.tar.gz" .

cd ../../../../Scripts
./targz2macosbundle.sh "../Source/bin/Publish/${artefact}.tar.gz" $version