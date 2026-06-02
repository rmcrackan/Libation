# Spatial Audio, Dolby Atmos, and Widevine DRM

This page explains why Libation **no longer downloads Dolby Atmos / spatial audiobooks**, what changed on Audible's side, and what you can still do with Libation today.

## Short answer

**Libation cannot download Dolby Atmos or spatial audio from Audible.** Not in 13.1.3+, and not by enabling **Use Widevine DRM** or **Request xHE-AAC Codec**. Those settings only affect **stereo** formats (AAC-LC or xHE-AAC). They do not restore the Atmos mix.

The **Request Spatial Audio** option was removed in [Libation 13.1.3](https://github.com/rmcrackan/Libation/releases/tag/v13.1.3). Audible now requires hardware-backed Widevine (L1) for many spatial titles, which Libation cannot satisfy on a desktop PC.

**What is still possible with Libation**

- Download and decrypt most titles in **stereo** (AAC-LC, or xHE-AAC with Widevine enabled).
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

See [Audio File Formats](./audio-file-formats.md) for codec details and [Supported Media Players](./audio-file-formats.md#supported-media-players) if you have trouble playing xHE-AAC.

### 3. If downloads fail even with spatial disabled

Check your log file ([FAQ: Where Can I Find the Log File?](/docs/frequently-asked-questions#where-can-i-find-the-log-file)) and search [open issues](https://github.com/rmcrackan/Libation/issues). Some failures are Audible-side (Plus throttling, temporary outages) rather than spatial-related. See also [Troubleshooting](/docs/advanced/troubleshoot).

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

- [E-AC-3 (Dolby Digital Plus Atmos)](./audio-file-formats.md#e-ac-3)
- [AC-4 (Dolby AC-4 Immersive Stereo)](./audio-file-formats.md#ac-4)

See [Supported Media Players](./audio-file-formats.md#supported-media-players) for an incomplete compatibility list.

**Playing a file is not the same as hearing Atmos.** Many players decode E-AC-3 as plain 5.1 surround without rendering spatial objects. Dolby Atmos rendering often requires extra software (for example [Dolby Access](https://apps.microsoft.com/detail/9n0866fs04w8) on Windows) or hardware support. See [Spatial Audio and Dolby Atmos](./audio-file-formats.md#spatial-audio-and-dolby-atmos) for more detail.

If you expected Atmos but your **new** download is stereo, that is expected with current Libation versions.

## Related documentation

- [Audio File Formats](./audio-file-formats.md) -- codecs, settings, and media players
- [FAQ: Non-spatial playback trouble](/docs/frequently-asked-questions#im-having-trouble-playing-my-non-spatial-audiobook-how-can-i-fix-this)
- [Troubleshooting](/docs/advanced/troubleshoot)
