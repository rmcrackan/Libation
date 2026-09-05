# Device registration (experimental)

When you sign in, Libation registers a virtual device with Amazon. Audible then ties download licenses to that device. The default is an Android emulator, which is required for [Widevine](/docs/features/audio-file-formats#use-widevine-drm).

Audible has been refusing licenses (`License Denied` / `CustomerThrottled`) for some emulator registrations even when the same title still plays in the official Audible app. If that happens, you can try an experimental registration, or import credentials from [mkb79's audible-cli](https://github.com/mkb79/audible-cli).

Changing this setting does **not** convert accounts you already signed in. Remove and re-add the account (or run `login-external`) after you pick a different profile.

## Where to find it

- **Chardonnay:** Settings -> Important -> **Device registration (experimental)**
- **Classic:** Settings -> Import library -> **Device registration (experimental)**
- **CLI / Docker:** `DeviceRegistrationKind` in `Settings.json`, or `--device-registration` on `login-external`. See [Command Line Interface](/docs/advanced/command-line-interface#log-in-with-an-external-browser-login-external).

## The three profiles

| Setting value | Label in Settings | Widevine | What it registers |
|---------------|-------------------|----------|-------------------|
| `CurrentAndroid` | Android emulator (default) | Yes | The emulator Libation has used for years |
| `RetailAndroid` | Android Pixel (experimental) | Yes | Same Android Audible app as the default, with a retail Pixel fingerprint |
| `Mkb79IPhone` | iPhone / audible-cli (experimental; no Widevine) | No | The virtual iPhone used by audible-cli |

Leave the default unless downloads fail for titles that still work in the official app.

## How to apply a new profile

1. Pick the profile in Settings (or set `DeviceRegistrationKind` / `--device-registration`).
2. Remove the account from Libation. Existing Amazon device records keep the old registration until you sign in again.
3. Add the account and sign in, or run `login-external`.
4. Scan and try the download again.

Importing an audible-cli JSON file with `import-account` is the other workaround: those credentials already come from audible-cli's iPhone registration, so you do not need to change this setting first.

## Widevine

**Use Widevine DRM** only works when the account was registered as an Android Audible app (`CurrentAndroid` or `RetailAndroid`). The iPhone profile cannot use Widevine. If you need Widevine later, remove the account and sign in again with an Android profile.

## Settings.json (Docker and CLI)

```json
{
  "DeviceRegistrationKind": "RetailAndroid"
}
```

Accepted values: `CurrentAndroid`, `RetailAndroid`, `Mkb79IPhone`. Then remove the account and sign in again. `login-external --device-registration Mkb79IPhone` overrides Settings for that one sign-in.

## If it still fails

Wait 24 to 48 hours: Audible also rate-limits heavy Plus use. See [Daily download limit](/docs/features/daily-download-limit) and [Retrying titles Audible refuses](/docs/features/retrying-refused-downloads). If the official app can play the title and a new registration still cannot download it, open a GitHub issue and attach your log.
