# Device registration (experimental)

When you sign in, Libation registers a virtual device with Amazon. Audible then ties download licenses to that device. The default is an Android emulator, which is required for [Widevine](/docs/features/audio-file-formats#use-widevine-drm).

Older Libation versions generated an Android device serial that was twice the expected length. Audible began refusing licenses (`License Denied` / `CustomerThrottled`) for some of those registrations even when the same title still played in the official Audible app. Current versions use the corrected Android registration.

Registration data is stored with the account, so updating Libation alone does **not** repair an existing registration. For throttling or license refusals, follow the [full recovery procedure](#how-to-register-an-account-again) first. Keep the corrected Android default unless you specifically need an alternative.

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

Audible changed something in early September 2026. For `CustomerThrottled`, or a license refusal when the official Audible app can still play the title, this is our recommended workaround. We cannot be certain every step is necessary, and it is not a guaranteed fix.

1. Open [Amazon's Audible device-management page (United States)](https://www.amazon.com/hz/mycd/digital-console/devicedetails?deviceFamily=AUDIBLE_APP) and **deregister entries named "Libation"**. For other regions, use your regional Amazon site and go to **Manage Your Content and Devices > Devices > Audible**. The app constructs a regional link, but links outside the US are unverified; use the manual route if yours does not work.
2. Upgrade to [the latest version of Libation](https://github.com/rmcrackan/Libation/releases/latest).
3. Go to **Settings > Accounts** and remove the affected account. **This preserves your library and downloaded files.** Save the removal, then close Libation.
4. Reopen Libation. Go to **Settings > Accounts**, verify the old account is absent, then re-add it using the same account and registration region. Save.
5. Scan and sign in, then retry the download. If the title is deferred, mark it **Download Pending** to try sooner.

<details>
<summary>Only if this login scans additional marketplaces</summary>

Before deleting the account in step 3, record its registration region and every additional marketplace checked under its **Marketplaces** button. After recreating the account, scan/sign in once to enable that button. Before retrying downloads, return to **Settings > Accounts**, click **Marketplaces** on that account's row, check every recorded additional marketplace, and save both dialogs. Scan again with all marketplaces restored, then retry the download.

The initial GUI scan is needed to sign in; the Marketplaces button is disabled until credentials exist. Recreating the account alone does not restore its additional marketplaces. For example, a UK registration that also scans the US needs the US checked again, or US downloads can fail with "No account found".

</details>

### CLI / Docker

Follow steps 1 and 2 above. Run `list-accounts` to note the affected account's registration region (**Locale**). Stop Libation and any running CLI/Docker jobs. Back up `AccountsSettings.json`, then remove **only the affected account object** from its `Accounts` array, keeping valid JSON and all other entries. Save it. For Docker, edit the persistent configuration mounted into the container, not a temporary internal copy.

Run `list-accounts` to verify the old account is absent. Then run `login-external --account <email> --locale <registration-region>` and complete sign-in. Run `scan`, then `liberate <ASIN>`. LibationCli has no remove-account command; `login-external` alone skips sign-in when existing credentials are still valid.

<details>
<summary>Only if this login scans additional marketplaces</summary>

Before deleting the account, also record **Also scans** from `list-accounts` and preserve the account's `AdditionalLocaleNames` array in your backup. After `login-external` finishes, with Libation and CLI/Docker jobs stopped, copy that array into the **newly created account object** in `AccountsSettings.json` before any scan or retry. For a UK registration with the US as an additional marketplace, that property is `"AdditionalLocaleNames": ["us"]`. Restore all recorded entries, not just the marketplace of the failed title. **Keep the new identity tokens; do not restore the old account object or credentials.** Save, then run `list-accounts` and verify **Locale** and **Also scans** match your notes. Only then run `scan` and `liberate <ASIN>`. There is no CLI verb for adding marketplaces.

</details>

### If registration recovery does not help

Audible also has temporary outages and rate limits, especially after heavy Plus use. **Wait 24 to 48 hours, sometimes a few days**, before retrying. You may still be able to listen through Audible's app or website. If the title no longer plays there either, check whether you still have access to it.

As a secondary option, try **iPhone / audible-cli** and repeat the account removal and sign-in steps, or import credentials from [audible-cli](https://github.com/mkb79/audible-cli) with `import-account`. This experimental alternative does not support Widevine. Imported audible-cli credentials already use its iPhone registration.

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

If the recovery procedure and waiting a few days do not help, open a GitHub issue and attach your log. See also [Daily download limit](/docs/features/daily-download-limit) and [Retrying titles Audible refuses](/docs/features/retrying-refused-downloads).
