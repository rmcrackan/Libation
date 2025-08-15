## [Download Libation](https://github.com/rmcrackan/Libation/releases/latest)

### If you found this useful, tell a friend. If you found this REALLY useful, you can click here to [PayPal.me](https://paypal.me/mcrackan?locale.x=en_us)
...or just tell more friends. As long as I'm maintaining this software, it will remain **free** and **open source**.



# Frequently Asked Questions

## Q: Where can I get help for my specific problem?

**A:** [You can open an issue here](https://github.com/rmcrackan/Libation/issues) for bug reports, feature requests, or specialized help.

## Q: What's the difference between 'Classic' and 'Chardonnay'?

**A:** First and most importantly: Classic and Chardonnay have the exact same features.

* **Classic** is Windows only. Its older 'grey boxes' look has a compact design which allows for more information on the screen. Notably, Classic was written using an older, more mature technology which has built-in support for screenreaders.

* **Chardonnay** is available for Windows, Mac, and Linux. Its modern design has a more open look and feel.

## Q: Now that I've downloaded my books, how can I listen to them?

**A:** You can use any app which plays m4b files (or mp3 files if you used that setting). Here are just a few ideas. Disclaimer: I have no affiliation with any of these companies:

* iOS: [BookPlayer](https://apps.apple.com/us/app/bookplayer/id1138219998)
* iOS: [Bound](https://apps.apple.com/us/app/bound-audiobook-player/id1041727137)
* Android: [Smart AudioBook Player](https://play.google.com/store/apps/details?id=ak.alizandro.smartaudiobookplayer&hl=en_US&gl=US)
* Android: [Listen](https://play.google.com/store/apps/details?id=ru.litres.android.audio&hl=en_US&gl=US)
* Desktop: [VLC](https://www.videolan.org/)
* Windows Desktop: [Audibly](https://github.com/rstewa/Audibly) -- a desktop player build specifically for audiobooks

Self-hosting online:

* [audiobookshelf](https://www.audiobookshelf.org). On [reddit](https://www.reddit.com/r/audiobookshelf/)
* [plex](https://www.plex.tv/). Listen with [Prologue](https://prologue.audio/) (iOS)

## Q: I'm having trouble playing my non-spatial audiobook, how can I fix this?

**A:** If you enabled the [Request xHE-AAC Codec](AudioFileFormats.md#request-xhe-aac-codec) option in settings, then the audiobook is being downloaded in the [xHE-AAC codec](AudioFileFormats.md#xhe-aac) which isn't widely supported. You have two options:
1. Use a media player which supports the xHE-AAC codec. [See an incomplete list of media players which support xHE-AAC](AudioFileFormats.md#supported-media-players).
2. Disable the [Request xHE-AAC Codec](AudioFileFormats.md#request-xhe-aac-codec) option in settings and re-download the audiobook. This will cause Libation to download audiobooks in the [AAC-LC codec](AudioFileFormats.md#aac-lc), which enjoys near-universal media player support.

## Q: I'm having trouble playing my book with 4D, spatial audio, or Dolby Atmos, how can I fix this?

**A:** Spatial audiobooks are delivered in two formats: [E-AC-3](AudioFileFormats.md#e-ac-3) and [AC-4](AudioFileFormats.md#ac-4). [See an incomplete list of media players which support those codecs](AudioFileFormats.md#supported-media-players).

## Q: I'm having trouble loggin into my Brazil account.

**A:** For reasons known only to Jeff Bezos and God, amazon and audible brazil handle logins slightly differently. The external browser login option is not possible for Brazil. [See this ticket for more details.](https://github.com/rmcrackan/Libation/issues/1103)

## Q: How do I use Libation with a South Africa account?

**A:** Like many countries, amazon gives South Africa it's own amazon site. [Unlike many other regions](https://www.audible.com/ep/country-selector) there is not South Africa specific audible site. Use `US` for your region -- ie: audible.com.

(Not exactly a *frequently* asked question but it's come up more than once)
