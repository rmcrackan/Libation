# Audiobookshelf Auto-Upload

Libation can optionally upload each book to your [Audiobookshelf](https://www.audiobookshelf.org/) server right after liberation. Local files are never deleted - upload only copies them to Audiobookshelf.

Available in Classic (WinForms), Chardonnay (Avalonia), and the CLI `liberate` command.

## Setup

1. Open **Settings** -> **Audiobookshelf**.
2. Enable **Automatically upload downloaded books**.
3. Enter your Audiobookshelf **Server URL** and **API Token** (see below).
4. Click **Connect / Refresh**. Libation loads your book libraries and folders.
5. Choose the target **Library** and **Folder**, then save settings.

Only libraries with media type `book` are listed (podcast libraries are excluded).

### Server URL

Use the **base address** of your Audiobookshelf server - the same host (and optional subpath) you use to open the web UI, **without** a library, book, or other page path.

Examples:

| Setup | Server URL |
| --- | --- |
| Local default install | `http://localhost:13378` |
| Hosted / reverse proxy at root | `https://abs.example.com` |
| Served under a subpath | `https://example.com/audiobookshelf` |

Do **not** paste a browser address for a specific library or book, such as:

- `http://localhost:13378/library/<id>`
- `https://example.com/audiobookshelf/library/<id>/bookshelf`

Libation calls the Audiobookshelf API at `{Server URL}/api/...`. If the Server URL already includes `/library/...`, that request path will be wrong.

Libation will try to strip common browser paths (and a trailing `/api`) automatically when you connect or save. If connection still fails with 404, double-check that the URL is only the server base (plus subpath, if any).

### API token storage

The API token is stored in Libation's settings. When Libation's token storage is set to encrypted (and the OS secret store is available), the Audiobookshelf token is encrypted the same way as Audible auth tokens. If encryption is not in effect, Settings shows a plaintext warning.

Create an API token in Audiobookshelf under your user account settings (see Audiobookshelf's docs for the current location).

### Connection troubleshooting

- **404 when fetching libraries** - Server URL is usually wrong (page URL instead of base, or missing/extra subpath). See [Server URL](#server-url).
- **401 / 403** - API token is missing, incorrect, or expired. Create a new token in Audiobookshelf and paste it again.
- **Timeout / could not reach server** - Confirm Audiobookshelf is running, the host and port are correct, and (if Libation runs on another machine) that the server is reachable on the network. `localhost` only works when Audiobookshelf is on the same computer as Libation.

## How it works

When auto-upload is enabled and configured:

1. Libation liberates the book as usual (download and decrypt, then PDF if any).
2. Libation uploads the liberated audio file(s) to the selected Audiobookshelf library and folder.
3. If Libation saved cover art for that book, the cover image is included in the upload.
4. Title, author, and series from Libation's library data are sent as upload metadata.

Upload runs for GUI download/decrypt and for CLI `liberate`. It does **not** run on the separate **Convert to MP3** queue or `libationcli convert` - those paths only convert local files.

Auto-upload only ever fires at the moment a book is liberated. To send books you liberated earlier, see [Uploading books you already liberated](#uploading-books-you-already-liberated).

## Uploading books you already liberated

Books liberated before Audiobookshelf was set up - or while it was turned off - are never sent by auto-upload. The `upload` command backfills them from the copies already on disk. Nothing is re-downloaded from Audible, and local files are never deleted.

Upload every liberated book:

```console
libationcli upload
```

Upload specific titles:

```console
libationcli upload B017V4IM1G
libationcli upload --id B017V4IM1G
```

The command:

- Considers only books Libation has marked as **liberated**. Books whose liberation errored are skipped, because an errored liberation may have left partial files.
- Finds audio using both Libation's file-path cache **and** a scan of your Books directory, so books whose cache entry was lost are still found.
- Checks the server before each upload and skips titles already present - see [Duplicate handling](#duplicate-handling).
- Prints a summary at the end: uploaded, already on server, no files found, failed, skipped. `skipped` counts books you named by ASIN that were not eligible - most often because they are not marked as liberated.

Libation does not record which books it has already uploaded, so every run re-checks each candidate against the server. That costs a couple of search requests per book, which is noticeable on a large library but makes repeat runs safe to retry.

## Duplicate handling

Before uploading, Libation searches the target Audiobookshelf library for an existing item with a matching title (and author when available). If a match is found, or Audiobookshelf reports that the destination already exists, Libation skips the upload and continues.

## Failures do not fail liberation

If the Audiobookshelf server is unreachable, the token is wrong, or upload fails for another reason:

- The book remains liberated on disk.
- The queue or CLI does not treat the book as a failed/bad liberate.
- Status text and the log still report the upload error.

This applies to `libationcli upload` too: a failed upload never marks a book as a bad liberate. Because uploading is that command's only job, it also reports each failure on stderr and counts it in the end-of-run summary. It still exits 0, matching Libation's other commands - read the summary and stderr together to see whether a backfill actually succeeded.

## Tips

- Point the target folder at the Audiobookshelf library folder you want new titles to land in.
- Prefer enabling **Save cover art** in Libation's audio/download settings if you want covers included in the upload.
- For listening apps and other self-hosted options, see [Now That I've Downloaded My Books, How Can I Listen to Them?](../frequently-asked-questions.md#now-that-ive-downloaded-my-books-how-can-i-listen-to-them).
