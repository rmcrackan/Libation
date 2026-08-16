# Command Line Interface

Libationcli.exe allows limited access to Libation's functionalities as a CLI.

Warnings about relying solely on the CLI:

- CLI will not perform any upgrades.
- It will show that there is an upgrade, but that will likely scroll by too fast to notice.
- It will not perform all post-upgrade migrations. Some migrations are only be possible by launching GUI.

> [!WARNING] NTFS filesystem limitations
>
> NTFS filesystems (Windows, and NTFS-formatted external drives on Linux/Mac) do not support colons (`:`) in filenames. Since many audiobook titles contain colons (e.g., "Title: A Subtitle"), downloads may produce invalid filenames.
>
> **Solution:** Configure custom replacement characters in `Settings.json` to replace colons with compatible characters. See [Set custom replacement characters](#set-custom-replacement-characters) below for configuration examples.

## Progress Bar

The `liberate` and `convert` commands show a progress bar in the terminal while downloading or converting (e.g. `[##########----------]  2.5 min remaining`). The progress bar is only shown when the CLI is run interactively with output not redirected.

To turn off the progress bar (for scripting, logging, or cleaner output), redirect standard output and/or standard error. The progress bar is automatically disabled when either stream is redirected.

```console
libationcli liberate > log.txt 2>&1
libationcli convert 2> errors.txt
```

Redirecting also avoids progress-bar control characters in log files.

## Help

LibationCli uses a **verb-first** layout: the first argument is always a command name (for example `scan`, `liberate`).

**Global help** — list every verb and its short description (use this when you are not sure which verb to run):

```console
libationcli --help
libationcli -h
```

On Windows, `/?`, `/h`, and `/help` are also accepted when they are the only argument.

**Verb-specific help** — options for a single command:

```console
libationcli scan --help
libationcli scan -h
```

The `help` verb is equivalent for many cases: `libationcli help scan`.

Help-only invocations exit with status code **0** (success), so scripts can treat them as non-errors.

## Libation files location

All verbs use the same Libation data directory as the GUI (where `AccountsSettings.json` and `Settings.json` live). To point the CLI elsewhere:

```console
libationcli --libationFiles "D:\path\to\LibationFiles" scan
```

You can also set the environment variable `LIBATION_FILES_DIR` to that directory instead of passing `--libationFiles` every time.

## Import an account (mkb79 / audible-cli JSON)

Imports a single account from a JSON file in the format produced by [mkb79's audible-cli](https://github.com/mkb79/audible-cli) export (Libation's GUI export to the same format is compatible). The file is validated, tokens are refreshed, and the account is appended to `AccountsSettings.json` unless the same account id and locale already exist.

```console
libationcli import-account "C:\path\to\account.json"
libationcli import-account /home/me/Audible/account.json
```

Use `libationcli import-account --help` for the exact options on your build.

## Export the encryption master key (`export-master-key`)

When authentication tokens are stored encrypted, the AES-GCM master key lives in the desktop OS secret store and does not travel with `AccountsSettings.json`. Export it to a file for Docker or another machine:

```console
libationcli export-master-key libation-master.key
libationcli export-master-key --path "C:\path\to\libation-master.key"
```

Requirements:

- The OS secret store must be available on the machine where you run the command.
- A master key must already exist (encrypt tokens at least once on that machine).

Treat the output file like a password: anyone with it can decrypt encrypted tokens in `AccountsSettings.json`. For Docker, copy `libation-master.key` into the config folder next to `AccountsSettings.json`, or set `LIBATION_MASTER_KEY_FILE` / `LIBATION_MASTER_KEY`. See [Docker - encrypted tokens](/docs/installation/docker#configuration).

You can also export from the GUI: **Settings -> Important -> Export encryption key...**

Use `libationcli export-master-key --help` for the exact options on your build.

## Log in with an external browser (`login-external`)

For headless servers or when you prefer not to use the GUI, this verb performs the same external browser OAuth flow as Libation's alternate login: the CLI prints a sign-in URL, you complete login in your own browser, then supply the full URL shown in the browser after Audible redirects you (it is normal if that page looks broken or says the page does not exist).

Required flags:

- `--account` / `-a` — Your Audible login id (email).
- `--locale` / `-l` — Marketplace country code or locale name (for example `us`, `uk`, `de`, or `germany`). Unknown values fail with an error instead of building a broken Amazon URL.

Interactive use (terminal attached to a keyboard):

```console
libationcli login-external --account you@example.com --locale us
```

Non-interactive use (stdin redirected, Docker without `-t`, scripts): pass the post-login URL explicitly:

```console
libationcli login-external -a you@example.com -l us --response-url "https://www.amazon.com/ap/maplanding?..."
```

If the account row already has valid saved tokens, the CLI reports that no browser login is needed and exits without opening the flow.

Use `libationcli login-external --help` for the exact options on your build.

## List configured accounts (`list-accounts`)

Prints each row from `AccountsSettings.json`: Audible login id, optional nickname, marketplace (`us`, `uk`, …), whether **Scan library** is enabled for that account, and whether stored identity tokens are currently **valid** (the same check `login-external` uses before starting a browser flow). Use this on headless setups to see which accounts still need `login-external` or `import-account`.

```console
libationcli list-accounts
libationcli list-accounts --bare
```

`--bare` (`-b`) prints tab-separated values with no table: account id, name, locale, scan library (`yes` / `no`), authenticated (`yes` / `no`), for scripts and `cut` / `awk`.

**Scan library** (`yes` / `no`) is the same checkbox as "Include in library scan?" in Accounts: it controls whether the main Libation app includes that account in automatic scans (startup / periodic scan behavior). It does **not** restrict `libationcli scan` with no arguments, which still imports from every configured account unless you pass specific account nicknames or ids.

If no accounts exist yet, the CLI prints `No accounts configured.` and exits successfully.

## Scan All Libraries

```console
libationcli scan
```

## Scan Only Libraries for Specific Accounts

```console
libationcli scan nickname1 nickname2
```

## Convert All m4b Files to mp3

```console
libationcli convert
```

## Liberate All Books and Pdfs

```console
libationcli liberate
```

If Audiobookshelf auto-upload is enabled in Settings, `liberate` also uploads each liberated book after download/decrypt (and PDF). See [Audiobookshelf Auto-Upload](/docs/features/audiobookshelf). The separate `convert` command does not upload. To upload books that were already liberated, use [`abs upload`](#upload-already-liberated-books-to-audiobookshelf).

Titles Audible has recently refused a license for are left out of the run and reported as one summary, rather than being requested again every time. This matters most for a scheduled run. See [Retrying titles Audible refuses](/docs/features/retrying-refused-downloads); naming an ASIN or passing `--force` overrides it.

The run covers both halves of "book and pdf backups": titles that need downloading, and titles whose audio you already have but whose PDF is missing. Before 13.7.10 it only did the first, so `liberate --pdf` was the only way to get a PDF for a title downloaded earlier.

Audiobookshelf auto-upload is not part of that second half. It runs when a title is liberated, so a run that only back-fills a PDF does not upload; use `abs upload` to send titles liberated earlier.

## Upload Already-Liberated Books to Audiobookshelf

Auto-upload only runs at the moment a book is liberated. Use `abs upload` to send books liberated earlier, using the files already on disk. Nothing is re-downloaded.

```console
libationcli abs upload
```

Upload specific titles:

```console
libationcli abs upload B017V4IM1G
libationcli abs upload --id B017V4IM1G
```

Requires Audiobookshelf to be enabled and fully configured; otherwise the command reports what is missing and exits without processing anything. Settings can be supplied per-run with `--override`, for example:

```console
libationcli abs upload -o AudiobookshelfServerUrl="https://abs.example.com" -o AudiobookshelfApiToken="..."
```

Titles already on the server are skipped. The run ends with a summary of uploaded, already-on-server, no-files-found, failed, and skipped counts. Failures are also written to stderr; the command exits 0 either way. See [Audiobookshelf Auto-Upload](/docs/features/audiobookshelf#uploading-books-you-already-liberated).

## Liberate Pdfs Only

```console
libationcli liberate --pdf
libationcli liberate -p
```

Downloads nothing but PDFs, and never downloads an audiobook. A plain `liberate` covers the same titles, so this is for when you want only the PDFs.

A PDF is saved beside its audiobook, or in the folder the [folder template](/docs/features/naming-templates) names for that title when Libation cannot find the audio files. Before 13.7.10 the second case put the PDF directly in your Books directory.

## Re-Liberate a Single Book

After Audible updates a title (or to replace a bad file), re-download just that book. Naming an ASIN always re-downloads it, even if it is already liberated:

```console
libationcli liberate B017V4IM1G
libationcli liberate --id B017V4IM1G
```

`--id` / `-i` may be repeated for several titles.

## Re-Liberate the Entire Library

```console
libationcli liberate --force
libationcli liberate -f
```

`--force` also attempts the titles Audible recently refused, which a plain run leaves alone. See [Retrying titles Audible refuses](/docs/features/retrying-refused-downloads).

## Limit How Much One Run Downloads

A large library can take a long time and a lot of disk to liberate in one go. These options stop a run once it has downloaded a given amount, leaving the rest for the next run:

```console
libationcli liberate --limit-books 10
libationcli liberate --limit-mb 500
libationcli liberate --limit-gb 20
```

The three are mutually exclusive; using two together is an error. Each applies to a single invocation, so a scheduled or scripted run downloads what it may, stops, and the next run continues where it left off.

Only successful audiobook downloads count. Failed titles do not, and PDFs are neither counted nor limited, so a limit cannot be combined with `--pdf`. Titles the limit stopped Libation from reaching remain un-liberated and are picked up next time. Reaching the limit is not a failure: the command still exits **0**.

MB and GB are approximate, because Libation does not know how large a title is until it has downloaded it. When deciding whether there is room for another book it assumes about 400 MB for it, the same estimate it uses to warn about low disk space. One download is always allowed, so a limit smaller than a single book downloads one book rather than nothing.

This is a per-run limit, separate from the [daily download limit](/docs/features/daily-download-limit) setting. Both apply if both are set.

## Liberate using a license file from the `get-license` command

```console
libationcli liberate --license /path/to/license.lic
libationcli liberate --license - < /path/to/license.lic
```

## List Libation Settings

```console
libationcli get-setting
libationcli get-setting -b
libationcli get-setting FileDownloadQuality
```

## Override Libation Settings for the Command

```console
libationcli liberate B017V4IM1G -override FileDownloadQuality=Normal
libationcli liberate B017V4IM1G -o FileDownloadQuality=normal -o UseWidevine=true Request_xHE_AAC=true
```

## Copy the Local SQLite Database to Postgres

```console
libationcli copydb --connectionString "my postgres connection string"
libationcli copydb -c "my postgres connection string"
```

## Export Library to File

```console
libationcli export --path "C:\foo\bar\my.json" --json
libationcli export -p "C:\foo\bar\my.json" -j
libationcli export -p "C:\foo\bar\my.csv" --csv
libationcli export -p "C:\foo\bar\my.csv" -c
libationcli export -p "C:\foo\bar\my.xlsx" --xlsx
libationcli export -p "C:\foo\bar\my.xlsx" -x
```

## Set Download Status

Set download statuses throughout library based on whether each book's audio file can be found.  
Must include at least one flag: --downloaded , --not-downloaded.  
Downloaded: If the audio file can be found, set download status to 'Downloaded'.  
Not Downloaded: If the audio file cannot be found, set download status to 'Not Downloaded'  
UI: Visible Books \> Set 'Downloaded' status automatically. Visible books. Prompts before saving changes  
CLI: Full library. No prompt

```console
libationcli set-status -d
libationcli set-status -n
libationcli set-status -d -n
```

## Get a Content License Without Downloading

```console
libationcli get-license B017V4IM1G
```

## Example Powershell Script to Download Four Different Versions of the Same Book

```powershell
$asin="B017V4IM1G"

$xHE_AAC=@('true', 'false')
$Qualities=@('Normal', 'High')
foreach($q in $Qualities){
  foreach($x in $xHE_AAC){
	$license = ./libationcli get-license $asin --override FileDownloadQuality=$q --override Request_xHE_AAC=$x
	echo $($license | ConvertFrom-Json).ContentMetadata.content_reference
	echo $license | ./libationcli liberate --force --license -
  }
}
```

## Set custom replacement characters

Libation detects the replacment characters for filenames by identifying the
currently running OS and not the target filesystem. This can lead to problems
when running the Libation CLI on Linux but targeting an NTFS drive for the
download.

To change (and override) the replacment characters, the code snippet below can
be defined in the `Settings.json`. The example below contains the `HiFi_NTFS`
replacements that allow for high fidelity filenames when targeting an NTFS file
system.

::: details Example NTFS ReplacementCharacters
```json
  "ReplacementCharacters": {
    "Replacement": [
      {
        "CharacterToReplace": "\u0000",
        "ReplacementString": "_",
        "Description": "All other invalid characters"
      },
      {
        "CharacterToReplace": "/",
        "ReplacementString": "∕",
        "Description": "Forward Slash (Filename Only)"
      },
      {
        "CharacterToReplace": "\\",
        "ReplacementString": "",
        "Description": "Back Slash (Filename Only)"
      },
      {
        "CharacterToReplace": "\"",
        "ReplacementString": "“",
        "Description": "Open Quote"
      },
      {
        "CharacterToReplace": "\"",
        "ReplacementString": "”",
        "Description": "Close Quote"
      },
      {
        "CharacterToReplace": "\"",
        "ReplacementString": "＂",
        "Description": "Other Quote"
      },
      {
        "CharacterToReplace": "<",
        "ReplacementString": "＜",
        "Description": "Open Angle Bracket"
      },
      {
        "CharacterToReplace": ">",
        "ReplacementString": "＞",
        "Description": "Close Angle Bracket"
      },
      {
        "CharacterToReplace": ":",
        "ReplacementString": "_",
        "Description": "Colon"
      },
      {
        "CharacterToReplace": "*",
        "ReplacementString": "✱",
        "Description": "Asterisk"
      },
      {
        "CharacterToReplace": "?",
        "ReplacementString": "？",
        "Description": "Question Mark"
      },
      {
        "CharacterToReplace": "|",
        "ReplacementString": "⏐",
        "Description": "Vertical Line"
      }
    ]
  }
```
:::
