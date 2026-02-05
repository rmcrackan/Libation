using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        public string AudibleCategoryId { get; }

        public string Name { get; }

        internal List<CategoryLadder> _categoryLadders = new();
        private ReadOnlyCollection<CategoryLadder>? _categoryLaddersReadOnly;
        public ReadOnlyCollection<CategoryLadder> CategoryLadders
		{
			get
			{
				if (_categoryLaddersReadOnly?.SequenceEqual(_categoryLadders) is not true)
					_categoryLaddersReadOnly = _categoryLadders.AsReadOnly();
				return _categoryLaddersReadOnly;
			}
		}

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
		private Category() { }
#pragma warning restore CS8618
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
