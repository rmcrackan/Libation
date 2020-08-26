using System;
using AudibleApiDTOs;

namespace InternalUtilities
{
	public class ImportItem
	{
		public Item DtoItem { get; set; }
		public Account Account { get; set; }

		public ImportItem() { }
		public ImportItem(Item dtoItem, Account account)
		{
			DtoItem = dtoItem;
			Account = account;
		}
	}
}
