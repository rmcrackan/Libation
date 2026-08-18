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

It seeds the full stoplight matrix - each lamp color, each PDF state, purchased and Audible Plus - plus both error icons, a podcast series with episodes, books missing from the last scan, and books in the trash. On success it prints a row-by-row list of what each seeded row should look like, so sort the grid by **Title** and read down.

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

#### Books in the trash

Three of the seeded books are in the trash, and they are the only ones that will not be in the grid. Removal is a soft delete: `GetLibrary()` filters `IsDeleted` out, which takes a trashed book out of the grid, out of the search index and out of every status count at once. Nothing then distinguishes it from a book that was never imported, which is what made [#1925](https://github.com/rmcrackan/Libation/issues/1925) take a week to answer. These rows are how the affordances that fixed that are checked.

The script prints them under their own heading, along with what should account for them:

```
3 seeded book(s) are in the trash, so they are NOT in the grid:
  28 Trashed | purchased                         in the trash bin only, red lamp there
  29 Trashed | PLUS                              in the trash bin only, green lamp with badge there
  30 Demo Series - episode 3 (trashed)           nested under Demo Series in the trash bin, absent from the grid
```

Four things to check, none of which needs a real Audible account:

- The status bar ends with a clickable **3 in trash**, which opens the trash bin. It disappears entirely once the trash is empty.
- **Settings > Trash Bin** reads `Trash Bin (3)`.
- Filtering for `Trashed` matches nothing in the library, so the grid says so and offers to open the trash bin. That hint only appears when the same filter matches something in the trash, so filtering for a word that is in neither place gives the plain "no books match" message.
- The trashed episode is nested under **Demo Series** inside the trash bin, even though the series itself is not deleted. `GetDeletedLibraryBooks` asks for every parent rather than only deleted ones, so an episode can still be shown beneath its series there. The series keeps its other two episodes in the main grid.

Restoring a book from the trash puts it straight back in the grid and drops the count, so the same three rows can be used more than once. Re-run the script to put them back.

#### Why some states cannot be seeded with SQL alone

Two of the Liberate icons are not stored in the database at all, which is worth knowing before you try to add a state to the script or reproduce one by hand:

- **The yellow lamp is a file on disk.** `Liberated_Status` reports `PartialDownload` when an `.aaxc` matching the book's ASIN is sitting in the `InProgress` folder. The status cannot be written directly either: `UserDefinedItem`'s `BookStatus` setter rewrites `PartialDownload` to `NotLiberated` on the way in. The script writes placeholder `.aaxc` files for its yellow rows.
- **Green and Error are database-only.** `AudioExists` is defined as `BookStatus is Liberated or Error`, so neither needs an audio file to exist. Creating one changes nothing.

One more trap, in the grid rather than the database: a podcast's series is identified by the **parent book's own ASIN**, not by an arbitrary series id, and `SeriesEntry.GetAllSeriesEntriesAsync` discards any series whose children it cannot match. Point the episodes at a different series id and the parent row disappears from the grid with no error.

### seed-download-history.cs

Fills the `DownloadHistory` table with fake completed downloads, so the [daily download limit](/docs/features/daily-download-limit) can be tested without downloading anything. Start Libation once first so the table exists.

Reach a limit of 50 Audible Plus titles and stay there:

```bash
dotnet run Scripts/seed-download-history.cs -- --count 50 --age-seconds 7200
```

The limit uses a rolling 24 hour window, and `--age-seconds` is how long ago each fake download finished, so it also controls when the window frees up. Dating the rows just under 24 hours old turns a multi-day wait into a one minute wait, which is how the pause-and-resume behavior is checked:

```bash
# a paused queue resumes on its own about a minute from now
dotnet run Scripts/seed-download-history.cs -- --count 50 --age-seconds 86325
```

Other flags: `--owned` seeds purchased titles instead of Plus ones (useful for checking that a Plus-only limit ignores them), `--mb` sets the size of each fake download for testing MB and GB limits, and `--clean` deletes every row.

Libation may be running while you seed. Each check re-queries the database, so a running queue picks up the new rows within seconds without a restart.

::: tip
The limit is off by default. Set **Settings > Download/Decrypt > Daily download limit** before expecting seeded rows to block anything.
:::
