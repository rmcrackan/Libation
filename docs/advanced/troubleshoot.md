# Troubleshooting Common Libation Errors

## How to run the Hangover App

When troubleshooting, you may be asked to run 'Hangover'. Hangover is a debugging app to help diagnose and solve some problems with Libation.
It is located alongside the Libation app (though not included in the docker container).

### Windows

Hangover.exe is located in the folder containing Libation.exe. Double-click it to rune it.

### macOS

Hangover is located inside the app bundle. Either:
1. From a terminal, run this command: `open -a Libation.app --args hangover`
2. Run it from within the app bundle.
   1. In finder, right-click the Libation app bundle and "Show Package Contents"
   2. Open folders "Contents" > "MacOS"
   3. Find the file named "Hangover" and double-click it to run it. 

### Linux (either the .deb or .rpm installers)

The installer creates shortcuts for `libation`, `libationcli`, and `hangover`. From a terminal, run `hangover`.

## SQLite Error 10: 'disk I/O error'.

There are two possible causes of this error.
1. Your hard disk is full. Check that you have space on the storage device containing your Libation Files (where the LibationContext.db and log files are). If that device still has available space, move on to #2 below.
2. The database's journaling mode is incompatible with your environment. Change the journaling mode to `DELETE` by one of two methods.
   1. [Run hangover](#how-to-run-the-hangover-app) and execute the following command in the "Database" tab: `PRAGMA journal_mode=DELETE`
   2. run this command in your terminal: `sqlite3 "path/to/libation/files/LibationContext.db" "PRAGMA journal_mode=DELETE;"`
