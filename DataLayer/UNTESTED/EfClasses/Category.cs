using System;
using System.Collections.Generic;
using System.Linq;
using Dinah.Core;
using Microsoft.EntityFrameworkCore;

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
        // Empty is a special case. use private ctor w/o validation
        public static Category GetEmpty() => new Category { CategoryId = -1, AudibleCategoryId = "", Name = "", ParentCategory = null };
        public bool IsEmpty() => string.IsNullOrWhiteSpace(AudibleCategoryId) || string.IsNullOrWhiteSpace(Name) || ParentCategory == null;

        internal int CategoryId { get; private set; }
        public string AudibleCategoryId { get; private set; }

        public string Name { get; private set; }
        public Category ParentCategory { get; private set; }

        private Category() { }
        /// <summary>special id class b/c it's too easy to get string order mixed up</summary>
        public Category(AudibleCategoryId audibleSeriesId, string name, Category parentCategory = null)
        {
            ArgumentValidator.EnsureNotNull(audibleSeriesId, nameof(audibleSeriesId));
            var id = audibleSeriesId.Id;
            ArgumentValidator.EnsureNotNullOrWhiteSpace(id, nameof(id));
            ArgumentValidator.EnsureNotNullOrWhiteSpace(name, nameof(name));

            AudibleCategoryId = id;
            Name = name;

            UpdateParentCategory(parentCategory);
        }

        public void UpdateParentCategory(Category parentCategory)
        {
            // don't overwrite with null but not an error
            if (parentCategory != null)
                ParentCategory = parentCategory;
        }

		public override string ToString() => $"[{AudibleCategoryId}] {Name}";
	}
}
