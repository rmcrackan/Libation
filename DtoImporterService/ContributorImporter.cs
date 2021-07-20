using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApiDTOs;
using DataLayer;
using InternalUtilities;

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
			//// BAD: very inefficient
			// var x = context.Contributors.Local.Where(c => !contribNames.Contains(c.Name));

			// GOOD: Except() is efficient. Due to hashing, it's close to O(n)
			var localNames = DbContext.Contributors.Local.Select(c => c.Name);
			var remainingContribNames = contributorNames
				.Distinct()
				.Except(localNames)
				.ToList();

			// load existing => local
			// remember to include default/empty/missing
			var emptyName = Contributor.GetEmpty().Name;
			if (remainingContribNames.Any())
				DbContext.Contributors.Where(c => remainingContribNames.Contains(c.Name) || c.Name == emptyName).ToList();
		}

		// only use after loading contributors => local
		private int upsertPeople(List<Person> people)
		{
			var qtyNew = 0;

			foreach (var p in people)
			{
				// Should be 'Single' not 'First'. A user had a duplicate get in somehow though so I'm now using 'First' defensively
				var person = DbContext.Contributors.Local.FirstOrDefault(c => c.Name == p.Name);
				if (person == null)
				{
					person = DbContext.Contributors.Add(new Contributor(p.Name, p.Asin)).Entity;
					qtyNew++;
				}
			}

			return qtyNew;
		}

		// only use after loading contributors => local
		private int upsertPublishers(List<string> publishers)
		{
			var qtyNew = 0;

			foreach (var publisherName in publishers)
			{
				if (DbContext.Contributors.Local.SingleOrDefault(c => c.Name == publisherName) == null)
				{
					DbContext.Contributors.Add(new Contributor(publisherName));
					qtyNew++;
				}
			}

			return qtyNew;
		}
	}
}
