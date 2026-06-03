# Spatial Audio, Dolby Atmos, and Widevine DRM


> [!IMPORTANT]
> tl;dr: Libation **cannot** download Dolby Atmos or spatial audio from Audible and it will **never** be able to do so in the future.

## Short answer

Cryptography is always a game of cat and mouse, so it's *possible* someone will release a hack tomorrow. However this is extremely unlikely. The cryptography discussed below is hardware-based DRM which has been used for years by huge companies including Netflix, Amazon Prime, and Disney+. A small army of hackers has been going at this since 2014 -- [no general reproducible hack has been shown](https://www.forasoft.com/learn/video-streaming/articles-streaming/widevine-l1-l2-l3). I'd love to be proven wrong but I'm not holding my breath.

**What is still possible with Libation**

- Download and decrypt most titles in **stereo** (AAC-LC, or xHE-AAC with Widevine enabled). (The Libation settings **Use Widevine DRM** or **Request xHE-AAC Codec** only affect **stereo** formats (AAC-LC or xHE-AAC). They do not restore the Atmos mix.)
- Play spatial files you **already downloaded before January 2026**, if your media player supports E-AC-3 or AC-4.

**What is not possible with Libation**

- Download new Dolby Atmos / spatial (E-AC-3 or AC-4) content from Audible.
- Get the Atmos mix by toggling Widevine or xHE-AAC settings.

For true spatial playback of current catalog titles, use the **Audible app** on a supported device. See [Dolby Atmos and spatial playback](#dolby-atmos-and-spatial-playback) at the bottom of this page, and [What you can do today](#what-you-can-do-today) for stereo download steps.

## What changed

### Libation 13.1.3 removed spatial download

In [Libation 13.1.3](https://github.com/rmcrackan/Libation/releases/tag/v13.1.3), the **Request Spatial Audio** and **Spatial Audio Codec** settings were removed from the UI. Internally, spatial requests are disabled. See [GitHub issue #1553](https://github.com/rmcrackan/Libation/issues/1553).

### Audible began requiring Widevine L1 for spatial titles

For a time, Audible delivered spatial (Dolby Atmos) audiobooks using **Widevine L3** -- software-based DRM. Libation could work with L3 thanks to community research, notably [MSWMan's Widevine CDM work](https://github.com/rmcrackan/Libation/issues/1553).

In early January 2026, Audible began requiring **Widevine L1** for many spatial titles. L1 ties decryption to hardware in a trusted execution environment -- the same class of protection used by Netflix, Disney+, and Prime Video. Libation runs on desktop with a software (L3) CDM and cannot satisfy L1 license requests.

When Libation (or any L3 client) requests spatial audio today, Audible typically responds with errors such as:

- `DRM license response not "OK"`
- `"The device is not allowed to consume content using this codec"` / `"InsufficientSecurityLevel"`

Streaming license requests for spatial content fail the same way. The official Audible Android app can still download spatial titles because it uses the phone's hardware-backed keys.

### What still works

| Format | Widevine level | Libation today |
| :--- | :--- | :--- |
| AAC-LC (AAXC / ADRM) | N/A | Yes, default path |
| xHE-AAC stereo (Widevine) | L3 | Yes, with **Use Widevine DRM** + **Request xHE-AAC** |
| Dolby Atmos / spatial (E-AC-3, AC-4) | L1 (for many titles) | No |

xHE-AAC via Widevine L3 continues to work for many books. That gives you higher-quality **stereo**, not Atmos.

> [!NOTE]
> Audible may expand stricter Widevine requirements over time. Some Audible Plus titles now require Widevine where AAXC no longer works ([#1580](https://github.com/rmcrackan/Libation/issues/1580)). If ordinary (non-spatial) books start failing with license errors, report them on GitHub with logs so the project can track how far Audible is taking these changes.

## What you can do today

### 1. Back up books you care about

Download and decrypt titles while Libation still supports the format Audible delivers for that book. DRM policies can change per title without notice.

### 2. Get the best stereo quality Libation can offer

1. Open **Settings** and enable **Use Widevine DRM**.
2. Enable **Request xHE-AAC Codec**.
3. Re-add your account if Libation prompts you (Widevine requires an Android-style device registration).
4. Re-download the title.

See [Audio File Formats](../features/audio-file-formats.md) for codec details and [Supported Media Players](../features/audio-file-formats.md#supported-media-players) if you have trouble playing xHE-AAC.

### 3. If downloads fail even with spatial disabled

Check your log file ([FAQ: Where Can I Find the Log File?](/docs/frequently-asked-questions#where-can-i-find-the-log-file)) and search [open issues](https://github.com/rmcrackan/Libation/issues). Some failures are Audible-side (Plus throttling, temporary outages) rather than spatial-related. See also [Troubleshooting](./troubleshoot.md).

## Will spatial download come back?

The Libation maintainers are watching Audible's DRM changes and will adapt if a viable path appears. There is no widely shared, reliable public method to decrypt Widevine L1 content on arbitrary PCs, and the project is not optimistic about spatial downloads returning soon.

Discussion continues on [GitHub issue #1553](https://github.com/rmcrackan/Libation/issues/1553). Maintainer context is also in [this Reddit comment](https://old.reddit.com/r/audiobooks/comments/1qu1qvy/libation_users_how_to_you_listen_to_your/o37dyw9/) from the Libation author.

## Dolby Atmos and spatial playback

> [!IMPORTANT]
> The **Request Spatial Audio** setting was **removed in Libation 13.1.3** (January 2026). Libation cannot download new spatial/Atmos content from Audible today. The sections below cover **playback** of spatial files you may have downloaded before that change.

### Does "Use Widevine DRM" download spatial audio?

**No.** **Use Widevine DRM** unlocks **Request xHE-AAC Codec** and higher-bitrate stereo formats. It does **not** re-enable spatial/Atmos download.

For titles where Audible still serves a high-quality stereo Widevine stream, Widevine + xHE-AAC is the best quality Libation can obtain. That is not the same as the Dolby Atmos mix in the Audible app.

### Listen in Dolby Atmos / spatial today

- **Dolby Atmos / spatial download** is not available in current Libation versions.
- Use the **Audible app** on a phone, tablet, or speaker that supports spatial playback for the full Atmos mix.
- **True spatial playback** from files you own: only if you backed up spatial titles before January 2026 (see below).

### Playing spatial files you already have

If you liberated spatial titles before January 2026, your files may be **E-AC-3** or **AC-4**. Libation no longer downloads new spatial content, but existing files remain playable if your player supports those codecs.

Spatial audiobooks from Audible use:

- [E-AC-3 (Dolby Digital Plus Atmos)](../features/audio-file-formats.md#e-ac-3)
- [AC-4 (Dolby AC-4 Immersive Stereo)](../features/audio-file-formats.md#ac-4)

See [Supported Media Players](../features/audio-file-formats.md#supported-media-players) for an incomplete compatibility list.

**Playing a file is not the same as hearing Atmos.** Many players decode E-AC-3 as plain 5.1 surround without rendering spatial objects. Dolby Atmos rendering often requires extra software (for example [Dolby Access](https://apps.microsoft.com/detail/9n0866fs04w8) on Windows) or hardware support. See [Spatial Audio and Dolby Atmos](../features/audio-file-formats.md#spatial-audio-and-dolby-atmos) for more detail.

If you expected Atmos but your **new** download is stereo, that is expected with current Libation versions.

## Related documentation

- [Audio File Formats](../features/audio-file-formats.md) -- codecs, settings, and media players
- [FAQ: Non-spatial playback trouble](/docs/frequently-asked-questions#im-having-trouble-playing-my-non-spatial-audiobook-how-can-i-fix-this)
- [Troubleshooting](./troubleshoot.md)
