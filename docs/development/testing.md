# Testing changes

How to run Libation's automated tests, and how to put the app into a known state so you can check a UI change by eye.

## Automated tests

Test projects live under `Source/_Tests/`. They use MSTest on the [Microsoft.Testing.Platform](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-intro) runner, which is selected by `global.json`.

That runner changes one thing about the usual command: `dotnet test` needs `--project` and rejects a bare project path.

```bash
# Run one project
dotnet test --project Source/_Tests/LibationUiBase.Tests/LibationUiBase.Tests.csproj
```

```bash
# Run everything, the way CI does
cd Source
dotnet test
```

Running from `Source/` works on every platform, including Linux. It restores the whole solution but only builds the test projects and what they reference, so the Windows-only projects (`LibationWinForms`, `HangoverWinForms`) are never compiled and do not break the run. Naming a single project is just quicker and gives less output while you iterate.

Each one tests the source project of the same name:

| Project | Covers |
|---------|--------|
| `ApplicationServices.Tests` | Library commands and queries, Audiobookshelf upload |
| `AudibleUtilities.Tests` | Audible API wrappers, accounts, and token storage |
| `FileLiberator.Tests` | Download, decrypt, and conversion steps |
| `FileManager.Tests` | Paths, filename templates, and file utilities |
| `LibationFileManager.Tests` | Configuration, settings, upgrades, and naming templates |
| `LibationSearchEngine.Tests` | Lucene indexing and search syntax |
| `LibationUiBase.Tests` | Shared grid view models and status icons |

`AssertionHelper` is a shared helper library, not a test project.

## Developer scripts

`Scripts/` holds developer utilities. They exist only in a source checkout - none of them ship in an install - so this page is where they are documented.

The testing scripts are [file-based C# apps](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/sdk#file-based-apps): a single `.cs` file with its NuGet references declared at the top. There is no project to restore and nothing to add to the solution. Run one directly with the .NET SDK the repo already pins:

```bash
dotnet run Scripts/<script-name>.cs
```

The script path is the only thing tied to your working directory, so an absolute path works from anywhere. The first run takes a few seconds while the SDK fetches the script's packages.

To pass arguments to the script rather than to `dotnet run`, put them after `--`:

```bash
dotnet run Scripts/<script-name>.cs -- --some-flag
```

Forgetting the `--` is the usual reason a flag appears to be ignored.

### seed-demo-library.cs

Fills a Libation library with fake books covering every icon the grid's **Liberate** column can draw, so a change to those icons can be checked at a glance instead of by hunting for a real book in the right state.

It seeds the full stoplight matrix - each lamp color, each PDF state, purchased and Audible Plus - plus both error icons and a podcast series with episodes. On success it prints a row-by-row list of what each seeded row should look like, so sort the grid by **Title** and read down.

Run Libation once first so the database exists, and close it before seeding:

```bash
dotnet run Scripts/seed-demo-library.cs
```

Remove the seeded books and their placeholder files:

```bash
dotnet run Scripts/seed-demo-library.cs -- --clean
```

The script looks for your Libation folder in the usual per-platform locations, including any redirect in an `appsettings.json` it finds. If it cannot locate the database, pass the folder:

```bash
dotnet run Scripts/seed-demo-library.cs -- "C:\Users\you\AppData\Local\Libation"
```

Both commands are safe to re-run. Seeding skips books that are already present, and only ever touches its own rows, which are prefixed `DEMO`.

::: warning
The seeded books are not real, so do not click their stoplights - that queues a download which cannot succeed. Expanding a seeded series row is fine.
:::

#### Why some states cannot be seeded with SQL alone

Two of the Liberate icons are not stored in the database at all, which is worth knowing before you try to add a state to the script or reproduce one by hand:

- **The yellow lamp is a file on disk.** `Liberated_Status` reports `PartialDownload` when an `.aaxc` matching the book's ASIN is sitting in the `InProgress` folder. The status cannot be written directly either: `UserDefinedItem`'s `BookStatus` setter rewrites `PartialDownload` to `NotLiberated` on the way in. The script writes placeholder `.aaxc` files for its yellow rows.
- **Green and Error are database-only.** `AudioExists` is defined as `BookStatus is Liberated or Error`, so neither needs an audio file to exist. Creating one changes nothing.

One more trap, in the grid rather than the database: a podcast's series is identified by the **parent book's own ASIN**, not by an arbitrary series id, and `SeriesEntry.GetAllSeriesEntriesAsync` discards any series whose children it cannot match. Point the episodes at a different series id and the parent row disappears from the grid with no error.
