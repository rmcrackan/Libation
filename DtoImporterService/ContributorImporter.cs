using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApiDTOs;
using DataLayer;

namespace DtoImporterService
{
	public class ContributorImporter : ItemsImporterBase
	{
		public override IEnumerable<Exception> Validate(IEnumerable<Item> items)
		{
			var exceptions = new List<Exception>();

			if (items.GetAuthorsDistinct().Any(a => string.IsNullOrWhiteSpace(a.Name)))
				exceptions.Add(new ArgumentException($"Collection contains {nameof(Item.Authors)} with null {nameof(Person.Name)}", nameof(items)));
			if (items.GetNarratorsDistinct().Any(a => string.IsNullOrWhiteSpace(a.Name)))
				exceptions.Add(new ArgumentException($"Collection contains {nameof(Item.Narrators)} with null {nameof(Person.Name)}", nameof(items)));

			return exceptions;
		}

		protected override int DoImport(IEnumerable<Item> items, LibationContext context)
		{
			// get distinct
			var authors = items.GetAuthorsDistinct().ToList();
			var narrators = items.GetNarratorsDistinct().ToList();
			var publishers = items.GetPublishersDistinct().ToList();

			// load db existing => .Local
			var allNames = authors
				.Select(a => a.Name)
				.Union(narrators.Select(n => n.Name))
				.Union(publishers)
				.ToList();
			loadLocal_contributors(allNames, context);

			// upsert
			var qtyNew = 0;
			qtyNew += upsertPeople(authors, context);
			qtyNew += upsertPeople(narrators, context);
			qtyNew += upsertPublishers(publishers, context);
			return qtyNew;
		}

		private void loadLocal_contributors(List<string> contributorNames, LibationContext context)
		{
			contributorNames.Remove(null);
			contributorNames.Remove("");

			//// BAD: very inefficient
			// var x = context.Contributors.Local.Where(c => !contribNames.Contains(c.Name));

			// GOOD: Except() is efficient. Due to hashing, it's close to O(n)
			var localNames = context.Contributors.Local.Select(c => c.Name);
			var remainingContribNames = contributorNames
				.Distinct()
				.Except(localNames)
				.ToList();

			// load existing => local
			if (remainingContribNames.Any())
				context.Contributors.Where(c => remainingContribNames.Contains(c.Name)).ToList();
			// _________________________________^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
			// i tried to extract this pattern, but this part prohibits doing so
			// wouldn't work anyway for Books.GetBooks()
		}

		// only use after loading contributors => local
		private int upsertPeople(List<Person> people, LibationContext context)
		{
			var qtyNew = 0;

			foreach (var p in people)
			{
				var person = context.Contributors.Local.SingleOrDefault(c => c.Name == p.Name);
				if (person == null)
				{
					person = context.Contributors.Add(new Contributor(p.Name)).Entity;
					qtyNew++;
				}

				person.UpdateAudibleAuthorId(p.Asin);
			}

			return qtyNew;
		}

		// only use after loading contributors => local
		private int upsertPublishers(List<string> publishers, LibationContext context)
		{
			var qtyNew = 0;

			foreach (var publisherName in publishers)
			{
				if (context.Contributors.Local.SingleOrDefault(c => c.Name == publisherName) == null)
				{
					context.Contributors.Add(new Contributor(publisherName));
					qtyNew++;
				}
			}

			return qtyNew;
		}
	}
}
