using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#nullable enable
namespace FileManager
{
	public record Replacement
	{
		public const int FIXED_COUNT = 6;

		internal const char QUOTE_MARK = '"';
		[JsonIgnore] public bool Mandatory { get; set; }
		[JsonProperty] public char CharacterToReplace { get; private set; }
		[JsonProperty] public string ReplacementString { get; private set; }
		[JsonProperty] public string Description { get; set; }
		public override string ToString() => $"{CharacterToReplace} → {ReplacementString} ({Description})";

		public Replacement(char charToReplace, string replacementString, string description)
		{
			CharacterToReplace = charToReplace;
			ReplacementString = replacementString;
			Description = description;
		}
		private Replacement(char charToReplace, string replacementString, string description, bool mandatory)
			: this(charToReplace, replacementString, description)
		{
			Mandatory = mandatory;
		}

		public void Update(char charToReplace, string replacementString, string description)
		{
			ReplacementString = replacementString;

			if (!Mandatory)
			{
				CharacterToReplace = charToReplace;
				Description = description;
			}
		}

		public static Replacement OtherInvalid(string replacement) => new(default, replacement, "All other invalid characters", true);
		public static Replacement FilenameForwardSlash(string replacement) => new('/', replacement, "Forward Slash (Filename Only)", true);
		public static Replacement FilenameBackSlash(string replacement) => new('\\', replacement, "Back Slash (Filename Only)", true);
		public static Replacement OpenQuote(string replacement) => new('"', replacement, "Open Quote", true);
		public static Replacement CloseQuote(string replacement) => new('"', replacement, "Close Quote", true);
		public static Replacement OtherQuote(string replacement) => new('"', replacement, "Other Quote", true);
		public static Replacement Colon(string replacement) => new(':', replacement, "Colon");
		public static Replacement Asterisk(string replacement) => new('*', replacement, "Asterisk");
		public static Replacement QuestionMark(string replacement) => new('?', replacement, "Question Mark");
		public static Replacement OpenAngleBracket(string replacement) => new('<', replacement, "Open Angle Bracket");
		public static Replacement CloseAngleBracket(string replacement) => new('>', replacement, "Close Angle Bracket");
		public static Replacement Pipe(string replacement) => new('|', replacement, "Vertical Line");

	}

	[JsonConverter(typeof(ReplacementCharactersConverter))]
	public class ReplacementCharacters
	{
		public override bool Equals(object? obj)
		{
			if (obj is ReplacementCharacters second && Replacements.Count == second.Replacements.Count)
			{
				for (int i = 0; i < Replacements.Count; i++)
					if (Replacements[i] != second.Replacements[i])
						return false;

				return true;
			}
			return false;
		}
		public override int GetHashCode() => Replacements.GetHashCode();

		public static readonly ReplacementCharacters Default
			= IsWindows
			? new()
			{
				Replacements = new Replacement[]
				{
					Replacement.OtherInvalid("_"),
					Replacement.FilenameForwardSlash("∕"),
					Replacement.FilenameBackSlash(""),
					Replacement.OpenQuote("“"),
					Replacement.CloseQuote("”"),
					Replacement.OtherQuote("＂"),
					Replacement.OpenAngleBracket("＜"),
					Replacement.CloseAngleBracket("＞"),
					Replacement.Colon(";"),
					Replacement.Asterisk("✱"),
					Replacement.QuestionMark("？"),
					Replacement.Pipe("⏐"),
				}
			}
			: new()
			{
				Replacements = new Replacement[]
				{
					Replacement.OtherInvalid("_"),
					Replacement.FilenameForwardSlash("∕"),
					Replacement.FilenameBackSlash("\\"),
					Replacement.OpenQuote("“"),
					Replacement.CloseQuote("”"),
					Replacement.OtherQuote("\"")
				}
			};

		public static readonly ReplacementCharacters LoFiDefault
			= IsWindows
			? new()
			{
				Replacements = new Replacement[]
				{
					Replacement.OtherInvalid("_"),
					Replacement.FilenameForwardSlash("_"),
					Replacement.FilenameBackSlash("_"),
					Replacement.OpenQuote("'"),
					Replacement.CloseQuote("'"),
					Replacement.OtherQuote("'"),
					Replacement.OpenAngleBracket("{"),
					Replacement.CloseAngleBracket("}"),
					Replacement.Colon("-"),
				}
			}
			: new ()
			{
				Replacements = new Replacement[]
				{
					Replacement.OtherInvalid("_"),
					Replacement.FilenameForwardSlash("_"),
					Replacement.FilenameBackSlash("\\"),
					Replacement.OpenQuote("\""),
					Replacement.CloseQuote("\""),
					Replacement.OtherQuote("\"")
				}
			};

		public static readonly ReplacementCharacters Barebones
			= IsWindows
			? new ()
			{
				Replacements = new Replacement[]
				{
					Replacement.OtherInvalid("_"),
					Replacement.FilenameForwardSlash("_"),
					Replacement.FilenameBackSlash("_"),
					Replacement.OpenQuote("_"),
					Replacement.CloseQuote("_"),
					Replacement.OtherQuote("_")
				}
			}
			: new ()
			{
				Replacements = new Replacement[]
				{
					Replacement.OtherInvalid("_"),
					Replacement.FilenameForwardSlash("_"),
					Replacement.FilenameBackSlash("\\"),
					Replacement.OpenQuote("\""),
					Replacement.CloseQuote("\""),
					Replacement.OtherQuote("\"")
				}
			};

		private static bool IsWindows => Environment.OSVersion.Platform is PlatformID.Win32NT;

		private static readonly char[] invalidPathChars = Path.GetInvalidFileNameChars().Except(new[] {
				Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar
			}).ToArray();

		private static readonly char[] invalidSlashes = Path.GetInvalidFileNameChars().Intersect(new[] {
				Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar
			}).ToArray();

		required public IReadOnlyList<Replacement> Replacements { get; init; }
		private string DefaultReplacement => Replacements[0].ReplacementString;
		private Replacement ForwardSlash => Replacements[1];
		private Replacement BackSlash => Replacements[2];
		private string OpenQuote => Replacements[3].ReplacementString;
		private string CloseQuote => Replacements[4].ReplacementString;
		private string OtherQuote => Replacements[5].ReplacementString;

		private string GetFilenameCharReplacement(char toReplace, char preceding, char succeding)
		{
			if (toReplace == ForwardSlash.CharacterToReplace)
				return ForwardSlash.ReplacementString;
			else if (toReplace == BackSlash.CharacterToReplace)
				return BackSlash.ReplacementString;
			else return GetPathCharReplacement(toReplace, preceding, succeding);
		}
		private string GetPathCharReplacement(char toReplace, char preceding, char succeding)
		{
			if (toReplace == Replacement.QUOTE_MARK)
			{
				if (
					preceding == default ||
						(
							!char.IsLetter(preceding) &&
							!char.IsNumber(preceding) &&
							(char.IsLetter(succeding) || char.IsNumber(succeding))
						)
					)
					return OpenQuote;
				else if (
						succeding == default ||
							(
								!char.IsLetter(succeding) &&
								!char.IsNumber(succeding) &&
								(char.IsLetter(preceding) || char.IsNumber(preceding))
							)
						)
					return CloseQuote;
				else
					return OtherQuote;
			}

			if (!IsWindows && toReplace == BackSlash.CharacterToReplace)
				return BackSlash.ReplacementString;

			//Replace any other non-mandatory characters
			for (int i = Replacement.FIXED_COUNT; i < Replacements.Count; i++)
			{
				var r = Replacements[i];
				if (r.CharacterToReplace == toReplace)
					return r.ReplacementString;
			}
			return DefaultReplacement;
		}

		public static bool ContainsInvalidPathChar(string path)
			=> path.Any(c => invalidPathChars.Contains(c));
		public static bool ContainsInvalidFilenameChar(string path)
			=> ContainsInvalidPathChar(path) || path.Any(c => invalidSlashes.Contains(c));

		public string ReplaceFilenameChars(string fileName)
		{
			if (string.IsNullOrEmpty(fileName)) return string.Empty;
			var builder = new System.Text.StringBuilder();
			for (var i = 0; i < fileName.Length; i++)
			{
				var c = fileName[i];

				if (invalidPathChars.Contains(c)
					|| invalidSlashes.Contains(c)
					|| Replacements.Any(r => r.CharacterToReplace == c) /* Replace any other legal characters that they user wants. */ )
				{
					char preceding = i > 0 ? fileName[i - 1] : default;
					char succeeding = i < fileName.Length - 1 ? fileName[i + 1] : default;
					builder.Append(GetFilenameCharReplacement(c, preceding, succeeding));
				}
				else
					builder.Append(c);
			}
			return builder.ToString();
		}

		public string ReplacePathChars(string pathStr)
		{
			if (string.IsNullOrEmpty(pathStr)) return string.Empty;

			var builder = new System.Text.StringBuilder();
			for (var i = 0; i < pathStr.Length; i++)
			{
				var c = pathStr[i];

				if (
					(
						invalidPathChars.Contains(c)
						|| (	// Replace any other legal characters that they user wants.
								c != Path.DirectorySeparatorChar
								&& c != Path.AltDirectorySeparatorChar
								&& Replacements.Any(r => r.CharacterToReplace == c)
							)
					)
					&& !(	// replace all colons except drive letter designator on Windows
							c == ':'
							&& i == 1
							&& Path.IsPathRooted(pathStr)
							&& IsWindows
					)
				)
				{
						char preceding = i > 0 ? pathStr[i - 1] : default;
						char succeeding = i < pathStr.Length - 1 ? pathStr[i + 1] : default;
						builder.Append(GetPathCharReplacement(c, preceding, succeeding));
				}
				else
					builder.Append(c);
			}
			return builder.ToString();
		}
	}

	#region JSON Converter
	internal class ReplacementCharactersConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
			=> objectType == typeof(ReplacementCharacters);

		public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			var jObj = JObject.Load(reader);
			var replaceArr = jObj[nameof(Replacement)];
			var dict
				= replaceArr?.ToObject<Replacement[]>()?.ToList()
				?? ReplacementCharacters.Default.Replacements;


			//Ensure that the first 6 replacements are for the expected chars and that all replacement strings are valid.
			//If not, reset to default.

			for (int i = 0; i < Replacement.FIXED_COUNT; i++)
			{
				if (dict.Count < Replacement.FIXED_COUNT
					|| dict[i].CharacterToReplace != ReplacementCharacters.Barebones.Replacements[i].CharacterToReplace
					|| dict[i].Description != ReplacementCharacters.Barebones.Replacements[i].Description)
				{
					dict = ReplacementCharacters.Default.Replacements;
					break;
				}

				//First FIXED_COUNT are mandatory
				dict[i].Mandatory = true;
			}

			return new ReplacementCharacters { Replacements = dict };
		}

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			if (value is not ReplacementCharacters replacements)
				return;

			var propertyNames = replacements.Replacements
				.Select(JObject.FromObject).ToList();

			var prop = new JProperty(nameof(Replacement), new JArray(propertyNames));

			var obj = new JObject();
			obj.AddFirst(prop);
			obj.WriteTo(writer);
		}
	}
	#endregion
}
