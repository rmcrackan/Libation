# Parallel downloads

Libation can download and decrypt several audiobooks at the same time instead of working through the queue one title at a time. It downloads **3 at once by default**, and you can change that to anything from 1 to 10.

Setting it to **1 downloads one book at a time**, which is how Libation behaved before this existed. There is no separate on/off switch — the number *is* the switch.

## Changing it

The control sits in the **Process Queue** panel, labelled **At once**, next to Auto-scroll and the download speed limit. It takes effect immediately: raise it during a run and Libation starts more books as soon as slots free up, lower it and the extra books finish before the queue narrows. Nothing needs to be requeued.

The setting is remembered between sessions.

## Why the default is 3 and not higher

Audible throttles license requests. Downloading too many titles at once leads it to refuse content licenses, which arrives in Libation as ordinary download failures with a "license denied" message — not as anything that identifies the cause.

**Above 3 concurrent downloads, license denials start appearing.** The exact threshold is Audible's and is not published, so 3 is the conservative choice rather than a measured maximum. The ceiling of 10 exists to stop the setting from being turned into a reliable way to get your downloads refused.

If you are seeing license denials, lowering this number is the first thing to try. See [Retrying titles Audible refuses](/docs/features/retrying-refused-downloads) for what Libation does with a title once it has been refused.

## Your machine may run fewer than you asked for

Downloading is limited by Audible, but decrypting is limited by your processor. On a machine with fewer processors than the number you chose, Libation runs fewer books at once — your setting is kept as you left it, so moving the same configuration to a larger machine picks up where you meant it to.

When the two differ, Libation says so next to the setting: Chardonnay shows a note reading **(2 at a time)**, and Classic puts the same thing in the control's tooltip.

## Auto-scroll

The **Auto-scroll** checkbox keeps newly started downloads in view as the queue advances. It only scrolls when the book above the new one is already on screen, so scrolling away to read something elsewhere in the queue is not interrupted by the next book starting. Unchecking it stops the queue scrolling on its own entirely.

## Stopping a run

**Cancel All** cancels every book currently downloading, not just the first one, and clears whatever is still queued.

**Abort**, in the dialog Libation shows when a book fails, stops the whole run. With several books in flight this means the books already running stop rather than each asking you the same question in turn. The other two answers are unchanged: Ignore and Retry apply to the book being asked about, and "Apply to all remaining books" extends your choice to the rest of the queue.

Libation shows one failure dialog at a time no matter how many books fail together.

## With a daily download limit

The two work together, with one thing worth knowing: when the [daily download limit](/docs/features/daily-download-limit) pauses the queue, **books already downloading carry on to completion**. The limit stops new books from starting; it does not interrupt work in progress. So a queue that pauses with 3 books in flight finishes those 3 and then waits.

Under "Plus titles only", the queue still moves Plus titles to the back and keeps downloading owned titles in the meantime, filling all available slots as it goes.

## Download speed limit

The speed limit is shared across everything downloading, not applied per book. Changing it mid-run updates every book currently in flight.

## Command line

This is a setting on the app's process queue. `libationcli liberate` does not use it, and its behaviour is unchanged.
