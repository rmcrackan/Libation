## [Download Libation](https://github.com/rmcrackan/Libation/releases/latest)

### If you found this useful, tell a friend. If you found this REALLY useful, you can click here to [PayPal.me](https://paypal.me/mcrackan?locale.x=en_us)
...or just tell more friends. As long as I'm maintaining this software, it will remain **free** and **open source**.



# Naming Templates
File and Folder names can be customized using Libation's built-in tag template naming engine. To edit how folder and file names are created, go to Settings \> Download/Decrypt and edit the naming templates. If you're splitting your audiobook into multiple files by chapter, you can also use a custom template to set each chapter's title metadata tag by editing the template in Settings \> Audio File Options.

These templates apply to both GUI and CLI.

# Table of Contents

- [Template Tags](#template-tags)
  - [Property Tags](#property-tags)
  - [Conditional Tags](#conditional-tags)
- [Tag Formatters](#tag-formatters)
  - [Text Formatters](#text-formatters)
  - [Series Formatters](#series-formatters)
  - [Series List Formatters](#series-list-formatters)
  - [Name Formatters](#name-formatters)
  - [Name List Formatters](#name-list-formatters)
  - [Number Formatters](#number-formatters)
  - [Date Formatters](#date-formatters)


# Template Tags

These are the naming template tags currently supported by Libation.

## Property Tags
These tags will be replaced in the template with the audiobook's values.

|Tag|Description|Type|
|-|-|-|
|\<id\> **†**|Audible book ID (ASIN)|Text|
|\<title\>|Full title with subtitle|[Text](#text-formatters)|
|\<title short\>|Title. Stop at first colon|[Text](#text-formatters)|
|\<audible title\>|Audible's title (does not include subtitle)|[Text](#text-formatters)|
|\<audible subtitle\>|Audible's subtitle|[Text](#text-formatters)|
|\<author\>|Author(s)|[Name List](#name-list-formatters)|
|\<first author\>|First author|[Name](#name-formatters)|
|\<narrator\>|Narrator(s)|[Name List](#name-list-formatters)|
|\<first narrator\>|First narrator|[Name](#name-formatters)|
|\<series\>|All series to which the book belongs (if any)|[Series List](#series-list-formatters)|
|\<first series\>|First series|[Series](#series-formatters)|
|\<series#\>|Number order in series (alias for \<first series[{#}]\>|[Number](#number-formatters)|
|\<bitrate\>|Bitrate (kbps) of the last downloaded audiobook|[Number](#number-formatters)|
|\<samplerate\>|Sample rate (Hz) of the last downloaded audiobook|[Number](#number-formatters)|
|\<channels\>|Number of audio channels in the last downloaded audiobook|[Number](#number-formatters)|
|\<codec\>|Audio codec of the last downloaded audiobook|[Text](#text-formatters)|
|\<file version\>|Audible's file version number of the last downloaded audiobook|[Text](#text-formatters)|
|\<libation version\>|Libation version used during last download of the audiobook|[Text](#text-formatters)|
|\<account\>|Audible account of this book|[Text](#text-formatters)|
|\<account nickname\>|Audible account nickname of this book|[Text](#text-formatters)|
|\<locale\>|Region/country|[Text](#text-formatters)|
|\<year\>|Year published|[Number](#number-formatters)|
|\<language\>|Book's language|[Text](#text-formatters)|
|\<language short\> **†**|Book's language abbreviated. Eg: ENG|Text|
|\<file date\>|File creation date/time.|[DateTime](#date-formatters)|
|\<pub date\>|Audiobook publication date|[DateTime](#date-formatters)|
|\<date added\>|Date the book added to your Audible account|[DateTime](#date-formatters)|
|\<ch count\> **‡**|Number of chapters|[Number](#number-formatters)|
|\<ch title\> **‡**|Chapter title|[Text](#text-formatters)|
|\<ch#\> **‡**|Chapter number|[Number](#number-formatters)|
|\<ch# 0\> **‡**|Chapter number with leading zeros|[Number](#number-formatters)|

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
**Text**, **Name List**, **Number**, and **DateTime** tags can be optionally formatted using format text in square brackets after the tag name. Below is a list of supported formatters for each tag type.

## Text Formatters
|Formatter|Description|Example Usage|Example Result|
|-|-|-|-|
|L|Converts text to lowercase|\<title[L]\>|a study in scarlet꞉ a sherlock holmes novel|
|U|Converts text to uppercase|\<title short[U]\>|A STUDY IN SCARLET|

## Series Formatters
|Formatter|Description|Example Usage|Example Result|
|-|-|-|-|
|\{N \| # \| ID\}|Formats the series using<br>the series part tags.<br>\{N\} = Series Name<br>\{#\} = Number order in series<br>\{ID\} = Audible Series ID<br><br>Default is \{N\}|`<first series>`<hr>`<first series[{N}]>`<hr>`<first series[{N}, {#}, {ID}]>`|Sherlock Holmes<hr>Sherlock Holmes<hr>Sherlock Holmes, 1, B08376S3R2|

## Series List Formatters
|Formatter|Description|Example Usage|Example Result|
|-|-|-|-|
|separator()|Speficy the text used to join<br>multiple series names.<br><br>Default is ", "|`<series[separator(; )]>`|Sherlock Holmes; Some Other Series|
|format(\{N \| # \| ID\})|Formats the series properties<br>using the name series tags.<br>See [Series Formatter Usage](#series-formatters) above.|`<series[format({N}, {#})`<br>`separator(; )]>`<hr>`<author[format({L}, {ID}) separator(; )]>`|Sherlock Holmes, 1; Some Other Series, 1<hr>herlock Holmes, B08376S3R2; Some Other Series, B000000000|
|max(#)|Only use the first # of series<br><br>Default is all series|`<series[max(1)]>`|Sherlock Holmes|

## Name Formatters
|Formatter|Description|Example Usage|Example Result|
|-|-|-|-|
|\{T \| F \| M \| L \| S \| ID\}|Formats the human name using<br>the name part tags.<br>\{T\} = Title (e.g. "Dr.")<br>\{F\} = First name<br>\{M\} = Middle name<br>\{L\} = Last Name<br>\{S\} = Suffix (e.g. "PhD")<br>\{ID\} = Audible Contributor ID<br><br>Default is \{P\} \{F\} \{M\} \{L\} \{S\}|`<first narrator[{L}, {F}]>`<hr>`<first author[{L}, {F} _{ID}_]>`|Fry, Stephen<hr>Doyle, Arthur \_B000AQ43GQ\_;<br>Fry, Stephen \_B000APAGVS\_|

## Name List Formatters
|Formatter|Description|Example Usage|Example Result|
|-|-|-|-|
|separator()|Speficy the text used to join<br>multiple people's names.<br><br>Default is ", "|`<author[separator(; )]>`|Arthur Conan Doyle; Stephen Fry|
|format(\{T \| F \| M \| L \| S \| ID\})|Formats the human name using<br>the name part tags.<br>See [Name Formatter Usage](#name-formatters) above.|`<author[format({L}, {F})`<br>`separator(; )]>`<hr>`<author[format({L}, {F}`<br>`_{ID}_) separator(; )]>`|Doyle, Arthur; Fry, Stephen<hr>Doyle, Arthur \_B000AQ43GQ\_;<br>Fry, Stephen \_B000APAGVS\_|
|sort(F \| M \| L)|Sorts the names by first, middle,<br>or last name<br><br>Default is unsorted|`<author[sort(M)]>`|Stephen Fry, Arthur Conan Doyle|
|max(#)|Only use the first # of names<br><br>Default is all names|`<author[max(1)]>`|Arthur Conan Doyle|

## Number Formatters
For more custom formatters and examples, [see this guide from Microsoft](https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-numeric-format-strings).
|Formatter|Description|Example Usage|Example Result|
|-|-|-|-|
|\[integer\]|Zero-pads the number|\<bitrate\[4\]\><br>\<series#\[3\]\><br>\<samplerate\[6\]\>|0128<br>001<br>044100|
|0|Replaces the zero with the corresponding digit if one<br>is present; otherwise, zero appears in the result string.|\<series#\[000.0\]\>|001.0|
|#|Replaces the "#" symbol with the corresponding digit if one<br> is present; otherwise, no digit appears in the result string|\<series#\[00.##\]\>|01|

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


