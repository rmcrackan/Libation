using Dinah.Core;
#if !DEBUG
using System.Linq;
#endif

namespace LibationFileManager;

public partial class Configuration : PropertyChangeFilter
{

	#region singleton stuff

	public static Configuration CreateMockInstance()
	{
#if !DEBUG
		if (!new System.Diagnostics.StackTrace().GetFrames().Select(f => f.GetMethod()?.DeclaringType?.Assembly.GetName().Name).Any(f => f?.EndsWith(".Tests") ?? false))
			throw new System.InvalidOperationException($"Can only mock {nameof(Configuration)} in Debug mode or in test assemblies.");
#endif

		var mockInstance = new Configuration() { JsonBackedDictionary = new EphemeralDictionary() };
		mockInstance.SetString("Light", "ThemeVariant");
		Instance = mockInstance;
		return mockInstance;
	}
	public static void RestoreSingletonInstance()
	{
		Instance = s_SingletonInstance;
	}
	private static readonly Configuration s_SingletonInstance = new();
	public static Configuration Instance { get; private set; } = s_SingletonInstance;
	public bool IsEphemeralInstance => JsonBackedDictionary is EphemeralDictionary;

	public Configuration CreateEphemeralCopy()
	{
		var copy = new Configuration();
		copy.LoadEphemeralSettings(Settings.GetJObject());
		return copy;
	}

	private Configuration() { }
	#endregion
}
