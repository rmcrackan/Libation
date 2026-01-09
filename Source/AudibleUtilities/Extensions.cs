using AudibleApi.Common;
using System;
using System.Linq;

namespace AudibleUtilities;

public static class Extensions
{
	extension(Item item)
	{
		/// <summary>
		/// Determines when your audible plus or free book will expire from your library  
		/// plan.IsAyce from underlying AudibleApi project determines the plans to look at, first plan found is used.
		/// In some cases current date is later than end date so exclude.
		/// </summary>
		/// <returns>The DateTime that this title will become unavailable, otherwise null</returns>
		public DateTime? GetExpirationDate()
			  => item.Plans
			  ?.Where(p => p.IsAyce)
			  .Select(p => p.EndDate)
			  .FirstOrDefault(end => end.HasValue && end.Value.Year is not (2099 or 9999) && end.Value.LocalDateTime >= DateTime.Now)
			  ?.DateTime;
	}
}
