using System.Collections.Generic;

namespace LibationCli;

internal static class CliCommandGroups
{
	public static IReadOnlyList<CliCommandGroup> All { get; } =
	[
		new(
			"abs",
			"Audiobookshelf commands.",
			[
				new("upload", "abs-upload", "Upload liberated audiobooks to Audiobookshelf."),
			]),
	];
}
