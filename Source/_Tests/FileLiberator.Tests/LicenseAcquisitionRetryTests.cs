using AudibleApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace FileLiberator.Tests;

[TestClass]
public class LicenseAcquisitionRetryTests
{
	[TestMethod]
	public void IsTransientLicenseRequestFailure_matches_transport_errors_and_rejects_api_status_errors()
	{
		Assert.IsTrue(DownloadOptions.IsTransientLicenseRequestFailure(
			new HttpRequestException(HttpRequestError.SecureConnectionError, "The SSL connection could not be established")));

		Assert.IsTrue(DownloadOptions.IsTransientLicenseRequestFailure(
			new IOException("Connection reset by peer", new SocketException(104))));

		var transportApiError = new ApiErrorException(
			new Uri("https://api.audible.com/1.0/content/B0TEST/licenserequest"),
			JObject.Parse("""
			{
			  "error": "The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.",
			  "error_message": "The SSL connection could not be established, see inner exception.",
			  "http_status_code": null,
			  "http_request_error": "SecureConnectionError"
			}
			"""));
		Assert.IsTrue(DownloadOptions.IsTransientLicenseRequestFailure(transportApiError));

		var httpApiError = new ApiErrorException(
			new Uri("https://api.audible.com/1.0/content/B0TEST/licenserequest"),
			JObject.Parse("""
			{
			  "error_code": "000307",
			  "http_status_code": 400
			}
			"""));
		Assert.IsFalse(DownloadOptions.IsTransientLicenseRequestFailure(httpApiError));

		Assert.IsFalse(DownloadOptions.IsTransientLicenseRequestFailure(new InvalidDataException("bad data")));
	}

	[TestMethod]
	public async Task ExecuteWithTransientRetryAsync_retries_transient_failures_and_succeeds()
	{
		var attempts = 0;
		var result = await DownloadOptions.ExecuteWithTransientRetryAsync(
			() =>
			{
				attempts++;
				if (attempts < 3)
					throw new HttpRequestException(HttpRequestError.SecureConnectionError, "transient reset");
				return Task.FromResult("ok");
			},
			"B0TEST",
			"test",
			CancellationToken.None,
			_ => TimeSpan.Zero);

		Assert.AreEqual("ok", result);
		Assert.AreEqual(3, attempts);
	}

	[TestMethod]
	public async Task ExecuteWithTransientRetryAsync_does_not_retry_non_transient_errors()
	{
		var attempts = 0;
		await Assert.ThrowsExactlyAsync<InvalidDataException>(() =>
			DownloadOptions.ExecuteWithTransientRetryAsync<string>(
				() =>
				{
					attempts++;
					throw new InvalidDataException("non-transient");
				},
				"B0TEST",
				"test",
				CancellationToken.None,
				_ => TimeSpan.Zero));

		Assert.AreEqual(1, attempts);
	}
}
