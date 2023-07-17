using System.Collections.Generic;
using Dinah.Core;

namespace DataLayer
{
    public class AudibleCategoryId
    {
        public string Id { get; }
        public AudibleCategoryId(string id)
        {
            ArgumentValidator.EnsureNotNullOrWhiteSpace(id, nameof(id));
            Id = id;
        }
    }

    public class Category
    {
        internal int CategoryId { get; private set; }
        public string AudibleCategoryId { get; private set; }

        public string Name { get; internal set; }

        internal List<CategoryLadder> _categoryLadders = new();
        public IReadOnlyCollection<CategoryLadder> CategoryLadders => _categoryLadders.AsReadOnly();

		private Category() { }
        /// <summary>special id class b/c it's too easy to get string order mixed up</summary>
        public Category(AudibleCategoryId audibleSeriesId, string name)
        {
            ArgumentValidator.EnsureNotNull(audibleSeriesId, nameof(audibleSeriesId));
            var id = audibleSeriesId.Id;
            ArgumentValidator.EnsureNotNullOrWhiteSpace(id, nameof(id));
            ArgumentValidator.EnsureNotNullOrWhiteSpace(name, nameof(name));

            AudibleCategoryId = id;
            Name = name;
        }

		public override string ToString() => $"[{AudibleCategoryId}] {Name}";
	}
}
