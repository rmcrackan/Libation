## [Download Libation](https://github.com/rmcrackan/Libation/releases/latest)

### If you found this useful, tell a friend. If you found this REALLY useful, you can click here to [PayPal.me](https://paypal.me/mcrackan?locale.x=en_us)
...or just tell more friends. As long as I'm maintaining this software, it will remain **free** and **open source**.



# Getting started: Table of Contents

- [Download Libation](#download-libation-1)
- [Installation](#installation)
- [Create Accounts](#create-accounts)
- [Import your library](#import-your-library)
- [Download your books -- DRM-free!](#download-your-books----drm-free)
- [Download PDF attachments](#download-pdf-attachments)
- [Details of downloaded files](#details-of-downloaded-files)
- [Export your library](#export-your-library)
- [I still need help](#i-still-need-help)



### [Download Libation](https://github.com/rmcrackan/Libation/releases)

##### Which version? Chardonnay vs Classic

Nearly 100% of the difference is look and feel -- it's a matter of preference.

Chardonnay has an updated look and will work and look the same on Windows, Mac, and Linux.  
Classic is Windows only. It has an older look because it's built with older, duller, and more mature technology. This tech has built into it better support for things like accessibility for screen readers.

### Installation

* Windows

Extract the zip file to a folder and then run `Libation.exe` from inside of that folder. Do not put it in Program Files. The inability to edit files from there causes problems with configuration and updating.

* [Linux](InstallOnLinux.md)
* [MacOS](InstallOnMac.md)

### Create Accounts

Create your account(s):

![Create your accounts, menu](images/v40_accounts.png)

New locale options include many more regions including old audible accounts which pre-date the amazon acquisition

![Choose your account locales](images/v40_locales.png)

### Import your library

Be default, Libation will periodically scan the accounts you added above with a checkbox next to them. Nothing for you to do. You can also scan manually.

Select Import > Scan Library:

![Import step 1](images/Import1.png)

Or if you have multiple accounts, you'll get to choose whether to scan all accounts or just the ones you select:

![Import which accounts](images/v40_import.png)

If this is a new installation, or you're scanning an account you haven't scanned before, you'll be prompted to enter your password for the Audible account.

![Login password](images/alt-login1.png)

Enter the password and click Submit. Audible will prompt you with a CAPTCHA image. 

![Login captcha](images/alt-login2.png)

Enter the CAPTCHA answer characters and click Submit. If all has gone well, Libation will start scanning the account. 

In rare instances, the Captcha image/response will fail in an endless loop. If this happens, delete the problem account, and then click Save. Re-add the account and click Save again. Now try to scan the account again. This time, instead of typing your password, click the link that says "Or click here". This will open the Audible External Login dialog shown below.

![Login alternative setup](images/alt-login3.png)

You can either copy the URL shown and paste it into your browser or launch the browser directly by clicking Launch in Browser. Audible will display its standard login page. Login, including answering the CAPTCHA on the next page. In some cases, you might have to approve the login from the email account associated with that login, but once the login is successful, you'll see an error message.

![Login alternative login result](images/alt-login4.png)

This actually means you've successfully logged in. Copy the entire URL shown in your browser and return to Libation. Paste that URL into the text box at the bottom of the Audible External Login window and click Submit.

You'll see this window while it's scanning:

![Import step 2](images/Import2.png)

Success! We see how many new titles are imported:

![Import step 3](images/Import3.png)

### Download your books -- DRM-free!

Automatically download some or all of your audible books. This shows you how much of your library is not yet downloaded and decrypted:

The stoplights will tell you a title's status:

* Green: downloaded and decrypted
* Yellow: downloaded but still encrypted with DRM
* Red: not downloaded
* PDF icon without arrow: downloaded
* PDF with arrow: not downloaded

Or hover over the button to see the status.

![Liberate book step 1](images/LiberateBook1.png)

Select Liberate > Begin Book Backups

You can also click on the stop light to download only that title and its PDF

![Liberate book step 2](images/LiberateBook2.png)

First the original book with DRM is downloaded

![Liberate book step 3](images/LiberateBook3.png)

Then it's decrypted so you can use it on any device you choose. The very first time you decrypt a book, this step will take a while. Every other book will go much faster. The first time, Libation has to figure out the special decryption key which allows your personal books to be unlocked.

![Liberate book step 4](images/LiberateBook4.png)

And voila! If you have multiple books not yet liberated, Libation will automatically move on to the next.

![Liberate book step 5](images/LiberateBook5.png)

The Audible id must be somewhere in the book's file or folder name for Libation to detect your downloaded book.

### Download PDF attachments

For books which include PDF downloads, Libation can download these for you as well and will attempt to store them with the book. "Book backup" will already download an available PDF. This additional option is useful when Audible adds a PDF to your book after you've already backed it up.

Select Liberate > Begin PDF Backups

![PDF download step 2](images/PdfDownload2.png)

The downloads work just like with books, only with no additional decryption needed.

![PDF download step 3](images/PdfDownload3.png)

### Details of downloaded files

![Post download](images/PostDownload.png)

When you set up Libation, you'll specify a Books directory. Libation looks inside that directory and all subdirectories to look for files or folders with each library book's audible id. This way, organization is completely up to you. When you download + decrypt a book, you get several files

* .m4b: your audiobook in m4b format. This is the most pure version of your audiobook and retains the highest quality. Now that it's decrypted, you can play it on any audio player and put it on any device. If you'd like, you can also use 3rd party tools to turn it into an mp3. The freedom to do what you want with your files was the original inspiration for Libation.
* .cue: this is a file which logs where chapter breaks occur. Many tools are able to use this if you want to split your book into files along chapter lines.

### Export your library

![Export](images/Export.png)

Export your library to Excel, CSV, or JSON

### I still need help

[You can open an issue here](https://github.com/rmcrackan/Libation/issues) for bug reports, feature requests, or specialized help.
