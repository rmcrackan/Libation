using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LibationUiBase;

public enum LibationContributorType
{
	Contributor,
	Collaborator,
	Creator
}

public class LibationContributor
{
	public string Name { get; }
	public LibationContributorType Type { get; }
	public Uri Link { get; }

	public static IEnumerable<LibationContributor> PrimaryContributors
		=> Contributors.Where(c => c.Type is LibationContributorType.Creator or LibationContributorType.Collaborator);
	public static IEnumerable<LibationContributor> AdditionalContributors
		=> Contributors.Where(c => c.Type is LibationContributorType.Contributor);

	public static IReadOnlyList<LibationContributor> Contributors { get; }
		= new ReadOnlyCollection<LibationContributor>([
			GitHubUser("rmcrackan", LibationContributorType.Creator),
			GitHubUser("Mbucari", LibationContributorType.Collaborator),
			GitHubUser("pixil98"),
			GitHubUser("hutattedonmyarm"),
			GitHubUser("seanke"),
			GitHubUser("wtanksleyjr"),
			GitHubUser("Dr.Blank"),
			GitHubUser("CharlieRussel"),
			GitHubUser("cbordeman"),
			GitHubUser("jwillikers"),
			GitHubUser("Shuvashish76"),
			GitHubUser("RokeJulianLockhart"),
			GitHubUser("maaximal"),
			GitHubUser("muchtall"),
			GitHubUser("ScubyG"),
			GitHubUser("patienttruth"),
			GitHubUser("stickystyle"),
			GitHubUser("cherez"),
			GitHubUser("delebash"),
			GitHubUser("twsouthwick"),
			GitHubUser("radiorambo"),
			GitHubUser("Youssef1313"),
            GitHubUser("niontrix"),
		]);

	private LibationContributor(string name, LibationContributorType type,Uri link)
	{
		Name = name;
		Type = type;
		Link = link;
	}

	private static LibationContributor GitHubUser(string name, LibationContributorType type = LibationContributorType.Contributor)
		=> new LibationContributor(name, type, new Uri($"ht" + $"tps://github.com/{name.Replace('.', '-')}"));
}
