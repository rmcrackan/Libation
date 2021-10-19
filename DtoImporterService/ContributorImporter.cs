using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApi.Common;
using AudibleUtilities;
using DataLayer;

namespace DtoImporterService
{
	public class ContributorImporter : ItemsImporterBase
	{
		public ContributorImporter(LibationContext context) : base(context) { }

		public override IEnumerable<Exception> Validate(IEnumerable<ImportItem> importItems) => new ContributorValidator().Validate(importItems.Select(i => i.DtoItem));

		protected override int DoImport(IEnumerable<ImportItem> importItems)
		{
			// get distinct
			var authors = importItems
				.Select(i => i.DtoItem)
				.GetAuthorsDistinct()
				.ToList();
			var narrators = importItems
				.Select(i => i.DtoItem)
				.GetNarratorsDistinct()
				.ToList();
			var publishers = importItems
				.Select(i => i.DtoItem)
				.GetPublishersDistinct()
				.ToList();

			// load db existing => .Local
			var allNames = publishers
				.Union(authors.Select(n => n.Name))
				.Union(narrators.Select(n => n.Name))
				.Where(name => !string.IsNullOrWhiteSpace(name))
				.ToList();
			loadLocal_contributors(allNames);

			// upsert
			var qtyNew = 0;
			qtyNew += upsertPeople(authors);
			qtyNew += upsertPeople(narrators);
			qtyNew += upsertPublishers(publishers);
			return qtyNew;
		}

		private void loadLocal_contributors(List<string> contributorNames)
		{
			// must include default/empty/missing
			contributorNames.Add(Contributor.GetEmpty().Name);

			//// BAD: very inefficient
			// var x = context.Contributors.Local.Where(c => !contribNames.Contains(c.Name));

			// GOOD: Except() is efficient. Due to hashing, it's close to O(n)
			var localNames = DbContext.Contributors.Local.Select(c => c.Name).ToList();
			var remainingContribNames = contributorNames
				.Distinct()
				.Except(localNames)
				.ToList();

			// load existing => local
			if (remainingContribNames.Any())
				DbContext.Contributors.Where(c => remainingContribNames.Contains(c.Name)).ToList();
		}

		// only use after loading contributors => local
		private int upsertPeople(List<Person> people)
		{
			var localNames = DbContext.Contributors.Local.Select(c => c.Name).ToList();
			var newPeople = people
				.Select(p => p.Name)
				.Distinct()
				.Except(localNames)
				.ToList();

			var groupby = people.GroupBy(
				p => p.Name,
				p => p,
				(key, g) => new { Name = key, People = g.ToList() }
				);
			foreach (var name in newPeople)
			{
				var p = groupby.Single(g => g.Name == name).People.First();
				DbContext.Contributors.Add(new Contributor(p.Name, p.Asin));
			}

			return newPeople.Count;
		}

		// only use after loading contributors => local
		private int upsertPublishers(List<string> publishers)
		{
			var localNames = DbContext.Contributors.Local.Select(c => c.Name).ToList();
			var newPublishers = publishers
				.Distinct()
				.Except(localNames)
				.ToList();

			foreach (var pub in newPublishers)
				DbContext.Contributors.Add(new Contributor(pub));

			return newPublishers.Count;
		}
	}
}
