using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dinah.Core;
using FileManager;
using Newtonsoft.Json.Linq;

#nullable enable
namespace LibationFileManager
{
	public partial class Configuration : PropertyChangeFilter
	{

		#region singleton stuff

		public static Configuration CreateMockInstance()
		{
#if !DEBUG
			if (!new StackTrace().GetFrames().Select(f => f.GetMethod()?.DeclaringType?.Assembly.GetName().Name).Any(f => f?.EndsWith(".Tests") ?? false))
				throw new InvalidOperationException($"Can only mock {nameof(Configuration)} in Debug mode or in test assemblies.");
#endif

			var mockInstance = new Configuration() { persistentDictionary = new MockPersistentDictionary() };
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


		private Configuration() { }
		#endregion
	}
}
