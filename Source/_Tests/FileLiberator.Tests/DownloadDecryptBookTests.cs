using System.Collections.Generic;
using AssertionHelper;
using AudibleApi.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileLiberator.Tests
{
	[TestClass]
	public class DownloadDecryptBookTests
	{
		private static Chapter[] HierarchicalChapters => new Chapter[]
		{
			new ()
			{
				Title = "Opening Credits",
				StartOffsetMs = 0,
				StartOffsetSec = 0,
				LengthMs = 10000,
			},
			new ()
			{
				Title = "Book 1",
				StartOffsetMs = 10000,
				StartOffsetSec = 10,
				LengthMs = 2000,
				Chapters = new Chapter[]
				{
					new ()
					{
						Title = "Part 1",
						StartOffsetMs = 12000,
						StartOffsetSec = 12,
						LengthMs = 2000,
						Chapters = new Chapter[]
						{
							new ()
							{
								Title = "Chapter 1",
								StartOffsetMs = 14000,
								StartOffsetSec = 14,
								LengthMs = 86000,
							},
							new()
							{
								Title = "Chapter 2",
								StartOffsetMs = 100000,
								StartOffsetSec = 100,
								LengthMs = 100000,
							},
						}
					},
					new()
					{
						Title = "Part 2",
						StartOffsetMs = 200000,
						StartOffsetSec = 200,
						LengthMs = 2000,
						Chapters = new Chapter[]
						{
							new()
							{
								Title = "Chapter 3",
								StartOffsetMs = 202000,
								StartOffsetSec = 202,
								LengthMs = 98000,
							},
							new()
							{
								Title = "Chapter 4",
								StartOffsetMs = 300000,
								StartOffsetSec = 300,
								LengthMs = 100000,
							},
						}
					}
				}
			},
			new()
			{
				Title = "Book 2",
				StartOffsetMs = 400000,
				StartOffsetSec = 400,
				LengthMs = 2000,
				Chapters = new Chapter[]
				{
					new()
					{
						Title = "Part 3",
						StartOffsetMs = 402000,
						StartOffsetSec = 402,
						LengthMs = 2000,
						Chapters = new Chapter[]
						{
							new()
							{
								Title = "Chapter 5",
								StartOffsetMs = 404000,
								StartOffsetSec = 404,
								LengthMs = 96000,
							},
							new()
							{
								Title = "Chapter 6",
								StartOffsetMs = 500000,
								StartOffsetSec = 500,
								LengthMs = 100000,
							},
						}
					},
					new()
					{
						Title = "Part 4",
						StartOffsetMs = 600000,
						StartOffsetSec = 600,
						LengthMs = 2000,
						Chapters = new Chapter[]
						{
							new()
							{
								Title = "Chapter 7",
								StartOffsetMs = 602000,
								StartOffsetSec = 602,
								LengthMs = 98000,
							},
							new()
							{
								Title = "Chapter 8",
								StartOffsetMs = 700000,
								StartOffsetSec = 700,
								LengthMs = 100000,
							},
						}
					}
				}
			},
			new()
			{
				Title = "End Credits",
				StartOffsetMs = 800000,
				StartOffsetSec = 800,
				LengthMs = 10000,
			},
		};


		private static Chapter[] HierarchicalChapters_LongerParents => new Chapter[]
		{
			new ()
			{
				Title = "Opening Credits",
				StartOffsetMs = 0,
				StartOffsetSec = 0,
				LengthMs = 10000,
			},
			new ()
			{
				Title = "Book 1",
				StartOffsetMs = 10000,
				StartOffsetSec = 10,
				LengthMs = 15000,
				Chapters = new Chapter[]
				{
					new ()
					{
						Title = "Part 1",
						StartOffsetMs = 25000,
						StartOffsetSec = 25,
						LengthMs = 2000,
						Chapters = new Chapter[]
						{
							new ()
							{
								Title = "Chapter 1",
								StartOffsetMs = 27000,
								StartOffsetSec = 27,
								LengthMs = 73000,
							},
							new()
							{
								Title = "Chapter 2",
								StartOffsetMs = 100000,
								StartOffsetSec = 100,
								LengthMs = 100000,
							},
						}
					},
					new()
					{
						Title = "Part 2",
						StartOffsetMs = 200000,
						StartOffsetSec = 200,
						LengthMs = 2000,
						Chapters = new Chapter[]
						{
							new()
							{
								Title = "Chapter 3",
								StartOffsetMs = 202000,
								StartOffsetSec = 202,
								LengthMs = 98000,
							},
							new()
							{
								Title = "Chapter 4",
								StartOffsetMs = 300000,
								StartOffsetSec = 300,
								LengthMs = 100000,
							},
						}
					}
				}
			},
			new()
			{
				Title = "Book 2",
				StartOffsetMs = 400000,
				StartOffsetSec = 400,
				LengthMs = 2000,
				Chapters = new Chapter[]
				{
					new()
					{
						Title = "Part 3",
						StartOffsetMs = 402000,
						StartOffsetSec = 402,
						LengthMs = 20000,
						Chapters = new Chapter[]
						{
							new()
							{
								Title = "Chapter 5",
								StartOffsetMs = 422000,
								StartOffsetSec = 422,
								LengthMs = 78000,
							},
							new()
							{
								Title = "Chapter 6",
								StartOffsetMs = 500000,
								StartOffsetSec = 500,
								LengthMs = 100000,
							},
						}
					},
					new()
					{
						Title = "Part 4",
						StartOffsetMs = 600000,
						StartOffsetSec = 600,
						LengthMs = 2000,
						Chapters = new Chapter[]
						{
							new()
							{
								Title = "Chapter 7",
								StartOffsetMs = 602000,
								StartOffsetSec = 602,
								LengthMs = 98000,
							},
							new()
							{
								Title = "Chapter 8",
								StartOffsetMs = 700000,
								StartOffsetSec = 700,
								LengthMs = 100000,
							},
						}
					}
				}
			},
			new()
			{
				Title = "End Credits",
				StartOffsetMs = 800000,
				StartOffsetSec = 800,
				LengthMs = 10000,
			},
		};

		[TestMethod]
		public void Chapters_CombineCredits()
		{
			var expected = new Chapter[]
			{
				new()
				{
					Title = "Book 1: Part 1: Chapter 1",
					StartOffsetMs = 0,
					StartOffsetSec = 0,
					LengthMs = 100000,
				},
				new()
				{
					Title = "Book 1: Part 1: Chapter 2",
					StartOffsetMs = 100000,
					StartOffsetSec = 100,
					LengthMs = 100000,
				},
				new()
				{
					Title = "Book 1: Part 2: Chapter 3",
					StartOffsetMs = 200000,
					StartOffsetSec = 200,
					LengthMs = 100000,
				},
				new()
				{
					Title = "Book 1: Part 2: Chapter 4",
					StartOffsetMs = 300000,
					StartOffsetSec = 300,
					LengthMs = 100000,
				},
				new()
				{
					Title = "Book 2: Part 3: Chapter 5",
					StartOffsetMs = 400000,
					StartOffsetSec = 400,
					LengthMs = 100000,
				},
				new()
				{
					Title = "Book 2: Part 3: Chapter 6",
					StartOffsetMs = 500000,
					StartOffsetSec = 500,
					LengthMs = 100000,
				},
				new()
				{
					Title = "Book 2: Part 4: Chapter 7",
					StartOffsetMs = 600000,
					StartOffsetSec = 600,
					LengthMs = 100000,
				},
				new()
				{
					Title = "Book 2: Part 4: Chapter 8",
					StartOffsetMs = 700000,
					StartOffsetSec = 700,
					LengthMs = 110000,
				}
			};

			var flatChapters = DownloadOptions.flattenChapters(HierarchicalChapters);
			DownloadOptions.combineCredits(flatChapters);
			checkChapters(flatChapters, expected);
		}


		[TestMethod]
		public void HierarchicalChapters_Flatten()
		{
			var expected = new Chapter[]
			{
				new()
				{
					Title = "Opening Credits",
					StartOffsetMs = 0,
					StartOffsetSec = 0,
					LengthMs = 10000,
				},
				new()
				{
					Title = "Book 1: Part 1: Chapter 1",
					StartOffsetMs = 10000,
					StartOffsetSec = 10,
					LengthMs = 90000,
				},
				new()
				{
					Title = "Book 1: Part 1: Chapter 2",
					StartOffsetMs = 100000,
					StartOffsetSec = 100,
					LengthMs = 100000,
				},
				new()
				{
					Title = "Book 1: Part 2: Chapter 3",
					StartOffsetMs = 200000,
					StartOffsetSec = 200,
					LengthMs = 100000,
				},
				new()
				{
					Title = "Book 1: Part 2: Chapter 4",
					StartOffsetMs = 300000,
					StartOffsetSec = 300,
					LengthMs = 100000,
				},
				new()
				{
					Title = "Book 2: Part 3: Chapter 5",
					StartOffsetMs = 400000,
					StartOffsetSec = 400,
					LengthMs = 100000,
				},
				new()
				{
					Title = "Book 2: Part 3: Chapter 6",
					StartOffsetMs = 500000,
					StartOffsetSec = 500,
					LengthMs = 100000,
				},
				new()
				{
					Title = "Book 2: Part 4: Chapter 7",
					StartOffsetMs = 600000,
					StartOffsetSec = 600,
					LengthMs = 100000,
				},
				new()
				{
					Title = "Book 2: Part 4: Chapter 8",
					StartOffsetMs = 700000,
					StartOffsetSec = 700,
					LengthMs = 100000,
				},
				new()
				{
					Title = "End Credits",
					StartOffsetMs = 800000,
					StartOffsetSec = 800,
					LengthMs = 10000,
				}
			};
			
			var flatChapters = DownloadOptions.flattenChapters(HierarchicalChapters);

			checkChapters(flatChapters, expected);
		}

		[TestMethod]
		public void HierarchicalChapters_LongerParents_Flatten()
		{
			var expected = new Chapter[]
			{
				new()
				{
					Title = "Opening Credits",
					StartOffsetMs = 0,
					StartOffsetSec = 0,
					LengthMs = 10000,
				},
				new()
				{
					Title = "Book 1",
					StartOffsetMs = 10000,
					StartOffsetSec = 10,
					LengthMs = 15000,
				},
				new()
				{
					Title = "Book 1: Part 1: Chapter 1",
					StartOffsetMs = 25000,
					StartOffsetSec = 25,
					LengthMs = 75000,
				},
				new()
				{
					Title = "Book 1: Part 1: Chapter 2",
					StartOffsetMs = 100000,
					StartOffsetSec = 100,
					LengthMs = 100000,
				},
				new()
				{
					Title = "Book 1: Part 2: Chapter 3",
					StartOffsetMs = 200000,
					StartOffsetSec = 200,
					LengthMs = 100000,
				},
				new()
				{
					Title = "Book 1: Part 2: Chapter 4",
					StartOffsetMs = 300000,
					StartOffsetSec = 300,
					LengthMs = 100000,
				},
				new()
				{
					Title = "Book 2: Part 3",
					StartOffsetMs = 400000,
					StartOffsetSec = 400,
					LengthMs = 22000,
				},
				new()
				{
					Title = "Book 2: Part 3: Chapter 5",
					StartOffsetMs = 422000,
					StartOffsetSec = 422,
					LengthMs = 78000,
				},
				new()
				{
					Title = "Book 2: Part 3: Chapter 6",
					StartOffsetMs = 500000,
					StartOffsetSec = 500,
					LengthMs = 100000,
				},
				new()
				{
					Title = "Book 2: Part 4: Chapter 7",
					StartOffsetMs = 600000,
					StartOffsetSec = 600,
					LengthMs = 100000,
				},
				new()
				{
					Title = "Book 2: Part 4: Chapter 8",
					StartOffsetMs = 700000,
					StartOffsetSec = 700,
					LengthMs = 100000,
				},
				new()
				{
					Title = "End Credits",
					StartOffsetMs = 800000,
					StartOffsetSec = 800,
					LengthMs = 10000,
				}
			};
			
			var flatChapters = DownloadOptions.flattenChapters(HierarchicalChapters_LongerParents);

			checkChapters(flatChapters, expected);
		}

		private static void checkChapters(IList<Chapter> value, IList<Chapter> expected)
		{
			value.Count.Should().Be(expected.Count);

			for (int i = 0; i < value.Count; i++)
			{
				value[i].Title.Should().Be(expected[i].Title);
				value[i].StartOffsetMs.Should().Be(expected[i].StartOffsetMs);
				value[i].StartOffsetSec.Should().Be(expected[i].StartOffsetSec);
				value[i].LengthMs.Should().Be(expected[i].LengthMs);
			}
		}
	}
}
