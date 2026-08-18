# Trash bin

Removing a book from Libation does not throw it away. The book moves to a trash bin, where it stays until you
restore it or delete it for good.

Removing a book never touches your audio files. Everything on this page is about Libation's own record of a
book, not the files it has downloaded.

## Removing books

Three ways, depending on how many books you mean:

- **Right-click one or more rows > Remove from library.** The usual way to remove a specific book.
- **Visible Books > Remove from library...** Removes everything the current filter is showing, so you can
  [search](/docs/features/searching-and-filtering) for what you want gone and remove the result in one go.
- **Import > Remove Library Books.** Scans your account first and pre-checks the books it did not find, which
  is how you clear out titles you no longer own. Nothing is removed until you click **Remove N Books from
  Libation** and confirm, and the confirmation lists every title.

A removed book disappears from the grid, from search results, and from the counts in the status bar. That is
the whole point, but it also means nothing about the main window will remind you the book exists.

## Finding what you removed

The status bar shows how many books are in the trash, and clicking it opens the trash bin:

```
Visible: 2,488          29 in trash          All 2,488 books backed up
```

The count is also on the **Settings > Trash Bin** menu item. Neither appears when the trash is empty.

If you search for a book that turns out to be in the trash, Libation says so rather than leaving you with an
empty grid:

> No books match "Epicenter".
> 1 matching book is in the trash. **[Open Trash Bin]**

## Restoring a book

Open **Settings > Trash Bin**, tick the books you want, and click **Restore**. They reappear in the grid
immediately, with their download status and tags intact.

The trash bin has its own search box, which is worth knowing if you have removed a lot over the years. The
**Everything** and **Audible Plus Books** checkboxes at the bottom tick whole groups at once.

Podcast episodes are listed under their series, even when the series itself is not in the trash.

## Removing a book for good

**Permanently Delete from Libation**, in the same dialog, deletes Libation's record of the book instead of
keeping it in the trash.

This is worth understanding before you use it, because it does *less* to keep a book away than the trash does:

| | Book in the grid | Comes back on the next scan | Your audio files |
|-|-|-|-|
| **Removed** (in the trash) | No | No | Untouched |
| **Permanently deleted** | No, until the next scan | Yes, if it is still in your Audible account | Untouched |

Libation recognizes a removed book on the next scan and leaves it in the trash. A permanently deleted book is
one it has never seen before, so it is imported again like any other new title.

So if you want a book gone from Libation and to stay gone, leave it in the trash. Permanently deleting is for
reclaiming space in Libation's database, or for starting a title over from scratch.

## Not the same as removing from Audible

The context menu also offers **Remove Plus Books from Audible Library** for Audible Plus titles. That one
reaches into your actual Audible account and returns the title, which is why it asks for confirmation and
warns that the only way back is through the Audible website or app. It is not undoable from Libation, and the
book is not put in the trash afterwards - it is gone from your Audible library, so there is nothing for a scan
to find.

## In the log

Libation records the size of the trash at startup, and every change to it afterwards:

```
Initial database statistics. {"LibraryBooksNotInTrash":2508,"LibraryBooksInTrash":29,"BookRecords":2537}
Trash bin changed. {"Action":"Moved to trash","Books":1,"BooksInTrash":30}
Trash bin changed. {"Action":"Restored from trash","Books":1,"BooksInTrash":29}
```

Useful if a book has gone missing and you want to know whether, and when, it was removed.
