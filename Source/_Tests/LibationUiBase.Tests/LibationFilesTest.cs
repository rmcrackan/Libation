using LibationFileManager;
using LibationUiBase.Forms;
using Newtonsoft.Json.Linq;

namespace LibationUiBase.Tests;

[TestClass]
public class LibationFilesTest
{
	//Mock MessageBox results in setup workflow
	private Queue<DialogResult> MessageBoxResults { get; } = new();
	private Task<DialogResult> ShowMessageBox(
			object? owner,
			string message,
			string caption,
			MessageBoxButtons buttons,
			MessageBoxIcon icon,
			MessageBoxDefaultButton defaultButton,
			bool _)
		=> Task.FromResult(MessageBoxResults.Dequeue());

	public LibationFilesTest()
	{
		MessageBoxBase.ShowAsyncImpl = ShowMessageBox;
	}

	[TestMethod]
	public async Task Setup_NoSelection()
	{
		var libationFiles = new LibationFiles(CreateAppSettings());
		var setup = CreateMockSetup(libationFiles, isNewUser: false, isReturningUser: false, selectedDirectory: null);

		Assert.IsFalse(await setup.RunSetupIfNeededAsync());
		Assert.IsFalse(libationFiles.SettingsAreValid);
	}

	[TestMethod]
	public async Task Setup_NoSelection_SettingsExistInDefaultLocation()
	{
		var libationFiles = new LibationFiles(CreateAppSettings());
		var setup = CreateMockSetup(libationFiles, isNewUser: false, isReturningUser: false, selectedDirectory: null);

		File.WriteAllText(Path.Combine(LibationFiles.DefaultLibationFilesDirectory, LibationFiles.SETTINGS_JSON), new JObject { { "Books", "SomePath" } }.ToString());
		Assert.IsTrue(await setup.RunSetupIfNeededAsync());
		Assert.IsTrue(libationFiles.SettingsAreValid);
	}

	[TestMethod]
	public async Task NewInstall_BadLibationFilesPath_ChooseDefault_Yes()
	{
		var libationFiles = new LibationFiles(CreateAppSettings(BadPath));
		var setup = CreateMockSetup(libationFiles, isNewUser: true, isReturningUser: false, selectedDirectory: null);

		//Error Creating LibationFiles. Move to default directory? 
		MessageBoxResults.Enqueue(DialogResult.Yes);

		Assert.IsTrue(await setup.RunSetupIfNeededAsync());
		Assert.IsTrue(libationFiles.SettingsAreValid);
	}

	private static string BadPath => Configuration.IsWindows ? "NONEXISTANT:\\" : "/NONEXISTANT";
	[TestMethod]
	public async Task NewInstall_BadLibationFilesPath_ChooseDefault_Yes_Fail()
	{
		var libationFiles = new LibationFiles(CreateAppSettings(BadPath));
		var setup = CreateMockSetup(libationFiles, isNewUser: true, isReturningUser: false, selectedDirectory: null);

		LibationFiles.s_DefaultLibationFilesDirectory = Path.Combine(BadPath, "AlsoInvalid");

		//Error Creating LibationFiles. Move to default directory? 
		MessageBoxResults.Enqueue(DialogResult.Yes);
		MessageBoxResults.Enqueue(DialogResult.OK);

		Assert.IsFalse(await setup.RunSetupIfNeededAsync());
		Assert.IsFalse(libationFiles.SettingsAreValid);
	}

	[TestMethod]
	public async Task NewInstall_BadLibationFilesPath_ChooseDefault_No()
	{
		var libationFiles = new LibationFiles(CreateAppSettings(BadPath));
		var setup = CreateMockSetup(libationFiles, isNewUser: true, isReturningUser: false, selectedDirectory: null);

		//Error Creating LibationFiles. Move to default directory? 
		MessageBoxResults.Enqueue(DialogResult.No);

		Assert.IsFalse(await setup.RunSetupIfNeededAsync());
		Assert.IsFalse(libationFiles.SettingsAreValid);
	}

	[TestMethod]
	public async Task NewInstall_ReadonlyDir_DefaultDir()
	{
		var libationFiles = new LibationFiles(CreateAppSettings());
		var setup = CreateMockSetup(libationFiles, isNewUser: true, isReturningUser: false, selectedDirectory: null);
		MakeUnixReadOnly(Directory.CreateDirectory(libationFiles.Location));

		//Dismiss error creating settings file MBox
		MessageBoxResults.Enqueue(DialogResult.OK);

		//Prevent installer from being able to create settings file
		using var fs = Configuration.IsWindows ? File.Open(libationFiles.SettingsFilePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read) : default(IDisposable);
		Assert.IsFalse(await setup.RunSetupIfNeededAsync());
		Assert.IsFalse(libationFiles.SettingsAreValid);
	}

	[TestMethod]
	public async Task NewInstall_ReadonlyDir_CustomDir_RevertToDefault_No()
	{
		var customDir = Path.GetFullPath("CustomDir");
		if (Directory.Exists(customDir))
			Directory.Delete(customDir, recursive: true);
		MakeUnixReadOnly(Directory.CreateDirectory(customDir));

		var libationFiles = new LibationFiles(CreateAppSettings(customDir));
		var setup = CreateMockSetup(libationFiles, isNewUser: true, isReturningUser: false, selectedDirectory: null);

		//Error Creating Settings.json. Move to default directory? 
		MessageBoxResults.Enqueue(DialogResult.No);
		//Prevent installer from being able to create settings file in customDir

		using var fs = Configuration.IsWindows ? File.Open(Path.Combine(customDir, LibationFiles.SETTINGS_JSON), FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read) : default(IDisposable);
		Assert.IsFalse(await setup.RunSetupIfNeededAsync());
		Assert.IsFalse(libationFiles.SettingsAreValid);
	}

	[TestMethod]
	public async Task NewInstall_ReadonlyDir_CustomDir_RevertToDefault_Yes()
	{
		var customDir = Path.GetFullPath("CustomDir");
		if (Directory.Exists(customDir))
			Directory.Delete(customDir, recursive: true);
		Directory.CreateDirectory(customDir);

		var libationFiles = new LibationFiles(CreateAppSettings(customDir));
		var setup = CreateMockSetup(libationFiles, isNewUser: true, isReturningUser: false, selectedDirectory: null);

		//Error Creating Settings.json. Move to default directory? 
		MessageBoxResults.Enqueue(DialogResult.Yes);
		//Prevent installer from being able to create settings file in customDir
		using var fs = File.Open(Path.Combine(customDir, LibationFiles.SETTINGS_JSON), FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
		Assert.IsTrue(await setup.RunSetupIfNeededAsync());
		Assert.IsTrue(libationFiles.SettingsAreValid);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("   ")]
	public async Task Returning_BadSelectedDir(string? selectedDir)
	{
		var libationFiles = new LibationFiles(CreateAppSettings());
		var setup = CreateMockSetup(libationFiles, isNewUser: false, isReturningUser: true, selectedDir);

		Assert.IsFalse(await setup.RunSetupIfNeededAsync());
		Assert.IsFalse(libationFiles.SettingsAreValid);
	}

	[TestMethod]
	public async Task Returning_SettingsFound()
	{
		var customDir = Path.GetFullPath("CustomDir");
		if (Directory.Exists(customDir))
			Directory.Delete(customDir, recursive: true);
		Directory.CreateDirectory(customDir);

		var libationFiles = new LibationFiles(CreateAppSettings());
		var setup = CreateMockSetup(libationFiles, isNewUser: false, isReturningUser: true, selectedDirectory: customDir);

		File.WriteAllText(Path.Combine(customDir, LibationFiles.SETTINGS_JSON), new JObject { { "Books", "SomePath" } }.ToString());
		Assert.IsTrue(await setup.RunSetupIfNeededAsync());
		Assert.IsTrue(libationFiles.SettingsAreValid);
	}

	[TestMethod]
	public async Task Returning_SettingsNotFound_CreateAt_Yes()
	{
		var customDir = Path.GetFullPath("CustomDir");
		if (Directory.Exists(customDir))
			Directory.Delete(customDir, recursive: true);
		Directory.CreateDirectory(customDir);

		var libationFiles = new LibationFiles(CreateAppSettings());
		var setup = CreateMockSetup(libationFiles, isNewUser: false, isReturningUser: true, selectedDirectory: customDir);

		//No valid settings were found. Create New Install? 
		MessageBoxResults.Enqueue(DialogResult.Yes);

		Assert.IsTrue(await setup.RunSetupIfNeededAsync());
		Assert.IsTrue(libationFiles.SettingsAreValid);
	}

	[TestMethod]
	public async Task Returning_SettingsNotFound_CreateAt_No()
	{
		var customDir = Path.GetFullPath("CustomDir");
		if (Directory.Exists(customDir))
			Directory.Delete(customDir, recursive: true);
		Directory.CreateDirectory(customDir);
		var appSettingsFile = CreateAppSettings();
		var libationFiles = new LibationFiles(appSettingsFile);

		var setup = CreateMockSetup(libationFiles, isNewUser: false, isReturningUser: true, selectedDirectory: customDir);

		//No valid settings were found. Create New Install? 
		MessageBoxResults.Enqueue(DialogResult.No);
		Assert.IsFalse(await setup.RunSetupIfNeededAsync());
	}

	private LibationSetup CreateMockSetup(LibationFiles libationFiles, bool isNewUser, bool isReturningUser, string? selectedDirectory)
	{
		Assert.IsTrue(File.Exists(libationFiles.AppsettingsJsonFile));
		Assert.IsFalse(libationFiles.SettingsAreValid);
		return new LibationSetup(libationFiles)
		{
			SetupPrompt = () => new MockLibationSetup { IsNewUser = isNewUser, IsReturningUser = isReturningUser },
			SelectFolderPrompt = () => new MockLibationInstallLocation { SelectedDirectory = selectedDirectory}
		};
	}
	private static void MakeUnixReadOnly(FileSystemInfo di)
	{
#pragma warning disable CA1416 // Validate platform compatibility
		if (!Configuration.IsWindows)
			di.UnixFileMode &= ~(UnixFileMode.UserWrite | UnixFileMode.GroupWrite | UnixFileMode.OtherWrite);
#pragma warning restore CA1416
	}

	private class MockLibationSetup : ILibationSetup
	{
		public bool IsNewUser { get; set; }
		public bool IsReturningUser { get; set; }
	}

	private class MockLibationInstallLocation : ILibationInstallLocation
	{
		public string? SelectedDirectory { get; set; }
	}

	private static string CreateAppSettings(string? libationFiles = null)
	{
		var file = Path.GetFullPath($"appsettings.test.json");

		var libationFilesDir = Path.Combine(Environment.CurrentDirectory, "LibationFilesTest");
		if (Directory.Exists(libationFilesDir))
			Directory.Delete(libationFilesDir, recursive: true);
		Directory.CreateDirectory(libationFilesDir);
		LibationFiles.s_DefaultLibationFilesDirectory = libationFilesDir;

		if (File.Exists(file))
			File.Delete(file);
		File.WriteAllText(file, new JObject { { LibationFiles.LIBATION_FILES_KEY, libationFiles ?? libationFilesDir } }.ToString());

		return file;
	}
}
