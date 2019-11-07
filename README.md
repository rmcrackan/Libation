# Libation: Liberate your Library

# Table of Contents

1. [Audible audiobook manager](#audible-audiobook-manager)
    - [The good](#the-good)
    - [The bad](#the-bad)
    - [The ugly](#the-ugly)
2. [Getting started](#getting-started)
    - [Import your library](#import-your-library)
    - [Download your books -- DRM-free!](#download-your-books----drm-free)
    - [Download PDF attachments](#download-pdf-attachments)
    - [Details of downloaded files](#details-of-downloaded-files)
3. [Searching and filtering](#searching-and-filtering)
    - [Tags](#tags)

## Audible audiobook manager

### The good

* Import library from audible, including cover art
* Download and remove DRM from all books
* Download accompanying PDFs
* Add tags to books for better organization
* Powerful advanced search built on the Lucene search engine
* Customizable saved filters for common searches
* Open source
* Tested on US Audible only. Should theoretically also work for Canada, UK, Germany, and France

<a name="theBad"/>

### The bad

* Windows only
* Several known speed/performance issues
* Made by a programmer, not a designer so the goals are function rather than beauty. And it shows

### The ugly

* Documentation? Yer lookin' at it
* This is a single-developer personal passion project. Support, response, updates, enhancements, bug fixes etc are as my free time allows
* I have a full-time job, a life, and a finite attention span. Therefore a lot of time can potentially go by with no improvements of any kind

Disclaimer: I've made every good-faith effort to include nothing insecure, malicious, anti-privacy, or destructive. That said: use at your own risk.

I made this for myself and I want to share it with the great programming and audible/audiobook communiites which have been so generous with their time and help.

## Getting started

### Import your library

Select Import > Scan Library:

![Import step 1](images/Import1.png)

You'll see this window while it's scanning:

![Import step 2](images/Import2.png)

Success! We see how many new titles are imported:

![Import step 3](images/Import3.png)

### Download your books -- DRM-free!

Automatically download some or all of your audible books. This shows you how much of your library is not yet downloaded and decrypted:

![Liberate book step 1](images/LiberateBook1.png)

Select Liberate > Begin Book Backups

![Liberate book step 2](images/LiberateBook2.png)

First the original book with DRM is downloaded

![Liberate book step 3](images/LiberateBook3.png)

Then it's decrypted so you can use it on any device you choose. The very first time you decrypt a book, this step will take a while. Every other book will go much faster. The first time, Libation has to figure out the special decryption key which allows your personal books to be unlocked.

![Liberate book step 4](images/LiberateBook4.png)

And voila! If you have multiple books not yet liberated, Libation will automatically move on to the next.

![Liberate book step 5](images/LiberateBook5.png)

### Download PDF attachments

For books which include PDF downloads, Libation can download these for you as well and will attempt to store them with the book

![PDF download step 1](images/PdfDownload1.png)

Select Liberate > Begin PDF Backups

![PDF download step 2](images/PdfDownload2.png)

The downloads work just like with books, only with no additional decryption needed.

![PDF download step 3](images/PdfDownload3.png)

Ta da!

![PDF download step 4](images/PdfDownload4.png)

### Details of downloaded files

![Post download](images/PostDownload.png)

When you set up Libation, you'll specify a Books directory. Libation looks inside that directory and all subdirectories to look for files or folders with each library book's audible id. This way, organization is completely up to you. When you download + decrypt a book, you get several files

* .m4b: your audiobook in m4b format. This is the most pure version of your audiobook and retains the highest quality. Now that it's decrypted, you can play it on any audio player and put it on any device. If you'd like, you can also use 3rd party tools to turn it into an mp3. The freedom to do what you want with your files was the original inspiration for Libation.
* .cue: this is a file which logs where chapter breaks occur. Many tools are able to use this if you want to split your book into files along chapter lines.
* .nfo: This is just some general info about the book and includes some technical stats about the audiofile.

## Searching and filtering

### Tags

To add tags to a title, click the tags button

![tags step 1](images/Tags1.png)

Add as many tags as you'd like. Tags are separated by a space. Each tag can contain letters, numbers, and underscores

![tags step 2](images/Tags2.png)

Tags are saved non-case specific for easy search. There is one special tag "hidden" which will also grey-out a title's text

![tags step 3](images/Tags3.png)

To edit tags, just click the button again.
