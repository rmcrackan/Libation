# Frequently Asked Questions

## Where Can I Get Help For My Specific Problem?

You can open an issue here for bug reports, feature requests, or specialized help.

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

## I'm Having Trouble Playing My Book with 4D, Spatial Audio, or Dolby Atmos, How Can I Fix This?

Spatial audiobooks are delivered in two formats: [E-AC-3](./features/audio-file-formats.md#e-ac-3) and [AC-4](./features/audio-file-formats.md#ac-4). [See an incomplete list of media players which support those codecs](./features/audio-file-formats.md#supported-media-players).

## I'm Having Trouble Logging Into My Brazil Account

For reasons known only to Jeff Bezos and God, amazon and audible brazil handle logins slightly differently. The external browser login option is not possible for Brazil. [See this ticket for more details.](https://github.com/rmcrackan/Libation/issues/1103)

## How Do I Use Libation With a South Africa Account?

Like many countries, amazon gives South Africa it's own amazon site. [Unlike many other regions](https://www.audible.com/ep/country-selector) there is not South Africa specific audible site. Use `US` for your region -- ie: audible.com.

> [!NOTE]
> (Not exactly a _frequently_ asked question but it's come up more than once)
