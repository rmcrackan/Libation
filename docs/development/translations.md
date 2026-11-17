# Translations

Chardonnay, the main Libation app, reads its UI text from `Source/LibationUiBase/Resources/Strings.resx`. A translation is a copy of that file with the values translated. Libation Classic and the CLI stay English.

## Adding UI text

Put user-facing text in `Strings.resx`, not inline:

- XAML: add `xmlns:res="using:LibationUiBase.Resources"` and write `Text="{x:Static res:Strings.TrashBinDialog_Restore}"`.
- C#: `Strings.TrashBinDialog_Restore`. The `Strings` class is generated at build time.
- Name keys after the file and the text: `<FileName>_<FirstWords>`.
- Text with values in it becomes `string.Format(Strings.Key, value)` with `{0}`, `{1}` holes. Say what each hole is in the entry's comment (`{0} = account name`), since translators can't see the code.

Write whole sentences. A translator can't reorder text that is glued together from pieces, and other languages change words around a number or a name.

- Counts: use `Counted.Books(n)` and `Counted.Titles(n)` from LibationUiBase, or a pair of entries, one for exactly one and one for the rest. Don't build plurals from English nouns.
- Choices like "this book" or "these books" get one entry each, not a swapped word.

Settings labels and enum names live in `[Description]` attributes in LibationFileManager, which can't see these resources. Show them through `SettingsText.Label(nameof(Configuration.X))` and `SettingsText.Of(value)`, and copy a new or changed description into `Strings.resx` as `Setting_<Property>` or `Enum_<Type>_<Member>`. `SettingsTextTests` fails until the copy matches. An enum member without a `[Description]` gets its member name as the English. When the UI shows a new enum type, add it to `EnumsShownInTheUi` in `SettingsTextTests` so its entries are checked too.

Text that is not translated on purpose stays inline:

- log messages,
- `Program.Main` and the crash handler in LibationAvalonia. They must work even when an interrupted upgrade left an older `LibationUiBase.dll` behind (see [#2001](https://github.com/rmcrackan/Libation/issues/2001)), so they must not name `Strings`, `UiLanguage` or any other type that is new in LibationUiBase. Calling types that already existed, like `CrashLogMessage`, is fine,
- search keywords and template tags, which users type.

Text left inline elsewhere still works. It just stays English in every language.

## Adding a language

1. Copy `Strings.resx` to `Strings.<language>.resx` in the same folder, for example `Strings.pt-BR.resx`.
2. Translate each `<value>`. Leave `{0}`-style holes in place, though you can move them within the text. The `<comment>` says what each one is.
3. Build. The translation lands in a `<language>` folder next to the app, and Settings > Important Settings > Language lists it. A new choice takes effect on the next start. By default Libation follows the system language.

Untranslated entries fall back to English. Dates and numbers follow the system's regional settings, not the chosen language.

Longer text should fit: dialogs wrap, scroll or widen when a label runs long. If something is cut off, report it with a screenshot.

## Not translated yet

- Messages built outside LibationUiBase, mainly in ApplicationServices, FileLiberator and AudibleUtilities: library scan problems, skipped or deferred downloads, bulk status changes, login errors.
- The naming template editor's tag descriptions and template warnings, which live in LibationFileManager.
- Settings tooltips and help text, from `Configuration.HelpText.cs` in LibationFileManager.
- A few names shown as-is: LAME encoder quality, log levels, bookmark file formats.
- Messages shown before settings load, such as "Libation is already running", which use the system language.
- The crash dialog's title and opening line, on purpose. Its paragraph about the log file is translated.
- Data grid column headers aren't cut to fit. In a long language they clip until the column is widened.
- Plurals have two forms, one and other. Languages with more forms, or with grammatical gender, may read awkwardly in places.
