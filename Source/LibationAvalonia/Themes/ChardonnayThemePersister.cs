using Avalonia.Media;
using Dinah.Core.IO;
using FileManager;
using LibationFileManager;
using Newtonsoft.Json;
using System;

namespace LibationAvalonia.Themes;

public class ChardonnayThemePersister : JsonFilePersister<ChardonnayTheme>
{
	public static string jsonPath = System.IO.Path.Combine(Configuration.Instance.LibationFiles.Location, "ChardonnayTheme.json");

	public ChardonnayThemePersister(string path)
		: base(path, null) { }
	public ChardonnayThemePersister(ChardonnayTheme target, string path)
		: base(target, path, null) { }

	protected override JsonSerializerSettings GetSerializerSettings()
		=> new JsonSerializerSettings { Converters = { new ColorConverter() } };

	public static ChardonnayThemePersister? Create()
	{
		try
		{
			return System.IO.File.Exists(jsonPath)
				? new ChardonnayThemePersister(jsonPath)
				: new ChardonnayThemePersister(ChardonnayTheme.GetLiveTheme(), jsonPath);
		}
		catch (Exception ex)
		{
			try
			{
				Serilog.Log.Logger.Error(ex, $"Failed to open {jsonPath}. Overwriting with empty file.");
				FileUtility.SaferDelete(jsonPath);
				return new ChardonnayThemePersister(ChardonnayTheme.GetLiveTheme(), jsonPath);
			}
			catch (Exception ex2)
			{
				Serilog.Log.Logger.Error(ex2, $"Failed to open {jsonPath}.");
				return null;
			}
		}
	}

	/// <summary> Store colors as #ARGB values so that the json file is easier to manually edit </summary>
	private class ColorConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType) => objectType == typeof(Color);

		public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
			=> reader.Value is string hexColor && Color.TryParse(hexColor, out var color) ? color : default;

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			if (value is Color color)
				writer.WriteValue(color.ToString());
		}
	}
}
