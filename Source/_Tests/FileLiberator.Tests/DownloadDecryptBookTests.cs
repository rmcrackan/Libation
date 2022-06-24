using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AudibleApi.Common;
using Dinah.Core;
using FileManager;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace FileLiberator.Tests
{
	[TestClass]
	public class DownloadDecryptBookTests
	{
		[TestMethod]
		public void HierarchicalChapters_Flatten()
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
					LengthMs = 100000,
				}
			};

			var hierarchicalChapters = new Chapter[]
				{
					new()
					{
						Title = "Book 1",
						StartOffsetMs = 0,
						StartOffsetSec = 0,
						LengthMs = 2000,
						Chapters = new Chapter[]
						{
							new()
							{   Title = "Part 1",
								StartOffsetMs = 2000,
								StartOffsetSec = 2,
								LengthMs = 2000,
								Chapters = new Chapter[]
								{
									new()
									{   Title = "Chapter 1",
										StartOffsetMs = 4000,
										StartOffsetSec = 4,
										LengthMs = 96000,
									},
									new()
									{   Title = "Chapter 2",
										StartOffsetMs = 100000,
										StartOffsetSec = 100,
										LengthMs = 100000,
									},
								}
							},
							new()
							{   Title = "Part 2",
								StartOffsetMs = 200000,
								StartOffsetSec = 200,
								LengthMs = 2000,
								Chapters = new Chapter[]
								{
									new()
									{   Title = "Chapter 3",
										StartOffsetMs = 202000,
										StartOffsetSec = 202,
										LengthMs = 98000,
									},
									new()
									{   Title = "Chapter 4",
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
							{   Title = "Part 3",
								StartOffsetMs = 402000,
								StartOffsetSec = 402,
								LengthMs = 2000,
								Chapters = new Chapter[]
								{
									new()
									{   Title = "Chapter 5",
										StartOffsetMs = 404000,
										StartOffsetSec = 404,
										LengthMs = 96000,
									},
									new()
									{   Title = "Chapter 6",
										StartOffsetMs = 500000,
										StartOffsetSec = 500,
										LengthMs = 100000,
									},
								}
							},
							new()
							{   Title = "Part 4",
								StartOffsetMs = 600000,
								StartOffsetSec = 600,
								LengthMs = 2000,
								Chapters = new Chapter[]
								{
									new()
									{   Title = "Chapter 7",
										StartOffsetMs = 602000,
										StartOffsetSec = 602,
										LengthMs = 98000,
									},
									new()
									{   Title = "Chapter 8",
										StartOffsetMs = 700000,
										StartOffsetSec = 700,
										LengthMs = 100000,
									},
								}
							}
						}
					}
				};

			var flatChapters = DownloadDecryptBook.getChapters(hierarchicalChapters).OrderBy(c => c.StartOffsetMs).ToArray();

			flatChapters.Length.Should().Be(flatChapters.Length);

			for (int i = 0; i < flatChapters.Length; i++)
			{
				flatChapters[i].Title.Should().Be(expected[i].Title);
				flatChapters[i].StartOffsetMs.Should().Be(expected[i].StartOffsetMs);
				flatChapters[i].StartOffsetSec.Should().Be(expected[i].StartOffsetSec);
				flatChapters[i].LengthMs.Should().Be(expected[i].LengthMs);
				flatChapters[i].Chapters.Should().BeNull();
			}
		}
	}
}
