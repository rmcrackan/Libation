using System.Collections.Generic;

#nullable enable
namespace LibationFileManager
{
	public partial class Configuration
	{
		/*
		 * Use this type in the getter for any Dictionary<TKey, TValue> settings,
		 * and be sure to clone it before returning. This allows Configuration to
		 * accurately detect if any of the Dictionary's elements have changed.
		 */
		private class EquatableDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull
		{
			public EquatableDictionary() { }
			public EquatableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs) : base(keyValuePairs) { }
			public EquatableDictionary<TKey, TValue> Clone() => new(this);
			public override bool Equals(object? obj)
			{
				if (obj is Dictionary<TKey, TValue> dic && Count == dic.Count)
				{
					foreach (var pair in this)
						if (!dic.TryGetValue(pair.Key, out var value) || pair.Value?.Equals(value) is not true)
							return false;

					return true;
				}
				return false;
			}
			public override int GetHashCode() => base.GetHashCode();
		}
	}
}
