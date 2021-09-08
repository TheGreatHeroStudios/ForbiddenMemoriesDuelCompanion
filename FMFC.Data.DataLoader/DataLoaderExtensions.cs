using FMDC.Model;
using FMDC.Utility;
using FMDC.Utility.PubSubUtility;
using FMDC.Utility.PubSubUtility.PubSubEventTypes;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FMDC.Data.DataLoader
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
				catch(Exception)
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
