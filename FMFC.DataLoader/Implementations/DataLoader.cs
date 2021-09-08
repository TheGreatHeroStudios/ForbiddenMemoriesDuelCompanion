using FMDC.DataLoader.Interfaces;
using FMDC.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FMDC.DataLoader.Implementations
{
	public abstract class DataLoader<DataType> : IDataLoader<DataType>
	{
		#region Private Fields
		private HttpClient _dataLoaderClient;
		#endregion



		#region Interface Implementations
		public abstract IEnumerable<DataType> LoadDataIntoMemory();
		public abstract int LoadDataIntoDatabase(IEnumerable<DataType> data);
		#endregion



		#region Constructor
		public DataLoader(string remoteContentURL = null, int requestTimeoutSeconds = 100, IEnumerable<(string Key, string Value)> defaultRequestHeaders = null)
		{
			if (remoteContentURL != null)
			{
				_dataLoaderClient = new HttpClient();
				_dataLoaderClient.BaseAddress = new Uri(remoteContentURL);
				_dataLoaderClient.Timeout = requestTimeoutSeconds > 0 ? TimeSpan.FromSeconds(requestTimeoutSeconds) : Timeout.InfiniteTimeSpan;
				defaultRequestHeaders?.ToList().ForEach(header => AddRequestHeader(header.Key, header.Value));
			}
		}

		public DataLoader(HttpClient client)
		{
			_dataLoaderClient = client;
		}
		#endregion



		#region Base Class Method(s)
		protected void AddRequestHeader(string key, string value)
		{
			if (_dataLoaderClient != null)
			{
				_dataLoaderClient.DefaultRequestHeaders.Add(key, value);
			}
			else
			{
				throw new NotSupportedException($"{MessageConstants.ADD_REQUEST_HEADER_FAILURE}  {MessageConstants.HTTP_CLIENT_NOT_SPECIFIED}");
			}
		}


		protected async Task<HttpResponseMessage> GetRemoteContentAsync(string relativePath)
		{
			if (_dataLoaderClient == null)
			{
				throw new NotSupportedException($"{MessageConstants.REMOTE_CONTENT_ACCESS_FAILURE}  {MessageConstants.HTTP_CLIENT_NOT_SPECIFIED}");
			}

			try
			{
				HttpResponseMessage response = await _dataLoaderClient.GetAsyncWithAutoRetry(relativePath, 10);
				response.EnsureSuccessStatusCode();

				return response;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}


		protected IEnumerable<string> LoadDataFile(string filePath)
		{
			return File.ReadAllLines(filePath);
		}
		#endregion
	}
}
