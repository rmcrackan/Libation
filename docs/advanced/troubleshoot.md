# Troubleshooting Common Libation Errors

## How to run the Hangover App

When troubleshooting, you may be asked to run 'Hangover'. Hangover is a debugging app to help diagnose and solve some problems with Libation.
It is located alongside the Libation app (though not included in the docker container).

### Windows

Hangover.exe is located in the folder containing Libation.exe. Double-click it to run it.

### macOS

**Hangover** is located inside the app bundle. Either:
1. From a terminal, run this command: `open /Applications/Libation.app --args hangover`
2. Run it from within the app bundle.
   1. In finder, right-click the Libation app bundle and "Show Package Contents"
   2. Open folders "Contents" > "MacOS"
   3. Find the file named "Hangover" and double-click it to run it.

**App won't start** (for example the Dock icon appears and bounces but no window opens, or **`~/Library/Application Support/Libation` is never created**): macOS may be blocking or mishandling launch of the app bundle. That can happen with strict security settings, quarantine flags on the download, or **unsupported macOS setups** (for example hardware past Apple's support window with tools such as OpenCore Legacy Patcher). Symptoms can include double-clicking Libation doing nothing useful, Activity Monitor showing almost no CPU use, no logs yet, and **`open /Applications/Libation.app --args hangover`** failing with error **-128** (`_LSOpenURLsWithCompletionHandler`). Libation is intended for **Apple-supported macOS releases** in [Install on MacOS](/docs/installation/mac); unofficial upgrades are **not supported**, and the steps below are community-tested workarounds.

Try the following in order:

1. **Clear extended attributes (including quarantine)** on the installed app, then try opening Libation again from Applications:
   ```bash
   xattr -cr /Applications/Libation.app
   ```
2. **Run the main executable from Terminal** (same idea as [Running LibationCli](/docs/installation/mac#running-libationcli), but for the GUI). This bypasses some Launch Services paths and has resolved "won't start" reports where `open` failed (for example with error **-128**):
   ```bash
   /Applications/Libation.app/Contents/MacOS/Libation
   ```
   To capture any output to a file (it may be empty):
   ```bash
   /Applications/Libation.app/Contents/MacOS/Libation > ~/Desktop/libation_debug.log 2>&1
   ```
3. If you still need Hangover and **`open /Applications/Libation.app --args hangover` fails**, run **Hangover** from the bundle using Finder (see option 2 under **Hangover** above).
4. Confirm you installed the **correct architecture** ([Install on MacOS](/docs/installation/mac)): **arm64** for Apple Silicon, **x64** for Intel.
5. **Crash logs**: open **Console** (Applications, then Utilities), or check **~/Library/Logs/DiagnosticReports** for recent **Libation** crash reports if the process exits abruptly.

### Linux (either the .deb or .rpm installers)

The installer creates shortcuts for `libation`, `libationcli`, and `hangover`. From a terminal, run `hangover`.

### Linux: in-app Audible login or "add account" fails

Embedded sign-in uses WebKit2GTK (`libwebkit2gtk`). If that native stack is missing, install the packages for your distro or use 'external browser' sign-in in Libation's import/library settings. Details: [Install on Linux](/docs/installation/linux) (section: Runtime dependencies (Audible sign-in)).

## SQLite Error 10: 'disk I/O error'.

There are two possible causes of this error.
1. Your hard disk is full. Check that you have space on the storage device containing your Libation Files (where the LibationContext.db and log files are). If that device still has available space, move on to #2 below.
2. The database's journaling mode is incompatible with your environment. Change the journaling mode to `DELETE` by one of two methods.
   1. [Run hangover](#how-to-run-the-hangover-app) and execute the following command in the "Database" tab: `PRAGMA journal_mode=DELETE`
   2. run this command in your terminal: `sqlite3 "path/to/libation/files/LibationContext.db" "PRAGMA journal_mode=DELETE;"`
