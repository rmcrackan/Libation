# Scripts

Developer utilities. None of these ship in a Libation install - they exist only in a source checkout.

| Script | Purpose | Documented in |
|--------|---------|---------------|
| `seed-demo-library.cs` | Seed a library covering every Liberate-column icon, for manual UI testing | [Testing Changes](https://getlibation.com/docs/development/testing) |
| `seed-download-history.cs` | Seed completed downloads so the daily download limit can be tested without downloading | [Testing Changes](https://getlibation.com/docs/development/testing) |
| `Bundle_Debian.sh` | Build the Linux `.deb` package | Used by `.github/workflows/build-linux.yml` |
| `Bundle_Redhat.sh` | Build the Linux RPM package | Used by `.github/workflows/build-linux.yml` |
| `Bundle_MacOS.sh` | Build the macOS app bundle | Used by `.github/workflows/build-mac.yml` |
| `Windows/` | Windows installer (Inno Setup) | [Windows/README.md](Windows/README.md) |

Usage for the testing scripts lives in the docs rather than here, so there is only one copy to keep current.
