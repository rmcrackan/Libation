# Device registration (experimental)

When you sign in, Libation registers a virtual device with Amazon. Audible then ties download licenses to that device. The default is an Android emulator, which is required for [Widevine](/docs/features/audio-file-formats#use-widevine-drm).

Older Libation versions generated an Android device serial that was twice the expected length. Audible began refusing licenses (`License Denied` / `CustomerThrottled`) for some of those registrations even when the same title still played in the official Audible app. Current versions use the corrected Android registration.

Registration data is stored with the account, so updating Libation or changing this setting does **not** repair an account you already signed in. Remove and re-add the affected account (or run `login-external`) to register it again. Try the corrected Android default first. If Audible still refuses licenses, the experimental iPhone/audible-cli profile is available as an alternative; you can also import credentials from [mkb79's audible-cli](https://github.com/mkb79/audible-cli).

## Where to find it

- **Chardonnay:** Settings -> Import library -> **Device registration (experimental)**
- **Classic:** Settings -> Import library -> **Device registration (experimental)**
- **CLI / Docker:** `DeviceRegistrationKind` in `Settings.json`, or `--device-registration` on `login-external`. See [Command Line Interface](/docs/advanced/command-line-interface#log-in-with-an-external-browser-login-external).

## The two profiles

| Setting value | Label in Settings | Widevine | What it registers |
|---------------|-------------------|----------|-------------------|
| `CurrentAndroid` | Android emulator (default) | Yes | The corrected Android Audible app registration |
| `Mkb79IPhone` | iPhone / audible-cli (experimental; no Widevine) | No | The virtual iPhone used by audible-cli |

`RetailAndroid` appeared briefly in Libation 14.1 but is no longer a separate option. Existing `RetailAndroid` values are treated as `CurrentAndroid`.

## How to register an account again

1. Leave **Android emulator (default)** selected unless you specifically want to try the iPhone alternative.
2. Remove the account from Libation. Existing Amazon device records keep the old registration until you sign in again.
3. Add the account and sign in, or run `login-external`.
4. Scan and try the download again.

If the corrected Android registration is still denied, repeat those steps with **iPhone / audible-cli**, or import an audible-cli JSON file with `import-account`. Imported audible-cli credentials already use its iPhone registration, so you do not need to change this setting first.

## Widevine

**Use Widevine DRM** only works when the account was registered with `CurrentAndroid`. The iPhone profile cannot use Widevine. If you need Widevine later, remove the account and sign in again with the Android profile.

## Settings.json (Docker and CLI)

```json
{
  "DeviceRegistrationKind": "Mkb79IPhone"
}
```

Supported choices are `CurrentAndroid` and `Mkb79IPhone`. Then remove the account and sign in again. `login-external --device-registration Mkb79IPhone` overrides Settings for that one sign-in. A legacy `RetailAndroid` value behaves as `CurrentAndroid`.

## If it still fails

Wait 24 to 48 hours: Audible also rate-limits heavy Plus use. See [Daily download limit](/docs/features/daily-download-limit) and [Retrying titles Audible refuses](/docs/features/retrying-refused-downloads). If the official app can play the title and a new registration still cannot download it, open a GitHub issue and attach your log.
