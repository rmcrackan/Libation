# Audiobookshelf Auto-Upload

Libation can optionally upload each book to your [Audiobookshelf](https://www.audiobookshelf.org/) server right after liberation. Local files are never deleted - upload only copies them to Audiobookshelf.

Available in Classic (WinForms), Chardonnay (Avalonia), and the CLI `liberate` command.

## Setup

1. Open **Settings** -> **Audiobookshelf**.
2. Enable **Automatically upload downloaded books**.
3. Enter your Audiobookshelf **Server URL** (for example `https://abs.example.com`) and **API Token**.
4. Click **Connect / Refresh**. Libation loads your book libraries and folders.
5. Choose the target **Library** and **Folder**, then save settings.

Only libraries with media type `book` are listed (podcast libraries are excluded).

### API token storage

The API token is stored in Libation's settings. When Libation's token storage is set to encrypted (and the OS secret store is available), the Audiobookshelf token is encrypted the same way as Audible auth tokens. If encryption is not in effect, Settings shows a plaintext warning.

Create an API token in Audiobookshelf under your user account settings (see Audiobookshelf's docs for the current location).

## How it works

When auto-upload is enabled and configured:

1. Libation liberates the book as usual (download and decrypt, then PDF if any).
2. Libation uploads the liberated audio file(s) to the selected Audiobookshelf library and folder.
3. If Libation saved cover art for that book, the cover image is included in the upload.
4. Title, author, and series from Libation's library data are sent as upload metadata.

Upload runs for GUI download/decrypt and for CLI `liberate`. It does **not** run on the separate **Convert to MP3** queue or `libationcli convert` - those paths only convert local files.

## Duplicate handling

Before uploading, Libation searches the target Audiobookshelf library for an existing item with a matching title (and author when available). If a match is found, or Audiobookshelf reports that the destination already exists, Libation skips the upload and continues.

## Failures do not fail liberation

If the Audiobookshelf server is unreachable, the token is wrong, or upload fails for another reason:

- The book remains liberated on disk.
- The queue or CLI does not treat the book as a failed/bad liberate.
- Status text and the log still report the upload error.

## Tips

- Point the target folder at the Audiobookshelf library folder you want new titles to land in.
- Prefer enabling **Save cover art** in Libation's audio/download settings if you want covers included in the upload.
- For listening apps and other self-hosted options, see [Now That I've Downloaded My Books, How Can I Listen to Them?](../frequently-asked-questions.md#now-that-ive-downloaded-my-books-how-can-i-listen-to-them).
