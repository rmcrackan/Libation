using System.Linq;
using FileManager.NamingTemplate;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NamingTemplateTests
{
	class TemplateTag : ITemplateTag
	{
		public string TagName { get; init; }
	}

	class PropertyClass1
	{
		public string Item1 { get; set; }
		public string Item2 { get; set; }
		public string Item3 { get; set; }
		public int Int1 { get; set; }
		public bool Condition { get; set; }
	}

	class PropertyClass2
	{
		public string Item1 { get; set; }
		public string Item2 { get; set; }
		public string Item3 { get; set; }
		public string Item4 { get; set; }
		public bool Condition { get; set; }
	}
	class PropertyClass3
	{
		public string Item1 { get; set; }
		public string Item2 { get; set; }
		public string Item3 { get; set; }
		public string Item4 { get; set; }
		public ReferenceType RefType { get; set; }
		public int? Int2 { get; set; }
		public bool Condition { get; set; }
	}
	class ReferenceType
	{
		public override string ToString()
		{
			return nameof(ReferenceType);
		}
	}


	[TestClass]
	public class GetPortionFilename
	{
		PropertyTagCollection<PropertyClass1> props1 = new()
		{
			{ new TemplateTag { TagName = "item1" }, i => i.Item1 },
			{ new TemplateTag { TagName = "item2" }, i => i.Item2 },
			{ new TemplateTag { TagName = "item3" }, i => i.Item3 }
		};

		PropertyTagCollection<PropertyClass2> props2 = new()
		{
			{ new TemplateTag { TagName = "item1" }, i => i.Item1 },
			{ new TemplateTag { TagName = "item2" }, i => i.Item2 },
			{ new TemplateTag { TagName = "item3" }, i => i.Item3 },
			{ new TemplateTag { TagName = "item4" }, i => i.Item4 },
		};
		PropertyTagCollection<PropertyClass3> props3 = new(true, GetVal)
		{
			{ new TemplateTag { TagName = "item3_1" }, i => i.Item1 },
			{ new TemplateTag { TagName = "item3_2" }, i => i.Item2 },
			{ new TemplateTag { TagName = "item3_3" }, i => i.Item3 },
			{ new TemplateTag { TagName = "item3_4" }, i => i.Item4 },
			{ new TemplateTag { TagName = "reftype" }, i => i.RefType },
		};
		ConditionalTagCollection<PropertyClass1> conditional1 = new()
		{
			{ new TemplateTag { TagName = "ifc1" }, i => i.Condition },
		};
		ConditionalTagCollection<PropertyClass2> conditional2 = new()
		{
			{ new TemplateTag { TagName = "ifc2" }, i => i.Condition },
		};
		ConditionalTagCollection<PropertyClass3> conditional3 = new()
		{
			{ new TemplateTag { TagName = "ifc3" }, i => i.Condition },
		};

		PropertyClass1 propertyClass1 = new()
		{
			Item1 = "prop1_item1",
			Item2 = "prop1_item2",
			Item3 = "prop1_item3",
			Int1 = 55,
			Condition = true,
		};

		PropertyClass2 propertyClass2 = new()
		{
			Item1 = "prop2_item1",
			Item3 = "prop2_item3",
			Item4 = "prop2_item4",
			Condition = false
		};

		PropertyClass3 propertyClass3 = new()
		{
			Item1 = "prop3_item1",
			Item2 = "prop3_item2",
			Item3 = "Prop3_Item3",
			Item4 = "prop3_item4",
			Condition = true
		};


		[TestMethod]
		[DataRow("<item1>", "prop1_item1", 1)]
		[DataRow("< item1>", "< item1>", 0)]
		[DataRow("<item1 >", "<item1 >", 0)]
		[DataRow("< item1 >", "< item1 >", 0)]
		[DataRow("<item3_1>", "prop3_item1", 1)]
		[DataRow("<item1> <item2> <item3> <item4>", "prop1_item1 prop1_item2 prop1_item3 prop2_item4", 4)]
		[DataRow("<item3_1> <item3_2> <item3> <item4>", "prop3_item1 prop3_item2 prop1_item3 prop2_item4", 4)]
		[DataRow("<ifc1-><item1><-ifc1><ifc2-><item4><-ifc2><ifc3-><item3_2><-ifc3>", "prop1_item1prop3_item2", 3)]
		[DataRow("<ifc1-><ifc3-><item1><ifc2-><item4><-ifc2><item3_2><-ifc3><-ifc1>", "prop1_item1prop3_item2", 3)]
		[DataRow("<ifc2-><ifc1-><ifc3-><item1><item4><item3_2><-ifc3><-ifc1><-ifc2>", "", 3)]
		[DataRow("<!ifc2-><ifc1-><ifc3-><item1><item4><item3_2><-ifc3><-ifc1><-ifc2>", "prop1_item1prop2_item4prop3_item2", 3)]
		public void test(string inStr, string outStr, int numTags)
		{
			var template = NamingTemplate.Parse(inStr, new TagCollection[] { props1, props2, props3, conditional1, conditional2, conditional3 });

			template.TagsInUse.Should().HaveCount(numTags);
			template.Warnings.Should().HaveCount(numTags > 0 ? 0 : 1);
			template.Errors.Should().HaveCount(0);

			var templateText = string.Concat(template.Evaluate(propertyClass3, propertyClass2, propertyClass1).Select(v => v.Value));

			templateText.Should().Be(outStr);
		}

		[TestMethod]
		[DataRow("<ifc2-><ifc1-><ifc3-><item1><item4><item3_2><-ifc3><-ifc1><ifc2->", new string[] { "Missing <-ifc2> closing conditional.", "Missing <-ifc2> closing conditional." })]
		[DataRow("<ifc2-><ifc1-><ifc3-><-ifc3><-ifc1><-ifc2>", new string[] { "Should use tags. Eg: <title>" })]
		[DataRow("<ifc1-><ifc3-><item1><-ifc3><-ifc1><-ifc2>", new string[] { "Missing <ifc2-> open conditional." })]
		[DataRow("<ifc1-><ifc3-><-ifc3><-ifc1><-ifc2>", new string[] { "Missing <ifc2-> open conditional.", "Should use tags. Eg: <title>" })]
		[DataRow("<ifc2-><ifc1-><ifc3-><item1><item4><item3_2><-ifc3><-ifc1>", new string[] { "Missing <-ifc2> closing conditional." })]
		[DataRow("<ifc2-><ifc1-><ifc3-><item1><item4><item3_2><-ifc3>", new string[] { "Missing <-ifc1> closing conditional.", "Missing <-ifc2> closing conditional." })]
		[DataRow("<ifc2-><ifc1-><ifc3-><item1><item4>", new string[] { "Missing <-ifc3> closing conditional.", "Missing <-ifc1> closing conditional.", "Missing <-ifc2> closing conditional." })]
		[DataRow("<ifc2-><ifc1-><ifc3-><item1><item4><item3_2><-ifc1><-ifc2>", new string[] { "Missing <-ifc3> closing conditional.", "Missing <-ifc3> closing conditional.", "Missing <-ifc1> closing conditional.", "Missing <-ifc2> closing conditional." })]
		public void condition_error(string inStr, string[] warnings)
		{
			var template = NamingTemplate.Parse(inStr, new TagCollection[] { props1, props2, props3, conditional1, conditional2, conditional3 });

			template.Errors.Should().HaveCount(0);
			template.Warnings.Should().BeEquivalentTo(warnings);
		}

		static string GetVal(ITemplateTag templateTag, ReferenceType referenceType, string format)
		{
			return "";
		}

		[TestMethod]
		[DataRow("<int1>", "55")]
		[DataRow("<int1[]>", "55")]
		[DataRow("<int1[5]>", "00055")]
		[DataRow("<int2>", "")]
		[DataRow("<int2[]>", "")]
		[DataRow("<int2[4]>", "")]
		[DataRow("<item3_format>", "Prop3_Item3")]
		[DataRow("<item3_format[]>", "Prop3_Item3")]
		[DataRow("<item3_format[rtreue5]>", "Prop3_Item3")]
		[DataRow("<item3_format[l]>", "prop3_item3")]
		[DataRow("<item3_format[u]>", "PROP3_ITEM3")]
		[DataRow("<item2_2_null>", "")]
		[DataRow("<item2_2_null[]>", "")]
		[DataRow("<item2_2_null[l]>", "")]
		[DataRow("<reftype[l]>", "")]
		public void formatting(string inStr, string outStr)
		{
			props1.Add(new TemplateTag { TagName = "int1" }, i => i.Int1, formatInt);
			props3.Add(new TemplateTag { TagName = "int2" }, i => i.Int2, formatInt);
			props3.Add(new TemplateTag { TagName = "item3_format" }, i => i.Item3, formatString);
			props2.Add(new TemplateTag { TagName = "item2_2_null" }, i => i.Item2, formatString);

			var template = NamingTemplate.Parse(inStr, new TagCollection[] { props1, props2, props3, conditional1, conditional2, conditional3 });

			template.Warnings.Should().HaveCount(0);
			template.Errors.Should().HaveCount(0);

			var templateText = string.Concat(template.Evaluate(propertyClass3, propertyClass2, propertyClass1).Select(v => v.Value));

			templateText.Should().Be(outStr);

			string formatInt(ITemplateTag templateTag, int value, string format)
			{
				if (int.TryParse(format, out var numDecs))
					return value.ToString($"D{numDecs}");
				return value.ToString();
			}

			string formatString(ITemplateTag templateTag, string value, string formatString)
			{
				if (string.Compare(formatString, "u", ignoreCase: true) == 0) return value?.ToUpper();
				else if (string.Compare(formatString, "l", ignoreCase: true) == 0) return value?.ToLower();
				else return value;
			}
		}
	}
}
