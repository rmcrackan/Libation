using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileManager
{
	public class Replacement : ICloneable
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

		public object Clone() => new Replacement(CharacterToReplace, ReplacementString, Description, Mandatory);

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
		public static readonly ReplacementCharacters Default = new()
		{
			Replacements = new List<Replacement>()
			{
				Replacement.OtherInvalid("_"),
				Replacement.FilenameForwardSlash("∕"),
				Replacement.FilenameBackSlash(""),
				Replacement.OpenQuote("“"),
				Replacement.CloseQuote("”"),
				Replacement.OtherQuote("＂"),
				Replacement.OpenAngleBracket("＜"),
				Replacement.CloseAngleBracket("＞"),
				Replacement.Colon("꞉"),
				Replacement.Asterisk("✱"),
				Replacement.QuestionMark("？"),
				Replacement.Pipe("⏐"),
			}
		};

		public static readonly ReplacementCharacters LoFiDefault = new()
		{
			Replacements = new List<Replacement>()
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
		};

		public static readonly ReplacementCharacters Barebones = new()
		{
			Replacements = new List<Replacement>()
			{
				Replacement.OtherInvalid("_"),
				Replacement.FilenameForwardSlash("_"),
				Replacement.FilenameBackSlash("_"),
				Replacement.OpenQuote("_"),
				Replacement.CloseQuote("_"),
				Replacement.OtherQuote("_"),
			}
		};

		private static readonly char[] invalidChars = Path.GetInvalidPathChars().Union(new[] {
				'*', '?', ':',
				// these are weird. If you run Path.GetInvalidPathChars() in Visual Studio's "C# Interactive", then these characters are included.
				// In live code, Path.GetInvalidPathChars() does not include them
				'"', '<', '>'
			}).ToArray();

		public IReadOnlyList<Replacement> Replacements { get; init; }
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

			for (int i = Replacement.FIXED_COUNT; i < Replacements.Count; i++)
			{
				var r = Replacements[i];
				if (r.CharacterToReplace == toReplace)
					return r.ReplacementString;
			}
			return DefaultReplacement;
		}


		public static bool ContainsInvalidPathChar(string path)
			=> path.Any(c => invalidChars?.Contains(c) == true);
		public static bool ContainsInvalidFilenameChar(string path)
			=> path.Any(c => invalidChars?.Concat(new char[] { '\\', '/' })?.Contains(c) == true);

		public string ReplaceInvalidFilenameChars(string fileName)
		{
			if (string.IsNullOrEmpty(fileName)) return string.Empty;
			var builder = new System.Text.StringBuilder();
			for (var i = 0; i < fileName.Length; i++)
			{
				var c = fileName[i];

				if (invalidChars.Contains(c) || c == ForwardSlash.CharacterToReplace || c == BackSlash.CharacterToReplace)
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

		public string ReplaceInvalidPathChars(string pathStr)
		{
			if (string.IsNullOrEmpty(pathStr)) return string.Empty;

			// replace all colons except within the first 2 chars
			var builder = new System.Text.StringBuilder();
			for (var i = 0; i < pathStr.Length; i++)
			{
				var c = pathStr[i];

				if (!invalidChars.Contains(c) || (c == ':' && i == 1 && Path.IsPathRooted(pathStr)))
					builder.Append(c);
				else
				{
					char preceding = i > 0 ? pathStr[i - 1] : default;
					char succeeding = i < pathStr.Length - 1 ? pathStr[i + 1] : default;
					builder.Append(GetPathCharReplacement(c, preceding, succeeding));
				}

			}
			return builder.ToString();
		}
	}

	#region JSON Converter
	internal class ReplacementCharactersConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
			=> objectType == typeof(ReplacementCharacters);

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var jObj = JObject.Load(reader);
			var replaceArr = jObj[nameof(Replacement)];
			IReadOnlyList<Replacement> dict = replaceArr
				.ToObject<Replacement[]>().ToList();

			//Ensure that the first 6 replacements are for the expected chars and that all replacement strings are valid.
			//If not, reset to default.

			var default0 = Replacement.OtherInvalid("");
			var default1 = Replacement.FilenameForwardSlash("");
			var default2 = Replacement.FilenameBackSlash("");
			var default3 = Replacement.OpenQuote("");
			var default4 = Replacement.CloseQuote("");
			var default5 = Replacement.OtherQuote("");

			if (dict.Count < Replacement.FIXED_COUNT ||
				dict[0].CharacterToReplace != default0.CharacterToReplace || dict[0].Description != default0.Description ||
				dict[1].CharacterToReplace != default1.CharacterToReplace || dict[1].Description != default1.Description ||
				dict[2].CharacterToReplace != default2.CharacterToReplace || dict[2].Description != default2.Description ||
				dict[3].CharacterToReplace != default3.CharacterToReplace || dict[3].Description != default3.Description ||
				dict[4].CharacterToReplace != default4.CharacterToReplace || dict[4].Description != default4.Description ||
				dict[5].CharacterToReplace != default5.CharacterToReplace || dict[5].Description != default5.Description ||
				dict.Any(r => ReplacementCharacters.ContainsInvalidPathChar(r.ReplacementString))
				)
			{
				dict = ReplacementCharacters.Default.Replacements;
			}
			//First FIXED_COUNT are mandatory
			for (int i = 0; i < Replacement.FIXED_COUNT; i++)
				dict[i].Mandatory = true;

			return new ReplacementCharacters { Replacements = dict };
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			ReplacementCharacters replacements = (ReplacementCharacters)value;

			var propertyNames = replacements.Replacements
				.Select(c => JObject.FromObject(c)).ToList();

			var prop = new JProperty(nameof(Replacement), new JArray(propertyNames));

			var obj = new JObject();
			obj.AddFirst(prop);
			obj.WriteTo(writer);
		}
	}
	#endregion
}
