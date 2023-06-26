using Dinah.Core;
using System;

namespace LibationUiBase
{
	public record EnumDiaplay<T> where T : Enum
	{
		public T Value { get; }
		public string Description { get; }
		public EnumDiaplay(T value, string description = null)
		{
			Value = value;
			Description = description ?? value.GetDescription() ?? value.ToString();
		}
		public override string ToString() => Description;
	}
}
