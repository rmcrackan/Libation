#!/bin/sh
# $Id: runasroot.sh 153224 2022-08-22 17:43:14Z klaus $
## @file
# VirtualBox privileged execution helper script for Linux and Solaris
#

#
# Copyright (C) 2009-2022 Oracle and/or its affiliates.
#
# This file is part of VirtualBox base platform packages, as
# available from https://www.virtualbox.org.
#
# This program is free software; you can redistribute it and/or
# modify it under the terms of the GNU General Public License
# as published by the Free Software Foundation, in version 3 of the
# License.
#
# This program is distributed in the hope that it will be useful, but
# WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
# General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program; if not, see <https://www.gnu.org/licenses>.
#
# SPDX-License-Identifier: GPL-3.0-only
#

# Deal with differing "which" semantics
mywhich() {
    which "$1" 2>/dev/null | grep -v "no $1"
}

# Get the name and execute switch for a useful terminal emulator
#
# Sets $gxtpath to the emulator path or empty
# Sets $gxttitle to the "title" switch for that emulator
# Sets $gxtexec to the "execute" switch for that emulator
# May clobber $gtx*
# Calls mywhich
getxterm() {
    # gnome-terminal uses -e differently to other emulators
    for gxti in "konsole --title -e" "gnome-terminal --title -x" "xterm -T -e"; do
        set $gxti
        gxtpath="`mywhich $1`"
        case "$gxtpath" in ?*)
            gxttitle=$2
            gxtexec=$3
            return
            ;;
        esac
    done
}

# Quotes its argument by inserting '\' in front of every character save
# for 'A-Za-z0-9/'.  Prints the result to stdout.
quotify() {
    echo "$1" | sed -e 's/\([^a-zA-Z0-9/]\)/\\\1/g'
}

ostype=`uname -s`
if test "$ostype" != "Linux" && test "$ostype" != "SunOS" ; then
  echo "Linux/Solaris not detected."
  exit 1
fi

HAS_TERMINAL=""
case "$1" in "--has-terminal")
    shift
    HAS_TERMINAL="yes"
    ;;
esac

case "$#" in "2"|"3")
    ;;
    *)
    echo "Usage: `basename $0` DESCRIPTION COMMAND [ADVICE]" >&2
    echo >&2
    echo "Attempt to execute COMMAND with root privileges, displaying DESCRIPTION if" >&2
    echo "possible and displaying ADVICE if possible if no su(1)-like tool is available." >&2
    exit 1
    ;;
esac

DESCRIPTION=$1
COMMAND=$2
ADVICE=$3
PATH=$PATH:/bin:/usr/bin:/usr/local/bin:/sbin:/usr/sbin:/usr/local/sbin:/usr/X11/bin

case "$ostype" in SunOS)
    PATH=$PATH:/usr/sfw/bin:/usr/gnu/bin:/usr/xpg4/bin:/usr/xpg6/bin:/usr/openwin/bin:/usr/ucb
    GKSU_SWITCHES="-au root"
    ;;
    *)
    GKSU_SWITCHES=""
    ;;
esac

case "$HAS_TERMINAL" in "")
    case "$DISPLAY" in ?*)
        KDESUDO="`mywhich kdesudo`"
        case "$KDESUDO" in ?*)
            eval "`quotify "$KDESUDO"` --comment `quotify "$DESCRIPTION"` -- $COMMAND"
            exit
            ;;
        esac

        KDESU="`mywhich kdesu`"
        case "$KDESU" in ?*)
            "$KDESU" -c "$COMMAND"
            exit
            ;;
        esac

        GKSU="`mywhich gksu`"
        case "$GKSU" in ?*)
            # Older gksu does not grok --description nor '--' and multiple args.
            # @todo which versions do?
            # "$GKSU" --description "$DESCRIPTION" -- "$@"
            # Note that $GKSU_SWITCHES is NOT quoted in the following
            "$GKSU" $GKSU_SWITCHES "$COMMAND"
            exit
            ;;
        esac
        ;;
    esac # $DISPLAY
    ;;
esac # ! $HAS_TERMINAL

# pkexec may work for ssh console sessions as well if the right agents
# are installed.  However it is very generic and does not allow for any
# custom messages.  Thus it comes after gksu.
## @todo should we insist on either a display or a terminal?
# case "$DISPLAY$HAS_TERMINAL" in ?*)
PKEXEC="`mywhich pkexec`"
case "$PKEXEC" in ?*)
    eval "\"$PKEXEC\" $COMMAND"
    exit
    ;;
esac
#    ;;S
#esac

case "$HAS_TERMINAL" in ?*)
        USE_SUDO=
        grep -q Ubuntu /etc/lsb-release 2>/dev/null && USE_SUDO=true
        # On Ubuntu we need sudo instead of su.  Assume this works, and is only
        # needed for Ubuntu until proven wrong.
        case $USE_SUDO in true)
            SUDO_COMMAND="`quotify "$SUDO"` -- $COMMAND"
            eval "$SUDO_COMMAND"
            exit
            ;;
        esac

    SU="`mywhich su`"
    case "$SU" in ?*)
        "$SU" - root -c "$COMMAND"
        exit
        ;;
    esac
    ;;
esac

# The ultimate fallback is running 'su -' within an xterm.  We use the
# title of the xterm to tell what is going on.
case "$DISPLAY" in ?*)
    SU="`mywhich su`"
    case "$SU" in ?*)
        getxterm
        case "$gxtpath" in ?*)
            "$gxtpath" "$gxttitle" "$DESCRIPTION - su" "$gxtexec" su - root -c "$COMMAND"
            exit
            ;;
        esac
    esac
esac # $DISPLAY

# Failure...
case "$DISPLAY" in ?*)
    echo "Unable to locate 'pkexec', 'gksu' or 'su+xterm'. $ADVICE" >&2
    ;;
    *)
    echo "Unable to locate 'pkexec'. $ADVICE" >&2
    ;;
esac

exit 1
