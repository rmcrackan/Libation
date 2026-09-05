# Troubleshooting Common Libation Errors

## Invalid Settings.json value (TokenStorageMethod, Serilog, etc.)

**Symptoms:** Libation or LibationCli refuses to start with **Invalid Settings.json** (GUI) or **Invalid configuration** (CLI). The message names the setting, the bad value, and (for enums) the allowed values.

**Common causes:**
- `TokenStorageMethod` mistyped (canonical values are `Encrypted` and `Plaintext` - casing variants like `PlainText` are accepted, but unknown spellings are not)
- `Serilog.WriteTo` missing, empty, or malformed (not an array of objects with `Name`). Hand-edited custom sink names are allowed; legacy `ZipFile` is migrated to `File` automatically, and a `File` sink missing the size-rolling arguments has them filled in
- `Serilog.MinimumLevel` set to a value that is not a Serilog level

**Fix:** Edit `Settings.json` in your Libation Files directory to a valid value and restart. Do not delete the whole file unless it is corrupt JSON.

## Invalid filenames or mangled paths (NTFS / Windows)

NTFS filesystems (Windows, and NTFS-formatted external drives on Linux or Mac) do not allow colons (`:`) in filenames. Libation chooses filename replacement rules from the **OS it is running on**, not from the filesystem where books are saved. On Linux or in Docker, that often means colons are left in names even when `LIBATION_BOOKS_DIR` points at an NTFS volume, which can produce invalid paths, failed moves, or mangled folder names.

**Fix:** Add or edit `ReplacementCharacters` in `Settings.json` on your config volume (or Libation files directory) so colons are replaced before download. The `HiFi_NTFS` example includes a colon replacement. See [Command Line Interface - Set custom replacement characters](/docs/advanced/command-line-interface#set-custom-replacement-characters).

## 'Input/output error' on the Books folder (removable or failing drive)

**Symptoms:** Downloads that were working start failing one after another, and the log fills with `System.IO.IOException: Input/output error` naming paths under your Books location. On a removable drive the folder often still appears to be there, because the mount point answers even though nothing on it can be read.

**Cause:** The drive itself, not Libation. A USB drive that has been pulled, is failing, or has gone to sleep reports an I/O error for every read. Libation now reports the folder as unreadable and carries on. Older versions closed with a fatal error instead, and kept closing on launch until the Books location was changed ([#1984](https://github.com/rmcrackan/Libation/issues/1984)).

**What to try:**

1. Eject and reconnect the drive, then check it with your OS disk tool (**Disk Utility** on macOS, `chkdsk` on Windows, `fsck` on Linux).
2. Keep the **in-progress / temporary** location in **Settings > Download/Decrypt** on your internal disk even when Books lives on an external drive. It is written to constantly during a download, so it is the first thing to fail.
3. If Libation cannot start while the drive is disconnected, set `Books` in `Settings.json` back to a folder on your internal disk.

Books already downloaded to the drive are unaffected; Libation finds them again once the drive is readable.

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

## A book in my Audible library is missing from Libation

Work down this list. The first two account for most reports, and neither leaves any trace in the log.

1. **Check the trash bin.** Removing a book from Libation hides it from the grid, from search results and
   from the status bar counts, so it looks exactly like a book that was never imported. The status bar shows
   a count when the trash is not empty, and **Settings > Trash Bin** has a search box. See
   [Trash Bin](/docs/features/trash-bin).
2. **Clear any filter.** A Quick Filter set as the default is applied at startup, so the grid can open
   already filtered. Empty the search box and click **Filter**.
3. **Check what you are importing.** In **Settings > Import**, "Import episodes" and "Import Audible Plus
   books" each exclude titles from every scan. Both are recorded near the top of the log:
   `"ImportEpisodes":true,"ImportPlusTitles":true`.
4. **Check the other marketplaces.** Audible keeps a separate library per marketplace. A title bought while
   your Amazon address was set to another country stays in that country's library, and a scan of your usual
   marketplace will never see it. **Settings > Accounts > Marketplaces > Check other marketplaces** asks each
   one what it holds, using the credentials you already have. See
   [Titles bought from another country](/docs/getting-started#titles-bought-from-another-country).
5. **Scan again.** Audible occasionally leaves titles out of a scan. Libation re-requests what it notices
   missing, but a title absent from the library listing itself cannot be recovered until Audible sends it.
6. **Read the scan summary in the log.** Every scan ends with a tally, and anything Libation dropped is named
   just above it:

```
Library scan tally. {"LibraryItems":434,"EpisodesFetched":1724,"OrphanedEpisodesDropped":15,
                     "ImportEpisodes":true,"EpisodeItemsExcluded":0,
                     "ImportPlusTitles":true,"PlusTitlesExcluded":0,"ItemsToImport":2143}
```

`PlusTitlesExcluded` or `EpisodeItemsExcluded` above zero means a setting from step 3 is dropping titles.
Each marketplace an account reads is scanned and tallied separately.
An `Audible did not return ... catalog products` warning means Audible sent an incomplete response even after
Libation asked again. `podcast episodes were not imported because their series parent was missing` names each
episode that was dropped.

If the title is still missing after all of that, open a bug report with the log and say which title it is -
the ASIN if you have it, from the book's Audible URL.

## Failed to decrypt ExistingAccessToken (Docker finds no new books)

**Symptoms:** The Windows (or other desktop) app shows your full library and new titles, but Docker / Linux finds no new books after you copy `AccountsSettings.json` from that machine. Container or `Libation.log` output includes `Failed to decrypt ExistingAccessToken`.

**Cause:** Libation can encrypt auth tokens in `AccountsSettings.json` using a key stored in the OS secret store (on Windows: DPAPI). That key does **not** travel when you copy only the JSON file into Docker, so the container cannot decrypt the tokens and the library scan fails.

**Quick check:** Open `AccountsSettings.json` on the Docker config volume. If you see `"IsEncrypted": true` near `ExistingAccessToken`, `RefreshToken`, or related fields, that is the problem.

**Preferred fix (keep encryption):** On the desktop machine, export the master key:

- **GUI:** Settings -> Important -> **Export encryption key...**
- **CLI:** `LibationCli export-master-key libation-master.key`

Copy `libation-master.key` into the Docker config folder next to `AccountsSettings.json` and restart. Or set `LIBATION_MASTER_KEY_FILE` / `LIBATION_MASTER_KEY` (see [Docker environment variables](/docs/installation/docker#environment-variables)). Treat the key file like a password.

**Fix with plaintext tokens:** In the desktop app, open **Settings -> Important**, uncheck **Store authentication tokens encrypted**, and when prompted choose **Yes** to decrypt and re-save existing tokens as plaintext. Copy the updated `AccountsSettings.json` (and `Settings.json`) into the Docker config folder and restart the container.

**Fix without copying Windows accounts:** Create or refresh credentials inside Docker with `login-external` or `import-account`. See [Docker - Adding Audible accounts without the GUI](/docs/installation/docker#adding-audible-accounts-without-the-gui).

Also listed under [Docker Troubleshooting](/docs/installation/docker#troubleshooting) and the [FAQ](/docs/frequently-asked-questions#docker-finds-no-new-books-failed-to-decrypt-existingaccesstoken).

## Failed to encrypt identity field (Saving as plaintext)

**Symptoms:** Docker or headless logs show an **Error** like `Failed to encrypt identity field ExistingAccessToken (locale us). Saving as plaintext so the app can continue.` (often several fields in a row). The container keeps scanning and liberating; it does not exit for this alone.

**Cause:** Token storage is set to encrypted, but no usable protector is available in that environment (typical in Docker without `libation-master.key` / `LIBATION_MASTER_KEY*` and without an OS secret store). On the next write - commonly after an access-token refresh - Libation tries to encrypt, fails, and falls back to plaintext so the app is not blocked.

**What to do:** Nothing is required for the app to keep working. To store tokens encrypted at rest instead, supply a master key (export from desktop, or see [Docker encrypted-tokens warning](/docs/installation/docker#configuration)). To avoid the encrypt attempts and the Error noise, set token storage to plaintext in Settings -> Important (or `TokenStorageMethod` in `Settings.json`) and convert existing tokens when prompted.

**Not the same as decrypt failure:** If the log says `Failed to decrypt ExistingAccessToken`, tokens are already ciphertext you cannot unlock - the plaintext-save fallback does not help. Use the [decrypt troubleshooting](#failed-to-decrypt-existingaccesstoken-docker-finds-no-new-books) steps.

## Login fails for an old pre-Amazon Audible account

If your Audible account predates Amazon and login fails when you use an email or a normal region, choose a **pre-amazon** locale and enter your old **username** in the Audible email/login field. See the [FAQ](/docs/frequently-asked-questions#my-audible-account-is-from-before-amazon---how-do-i-add-it).

## How to run the Hangover App

When troubleshooting, you may be asked to run 'Hangover'. Hangover is a debugging app to help diagnose and solve some problems with Libation.
It is located alongside the Libation app (though not included in the docker container).

Platform-specific steps: [Windows](#hangover-windows) · [macOS](#hangover-macos) · [Linux](#hangover-linux)

## Windows

### Smart App Control blocks Libation {#windows-smart-app-control-and-in-app-upgrades}

Libation fails to start, or fails part way through, with an error like:

`An Application Control policy has blocked this file. (0x800711C7)`

**Cause:** Libation's Windows builds are not code-signed. Smart App Control runs code only when Microsoft's cloud reputation service recognises it or when it carries a signature from a trusted certificate authority, so it blocks Libation's files. The blocked path is a file in your **Libation install folder** (where `Libation.exe` lives), not your user data folder (`%UserProfile%\Libation`), and it is often a third-party library rather than a Libation one.

An in-app upgrade frequently triggers the first block, because the upgrader writes fresh files that have no reputation yet.

**Symptoms**

- Fatal crash on start, often right after an in-app upgrade (Chardonnay / Avalonia).
- Classic may start but library import or database access fails with the same `0x800711C7` message on a `.dll` in the install folder.
- Windows Security may also warn about an unsigned library.

**Check which mode Smart App Control is in**

Open **Settings** -> **Privacy & Security** -> **Windows Security** -> **App & browser control** -> **Smart App Control settings**.

| Mode | Blocks Libation? |
|------|------------------|
| Off | No |
| Evaluation | No. This mode observes only; it never blocks anything |
| On | Yes |

Windows can move itself from Evaluation to On on its own, which is why Libation can work one day and be blocked the next without you changing anything.

**If it is On**

Windows has no way to allow a single app through Smart App Control. Microsoft's guidance is to turn it off or to ask the developer to sign the app. Reinstalling, extracting to a different folder, and unblocking files all leave the signature missing, so none of them help.

That leaves three options: wait for signed builds, run Libation on a machine that does not have Smart App Control on, or turn Smart App Control off.

Code signing is in progress. Libation has applied to the [SignPath Foundation](https://signpath.io/), which signs open source projects for free. Signed builds run under Smart App Control with nothing to change on your side. The application has to be approved first, so there is no date for it; watch the [releases page](https://github.com/rmcrackan/Libation/releases). Signing will not silence every Windows warning at once, because SmartScreen keeps warning about new downloads until they earn a reputation, signed or not.

> [!WARNING] Turning Smart App Control off cannot be undone
> Windows will not turn Smart App Control back on without a reset or reinstall, so weigh that against simply waiting. An earlier version of this page suggested disabling it temporarily and re-enabling it afterwards. That is not possible; ignore that advice if you saw it.

**If it is already Off**

Then the block comes from a different Application Control or Device Guard policy, normally one set by whoever manages the PC. Ask them to allow Libation.

Reports: [#1873](https://github.com/rmcrackan/Libation/issues/1873), [#1876](https://github.com/rmcrackan/Libation/issues/1876), [#1967](https://github.com/rmcrackan/Libation/issues/1967).

### Recover from an incomplete in-app upgrade {#windows-incomplete-in-app-upgrade}

If Libation reports that an in-app upgrade did not replace every install file, or fails to load a component after an upgrade, the install folder holds a mix of old and new files. This is a different problem from a Smart App Control block, and reinstalling does fix it.

1. Quit Libation completely.
2. Download the latest [release](https://github.com/rmcrackan/Libation/releases/latest) from GitHub. The `*-setup.exe` installer is the easiest option.
3. If you use the zip instead, extract it to a **new folder** (for example `C:\Apps\Libation`). Do **not** copy new files on top of the old install folder.
4. Run Libation from the new install. Your library database, accounts, and settings in `%UserProfile%\Libation` (or the path in `appsettings.json` -> `LibationFiles`) are separate and should still work.

### Libation installed in OneDrive or another synced folder {#windows-cloud-sync-install}

Install Libation to a normal local path, not inside OneDrive, Dropbox, or a similar synced folder. The `*-setup.exe` installer does this for you by installing under `%LocalAppData%`.

Sync clients replace files with placeholders, restore old copies, and leave conflict copies behind. Inside an install folder that breaks in-app upgrades, and inside your Libation data folder it can corrupt the search index.

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
2. **Virtual-device registration** -- the official Audible app can play the title, but Libation cannot. Try an [experimental device registration](/docs/advanced/device-registration) (then remove and re-add the account), or import credentials from [audible-cli](https://github.com/mkb79/audible-cli).
3. **Title requires Widevine** -- some Plus titles no longer download as AAXC; enable **Use Widevine DRM** in Settings and re-add your account if prompted. The iPhone registration cannot use Widevine. See [issue #1580](https://github.com/rmcrackan/Libation/issues/1580) and [Device registration](/docs/advanced/device-registration#widevine).
4. **Spatial / Dolby Atmos requested (older Libation versions)** -- Audible now requires Widevine L1 for many spatial titles. Libation 13.1.3+ no longer offers spatial download. See [Spatial Audio & DRM](/docs/advanced/spatial-audio).
5. **You no longer have rights to the title** -- it was returned, it left the Plus catalog, or the account that owned it is no longer active. Check the title in the Audible app or website.

After a refusal Libation waits before asking about that title again, so you see the explanation once rather than on every run. It attempts the title again by itself; to try it sooner, name it (`libationcli liberate <ASIN>`) or mark it **Download Pending** (previously "Not Downloaded"). See [Retrying titles Audible refuses](/docs/features/retrying-refused-downloads).

Attach your log file when opening a GitHub issue.

## PDFs are missing, or land loose in the Books directory

Both were fixed in 13.7.9.

**`libationcli liberate` downloaded no PDFs.** A plain run only looked at titles that needed an audiobook, so a title whose audio was already downloaded was never reached and its PDF was never fetched. `libationcli liberate --pdf` was the only way to get them. A plain run now covers both. If your library predates the fix, one `libationcli liberate` (or **Liberate** \> **Begin Book and PDF Backups** in the app) collects the PDFs you are missing.

**PDFs went into the Books directory instead of the book's folder.** Libation saves a PDF beside its audiobook, which it locates by looking for the title's ASIN in the file path. When that lookup found nothing it fell back to the Books directory itself. It now falls back to the folder the [folder template](/docs/features/naming-templates) names for that title.

The lookup finds nothing in two situations, and the second is worth checking:

1. The audio files are not on this machine — the title is marked downloaded but the files live elsewhere, or were deleted.
2. **Your folder and file templates have no `<id>` tag.** Then no file Libation writes has the ASIN in its path, so Libation cannot recognise its own output for any title. Add `<id>` back in Settings \> Download/Decrypt; the defaults are `<title short> [<id>]` for folders and `<title> [<id>]` for files. This also explains PDFs with no ASIN in the name: the file name comes from your file template.

Already-misplaced PDFs are not moved. Move them into their book folders yourself, or mark the affected titles' PDFs **Download Pending** and download them again.

## The log file is too large to attach to a bug report

From 13.7.9 the log rolls every 10 MB as well as every month, keeping the 20 newest files, so the current
`LogYYYYMM.log` is always small enough to upload. Existing installs pick this up on the next start: Libation
fills in the size-rolling settings your `Settings.json` is missing without touching anything you set yourself.

Before that, `rollingInterval: "Month"` was the only rolling rule, so one file grew for the whole month --
tens of MB for an install with several accounts scanned on a frequent schedule, and Serilog's own 1 GB
ceiling would eventually stop it logging at all until the month rolled over.

If a single file is still too large for what you need, lower `fileSizeLimitBytes` (and, for total disk use,
`retainedFileCountLimit`) in the `File` sink's `Args` in `Settings.json`. See [Docker -
Logging](/docs/installation/docker#logging) for the full sink configuration.
