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

BASEDIR=$(pwd)

delfiles=('LinuxConfigApp' 'LinuxConfigApp.deps.json' 'LinuxConfigApp.runtimeconfig.json')
if [[ "$ARCH" == "x64" ]]
then
  ARCH_RPM="x86_64"
  ARCH="amd64"
else
  ARCH_RPM="aarch64"
fi

notinstalled=('libcoreclrtraceptprovider.so' 'libation_glass.svg' 'Libation.desktop')

mkdir -p ~/rpmbuild/SPECS
mkdir ~/rpmbuild/BUILD
mkdir ~/rpmbuild/RPMS

echo "Name:           libation
Version:        ${VERSION}
Release:        1
Summary:        Liberate your Audible Library

License:        GPLv3+
URL:            https://github.com/rmcrackan/Libation
Source0:        https://github.com/rmcrackan/Libation   

Requires:       bash gtk3 webkit2gtk4.1


%define __os_install_post %{nil}

%description
Liberate your Audible Library

%install
mkdir -p %{buildroot}%{_libdir}/%{name}
mkdir -p %{buildroot}%{_datadir}/icons/hicolor/scalable/apps
mkdir -p %{buildroot}%{_datadir}/applications

if test -f 'libcoreclrtraceptprovider.so'; then
    rm 'libcoreclrtraceptprovider.so'
fi


install -m 666 libation_glass.svg %{buildroot}%{_datadir}/icons/hicolor/scalable/apps/libation.svg
install -m 666 Libation.desktop %{buildroot}%{_datadir}/applications/Libation.desktop

rm libation_glass.svg
rm Libation.desktop

install * %{buildroot}%{_libdir}/%{name}/

%post

if [ \$1 -eq 1 ] ; then
  # Initial installation
  
  ln -s %{_libdir}/%{name}/Libation %{_bindir}/libation
  ln -s %{_libdir}/%{name}/Hangover %{_bindir}/hangover
  ln -s %{_libdir}/%{name}/LibationCli %{_bindir}/libationcli

  gtk-update-icon-cache -f %{_datadir}/icons/hicolor/
    
  if ! grep -q 'fs.inotify.max_user_instances=524288' /etc/sysctl.conf; then
    echo fs.inotify.max_user_instances=524288 | tee -a /etc/sysctl.conf && sysctl -p
  fi
fi

%postun
if [ \$1 -eq 0 ] ; then
  # Uninstall
  rm %{_bindir}/libation
  rm %{_bindir}/hangover
  rm %{_bindir}/libationcli
fi

%files
%{_datadir}/icons/hicolor/scalable/apps/libation.svg
%{_datadir}/applications/Libation.desktop" >> ~/rpmbuild/SPECS/libation.spec


cd "$BIN_DIR"

for f in *; do
  if [[ " ${delfiles[*]} " =~ " ${f} " ]]; then
    echo "Deleting $f"
  elif [[ ! " ${notinstalled[*]} " =~ " ${f} " ]]; then
    echo "%{_libdir}/%{name}/${f}" >> ~/rpmbuild/SPECS/libation.spec
    cp $f ~/rpmbuild/BUILD/
  else
    cp $f ~/rpmbuild/BUILD/
  fi
done

cd ~/rpmbuild/SPECS/
rpmbuild -bb --target $ARCH_RPM libation.spec

cd $BASEDIR
RPM_FILE=$(ls ~/rpmbuild/RPMS/${ARCH_RPM})

mkdir bundle

mv ~/rpmbuild/RPMS/${ARCH_RPM}/$RPM_FILE "./bundle/Libation.${VERSION}-linux-chardonnay-${ARCH}.rpm"
