# Naming Templates

File and Folder names can be customized using Libation's built-in tag template naming engine. To edit how folder and file names are created, go to Settings \> Download/Decrypt and edit the naming templates. If you're splitting your audiobook into multiple files by chapter, you can also use a custom template to set each chapter's title metadata tag by editing the template in Settings \> Audio File Options.

These templates apply to both GUI and CLI.

## Template Tags

These are the naming template tags currently supported by Libation.

### Property Tags

These tags will be replaced in the template with the audiobook's values.

| Tag                      | Description                                                    | Type                                   |
|--------------------------|----------------------------------------------------------------| -------------------------------------- |
| \<id\> **†**             | Audible book ID (ASIN)                                         | Text                                   |
| \<title\>                | Full title with subtitle                                       | [Text](#text-formatters)               |
| \<title short\>          | Title. Stop at first colon                                     | [Text](#text-formatters)               |
| \<audible title\>        | Audible's title (does not include subtitle)                    | [Text](#text-formatters)               |
| \<audible subtitle\>     | Audible's subtitle                                             | [Text](#text-formatters)               |
| \<author\>               | Author(s)                                                      | [Name List](#name-list-formatters)     |
| \<first author\>         | First author                                                   | [Name](#name-formatters)               |
| \<narrator\>             | Narrator(s)                                                    | [Name List](#name-list-formatters)     |
| \<first narrator\>       | First narrator                                                 | [Name](#name-formatters)               |
| \<series\>               | All series to which the book belongs (if any)                  | [Series List](#series-list-formatters) |
| \<first series\>         | First series                                                   | [Series](#series-formatters)           |
| \<series#\>              | Number order in series (alias for \<first series[{#}]\>        | [Number](#number-formatters)           |
| \<minutes\>              | Duration of the audiobook in minutes                           | [TimeSpan](#timespan-formatters)       |
| \<bitrate\>              | Bitrate (kbps) of the last downloaded audiobook                | [Number](#number-formatters)           |
| \<samplerate\>           | Sample rate (Hz) of the last downloaded audiobook              | [Number](#number-formatters)           |
| \<channels\>             | Number of audio channels in the last downloaded audiobook      | [Number](#number-formatters)           |
| \<codec\>                | Audio codec of the last downloaded audiobook                   | [Text](#text-formatters)               |
| \<file version\>         | Audible's file version number of the last downloaded audiobook | [Text](#text-formatters)               |
| \<libation version\>     | Libation version used during last download of the audiobook    | [Text](#text-formatters)               |
| \<account\>              | Audible account of this book                                   | [Text](#text-formatters)               |
| \<account nickname\>     | Audible account nickname of this book                          | [Text](#text-formatters)               |
| \<tag\>                  | Tag(s)                                                         | [Text List](#text-list-formatters)     |
| \<first tag\>            | First tag                                                      | [Text](#text-formatters)               |
| \<locale\>               | Region/country                                                 | [Region](#region-formatters)           |
| \<year\>                 | Year published                                                 | [Number](#number-formatters)           |
| \<language\>             | Book's language                                                | [Language](#language-formatters)       |
| \<language short\> **†** | Book's language abbreviated. Eg: ENG                           | Text                                   |
| \<os\>                   | Language currently set in the operating system                 | [Language](#language-formatters)       |
| \<ui\>                   | User interface language                                        | [Language](#language-formatters)       |
| \<file date\>            | File creation date/time.                                       | [DateTime](#date-formatters)           |
| \<pub date\>             | Audiobook publication date                                     | [DateTime](#date-formatters)           |
| \<date added\>           | Date the book added to your Audible account                    | [DateTime](#date-formatters)           |
| \<ch count\> **‡**       | Number of chapters                                             | [Number](#number-formatters)           |
| \<ch title\> **‡**       | Chapter title                                                  | [Text](#text-formatters)               |
| \<ch#\> **‡**            | Chapter number                                                 | [Number](#number-formatters)           |
| \<ch# 0\> **‡**          | Chapter number with leading zeros                              | [Number](#number-formatters)           |

**†** Does not support custom formatting

**‡** Only valid for Chapter Filename and Chapter Tile Metadata

To change how these properties are displayed, [read about custom formatters](#tag-formatters)

### Conditional Tags

Anything between the opening tag (`<tagname->`) and closing tag (`<-tagname>`) will only appear in the name if the condition evaluates to true.

| Tag                                                                | Description                                                                                     | Type        |
| ------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------- | ----------- |
| \<if series-\>...\<-if series\>                                    | Only include if part of a book series or podcast                                                | Conditional |
| \<if podcast-\>...\<-if podcast\>                                  | Only include if part of a podcast                                                               | Conditional |
| \<if bookseries-\>...\<-if bookseries\>                            | Only include if part of a book series                                                           | Conditional |
| \<if podcastparent-\>...\<-if podcastparent\>**†**                 | Only include if item is a podcast series parent                                                 | Conditional |
| \<if abridged-\>...\<-if abridged\>                                | Only include if item is abridged                                                                | Conditional |
| \<has PROPERTY-\>...\<-has\>                                       | Only include if the PROPERTY has a value (i.e. not null or empty)                               | Conditional |
| \<is PROPERTY[[CHECK](#checks)]-\>...\<-is\>                       | Only include if the PROPERTY or a single value of a list PROPERTY satisfies the CHECK           | Conditional |
| \<is PROPERTY[FORMAT][[CHECK](#checks)]-\>...\<-is\>               | Only include if the formatted PROPERTY or a single value of a list PROPERTY satisfies the CHECK | Conditional |
| \<is PROPERTY[...separator(...)...][[CHECK](#checks)]-\>...\<-is\> | Only include if the joined form of all formatted values of a list PROPERTY satisfies the CHECK  | Conditional |

**†** Only affects the podcast series folder naming if "Save all podcast episodes to the series parent folder" option is checked.

For example, `<if podcast-><series><-if podcast>` will evaluate to the podcast's series name if the file is a podcast. For audiobooks that are not podcasts, that tag will be blank.

You can invert the condition (instead of displaying the text when the condition is true, display the text when it is false) by playing a `!` symbol before the opening tag name.

| Inverted Tag                                        | Description                                                                                      | Type        |
| --------------------------------------------------- | ------------------------------------------------------------------------------------------------ | ----------- |
| \<!if series-\>...\<-if series\>                    | Only include if _not_ part of a book series or podcast                                           | Conditional |
| \<!if podcast-\>...\<-if podcast\>                  | Only include if _not_ part of a podcast                                                          | Conditional |
| \<!if bookseries-\>...\<-if bookseries\>            | Only include if _not_ part of a book series                                                      | Conditional |
| \<!if podcastparent-\>...\<-if podcastparent\>**†** | Only include if item is _not_ a podcast series parent                                            | Conditional |
| \<!has PROPERTY-\>...\<-has\>                       | Only include if the PROPERTY _does not_ have a value (i.e. is null or empty)                     | Conditional |
| \<!is PROPERTY[[CHECK](#checks)]-\>...\<-is\>       | Only include if neither the whole PROPERTY nor the values of a list PROPERTY satisfies the CHECK | Conditional |

**†** Only affects the podcast series folder naming if "Save all podcast episodes to the series parent folder" option is checked.

As an example, this folder template will place all Liberated podcasts into a "Podcasts" folder and all liberated books (not podcasts) into a "Books" folder.

`<if podcast->Podcasts<-if podcast><!if podcast->Books<-if podcast><title>`

This example will add a number if the `<series#>` tag has a value:

`<has series#><series#><-has>`

This example will put non-series books in a "Standalones" folder:

`<!if series->Standalones/<-if series>`

And this example will customize the title based on whether the book has a subtitle:

`<audible title><has audible subtitle->-<audible subtitle><-has>`

## Tag Formatters

**Text**, **Name**, **Series**, **Number**, **TimeSpan**, **DateTime**, **Region**, **Language** and their **List** tags can be optionally formatted using format text in square brackets after the tag name. Below is a list of supported formatters for each tag type.

### Text Formatters

Text formatting can change length and case of the text. Use \<#\>, \<#\>\<case\> or \<case\> to specify one or both of these.

| Formatter | Description                                                     | Example Usage      | Example Result                              |
| --------- | --------------------------------------------------------------- | ------------------ | ------------------------------------------- |
| #         | Cuts down the text to the specified number of characters        | \<title[14]\>      | A Study in Scar                             |
| L         | Converts text to lowercase                                      | \<title[L]\>       | a study in scarlet꞉ a sherlock holmes novel |
| U         | Converts text to uppercase                                      | \<title short[U]\> | A STUDY IN SCARLET                          |
| t         | Converts text to title case                                     | \<title[t]\>       | The Abc Murders                             |
| T         | Converts text to title case where uppercase words are preserved | \<title[T]\>       | The ABC Murders                             |
|           |                                                                 | \<title[6T]\>      | The AB                                      |

### Text List Formatters

| Formatter     | Description                                                                                                                                                              | Example Usage                                | Example Result                               |
| ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |--------------------------------------------- | ---------------------------------------------|
| separator()   | Specify the text used to join<br>multiple entries.<br><br>Default is ", "                                                                                                | `<tag[separator(_)]>`                        | Tag1_Tag2_Tag3_Tag4_Tag5                     |
| format(\{S\}) | Formats the entries by placing their values into the specified template.<br>Use {S:[Text_Formatter](#text-formatters)} to place the entry and optionally apply a format. | `<tag[format(Tag={S:l})`<br>`separator(;)]>` | Tag=tag1;Tag=tag2;Tag=tag3;Tag=tag4;Tag=tag5 |
| sort(S)       | Sorts the elements by their value.<br><br>*Sorting direction:*<br>uppercase = ascending<br>lowercase = descending<br><br>Default is unsorted                             | `<tag[sort(s)`<br>`separator(;)]>`           | Tag5;Tag4;Tag3;Tag2;Tag1                     |
| max(#)        | Only use the first # of entries                                                                                                                                          | `<tag[max(1)]>`                              | Tag1                                         |
| slice(#)      | Only use the nth entry of the list                                                                                                                                       | `<tag[slice(2)]>`                            | Tag2                                         |
| slice(#..)    | Only use entries of the list starting from #                                                                                                                             | `<tag[slice(2..)]>`                          | Tag2, Tag3, Tag4, Tag5                       |
| slice(..#)    | Like max(#). Only use the first # of entries                                                                                                                             | `<tag[slice(..1)]>`                          | Tag1                                         |
| slice(#..#)   | Only use entries of the list starting from # and ending at # (inclusive)                                                                                                 | `<tag[slice(2..4)]>`                         | Tag2, Tag3, Tag4                             |
| slice(-#..-#) | Numbers may be specified negative. In that case positions ar counted from the end with -1 pointing at the last member                                                    | `<tag[slice(-3..-2)]>`                       | Tag3, Tag4                                   |

### Series Formatters

| Formatter        | Description                                                                                                                                                                                                                                                                                                                                                                                                                               | Example Usage                                                                                                                | Example Result                                                                                              |
| ---------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| \{N \| # \| ID\} | Formats the series using<br>the series part tags.<br>\{N:[Text_Formatter](#text-formatters)\} = Series Name<br>\{#:[Number_Formatter](#number-formatters)\} = Number order in series<br>\{ID:[Text_Formatter](#text-formatters)\} = Audible Series ID<br><br>Formatter parts are optional and introduced by the colon. If specified the string will be used to format the part using the corresponding formatter.<br><br>Default is \{N\} | `<first series>`\<hr\>`<first series[{N:l}]>`\<hr\>`<first series[{N}, {#}, {ID}]>`\<hr\>`<first series[{N:10U}, {ID}, {#:00.0}]>` | Sherlock Holmes\<hr\>sherlock holmes\<hr\>Sherlock Holmes, 1-6, B08376S3R2\<hr\>SHERLOCK H, B08376S3R2, 01.0-06.0 |

### Series List Formatters

| Formatter                | Description                                                                                                                                                                                                                                                                                                  | Example Usage                                                                             | Example Result                                                                                                      |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------- |
| separator()              | Specify the text used to join<br>multiple series names.<br><br>Default is ", "                                                                                                                                                                                                                               | `<series[separator(; )]>`                                                                 | Sherlock Holmes; Some Other Series                                                                                  |
| format(\{N \| # \| ID\}) | Formats the series properties<br>using the name series tags.<br>See [Series Formatter Usage](#series-formatters) above.                                                                                                                                                                                      | `<series[format({N}, {#})`<br>`separator(; )]>`\<hr\>`<series[format({ID}-{N}, {#:00.0})]>` | Sherlock Holmes, 1-6; Book Collection, 1\<hr\>B08376S3R2-Sherlock Holmes, 01.0-06.0, B000000000-Book Collection, 01.0 |
| sort(N \| # \| ID)       | Sorts the series by name, number or ID.<br><br>These terms define the primary, secondary, tertiary, … sorting order.<br>You may combine multiple terms in sequence to specify multi‑level sorting.<br><br>*Sorting direction:*<br>uppercase = ascending<br>lowercase = descending<br><br>Default is unsorted | `<series[sort(N)`<br>`separator(; )]>`                                                    | Book Collection, 1; Sherlock Holmes, 1-6                                                                            |
| max(#)                   | Only use the first # of series                                                                                                                                                                                                                                                                               | `<series[max(1)]>`                                                                        | Sherlock Holmes                                                                                                     |
| slice(#..#)              | Only use entries of the series list starting from # and ending at # (inclusive)<br><br>See [Text List Formatter Usage](#Text-List-Formatters) above for details on all the variants of `slice()`                                                                                                             | `<series[slice(..-2)]>`                                                                   | Sherlock Holmes                                                                                                     |

### Name Formatters

| Formatter                       | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       | Example Usage                                                          | Example Result                          |
| ------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------- | --------------------------------------- |
| \{T \| F \| M \| L \| S \| ID\} | Formats the human name using<br>the name part tags.<br>\{T:[Text_Formatter](#text-formatters)\} = Title (e.g. "Dr.")<br>\{F:[Text_Formatter](#Text-Formatters)\} = First name<br>\{M:[Text_Formatter](#text-formatters)\} = Middle name<br>\{L:[Text_Formatter](#text-formatters)\} = Last Name<br>\{S:[Text_Formatter](#text-formatters)\} = Suffix (e.g. "PhD")<br>\{ID:[Text_Formatter](#text-formatters)\} = Audible Contributor ID<br><br>Formatter parts are optional and introduced by the colon. If specified the string will be used to format the part using the correspoing formatter.<br><br>Default is \{T\} \{F\} \{M\} \{L\} \{S\} | `<first narrator[{L}, {F:1}.]>`\<hr\>`<first author[{L:u}, {F} _{ID}_]>` | Fry, S.\<hr\>DOYLE, Arthur \_B000AQ43GQ\_ |

### Name List Formatters

| Formatter                               | Description                                                                                                                                                                                                                                                                                                                                                         | Example Usage                                                                                                  | Example Result                                                                                                                                           |
| --------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| separator()                             | Specify the text used to join<br>multiple people's names.<br><br>Default is ", "                                                                                                                                                                                                                                                                                    | `<author[separator(; )]>`                                                                                      | Arthur Conan Doyle; Stephen Fry                                                                                                                          |
| format(\{T \| F \| M \| L \| S \| ID\}) | Formats the human name using<br>the name part tags.<br>See [Name Formatter Usage](#name-formatters) above.                                                                                                                                                                                                                                                          | `<author[format({L:u}, {F})`<br>`separator(; )]>`\<hr\>`<author[format({L}, {F:1}.`<br>`_{ID}_) separator(; )]>` | DOYLE, Arthur; FRY, Stephen\<hr\>Doyle, A. \_B000AQ43GQ\_;<br>Fry, S. \_B000APAGVS\_                                                                       |
| sort(T \| F \| M \| L \| S \| ID)       | Sorts the names by title,<br> first, middle, or last name,<br>suffix or Audible Contributor ID<br><br>These terms define the primary, secondary, tertiary, … sorting order.<br>You may combine multiple terms in sequence to specify multi‑level sorting.<br><br>*Sorting direction:*<br>uppercase = ascending<br>lowercase = descending<br><br>Default is unsorted | `<author[sort(M)]>`\<hr\>`<author[sort(Fl)]>`\<hr\>`<author[sort(L FM ID)]>`                                         | Stephen Fry, Arthur Conan Doyle\<hr\>Stephen King, Stephen Fry\<hr\>John P. Smith \_B000TTTBBB\_, John P. Smith \_B000TTTCCC\_, John S. Smith \_B000HHHVVV\_ |
| max(#)                                  | Only use the first # of names<br><br>Default is all names                                                                                                                                                                                                                                                                                                           | `<author[max(1)]>`                                                                                             | Arthur Conan Doyle                                                                                                                                       |
| slice(#..#)                             | Only use entries of the names list starting from # and ending at # (inclusive)<br><br>See [Text List Formatter Usage](#Text-List-Formatters) above for details on all the variants of `slice()`                                                                                                                                                                     | `<author[slice(..-2)]>`                                                                                        | Arthur Conan Doyle                                                                                                                                       |

### TimeSpan Formatters
For more custom formatters and examples, [see this guide from Microsoft](https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-timespan-format-strings).

| Formatter | Description                                                                                                                                                                                                                                                                                                                                                                                                                                            | Example Usage             | Example Result |
|-----------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------------------|----------------|
| d         | The "d" custom format specifier outputs the value of the TimeSpan.Days property, which represents the number of whole days in the time interval. It outputs the full number of days in a TimeSpan value, even if the value has more than one digit. If the value of the TimeSpan.Days property is zero, the specifier outputs "0".<br><br>Use "dd"-"dddddddd" for zero padding up to the specified size.                                               | \<minutes[dd]\>             | 02             |
| h         | The "h" custom format specifier outputs the value of the TimeSpan.Hours property, which represents the number of whole hours in the time interval that isn't counted as part of its day component. It returns a one-digit string value if the value of the TimeSpan.Hours property is 0 through 9, and it returns a two-digit string value if the value of the TimeSpan.Hours property ranges from 10 to 23.<br><br>Use "hh" for zero padding.         | \<minutes[hh]\>             | 14             |
| m         | The "m" custom format specifier outputs the value of the TimeSpan.Minutes property, which represents the number of whole minutes in the time interval that isn't counted as part of its day component. It returns a one-digit string value if the value of the TimeSpan.Minutes property is 0 through 9, and it returns a two-digit string value if the value of the TimeSpan.Minutes property ranges from 10 to 59.<br><br>Use "mm" for zero padding. | \<minutes[m]\>              | 42             |
| 'string'  | Literal string delimiter.                                                                                                                                                                                                                                                                                                                                                                                                                              | \<minutes[d'd 'h'h 'm'm']\> | 2d 14h 42m     |
| \\        | The escape character.                                                                                                                                                                                                                                                                                                                                                                                                                                  | \<minutes[d\\d h\\h m\\m]\> | 2d 14h 42m     |

These formatters have been enhanced to allow the display of days, hours or months beyond their usual limits. For example, the total number of hours, even if it exceeds 23.
Here, a number format is inserted for the desired part in accordance with [Microsoft’s instructions](https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-numeric-format-strings). Unlike standard number formats, however, the letters D, H or M (uppercase) are used instead of zeros.

| Formatter | Description                                                                                                                                         | Example Usage                  | Example Result    |
|-----------|-----------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------|-------------------|
| D         | A number format with "D" instead of "0". Using this will output the total number of days and reduce the amount of minutes avalable for "H" and "M". | \<minutes[DD]\>                  | 02                |
| H         | A number format with "H" instead of "0". Using this will output the total number of hours and reduce the amount of minutes available for "M".       | \<minutes[HH]\>                  | 62                |
| M         | A number format with "M" instead of "0". Using this will output the total number of minutes.                                                        | \<minutes[#,#MM]\>               | 3,762             |
| D H M     | A combination of the above.                                                                                                                         | \<minutes[D'days 'MM'minutes']\> | 02days 882minutes |

### Number Formatters

For more custom formatters and examples, [see this guide from Microsoft](https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-numeric-format-strings).

|Formatter|Description|Example Usage|Example Result|
|-|-|-|-|
|\[integer\]|Zero-pads the number|\<bitrate\[4\]\><br>\<series#\[3\]\><br>\<samplerate\[6\]\>|0128<br>001<br>044100|
|0|Replaces the zero with the corresponding digit if one<br>is present; otherwise, zero appears in the result string.|\<series#\[000.0\]\>|001.0|
|#|Replaces the "#" symbol with the corresponding digit if one<br> is present; otherwise, no digit appears in the result string|\<series#\[00.##\]\>|01|

### Date Formatters

For more standard formatters, [see this guide from Microsoft](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings).

#### Standard DateTime Formatters

| Formatter | Description                 | Example Usage    | Example Result      |
| --------- | --------------------------- | ---------------- | ------------------- |
| s         | Sortable date/time pattern. | \<file date[s]\> | 2023-02-14T13:45:30 |
| Y         | Year month pattern.         | \<file date[Y]\> | February 2023       |

#### Custom DateTime Formatters

You can use custom formatters to construct customized DateTime string. For more custom formatters and examples, [see this guide from Microsoft](https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings).
|Formatter|Description|Example Usage|Example Result|
|-|-|-|-|
|yyyy|4-digit year|\<file date[yyyy]\>|2023|
|yy|2-digit year|\<file date[yy]\>|23|
|MM|2-digit month|\<file date[MM]\>|02|
|dd|2-digit day of the month|\<file date[yyyy-MM-dd]\>|2023-02-14|
|HH<br>mm|The hour, using a 24-hour clock from 00 to 23<br>The minute, from 00 through 59.|\<file date[HH:mm]\>|14:45|

### Region Formatters

You can specify which part of a region you are interested in.

| Formatter                                             | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     | Example Usage                                        | Example Result                        |
|-------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------|---------------------------------------|
| \{O \| I \| I2 \| I3 \| E \| N \| W \| L \| T \| ID\} | Formats the region using<br>the region part tags.<br>\{O:[Text_Formatter](#text-formatters)\} = Region as used in Libation<br>\{I:[Text_Formatter](#text-formatters)\} = Two letter ISO code<br>\{I2:[Text_Formatter](#text-formatters)\} = Two letter ISO code<br>\{I3:[Text_Formatter](#text-formatters)\} = Three letter ISO code<br>\{E:[Text_Formatter](#text-formatters)\} = English name<br>\{N:[Text_Formatter](#text-formatters)\} = Native name - OS dependent<br>\{W:[Text_Formatter](#number-formatters)\} = Unique Windows code<br>\{L:[Text_Formatter](#text-formatters)\} = Lang code used for this region/store<br>\{T:[Text_Formatter](#number-formatters)\} = TLD under which the audible store is hosted<br>\{ID:[Text_Formatter](#text-formatters)\} = Region code<br> <br><br>Formatter parts are optional and introduced by the colon. If specified the string will be used to format the part using the corresponding formatter.<br><br>Default is \{O\} | `<locale[{I} ({E})]>`<hr>`www.audible.<locale[{T}]>` | US (United States)<hr>www.audible.com |
| \{D\}  **†**                                          | Display name interpreted by the current language settings.<br>To ensure output in a specific language the lang-code to use might be specified with a leading '@'.<br>Formatter part is also optional and introduced by the colon.<br>\{D@LANG:[Text_Formatter](#text-formatters)\}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | `<locale[{D@es:u}]>`                                 | ESTADOS UNIDOS                        |

**†** LANG may be any code from the [ISO 639-1](https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes) standard like `es` for Spanish, `en` for English, `de` for German, etc. or even a [IETF language tag](https://en.wikipedia.org/wiki/IETF_language_tag) like 'fr-CA'.

### Language Formatters

You can specify which part of a language you are interested in.

| Formatter                                   | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | Example Usage              | Example Result |
|---------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------|----------------|
| \{O \| I \| I2 \| I3 \| E \| N \| W \| ID\} | Formats the language using<br>the language part tags.<br>\{O:[Text_Formatter](#text-formatters)\} = Language as provided by audible<br>\{I:[Text_Formatter](#text-formatters)\} = Two letter ISO code<br>\{I2:[Text_Formatter](#text-formatters)\} = Two letter ISO code<br>\{I3:[Text_Formatter](#text-formatters)\} = Three letter ISO code<br>\{E:[Text_Formatter](#text-formatters)\} = English name<br>\{N:[Text_Formatter](#text-formatters)\} = Native name - OS dependent<br>\{W:[Text_Formatter](#number-formatters)\} = Unique Windows code<br>\{ID:[Text_Formatter](#text-formatters)\} = Lang code<br><br>Formatter parts are optional and introduced by the colon. If specified the string will be used to format the part using the corresponding formatter.<br><br>Default is \{O\} | `<language[{I3:l} ({E})]>` | fra (French)   |
| \{D\}                                       | Display name interpreted by the current language settings.<br>To ensure output in a specific language the lang-code to use might be specified with a leading '@'.<br>Formatter part is also optional and introduced by the colon.<br>\{D@LANG:[Text_Formatter](#text-formatters)\}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | `<language[{D@es}]>`       | francés        |

### Checks

| Check-Pattern    | Description                                                                     | Example                                    |
| ---------------- | ------------------------------------------------------------------------------- | ------------------------------------------ |
| =STRING **†**    | Matches if one item is equal to STRING (case ignored)                           | \<is tag[=Tag1]-\>                         |
| !=STRING **†**   | Matches if one item is not equal to STRING (case ignored)                       | \<is first author[!=Arthur]-\>             |
| ~STRING **†**    | Matches if one items is matched by the regular expression STRING (case ignored) | \<is title[~(\[XYZ\]).*\\1]-\>             |
| #=NUMBER **‡**   | Matches if the number value is equal to NUMBER                                  | \<is channels[#=2]-\>                      |
| #!=NUMBER **‡**  | Matches if the number value is not equal to NUMBER                              | \<is author[#!=1]-\>                       |
| #\>=NUMBER **‡** | Matches if the number value is greater than or equal to NUMBER                  | \<is bitrate[#\>=128]-\>                   |
| #\>NUMBER **‡**  | Matches if the number value is greater than NUMBER                              | \<is title[#\>30]-\>                       |
| #\<=NUMBER **‡** | Matches if the number value is less than or equal to NUMBER                     | \<is first narrator[format({F})][#\<=1]-\> |
| #\<NUMBER **‡**  | Matches if the number value is less than NUMBER                                 | \<is author[#\<3]-\>                       |

**†** STRING maybe escaped with a backslash. So even square brackets could be used. If a single backslash should be part of the string, it must be doubled.

**‡** NUMBER checks on lists are checking the size of the list. If the value to check is a string, its length is used.

#### More complex examples

This example will truncate the title to 4 characters and check its (trimmed) value to be "the" in any case:

`<is title[4][=the]>`

Here the second to fourth tag is taken and joined with a colon. The result is then checked to be equal to "Tag2:Tag3:Tag4":

`<is tag[separator(:)slice(2..4)][=Tag2:Tag3:Tag4]->`
