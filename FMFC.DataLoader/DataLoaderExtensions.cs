using FMDC.Model;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace FMDC.DataLoader
{
	public static class DataLoaderExtensions
	{
		public static async Task<HttpResponseMessage> GetAsyncWithAutoRetry(this HttpClient client, string requestURL, int retryCount = 5)
		{
			int initialRetries = retryCount;

			while (retryCount > 0 || retryCount == -1)
			{
				try
				{
					HttpResponseMessage response = await client.GetAsync(requestURL);
					response.EnsureSuccessStatusCode();
					return response;
				}
				catch (Exception)
				{
					retryCount--;
					/*if(retryCount > 0)
					{
						LoggingUtility.LogVerbose($"Retrying... (Attempt {initialRetries - retryCount + 1} of {initialRetries})");
					}*/
				}
			}

			throw new HttpRequestException(MessageConstants.HTTP_REQUEST_RETRY_FAILURE);
		}
	}
}
