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
- CI runs `dotnet test` from `Source/`, which builds the entire solution first and is slow;
  prefer running the seven test projects individually on Linux.
- `AudibleUtilities.Tests` contains a network-dependent test that is usually fast but can
  intermittently take ~2 minutes (a socket timeout when there is no network). It still passes; do
  not assume the suite is hung if only that project is slow.

### Running the apps
- GUI: a display is available on `DISPLAY=:1`. Run with
  `cd Source/LibationAvalonia && dotnet run`. First launch shows a Welcome/walkthrough and creates
  config under `~/.local/share/Libation/` (`Settings.json`, `AccountsSettings.json`,
  `LibationContext.db`).
- CLI: `cd Source/LibationCli && dotnet run -- <command>` (e.g. `version`, `get-setting`,
  `list-accounts`, `scan`, `liberate`). The CLI reads the same `~/.local/share/Libation/` config
  and database as the GUI.
- Actually scanning/downloading a library requires signing into a real Audible account, so full
  end-to-end liberation cannot be exercised without credentials.
