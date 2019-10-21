using System;
using System.Threading.Tasks;

namespace AudibleApiDomainService
{
	public class AudibleApiLibationClient
	{
		private Settings settings;
		public AudibleApiLibationClient(Settings settings)
			=> this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public async Task ImportLibraryAsync()
		{
			// call api
			// translate to DTOs
			// update database
		}
	}
}
