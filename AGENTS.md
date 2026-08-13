# AGENTS.md

## Cursor Cloud specific instructions

Libation is a cross-platform .NET desktop/CLI app for downloading and de-DRMing Audible
audiobooks. The core product is the Avalonia GUI (`LibationAvalonia`, assembly name `Libation`)
plus a headless CLI (`LibationCli`) that shares the same config and SQLite library database.

### Toolchain / environment
- Requires the **.NET 10 SDK** pinned by `global.json` (`10.0.101`). It is preinstalled at
  `~/.dotnet`; `~/.bashrc` exports `DOTNET_ROOT` and adds `~/.dotnet` (and `~/.dotnet/tools`) to
  `PATH`. New non-login shells may not have it — use `dotnet` after a login shell, or call
  `~/.dotnet/dotnet` directly.
- The startup update script runs `dotnet tool restore` (restores `dotnet-ef`) and
  `dotnet restore Source/Libation.slnx`. No other setup is needed.
- The solution file is the new XML format: `Source/Libation.slnx` (there is no classic `.sln`).

### Building — Linux only builds the cross-platform projects
- Do **not** build the whole solution on Linux. Several projects are Windows/OS-specific and will
  not build here: `LibationWinForms`, `HangoverWinForms` (target `net10.0-windows7.0`) and the
  `LoadByOS/{Windows,MacOS}ConfigApp` helpers.
- Build the runnable cross-platform apps directly:
  - `dotnet build Source/LibationAvalonia/LibationAvalonia.csproj`
  - `dotnet build Source/LibationCli/LibationCli.csproj`
- There is no dedicated lint step; the repo's `.editorconfig` is minimal and CI
  (`.github/workflows/validate.yml`) only builds and tests. The compiler/analyzer warnings from a
  normal build serve as the static-analysis check. (A known transitive `NU1903` SQLite
  vulnerability warning is expected and harmless.)

### Testing
- Test projects live under `Source/_Tests/` and use MSTest on **Microsoft.Testing.Platform**
  (configured via `global.json` `test.runner`). Because of this runner, `dotnet test` requires
  `--project` for a single project (a positional project path is rejected):
  `dotnet test --project Source/_Tests/FileManager.Tests/FileManager.Tests.csproj`
- CI runs `dotnet test` from `Source/` on every platform, Linux included, and it works here too: the
  restore covers the whole solution but only the test projects and their references are compiled, so
  the Windows-only projects never break the run. Naming one project is simply quicker.
- **Manual UI testing:** `dotnet run Scripts/seed-demo-library.cs` fills the library with fake books
  covering every Liberate-column icon and prints the expected result for each row
  (`-- --clean` removes them). Read `docs/development/testing.md` before seeding library state by
  hand: the yellow lamp is an `.aaxc` file on disk rather than a stored status, `AudioExists` is a
  database check so green/error need no files, and a podcast's series is keyed off the parent
  book's own ASIN or the grid silently drops the parent row.
- **GNOME Keyring / OS secret store:** Libation's default `TokenStorageMethod` is `Encrypted`,
 and the AES-GCM master key is stored via the OS secret store (`OsSecretStore` /
 `IdentityTokenStorageWiring`). On Linux that is GNOME Keyring (Secret Service), which **blocks
 indefinitely** here: the login keyring is locked with a password nobody has, and the D-Bus call
 hangs even with no prompt on screen. Probing availability does not help - the probe is the
 blocking call.
 Every test project should finish in about a second. If one runs for minutes, assume something
 reached the OS secret store; do not sit through it and do not re-run it hoping for a different
 result. `ResolveSecretStore` short-circuits on `LIBATION_MASTER_KEY_FILE`, an existing
 `libation-master.key` under the Libation files dir, or `LIBATION_MASTER_KEY`, so setting one of
 those keeps a test off the OS store entirely.
 The tests that deliberately exercise the real store are opt-in via
 `LIBATION_TEST_OS_SECRET_STORE=1` and are skipped otherwise. Leave them skipped on Linux; they
 will hang if enabled. Always run tests under `timeout` so a regression here cannot stall a session.

### Running the apps
- GUI: a display is available on `DISPLAY=:1`. Run with
  `cd Source/LibationAvalonia && dotnet run`. First launch shows a Welcome/walkthrough and creates
  config under `~/.local/share/Libation/` (`Settings.json`, `AccountsSettings.json`,
  `LibationContext.db`). The same keyring note above applies when the GUI/CLI first encrypts
  account tokens.
- CLI: `cd Source/LibationCli && dotnet run -- <command>` (e.g. `version`, `get-setting`,
  `list-accounts`, `scan`, `liberate`). The CLI reads the same `~/.local/share/Libation/` config
  and database as the GUI.
- Actually scanning/downloading a library requires signing into a real Audible account, so full
  end-to-end liberation cannot be exercised without credentials.
