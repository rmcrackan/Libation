using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileManager
{
	public class Replacement
	{
		public const int FIXED_COUNT = 4;

		internal const char QUOTE_MARK = '"';
		internal const string DEFAULT_DESCRIPTION = "Any other invalid characters";
		internal const string OPEN_QUOTE_DESCRIPTION = "Open Quote";
		internal const string CLOSE_QUOTE_DESCRIPTION = "Close Quote";
		internal const string OTHER_QUOTE_DESCRIPTION = "Other Quote";
		[JsonIgnore] public bool Mandatory { get; internal set; }
		[JsonProperty] public char CharacterToReplace { get; private set; }
		[JsonProperty] public string ReplacementString { get; private set; }
		[JsonProperty] public string Description { get; private set; }
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

		public static Replacement OtherInvalid(string replacement) => new(default, replacement, DEFAULT_DESCRIPTION, true);
		public static Replacement OpenQuote(string replacement) => new('"', replacement, OPEN_QUOTE_DESCRIPTION, true);
		public static Replacement CloseQuote(string replacement) => new('"', replacement, CLOSE_QUOTE_DESCRIPTION, true);
		public static Replacement OtherQuote(string replacement) => new('"', replacement, OTHER_QUOTE_DESCRIPTION, true);
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
				Replacement.OpenQuote("'"),
				Replacement.CloseQuote("'"),
				Replacement.OtherQuote("'"),
				Replacement.OpenAngleBracket("{"),
				Replacement.CloseAngleBracket("}"),
				Replacement.Colon("-"),
				Replacement.Asterisk(""),
				Replacement.QuestionMark(""),
			}
		};

		public static readonly ReplacementCharacters Minimum = new()
		{
			Replacements = new List<Replacement>()
			{
				Replacement.OtherInvalid("_"),
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
		private string OpenQuote => Replacements[1].ReplacementString;
		private string CloseQuote => Replacements[2].ReplacementString;
		private string OtherQuote => Replacements[3].ReplacementString;

		private string GetReplacement(char toReplace, char preceding, char succeding)
		{
			if (toReplace == Replacement.QUOTE_MARK)
			{
				if (
					preceding != default
					&& !char.IsLetter(preceding)
					&& !char.IsNumber(preceding)
					&& (char.IsLetter(succeding) || char.IsNumber(succeding))
					)
					return OpenQuote;
				else if (
					succeding != default
					&& !char.IsLetter(succeding)
					&& !char.IsNumber(succeding)
					&& (char.IsLetter(preceding) || char.IsNumber(preceding))
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

		public static bool ContainsInvalid(string path)
			=> path.Any(c => invalidChars.Contains(c));

		public string ReplaceInvalidChars(string pathStr)
		{
			// replace all colons except within the first 2 chars
			var builder = new System.Text.StringBuilder();
			for (var i = 0; i < pathStr.Length; i++)
			{
				var c = pathStr[i];

				if (!invalidChars.Contains(c) || (i <= 2 && Path.IsPathRooted(pathStr)))
					builder.Append(c);
				else
				{
					char preceding = i > 0 ? pathStr[i - 1] : default;
					char succeeding = i < pathStr.Length - 1 ? pathStr[i + 1] : default;
					builder.Append(GetReplacement(c, preceding, succeeding));
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
			var dict = replaceArr
				.ToObject<Replacement[]>().ToList();

			//Ensure that the first 4 replacements are for the expected chars and that all replacement strings are valid.
			//If not, reset to default.
			if (dict.Count < Replacement.FIXED_COUNT ||
				dict[0].CharacterToReplace != default || dict[0].Description != Replacement.DEFAULT_DESCRIPTION ||
				dict[1].CharacterToReplace != Replacement.QUOTE_MARK || dict[1].Description != Replacement.OPEN_QUOTE_DESCRIPTION ||
				dict[2].CharacterToReplace != Replacement.QUOTE_MARK || dict[2].Description != Replacement.CLOSE_QUOTE_DESCRIPTION ||
				dict[3].CharacterToReplace != Replacement.QUOTE_MARK || dict[3].Description != Replacement.OTHER_QUOTE_DESCRIPTION ||
				dict.Any(r => ReplacementCharacters.ContainsInvalid(r.ReplacementString))
				)
			{
				dict = ReplacementCharacters.Default.Replacements;
			}
			//First 4 are mandatory
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
