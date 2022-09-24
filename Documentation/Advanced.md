## [Download Libation](https://github.com/rmcrackan/Libation/releases/latest)

### If you found this useful, tell a friend. If you found this REALLY useful, you can click here to [PalPal.me](https://paypal.me/mcrackan?locale.x=en_us)
...or just tell more friends. As long as I'm maintaining this software, it will remain **free** and **open source**.



# Advanced: Table of Contents

- [Files and folders](#files-and-folders)
- [Settings](#settings)
- [Custom File Naming](#custom-file-naming)
- [Command Line Interface](#command-line-interface)



### Files and folders

To make upgrades and reinstalls easier, Libation separates all of its responsibilities to a few different folders. If you don't want to mess with this stuff: ignore it. Read on if you like a little more control over your files.

* In Libation's initial folder are the files that make up the program. Since nothing else is here, just copy new files here to upgrade the program. Delete this folder to delete Libation.

* In a separate folder, Libation keeps track of all of the files it creates like settings and downloaded images. After an upgrade, Libation might think that's its being run for the first time. Just click ADVANCED SETUP and point to this folder. Libation will reload your library and settings.

* The last important folder is the "books location." This is where Libation looks for your downloaded and decrypted books. This is how it knows which books still need to be downloaded. The Audible id must be somewhere in the book's file or folder name for Libation to detect your downloaded book.

### Settings

* Allow Libation to fix up audiobook metadata. After decrypting a title, Libation attempts to fix details like chapters and cover art. Some power users and/or control freaks prefer to manage this themselves. By unchecking this setting, Libation will only decrypt the book and will leave metadata as-is, warts and all.

### Custom File Naming

In Settings, on the Download/Decrypt tab, you can specify the format in which you want your files to be named. As you edit these templates, a live example will be shown. Parameters are listed for folders, files, and files split by chapter including an explanation of what each naming option means. For instance: you can use template `<title short> - <ch# 0> of <ch count> - <ch title>` to create the file `A Study in Scarlet - 04 of 10 - A Flight for Life.m4b`.

These templates apply to GUI and CLI.

### Command Line Interface

Libationcli.exe allows limited access to Libation's functionalities as a CLI.

Warnings about relying solely on on the CLI:
* CLI will not perform any upgrades.
* It will show that there is an upgrade, but that will likely scroll by too fast to notice.
* It will not perform all post-upgrade migrations. Some migrations are only be possible by launching GUI.

```
help
  libationcli --help
  
verb-specific help
  libationcli scan --help
  
scan all libraries
  libationcli scan
scan only libraries for specific accounts
  libationcli scan nickname1 nickname2
  
convert all m4b files to mp3
  libationcli convert
  
liberate all books and pdfs
  libationcli liberate
liberate pdfs only
  libationcli liberate --pdf
  libationcli liberate -p
  
export library to file
  libationcli export --path "C:\foo\bar\my.json" --json
  libationcli export -p "C:\foo\bar\my.json" -j
  libationcli export -p "C:\foo\bar\my.csv" --csv
  libationcli export -p "C:\foo\bar\my.csv" -c
  libationcli export -p "C:\foo\bar\my.xlsx" --xlsx
  libationcli export -p "C:\foo\bar\my.xlsx" -x
```
