using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApi.Common;
using AudibleUtilities;
using DataLayer;
using Dinah.Core.Collections.Generic;

namespace DtoImporterService
{
	public class ContributorImporter : ItemsImporterBase
	{
		protected override IValidator Validator => new ContributorValidator();

		public Dictionary<string, Contributor> Cache { get; private set; } = new();

		public ContributorImporter(LibationContext context) : base(context) { }

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

			// load existing => local
			Cache = DbContext.Contributors
				.Where(c => contributorNames.Contains(c.Name))
				.ToDictionarySafe(c => c.Name);
		}

		private int upsertPeople(List<Person> people)
		{
			var qtyNew = 0;
			foreach (var person in people)
			{
				if (!Cache.TryGetValue(person.Name, out var contributor))
				{
					contributor = createContributor(person.Name, person.Asin);
					qtyNew++;
				}

				updateContributor(person, contributor);
			}

			return qtyNew;
		}

		// only use after loading contributors => local
		private int upsertPublishers(List<string> publishers)
		{
			var hash = publishers
				// new publishers only
				.Where(p => !Cache.ContainsKey(p))
				// remove duplicates
				.ToHashSet();

			foreach (var pub in hash)
				createContributor(pub);

			return hash.Count;
		}

		private void updateContributor(Person person, Contributor contributor)
		{
			if (person.Asin != contributor.AudibleContributorId)
				contributor.SetAudibleContributorId(person.Asin);
		}

		private Contributor createContributor(string name, string id = null)
		{
			try
			{
				var newContrib = new Contributor(name, id);

				var entityEntry = DbContext.Contributors.Add(newContrib);
				var entity = entityEntry.Entity;

				Cache.Add(entity.Name, entity);
				return entity;
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Error adding contributor. {@DebugInfo}", new { name, id });
				throw;
			}
		}
	}
}
