# Troubleshooting Common Libation Errors

## Invalid filenames or mangled paths (NTFS / Windows)

NTFS filesystems (Windows, and NTFS-formatted external drives on Linux or Mac) do not allow colons (`:`) in filenames. Libation chooses filename replacement rules from the **OS it is running on**, not from the filesystem where books are saved. On Linux or in Docker, that often means colons are left in names even when `LIBATION_BOOKS_DIR` points at an NTFS volume, which can produce invalid paths, failed moves, or mangled folder names.

**Fix:** Add or edit `ReplacementCharacters` in `Settings.json` on your config volume (or Libation files directory) so colons are replaced before download. The `HiFi_NTFS` example includes a colon replacement. See [Command Line Interface - Set custom replacement characters](/docs/advanced/command-line-interface#set-custom-replacement-characters).

## SQLite Error 10: 'disk I/O error'.

There are two possible causes of this error.
1. Your hard disk is full. Check that you have space on the storage device containing your Libation Files (where the LibationContext.db and log files are). If that device still has available space, move on to #2 below.
2. The database's journaling mode is incompatible with your environment. Change the journaling mode to `DELETE` by one of two methods.
   1. [Run hangover](#how-to-run-the-hangover-app) and execute the following command in the "Database" tab: `PRAGMA journal_mode=DELETE`
   2. run this command in your terminal: `sqlite3 "path/to/libation/files/LibationContext.db" "PRAGMA journal_mode=DELETE;"`

## Library scan fails ("Unexpected character" or "HTML instead of JSON")

Audible returned an HTML page instead of JSON. Common causes: transient outage, expired login, VPN/proxy, or rate limiting. What to try:

1. Scan again after a few minutes.
2. Sign in to Audible in a browser on the same network.
3. Disable VPN/proxy and scan again.
4. Remove and re-add the account in Libation.

## How to run the Hangover App

When troubleshooting, you may be asked to run 'Hangover'. Hangover is a debugging app to help diagnose and solve some problems with Libation.
It is located alongside the Libation app (though not included in the docker container).

Platform-specific steps: [Windows](#hangover-windows) · [macOS](#hangover-macos) · [Linux](#hangover-linux)

## Windows

### Smart App Control and in-app upgrades {#windows-smart-app-control-and-in-app-upgrades}

After accepting an in-app update, Libation may fail to restart with an error like:

`An Application Control policy has blocked this file. (0x800711C7)`

Windows **Smart App Control** (and similar Application Control policies on recent Windows 11 builds) can block DLLs that were just written when the in-app upgrader overlays a new release onto your existing install folder. The blocked path is usually under your **Libation install folder** (where `Libation.exe` lives), not your user data folder (`%UserProfile%\Libation`).

**Symptoms**

- Fatal crash immediately after an in-app upgrade (Chardonnay / Avalonia).
- Classic may start but library import or database access fails with the same `0x800711C7` message on a `.dll` in the install folder.
- Windows Security may also warn about an unsigned library.

**Fix (recommended)**

1. Quit Libation completely.
2. Download the latest [release zip](https://github.com/rmcrackan/Libation/releases/latest) from GitHub.
3. Extract to a **new folder** (for example `C:\Apps\Libation`). Do **not** copy new files on top of the old install folder.
4. Run `Libation.exe` from the new folder. Your library database, accounts, and settings in `%UserProfile%\Libation` (or the path in `appsettings.json` -> `LibationFiles`) are separate and should still work.

**If Windows still blocks the new install**

1. Open **Windows Security** -> **App & browser control** and review **Smart App Control** (Evaluation or On modes are the usual trigger).
2. In PowerShell, unblock the install folder (adjust the path):

   ```powershell
   Unblock-File -Path 'C:\Apps\Libation\*' -Recurse
   ```

3. Avoid running Libation from cloud-sync folders (OneDrive, etc.) if you can; use a normal local path for the install folder.

Related reports: [#1876](https://github.com/rmcrackan/Libation/issues/1876), [#1873](https://github.com/rmcrackan/Libation/issues/1873).

### Hangover (Windows)

Hangover.exe is located in the folder containing Libation.exe. Double-click it to run it.

## macOS

### Hangover (macOS)

**Hangover** is located inside the app bundle. Either:
1. From a terminal, run this command: `open /Applications/Libation.app --args hangover`
2. Run it from within the app bundle.
   1. In finder, right-click the Libation app bundle and "Show Package Contents"
   2. Open folders "Contents" > "MacOS"
   3. Find the file named "Hangover" and double-click it to run it.

### App won't start

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

## Linux

### Hangover (Linux)

The installer creates shortcuts for `libation`, `libationcli`, and `hangover`. From a terminal, run `hangover`.

### UI too small

If the Linux UI is tiny, try `AVALONIA_GLOBAL_SCALE_FACTOR=2 libation` (tune the number); see [#634](https://github.com/rmcrackan/Libation/issues/634).

### In-app Audible login or "add account" fails

Embedded sign-in uses WebKit2GTK (`libwebkit2gtk`). If that native stack is missing, install the packages for your distro or use 'external browser' sign-in in Libation's import/library settings. Details: [Install on Linux](/docs/installation/linux) (section: Runtime dependencies (Audible sign-in)).

### Very long paths or encrypted home directory

On some Linux setups the home directory or default temp area sits on a stacked or encrypted filesystem. That often means a shorter usable path length than a plain `ext4` mount. Together with a deep Books folder or long paths from naming templates, Libation can fail during or after decryption when moving finished files into the library.

**What to try:** In **Settings -> Download/Decrypt**, set **Books** and the **in-progress / temporary** location (the folder used while files are downloaded and decrypted) to **shorter paths** on a normal, unencrypted volume if you can—for example an external drive mounted without an extra encryption layer. A user on Mint described this approach in [GitHub issue #1199](https://github.com/rmcrackan/Libation/issues/1199) (that thread also mentions `MissingMethodException`, which usually indicates a mismatched or partial install rather than path length alone).

### Linux Snap and SQLite write failures {#linux-snap-and-sqlite-write-failures}

Symptoms include a crash on startup that mentions `LibationContext.db` under a path like `~/snap/libation/<number>/.local/share/Libation/`.

1. **Permissions** - The whole Libation data directory must be writable by your user, including `LibationContext.db`, `LibationContext.db-wal`, and `LibationContext.db-shm` when they exist. Fix ownership with `chown` if needed.

2. **Stale `LibationFiles` after a Snap refresh** - Snap may install a new revision folder (new `<number>`) while `appsettings.json` inside the **new** folder still points `LibationFiles` at the **previous** revision path. Libation then targets the old path while the app runs from the new revision, which often surfaces as a read-only or migration failure even when permissions on both trees look fine.

   **Fix:** edit `appsettings.json` in the active revision (for example under `~/snap/libation/current/...`) so the `LibationFiles` value uses the **same** `.../snap/libation/<number>/...` as that file, or use `LIBATION_FILES_DIR`. Step-by-step context: [Install on Linux - Snap](/docs/installation/linux#snap) and [issue #1776](https://github.com/rmcrackan/Libation/issues/1776).

3. **Non-Snap build** - If you still suspect Snap confinement after the above, try a `.deb` / `.rpm` / AppImage build from [Releases](https://github.com/rmcrackan/Libation/releases) to compare behavior.

## Download fails with "DRM license response not OK" or "Content license denied"

These errors come from Audible refusing to grant a download license. Common causes:

1. **Temporary Audible outage or Plus throttling** -- wait 24 to 48 hours and try again. See the [FAQ](/docs/frequently-asked-questions).
2. **Title requires Widevine** -- some Plus titles no longer download as AAXC; enable **Use Widevine DRM** in Settings and re-add your account if prompted. See [issue #1580](https://github.com/rmcrackan/Libation/issues/1580).
3. **Spatial / Dolby Atmos requested (older Libation versions)** -- Audible now requires Widevine L1 for many spatial titles. Libation 13.1.3+ no longer offers spatial download. See [Spatial Audio & DRM](/docs/advanced/spatial-audio).

Attach your log file when opening a GitHub issue.
