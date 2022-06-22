using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FileManager
{
	public class Replacement
	{
		[JsonIgnore]
		public bool Mandatory { get; set; }
		[JsonProperty]
		public char CharacterToReplace { get; init; }
		[JsonProperty]
		public string ReplacementString { get; set; }
		[JsonProperty]
		public string Description { get; set; }

		public Replacement Clone() => new()
			{
				Mandatory = Mandatory,
				CharacterToReplace = CharacterToReplace,
				ReplacementString = ReplacementString,
				Description = Description
			};

		public override string ToString() => $"{CharacterToReplace} → {ReplacementString} ({Description})";

		public static Replacement Colon(string replacement) => new Replacement { CharacterToReplace = ':', Description = "Colon", ReplacementString = replacement};
		public static Replacement Asterisk(string replacement) => new Replacement { CharacterToReplace = '*', Description = "Asterisk", ReplacementString = replacement };
		public static Replacement QuestionMark(string replacement) => new Replacement { CharacterToReplace = '?', Description = "Question Mark", ReplacementString = replacement };
		public static Replacement OpenAngleBracket(string replacement) => new Replacement { CharacterToReplace = '<', Description = "Open Angle Bracket", ReplacementString = replacement };
		public static Replacement CloseAngleBracket(string replacement) => new Replacement { CharacterToReplace = '>', Description = "Close Angle Bracket", ReplacementString = replacement };
		public static Replacement OpenQuote(string replacement) => new Replacement { CharacterToReplace = '"', Description = "Open Quote", ReplacementString = replacement };
		public static Replacement CloseQuote(string replacement) => new Replacement { CharacterToReplace = '"', Description = "Close Quote", ReplacementString = replacement };
		public static Replacement OtherQuote(string replacement) => new Replacement { CharacterToReplace = '"', Description = "Other Quote", ReplacementString = replacement };
		public static Replacement Pipe(string replacement) => new Replacement { CharacterToReplace = '|', Description = "Vertical Line", ReplacementString = replacement };
		public static Replacement OtherInvalid(string replacement) => new Replacement { CharacterToReplace = default, Description = "Any other invalid characters", ReplacementString = replacement };
	}

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

			//Add any missing defaults and ensure they are in the expected order.
			for (int i = 0; i < ReplacementCharacters.Default.Replacements.Count; i++)
			{
				var rep = ReplacementCharacters.Default.Replacements[i].Clone();

				if (i < dict.Count)
				{
					var replacementStr = dict[i].ReplacementString;
					dict[i] = rep;
					dict[i].ReplacementString = replacementStr;
				}
				else
				{
					dict.Insert(i, rep);
				}
			}

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

	[JsonConverter(typeof(ReplacementCharactersConverter))]
	public class ReplacementCharacters
	{
		public static readonly ReplacementCharacters Default = new()
		{
			Replacements = new()
			{
				Replacement.OtherInvalid("_"),
				Replacement.OpenQuote("“"),
				Replacement.CloseQuote("”"),
				Replacement.OtherQuote("＂"),
				Replacement.Colon("꞉"),
				Replacement.Asterisk("✱"),
				Replacement.QuestionMark("？"),
				Replacement.OpenAngleBracket("＜"),
				Replacement.CloseAngleBracket("＞"),
				Replacement.Pipe("⏐"),
			}			
		};
		
		public static readonly ReplacementCharacters LoFiDefault = new()
		{
			Replacements = new()
			{
				Replacement.OtherInvalid("_"),
				Replacement.OpenQuote("'"),
				Replacement.CloseQuote("'"),
				Replacement.OtherQuote("'"),
				Replacement.Colon("-"),
				Replacement.Asterisk(""),
				Replacement.QuestionMark(""),
				Replacement.OpenAngleBracket("["),
				Replacement.CloseAngleBracket("]"),
				Replacement.Pipe("_"),
			}
		};

		public List<Replacement> Replacements { get; init; }
		public string DefaultReplacement => Replacements[0].ReplacementString;
		public string OpenQuote => Replacements[1].ReplacementString;
		public string CloseQuote => Replacements[2].ReplacementString;
		public string OtherQuote => Replacements[3].ReplacementString;

		private const char QuoteMark = '"';

		public string GetReplacement(char toReplace, char preceding, char succeding)
		{
			if (toReplace == QuoteMark)
			{
				if (preceding != default && !char.IsLetter(preceding) && !char.IsNumber(preceding))
					return OpenQuote;
				else if (succeding != default && !char.IsLetter(succeding) && !char.IsNumber(succeding))
					return CloseQuote;
				else
					return OtherQuote;
			}

			for (int i = 4; i < Replacements.Count; i++)
			{
				var r = Replacements[i];
				if (r.CharacterToReplace == toReplace)
					return r.ReplacementString;
			}
			return DefaultReplacement;
		}
	}
}
