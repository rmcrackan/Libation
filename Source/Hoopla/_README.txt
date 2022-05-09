From a Libation user about possibility of integrating Hoopla:

I have a powershell script.  I didn't write it, and neither did the person that gave it to me.  It works most of the time (98%).  Some titles, it doesn't play well with, but does allow to keep the downloaded data, whether the decrypt was successful, or not, and then you can mess with the data, from there.

If you run the script with no parameters, then all the books in your library will download, and decrypt into the same directory as the script, into a folder named Completed.

If you run the script with the command:
  '.\HooplaDownloader.newer.ps1 -KeepDecryptedData'
then it will, and will notify you, when complete, where it was stored.

There is a parameter to download a specific titleID#, whether it's in your library, or not, but I've not played with it that far, as the method to accomplish it still reserves it to your library, and then proceeds as normal.  I can tell you, if it's a "trial and error concern", the title will not be removed from your library, after you run the script, whether it succeeds or not.  So, if it fails, you can retry, or try the  -KeepDecryptedData option.  I received no documentation for it, which is why I'm telling you as much as I know about using it.

[ see HooplaDownloader.newer.ps1 ]
