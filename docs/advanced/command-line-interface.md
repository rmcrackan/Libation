# Command Line Interface

Libationcli.exe allows limited access to Libation's functionalities as a CLI.

Warnings about relying solely on on the CLI:

- CLI will not perform any upgrades.
- It will show that there is an upgrade, but that will likely scroll by too fast to notice.
- It will not perform all post-upgrade migrations. Some migrations are only be possible by launching GUI.

## Progress Bar

The **liberate** and **convert** commands show a progress bar in the terminal while downloading or converting (e.g. `[##########----------]  2.5 min remaining`). The progress bar is only shown when the CLI is run interactively with output not redirected.

To **turn off the progress bar** (for scripting, logging, or cleaner output), redirect standard output and/or standard error. The progress bar is automatically disabled when either stream is redirected.

```console
libationcli liberate > log.txt 2>&1
libationcli convert 2> errors.txt
```

Redirecting also avoids progress-bar control characters in log files.

## Help

```console
libationcli --help
```

## Verb-Specific Help

```console
libationcli scan --help
```

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

## Liberate Pdfs Only

```console
libationcli liberate --pdf
libationcli liberate -p
```

## Force Book(s) to Re-Liberate

```console
libationcli liberate --force
libationcli liberate -f
```

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
libationcli liberate B017V4IM1G -o FileDownloadQuality=normal -o UseWidevine=true Request_xHE_AAC=true -f
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
