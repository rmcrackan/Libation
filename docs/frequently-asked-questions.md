# Frequently Asked Questions

## Where Can I Get Help For My Specific Problem?

You can open an issue here for bug reports, feature requests, or specialized help. For bug reports, attach your log file (see [Where Can I Find the Log File?](#where-can-i-find-the-log-file) below). Reports with limited information or no log may get limited or delayed help.

## Where Can I Find the Log File?

Libation keeps its log files in your Libation files folder. When reporting a bug, attach the Libation log file from that folder. If the folder contains **LibationCrash.log**, attach that too.

**Default log folder locations**

| Platform | Folder |
| - | - |
| Windows | `%userprofile%\Libation` |
| macOS | `~/Library/Application Support/Libation` |
| Linux | `~/.local/share/Libation` |

**macOS:** If that folder does not exist because Libation never starts, try launching from Terminal and note whether it works: `/Applications/Libation.app/Contents/MacOS/Libation` (see [Troubleshooting for macOS](/docs/advanced/troubleshoot#app-wont-start)).

Alternatively, open the log folder from within Libation: open **Settings**, and on the first tab click **Open log folder**.

## What's the Difference Between 'Classic' and 'Chardonnay'?

First and most importantly: Classic and Chardonnay have the exact same features.

- **Classic** is Windows only. Its older 'grey boxes' look has a compact design which allows for more information on the screen. Notably, Classic was written using an older, more mature technology which has built-in support for screenreaders.

- **Chardonnay** is available for Windows, Mac, and Linux. Its modern design has a more open look and feel.

## Now That I've Downloaded My Books, How Can I Listen to Them?

You can use any app which plays m4b files (or mp3 files if you used that setting). Here are just a few ideas. Disclaimer: I have no affiliation with any of these companies:

- iOS: [BookPlayer](https://apps.apple.com/us/app/bookplayer/id1138219998)
- iOS: [Bound](https://apps.apple.com/us/app/bound-audiobook-player/id1041727137)
- Android: [Smart AudioBook Player](https://play.google.com/store/apps/details?id=ak.alizandro.smartaudiobookplayer&hl=en_US&gl=US)
- Android: [Listen](https://play.google.com/store/apps/details?id=ru.litres.android.audio&hl=en_US&gl=US)
- Desktop: [VLC](https://www.videolan.org/)
- Windows Desktop: [Audibly](https://github.com/rstewa/Audibly) -- a desktop player build specifically for audiobooks

Self-hosting online:

- [audiobookshelf](https://www.audiobookshelf.org). On [reddit](https://www.reddit.com/r/audiobookshelf/)
- [plex](https://www.plex.tv/). Listen with [Prologue](https://prologue.audio/) (iOS)

## I'm Having Trouble Playing My Non-Spatial Audiobook, How Can I Fix This?

If you enabled the [Request xHE-AAC Codec](./features/audio-file-formats.md#request-xhe-aac-codec) option in settings, then the audiobook is being downloaded in the [xHE-AAC codec](./features/audio-file-formats.md#xhe-aac) which isn't widely supported. You have two options:

1. Use a media player which supports the xHE-AAC codec. [See an incomplete list of media players which support xHE-AAC](./features/audio-file-formats.md#supported-media-players).
2. Disable the [Request xHE-AAC Codec](./features/audio-file-formats.md#request-xhe-aac-codec) option in settings and re-download the audiobook. This will cause Libation to download audiobooks in the [AAC-LC codec](./features/audio-file-formats.md#aac-lc), which enjoys near-universal media player support.

## I'm Having Trouble Logging Into My Brazil Account

For reasons known only to Jeff Bezos and God, amazon and audible brazil handle logins slightly differently. The external browser login option is not possible for Brazil. [See this ticket for more details.](https://github.com/rmcrackan/Libation/issues/1103)

## Snap refreshed and Libation crashes on the database - what should I check?

Snap keeps per-version folders under `~/snap/libation/<revision>/`. After an update, `appsettings.json` in the **new** folder may still list `"LibationFiles"` with an **old** revision path. Libation then opens the wrong directory and you can see errors about SQLite or migrations even when permissions look fine.

Open `appsettings.json` next to your running install (often under `~/snap/libation/current/...`), set `LibationFiles` to the Libation data path **inside that same revision**, save, and launch again. Details: [Linux install - Snap](/docs/installation/linux#snap), [Troubleshooting - Snap](/docs/advanced/troubleshoot#linux-snap-and-sqlite-write-failures), and [issue #1776](https://github.com/rmcrackan/Libation/issues/1776).

## How Do I Use Libation With a South Africa Account?

Like many countries, amazon gives South Africa it's own amazon site. [Unlike many other regions](https://www.audible.com/ep/country-selector) there is not South Africa specific audible site. Use `US` for your region -- ie: audible.com.

> [!NOTE]
> (Not exactly a _frequently_ asked question but it's come up more than once)

## Why Was "Request Spatial Audio" Removed? Can I Still Download Dolby Atmos?

Libation removed the **Request Spatial Audio** option in version 13.1.3 (January 2026). Most Dolby Atmos / spatial titles can no longer be downloaded. For many titles, **Use Widevine DRM** + **Request xHE-AAC** still gets the highest **stereo** quality available -- not the Atmos mix.

See **[Spatial Audio, Dolby Atmos, and Widevine DRM](./features/spatial-audio.md)** for what changed on Audible's side, what still works, and what to do instead.

## Does "Use Widevine DRM" Still Download Spatial Audio?

No. **Use Widevine DRM** unlocks **Request xHE-AAC Codec** for higher-bitrate stereo. It does not re-enable spatial/Atmos download. See **[Spatial Audio, Dolby Atmos, and Widevine DRM](./features/spatial-audio.md#does-use-widevine-drm-download-spatial-audio)**.

## I'm Having Trouble Playing My Book with 4D, Spatial Audio, or Dolby Atmos, How Can I Fix This?

Libation no longer requests spatial/Atmos from Audible (see [Spatial Audio, Dolby Atmos, and Widevine DRM](./features/spatial-audio.md)). Books you liberated **before** 13.1.3 may be E-AC-3 or AC-4; newer downloads are stereo unless you kept older files.

For spatial files you already have, see **[Playing spatial files you already have](./features/spatial-audio.md#playing-spatial-files-you-already-have)** and [Supported Media Players](./features/audio-file-formats.md#supported-media-players).
