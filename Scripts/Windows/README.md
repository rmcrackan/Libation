# Windows installer (Inno Setup)

Step 8.1 of the Windows installers plan: a parameterized Inno Setup script that installs the same flat publish layout as the release zip, under a per-user directory, so the in-app upgrader can still overlay files via `ZipExtractor.exe`.

## Prerequisites

1. [.NET SDK](https://dotnet.microsoft.com/download) (same version as the repo; see `build.yml`).
2. [Inno Setup 6](https://jrsoftware.org/isdl.php) with `ISCC.exe` on PATH or in a default install location.

## Quick build (publish + installer)

From PowerShell, repo root or this folder:

```powershell
.\Scripts\Windows\Build-WindowsInstaller.ps1 -Version 13.4.1 -Ui Avalonia -Architecture x64
```

Outputs:

- Publish tree: `bin\` (repo root)
- Installer: `bin\Libation.13.4.1-windows-chardonnay-x64-setup.exe`

### Variants

| UI | `-Ui` | `-Architecture` | Install folder | Output prefix |
|----|-------|-----------------|----------------|---------------|
| Chardonnay x64 | `Avalonia` | `x64` | `%LocalAppData%\Libation` | `Libation.*` |
| Chardonnay arm64 | `Avalonia` | `arm64` | `%LocalAppData%\Libation` | `Libation.*` |
| Classic x64 | `WinForms` | `x64` (default) | `%LocalAppData%\Libation-Classic` | `Libation-Classic.*-setup.exe` |

Examples:

```powershell
.\Scripts\Windows\Build-WindowsInstaller.ps1 -Version 13.4.1 -Ui WinForms
.\Scripts\Windows\Build-WindowsInstaller.ps1 -Version 13.4.1 -Ui Avalonia -Architecture arm64
```

### Reuse an existing publish folder

```powershell
.\Scripts\Windows\Build-WindowsInstaller.ps1 -Version 13.4.1 -Ui Avalonia -BinDir C:\path\to\bin -SkipPublish
```

`BinDir` must already match CI layout (including `ZipExtractor.exe`, without standalone `WindowsConfigApp.exe`).

## Manual ISCC (without the helper script)

After publish and cleanup (remove `WindowsConfigApp.exe`, `WindowsConfigApp.runtimeconfig.json`, `WindowsConfigApp.deps.json` from `bin`), from `Scripts\Windows`:

```text
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" Libation.iss ^
  /DMyVersion=13.4.1 ^
  /DMyPrefix= ^
  /DMyReleaseName=chardonnay ^
  /DMyArchitecture=x64 ^
  /DInstallFolderName=Libation ^
  /DAppName=Libation ^
  /DOutputBaseFilename=Libation.13.4.1-windows-chardonnay-x64-setup ^
  /DSourceDir=C:\Dev\_rmcrackan\Libation\bin ^
  /DOutputDir=C:\Dev\_rmcrackan\Libation\bin ^
  /DArchitecturesAllowed=x64compatible
```

Classic x64: use `MyReleaseName=classic` and matching `OutputBaseFilename` (`Libation-Classic.{version}-windows-classic-x64-setup`). Install folder and display name are set in `Libation.iss`. GitHub zip assets use `Libation-Classic.{version}-windows-classic-x64.zip`; `.releaseindex.json` also accepts legacy `Classic-Libation.*` zips on older releases.

arm64: set `MyArchitecture=arm64` and `ArchitecturesAllowed=arm64`.

## Installer behavior

- **Per-user install** (`PrivilegesRequired=lowest`, default dir under `{localappdata}`).
- **Shortcuts** target `{app}\Libation.exe` (required for upgrade path detection).
- **Does not** create `appsettings.json` or Libation Files; same as zip install.
- **Includes** full publish output so `ZipExtractor.exe` remains for in-app updates.
- **Settings -> Apps:** shows `Libation (Chardonnay)` or `Libation (Classic)` with version in `DisplayVersion` (Inno at install; synced after in-app zip upgrades via `WindowsUninstallRegistrySync`); icon from `SetupIconFile` / `UninstallDisplayIcon` (`Scripts/Windows/libation.ico` and `{app}\Libation.exe`).

## Next plan step (8.2)

Install via `*-setup.exe`, run the app, then verify an older release can upgrade in-app using the existing zip-based upgrader.

CI wiring (`build-windows.yml`) is step 8.3.

## Files in this folder

| File | Purpose |
|------|---------|
| `Libation.iss` | Parameterized Inno Setup script |
| `libation.ico` | Setup/uninstaller icon (copied from Avalonia assets) |
| `Build-WindowsInstaller.ps1` | Local publish + ISCC helper |
| `README.md` | This document |
