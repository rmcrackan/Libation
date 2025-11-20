## [Download Libation](https://github.com/rmcrackan/Libation/releases/latest)

### If you found this useful, tell a friend. If you found this REALLY useful, you can click here to [PayPal.me](https://paypal.me/mcrackan?locale.x=en_us)
...or just tell more friends. As long as I'm maintaining this software, it will remain **free** and **open source**.



# Advanced: Table of Contents

- [Files and folders](#files-and-folders)
- [Settings](#settings)
- [Custom File Naming](NamingTemplates.md)
- [Custom Theme Colors](#custom-theme-colors) (Chardonnay Only)
- [Command Line Interface](#command-line-interface)
- [Audio Formats (Dolby Atmos, Widevine, Spacial Audio)](AudioFileFormats.md)

### Files and folders

To make upgrades and reinstalls easier, Libation separates all of its responsibilities to a few different folders. If you don't want to mess with this stuff: ignore it. Read on if you like a little more control over your files.

* In Libation's initial folder are the files that make up the program. Since nothing else is here, just copy new files here to upgrade the program. Delete this folder to delete Libation.

* In a separate folder, Libation keeps track of all of the files it creates like settings and downloaded images. After an upgrade, Libation might think that's its being run for the first time. Just click ADVANCED SETUP and point to this folder. Libation will reload your library and settings.

* The last important folder is the "books location." This is where Libation looks for your downloaded and decrypted books. This is how it knows which books still need to be downloaded. The Audible id must be somewhere in the book's file or folder name for Libation to detect your downloaded book.

### Settings

* Allow Libation to fix up audiobook metadata. After decrypting a title, Libation attempts to fix details like chapters and cover art. Some power users and/or control freaks prefer to manage this themselves. By unchecking this setting, Libation will only decrypt the book and will leave metadata as-is, warts and all.

In addition to the options that are enabled if you allow Libation to "fix up" the audiobook, it does the following:

* Adds the `TCOM` (`@wrt` in M4B files) metadata tag for the narrators.
* Sets the `©gen` metadata tag for the genres.
* Unescapes the copyright symbol (replace `&#169;` with `©`)
* Replaces the recording copyright `(P)` string with `℗`
* Replaces the chapter markers embedded in the aax file with the chapter markers retrieved from Audible's API.
* Sets the embedded cover art image with the 500x500 px cover art retrieved from Audible

### Custom Theme Colors

In Libation Chardonnay (not Classic), you may adjust the app colors using the built-in theme editor. Open the Settings window (from the menu bar: Settings > Settings). On the "Important" settings tab, click "Edit Theme Colors".

#### Theme Editor Window

The theme editor has a list of style names and their currently assigned colors. To change a style color, click on the color swatch in the left-hand column to open the color editor for that style. Observe the color changes in real-time on the built-in preview panel on the right-hand side of the theme editor.

You may import or export themes using the buttons at the bottom-left of the theme editor.
"Cancel" or closing the window will revert any changes you've made in the theme editor.
"Reset" will reset any changes you've made in the theme editor.
"Defaults" will restore the application default colors for the active theme ("Light" or "Dark")
"Save" will save the theme colors to the ChardonnayTheme.json file and close the editor.

Note: you may only edit the currently applied theme ("Light" or "Dark").

#### Video Walkthrough
The below video demonstrates using the theme editor to make changes to the Dark theme color pallet.

[](https://github.com/user-attachments/assets/05c0cb7f-578f-4465-9691-77d694111349)

### Command Line Interface

Libationcli.exe allows limited access to Libation's functionalities as a CLI.

Warnings about relying solely on on the CLI:
* CLI will not perform any upgrades.
* It will show that there is an upgrade, but that will likely scroll by too fast to notice.
* It will not perform all post-upgrade migrations. Some migrations are only be possible by launching GUI.

#### Help
```console
libationcli --help
```
#### Verb-Specific Help
```console
libationcli scan --help
```
#### Scan All Libraries
```console
libationcli scan
```
#### Scan Only Libraries for Specific Accounts
```console
libationcli scan nickname1 nickname2
```
#### Convert All m4b Files to mp3
```console
libationcli convert
```
#### Liberate All Books and Pdfs
```console
libationcli liberate
```
#### Liberate Pdfs Only
```console
libationcli liberate --pdf
libationcli liberate -p
```
#### Force Book(s) to Re-Liberate
```console
libationcli liberate --force
libationcli liberate -f
```
#### List Libation Settings
```console
libationcli get-setting
libationcli get-setting -b
libationcli get-setting FileDownloadQuality
```
#### Override Libation Settings for the Command
```console
libationcli liberate B017V4IM1G -override FileDownloadQuality=Normal
libationcli liberate B017V4IM1G -o FileDownloadQuality=normal -o UseWidevine=true Request_xHE_AAC=true -f
```
#### Copy the Local SQLite Database to Postgres
```console
libationcli copydb --connectionString "my postgres connection string"
libationcli copydb -c "my postgres connection string"
```
#### Export Library to File
```console
libationcli export --path "C:\foo\bar\my.json" --json
libationcli export -p "C:\foo\bar\my.json" -j
libationcli export -p "C:\foo\bar\my.csv" --csv
libationcli export -p "C:\foo\bar\my.csv" -c
libationcli export -p "C:\foo\bar\my.xlsx" --xlsx
libationcli export -p "C:\foo\bar\my.xlsx" -x
```
#### Set Download Status
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
#### Get a Content License Without Downloading
```console
libationcli get-license B017V4IM1G
```
#### Example Powershell Script to Download Four Differenf Versions f the Same Book
```powershell
$asin="B017V4IM1G"

$xHE_AAC=@('true', 'false')
$Qualities=@('Normal', 'High')
foreach($q in $Qualities){
  foreach($x in $xHE_AAC){
	$license = ./libationcli get-license $asin --override FileDownloadQuality=$q --override Request_xHE_AAC=$x
	echo $($license | ConvertFrom-Json).ContentMetadata.content_reference
	echo $license | ./libationcli liberate --force
  }
}
```
