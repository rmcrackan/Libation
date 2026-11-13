# Device registration

When you sign in, Libation registers a virtual device with Amazon. Audible then ties download licenses to that device. The default is an Android emulator (`CurrentAndroid`), which is required for [Widevine](/docs/features/audio-file-formats#use-widevine-drm).

## Understanding Throttling: Registration vs. Volume

When Audible denies a content license with `License Denied` or `CustomerThrottled`, it is typically caused by one of two distinct situations:

1. **Obsolete or incompatible device registration:**
   - In early September 2026, Audible began refusing licenses for device registrations using non-Android profiles (e.g. `Mkb79IPhone`), registrations created when Widevine was disabled, or older registrations with legacy device serial numbers (which were 20 bytes / 40 hex characters rather than the modern 10 bytes / 20 hex characters).
   - In this situation, the official Audible mobile app can still play the title, but Libation cannot obtain a download license.
   - **Libation now resolves this automatically:** See [Automatic Registration Recovery](#automatic-registration-recovery) below.

2. **Ordinary volume rate limiting:**
   - Downloading a large number of titles in a short window—such as downloading dozens of Audible Plus catalog titles, or running [Scan for Better Quality Audiobooks](/docs/features/audio-file-formats#upgrading-existing-audiobooks-and-rate-limiting) to re-download a large portion of your library—can trigger Audible's server-side rate limits.
   - In this situation, your device registration is healthy, but Audible temporarily blocks license issuance for 24 to 48 hours (and sometimes a few days).
   - **Solution:** Wait 24 to 48 hours for the limit to expire, and turn on the [Daily download limit](/docs/features/daily-download-limit) (set to "All books" when performing bulk re-downloads) to pace your downloads.

## Automatic Registration Recovery

If Audible reports `CustomerThrottled` on an obsolete registration profile or while Widevine is disabled, Libation automatically intervenes without requiring you to delete your accounts or lose your configuration:

1. **Automatic Settings Update:** Libation automatically enables Widevine DRM and sets the device profile to Android emulator (`CurrentAndroid`).
2. **Account Preservation:** Your accounts, library records, download history, and configured secondary marketplaces (`AdditionalLocaleNames`) are **completely preserved**. No accounts need to be deleted.
3. **Re-registration Prompt:**
   - **In the GUI (Chardonnay & Classic):** Libation marks the account as needing a fresh device registration and will prompt you to complete a browser sign-in on your next library scan or download attempt. Signing in registers the new virtual Android device with Amazon and clears the prompt. In **Settings > Accounts**, the **Signed in?** column displays your status (checked = modern, half-checked/indeterminate = legacy, unchecked = needs sign-in); you can trigger a refresh manually at any time by unchecking it and saving.
   - **In the CLI / Docker:** Running `libationcli list-accounts` displays `yes` (modern), `legacy` (legacy credentials), or `re-login needed` under the **Authenticated** column. Simply run `libationcli login-external --account <email> --locale <region>` to complete the sign-in. The CLI automatically invalidates the old device tokens, establishes the new registration, and clears the re-login flag.
   - **In Settings:** If you manually check **Use Widevine DRM** in Settings, Libation will ask whether you want to mark your accounts for re-registration on their next sign-in, keeping all your account settings intact.

## Where to Find Device Registration Settings

- **Chardonnay:** Settings -> Import library -> **Device registration (experimental)**
- **Classic:** Settings -> Import library -> **Device registration (experimental)**
- **CLI / Docker:** `DeviceRegistrationKind` in `Settings.json`, or `--device-registration` on `login-external`. See [Command Line Interface](/docs/advanced/command-line-interface#log-in-with-an-external-browser-login-external).

## The Two Profiles

| Setting value | Label in Settings | Widevine | What it registers |
|---------------|-------------------|----------|-------------------|
| `CurrentAndroid` | Android emulator (default) | Yes | The corrected Android Audible app registration (recommended) |
| `Mkb79IPhone` | iPhone / audible-cli (experimental; no Widevine) | No | The virtual iPhone profile used by audible-cli |

`RetailAndroid` appeared briefly in Libation 14.1 but is treated as `CurrentAndroid`.

## Manual Troubleshooting Fallback

If automatic recovery does not resolve a persistent license refusal:

1. Open [Amazon's Audible device-management page (United States)](https://www.amazon.com/hz/mycd/digital-console/devicedetails?deviceFamily=AUDIBLE_APP) (or your regional Amazon site: **Manage Your Content and Devices > Devices > Audible**) and deregister old entries named "Libation".
2. If you are experiencing volume throttling, **wait 24 to 48 hours** before attempting further downloads, and configure a [Daily download limit](/docs/features/daily-download-limit).
3. If issues persist after several days, open a GitHub issue and attach your log. See also [Retrying titles Audible refuses](/docs/features/retrying-refused-downloads).
