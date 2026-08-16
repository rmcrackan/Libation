# Daily download limit

Libation can pace itself, downloading only so much in any 24 hour period. The limit is **off by default**: nothing changes until you turn it on.

Two different reasons to use it:

- **Audible throttling.** Downloading a lot of Audible Plus titles in a short time can lead Audible to deny content licenses for a day or two ("license denied"). That limit is Audible's, not Libation's, and Audible does not publish a number, but community reports put it in the dozens of titles. Capping your Plus downloads avoids provoking it.
- **Disk space.** A large library can fill a drive. A limit in MB or GB stops a long run before your disk does.

## Turning it on

Settings > Download/Decrypt > **Daily download limit**.

| Option | Effect |
|--------|--------|
| No limit | The default. Libation downloads as fast as it can. |
| Plus titles only | Limits Audible Plus titles (the ones with an orange plus badge). Titles you own are unaffected. |
| All books | Limits everything Libation downloads. |

Choosing either limit shows a quantity and a unit: **books**, **MB** or **GB**. 50 books per day is a reasonable starting point for Plus titles.

## Rolling 24 hours, not a calendar day

The window always looks back 24 hours from right now. Capacity comes back 24 hours after each individual download, not at midnight. Downloading 50 titles at 11pm does not let you download 50 more at 1am; you get one back at 11pm the following night, and so on.

## What counts

Only downloads Libation itself completed successfully. That means:

- Failed and cancelled downloads do not count.
- PDF-only downloads and mp3 conversions never count, and are never blocked.
- Books you download in the Audible app or on Audible's website are invisible to Libation and cannot be counted.
- Re-downloading a title counts again, because it is another download.

Libation records your downloads whether or not a limit is set, so if you turn the limit on after a heavy session, it already knows what you have done today.

## About MB and GB

Libation does not know how large an audiobook is until it has downloaded it, so a size limit is necessarily approximate. When deciding whether there is room for another book, Libation assumes about 400 MB for it — the same estimate it uses to warn about low disk space. Sizes already recorded are exact; only the next book is a guess.

Because of that estimate, one download is always allowed when your window is empty. Otherwise a limit smaller than one book would stop downloading entirely and report "limit reached" to someone who had downloaded nothing.

## What happens when the limit is reached

The limit is checked immediately before each book downloads, not when you queue them, so your queue keeps its contents and you can change your mind at any time.

**In the app:** the queue pauses instead of emptying. You get one notice, the waiting title shows when it expects to resume, and Libation continues on its own once capacity returns — including days later, if you leave it running with a long queue. Raise or turn off the limit in Settings and the queue picks that up within a few seconds, with nothing to requeue. Cancel All stops it instead.

Under "Plus titles only" a queue holding both kinds keeps going: Plus titles move to the back and your owned titles download in the meantime.

**On the command line:** `libationcli liberate` does not wait, since a scripted or scheduled run should not sit idle for hours. It skips the titles the limit covers, says so, and reports how many it skipped. Those titles remain un-liberated and are picked up by the next run.

## A limit for one run

The command line has a second, separate limit: `libationcli liberate --limit-books 10` (or `--limit-mb` / `--limit-gb`) stops that one run after downloading that much, whatever your daily limit says. It is useful for taking a large library a slice at a time, and both limits apply if both are set. See [Limit how much one run downloads](/docs/advanced/command-line-interface#limit-how-much-one-run-downloads).

There is no equivalent in the app, where selecting rows in the grid and choosing **Download selected audiobooks** already says exactly which titles to download.

## Docker and the command line

Containers have no settings dialog, so add the keys to the `Settings.json` you mount at `/config`:

```json
{
  "DailyDownloadLimit": "PlusOnly",
  "DailyDownloadLimitQuantity": 50,
  "DailyDownloadLimitUnit": "Books"
}
```

`DailyDownloadLimit` accepts `NoLimit`, `PlusOnly` or `AllBooks`, and `DailyDownloadLimitUnit` accepts `Books`, `MB` or `GB`. Download counts are kept in Libation's database, so they survive container restarts as long as your database is on a mounted volume, as it is in the standard setup.

A container that liberates on a schedule combines well with a limit: each run downloads what it may and stops, and the next run continues where it left off.

## When a license is denied anyway

If Audible refuses a license despite the limit, Libation waits before asking about that title again instead of re-requesting it on every run. See [Retrying titles Audible refuses](/docs/features/retrying-refused-downloads).
