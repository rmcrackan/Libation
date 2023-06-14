# Naming Templates
File and Folder names can be customized using Libation's built-in tag template naming engine. To edit how folder and file names are created, go to Settings \> Download/Decrypt and edit the naming templates. If you're splitting your audiobook into multiple files by chapter, you can also use a custom template to set each chapter's title metadata tag by editing the template in Settings \> Audio File Options.

These templates apply to both GUI and CLI.

# Table of Contents

- [Template Tags](#template-tags)
  - [Property Tags](#property-tags)
  - [Conditional Tags](#conditional-tags)
- [Tag Formatters](#tag-formatters)
  - [Text Formatters](#text-formatters)
  - [Name List Formatters](#name-list-formatters)
  - [Integer Formatters](#integer-formatters)
  - [Date Formatters](#date-formatters)


# Template Tags

These are the naming template tags currently supported by Libation.

## Property Tags
These tags will be replaced in the template with the audiobook's values.

|Tag|Description|Type|
|-|-|-|
|\<id\> **†**|Audible book ID (ASIN)|Text|
|\<title\>|Full title|Text|
|\<title short\>|Title. Stop at first colon|Text|
|\<author\>|Author(s)|Name List|
|\<first author\>|First author|Text|
|\<narrator\>|Narrator(s)|Name List|
|\<first narrator\>|First narrator|Text|
|\<series\>|Name of series|Text|
|\<series#\>|Number order in series|Text|
|\<bitrate\>|File's original bitrate (Kbps)|Integer|
|\<samplerate\>|File's original audio sample rate|Integer|
|\<channels\>|Number of audio channels|Integer|
|\<account\>|Audible account of this book|Text|
|\<account nickname\>|Audible account nickname of this book|Text|
|\<locale\>|Region/country|Text|
|\<year\>|Year published|Integer|
|\<language\>|Book's language|Text|
|\<language short\> **†**|Book's language abbreviated. Eg: ENG|Text|
|\<file date\>|File creation date/time.|DateTime|
|\<pub date\>|Audiobook publication date|DateTime|
|\<date added\>|Date the book added to your Audible account|DateTime|
|\<ch count\> **‡**|Number of chapters|Integer|
|\<ch title\> **‡**|Chapter title|Text|
|\<ch#\> **‡**|Chapter number|Integer|
|\<ch# 0\> **‡**|Chapter number with leading zeros|Integer|

**†** Does not support custom formatting

**‡** Only valid for Chapter Filename and Chapter Tile Metadata

To change how these properties are displayed, [read about custom formatters](#tag-formatters)

## Conditional Tags
Anything between the opening tag (`<tagname->`) and closing tag (`<-tagname>`) will only appear in the name if the condition evaluates to true. 

|Tag|Description|Type|
|-|-|-|
|\<if series-\>...\<-if series\>|Only include if part of a book series or podcast|Conditional|
|\<if podcast-\>...\<-if podcast\>|Only include if part of a podcast|Conditional|
|\<if bookseries-\>...\<-if bookseries\>|Only include if part of a book series|Conditional|
|\<if podcastparent-\>...\<-if podcastparent\>**†**|Only include if item is a podcast series parent|Conditional|

**†** Only affects the podcast series folder naming if "Save all podcast episodes to the series parent folder" option is checked.

For example, <if podcast-\>\<series\>\<-if podcast\> will evaluate to the podcast's series name if the file is a podcast. For audiobooks that are not podcasts, that tag will be blank.

You can invert the condition (instead of displaying the text when the condition is true, display the text when it is false) by playing a '!' symbol before the opening tag name.

As an example, this folder template will place all Liberated podcasts into a "Podcasts" folder and all liberated books (not podcasts) into a "Books" folder.

\<if podcast-\>Podcasts<-if podcast\>\<!if podcast-\>Books\<-if podcast\>\\\<title\>


# Tag Formatters
**Text**, **Name List**, **Integer**, and **DateTime** tags can be optionally formatted using format text in square brackets after the tag name. Below is a list of supported formatters for each tag type.

## Text Formatters
|Formatter|Description|Example Usage|Example Result|
|-|-|-|-|
|L|Converts text to lowercase|\<title[L]\>|a study in scarlet꞉ a sherlock holmes novel|
|U|Converts text to uppercase|\<title short[U]\>|A STUDY IN SCARLET|

## Name List Formatters
|Formatter|Description|Example Usage|Example Result|
|-|-|-|-|
|separator()|Speficy the text used to join multiple people's names.<br><br>Default is ", "|`<author[separator(; )]>`|Arthur Conan Doyle; Stephen Fry|
|format(\{T \| F \| M \| L \| S\})|Formats the human name using the name part tags.<br>\{T\} = Title (e.g. "Dr.")<br>\{F\} = First name<br>\{M\} = Middle name<br>\{L\} = Last Name<br>\{S\} = Suffix (e.g. "PhD")<br><br>Default is \{P\} \{F\} \{M\} \{L\} \{S\} |`<author[format({L}, {F}) separator(; )]>`|Doyle, Arthur; Fry, Stephen|
|sort(F \| M \| L)|Sorts the names by first, middle, or last name<br><br>Default is unsorted|`<author[sort(M)]>`|Stephen Fry, Arthur Conan Doyle|
|max(#)|Only use the first # of names<br><br>Default is all names|`<author[max(1)]>`|Arthur Conan Doyle|

## Integer Formatters
|Formatter|Description|Example Usage|Example Result|
|-|-|-|-|
|# (a number)|Zero-pads the number|\<bitrate\[4\]\><br>\<series#\[3\]\><br>\<samplerate\[6\]\>|0128<br>001<br>044100|

## Date Formatters
Form more standard formatters, [see this guide from Microsoft](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings).
### Standard DateTime Formatters
|Formatter|Description|Example Usage|Example Result|
|-|-|-|-|
|s|Sortable date/time pattern.|\<file date[s]\>|2023-02-14T13:45:30|
|Y|Year month pattern.|\<file date[Y]\>|February 2023|

### Custom DateTime Formatters
You can use custom formatters to construct customized DateTime string. For more custom formatters and examples, [see this guide from Microsoft](https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings).
|Formatter|Description|Example Usage|Example Result|
|-|-|-|-|
|yyyy|4-digit year|\<file date[yyyy]\>|2023|
|yy|2-digit year|\<file date[yy]\>|23|
|MM|2-digit month|\<file date[MM]\>|02|
|dd|2-digit day of the month|\<file date[yyyy-MM-dd]\>|2023-02-14|
|HH<br>mm|The hour, using a 24-hour clock from 00 to 23<br>The minute, from 00 through 59.|\<file date[HH:mm]\>|14:45|


