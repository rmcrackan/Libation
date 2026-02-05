using AudibleApi.Common;
using System;
using System.Collections.Generic;

namespace AudibleUtilities;

public class ImportValidationException : AggregateException
{
	public List<Item> Items { get; }
	public ImportValidationException(List<Item> items, IEnumerable<Exception> exceptions) : base(exceptions)
	{
		Items = items;
	}
}
