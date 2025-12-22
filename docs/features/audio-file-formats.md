# Audio Formats Produced by Libation

Libation will download audio in a number of different audio formats, depending on the settings you choose within Libation and the per-title availability of audio formats from Audible. The Libation settings which affect the format downloaded by Libation are shown in the Settings menu screenshot below.

> [!NOTE]
>
> - Audiobook file extensions are either `.m4b` or `.mp3`. Libation uses the `.m4b` file extension for all non-MP3 files, regardless of the audio codec contained therein. Some media players don't recognize the `.m4b` file extension and may require the extension be changed to `.m4a` or `.mp4`.
> - Most (but not all) podcasts are delivered by Audible as native MP3 files. None of the following audio formats and settings discussions pertain to those podcasts because MP3s have no DRM, and those episodes are copied directly to their output folders.

![Audio format settings menu](../images/AudioFormatSettings.png)

## Settings Summary

### Audio Quality to Request From Audible

Audiobooks can be requested from Audible as "Normal" quality or "High" quality, matching the settings in the Audible mobile apps. This setting affects the audio bitrate and, sometimes, the number of audio channels. This setting has no effect on the _audio codec_.

### Use Widevine DRM

When this setting is disabled, all audiobooks will be downloaded using Audible's in-house DRM (AAX(C)) in the [AAC-LC](#aac-lc) format.
When this setting is enabled, Libation will request audio files protected by Google's Widevine Digital Rights Managements scheme, and two additional settings will be unlocked: [Request xHE-AAC Codec](#request-xhe-aac-codec) and [Request Spatial Audio](#request-spatial-audio) (explained further below).

If you don't enable either of those additional options, then enabling 'Use Widevine DRM' will have no pratcical effect in nearly all circumstances. Audiobooks will be downloaded in the same [AAC-LC](#aac-lc) format with the same bitrate and the same number of audio channels. On rare occasions, enabling 'Use Widevine DRM' without the other two options will result in audio files with a different bitrate.

### Request xHE-AAC Codec

Enable this setting to request audiobooks in the [xHE-AAC](#xhe-aac) format. This codec is generally better quality than the [AAC-LC](#aac-lc) codec at the same bitrate, but it isn't as commonly supported by media players, so you may have some difficulty playing these audiobooks. The highest bitrate version of some audiobooks is only available as [xHE-AAC](#xhe-aac).

### Request Spatial Audio

Enable this setting to request audiobooks in a "spatial" ([Dolby Atmos](#dolby-atmos)) audio format. If an audiobook is not available in a spatial format, it will instead be downloaded in the [xHE-AAC codec](#xhe-aac).

### Spatial Audio Codec

Choose whether spatial audiobooks are downloaded in the [E-AC-3](#e-ac-3) or [AC-4](#ac-4) format.

### Download My Books in the Original Audio Format (Lossless)

If selected, Audiobooks will be downloaded and saved in the format delivered by audible (which depends on the settings explained above). Libation will not change the audio.

### Download My Books as .MP3 Files (Transcode if Necessary)

If selected, Libation will decode [AAC-LC](#aac-lc), [xHE-AAC](#xhe-aac), and [E-AC-3](#e-ac-3) audiobooks and re-encode them as MP3s using the MP3 encoder settings ([read about LAME MP3 encoder settings](https://lame.sourceforge.io/lame_ui_example.php)). Note that Libation cannot convert [AC-4](#ac-4) audio to MP3.

## Audio Formats

### Traditional Mono and Stereo Formats

#### AAC-LC

**Full Name:** Advanced Audio Coding - Low Complexity

**Description:** This is the base profile for AAC audio and has existed since AAC's initial release in 1997. It enjoys wide support on nearly every conceivable platform capable of playing digital audio, as ubiquitous as MP3.
If Widevine support is not enabled, or if the book is not available in the more high-definition formats, Libation will download audiobooks in this format.

#### MP3

**Full Name:** MPEG-1 Audio Layer III or MPEG-2 Audio Layer III

**Description:**

An older (released in 1991) but still nearly universally supported audio codec. Its audio quality is generally worse than AAC-LC at similar bitrates. Audible delivers some podcasts in MP3 format, but no audiobooks are natively availble as MP3. Libation supports converting Audiobooks delivered in other audio formats to MP3. Note that the MP3 format supports a maximum of two audio channels, so multichannel E-AC-3 audio will be downsampled to stereo or mono (depending on the Libation's settings). [AC-4](#ac-4) cannot be converted to MP3.

#### xHE-AAC

**Full Name:** Extended High-Efficiency Advanced Audio Coding

**Description:**

This is a proprietary codec created by the [Fraunhofer Institute for Integrated Circuits IIS](https://www.iis.fraunhofer.de/en/ff/amm/broadcast-streaming/xheaac.html). It combines features of the HE-AAC v2 and the baseline USAC (Unified Speech and Audio Coding) profiles with the parts of the MPEG-D DRC Loudness Control Profile or Dynamic Range Control Profile. Therefore, USAC and xHE-AAC are not synonymous and should not be used interchangeably. A player capable of decoding USAC will not necessarily be able to decode xHE-AAC.

xHE-AAC boasts significantly higher quality audio at low bitrates. Though it has existed since at least 2016, playback support is still quite limited. FFmpeg has recently added partial decoder support for the USAC profiles, but it is insufficient to decode the xHE-AAC audio files acquired from Audible (due to FFmpeg's lack of support for MPEG Surround for Mono to Stereo Upmixing; ISO 23003-3:2012 §7.11)

Note that the xHE-AAC files authored by Audible have some USAC conformance errors including:

- Number of samples per frame not matching the UsacConfig coreCoderFrameLength value.
- Disagreement between stts and UsacFrame usacIndependencyFlag value.
- Stts indicating a frame is an immediate play-out frame, but USAC AudioPreRoll is absent.

### Dolby Atmos

Atmos is a surround sound technology that expands on existing surround sound systems by adding height channels as well as free-moving sound objects. Audible delivers Dolby Atmos in two formats: E-AC-3 and AC-4.

Your device's ability to play audio from these formats does not necessarily mean that the audio you are hearing is Atmos (spatial). For instance, downloading the AC-4 codec for Windows ([links in the [Supported media Players](#supported-media-players) section) will enable you to play AC-4 audiobooks, but you'll still need to download [Dolby Access](https://apps.microsoft.com/detail/9n0866fs04w8?hl=en-US&gl=US) and pay $15 to enable _Dolby Atmos For Headphones_. Please refer to [this comment](https://github.com/rmcrackan/Libation/pull/1331#discussion_r2268660524) for additional context.

#### E-AC-3

**Full Name:** Dolby Digital Plus (a.k.a Enhanced AC-3, DDP, DD+, and EC-3)

**Description:**

A proprietary digital audio compression scheme developed by Dolby Digital for the transport and storage of multichannel audio. This format can be extended to add support for Atmos, making the codec _Dolby Digital Plus Atmos_. _Dolby Digital Plus Atmos_ is backwards compatible with Dolby Digital Plus, so any media player capable of playing Dolby Digital Plus can play _Dolby Digital Plus Atmos_. Audible spatial audiobooks downloaded in the E-AC-3 format are _Dolby Digital Plus Atmos_. If they are played by a media player that supports Atmos, they will play as Atmos audio. If they are played by a media player that does not support Atmos, they will be played as traditional 5.1 surround audio.

#### AC-4

**Full Name:** Dolby AC-4

**Description:**

A proprietary audio compression technology developed by Dolby Digital for the transport and storage of audio channels and/or audio objects. Audible spatial audiobooks downloaded in the AC-4 format are 2-channel AC-4 Immersive Stereo (AC4-IMS) audio, intended for playback in headphones or earbuds (though apparently [not supported on Apple devices](https://github.com/rmcrackan/Libation/issues/996#issuecomment-3169574514)).

## Supported Media Players

Below is an incomplete matrix of codec support across various media players and platforms.
| Player | [AAC-LC](#aac-lc) | [xHE-AAC](#xhe-aac) | [E-AC-3](#e-ac-3) | [AC-4](#ac-4) |
| :--- | :---: | :---: | :---: | :---: |
|Windows Native Support|Yes|Yes<sup>1</sup>|Yes<sup>2,3</sup>|Yes<sup>4</sup>|
|macOS Native Support|Yes|Yes|Yes<sup>3</sup>| |
|Android Native Support<sup>5</sup>|Yes|Yes| | |
|FFmpeg (all platforms)|Yes|Yes<sup>6</sup>|Yes<sup>3</sup>||
|[VLC](https://www.videolan.org/vlc/) (Windows)|Yes| |Yes<sup>3</sup> | |
|[foobar2000](https://www.foobar2000.org/components) (Windows and Mac)|Yes|Yes<sup>7</sup> | | |
|[PotPlayer](https://potplayer.daum.net/) (Windows)|Yes|Yes|Yes<sup>3</sup>| |
|[Samsung Media Player](https://play.google.com/store/apps/details?id=com.sec.android.app.music)<sup>8</sup> (Samsung devices) |Yes|Yes|Yes|Yes|

1. Windows 11 22H2 and later
2. On Windows [prior to Windows 11, version 24H2](https://support.microsoft.com/en-us/windows/codecs-in-media-player-d5c2cdcd-83a2-4805-abb0-c6888138e456). You can still get the codec by running the following command from a Windows PowerShell console: `winget install --id 9nvjqjbdkn97`
3. As mentioned in the [Dolby Atmos](#dolby-atmos) section, just because a media player can play a file does not mean it's rendering Atmos. _Dolby Digital Plus Atmos_ is backwards compatible with _Dolby Digital Plus_, so media players which only support _Dolby Digital Plus_ will play E-AC-3 audio files as regular 5.1 surround without rendering the Atmos spatial qualities. Additional software or hardware support may be required for Dolby Atmos playback.
4. You can download the AC-4 codec for Windows from 3rd party sites like [Major Geeks](https://www.majorgeeks.com/files/details/dolby_ac_3ac_4_installer.html) and [Free-Codecs](https://www.free-codecs.com/dolby-ac-4-decoder_download.htm). Once you install the codec bundle from one of those sources, the Windows store app will keep it updated. Read more about the process [in this comment](https://github.com/rmcrackan/Libation/pull/1331#discussion_r2268660524).
5. All Android devices will support AAC-LC and xHE-AAC. Some manufactures (such as Samsung) will include Dolby codecs for playing E-AC-3 and AC-4 audio.
6. requires FFmpeg to be [built with fdk-aac](https://trac.ffmpeg.org/wiki/Encode/AAC#fdk_aac). You will almost certainly not find pre-build binaries in the wild due to licensing restrictions.
7. Requires the [fdk-aac plugin](https://www.foobar2000.org/components/view/foo_pd_aac) (Windows only)
8. Requires audio file extensions to be `.m4a` or `.mp4`. Libation sets the file extensions to `.m4b`, so you must manually change it to `.m4a` by renaming the audio file.
