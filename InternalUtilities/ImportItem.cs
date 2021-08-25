using System;
using AudibleApi.Common;

namespace InternalUtilities
{
	public class ImportItem
	{
		public Item DtoItem { get; set; }
		public string AccountId { get; set; }
		public string LocaleName { get; set; }
	}
}
