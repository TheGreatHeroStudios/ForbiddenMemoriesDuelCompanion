using FMDC.DataLoader.Interfaces;
using FMDC.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TGH.Common.Patterns.IoC;
using TGH.Common.Repository.Interfaces;

namespace FMDC.DataLoader.Implementations
{
	public abstract class DataLoader<TDataType> : IDataLoader<TDataType>
		where TDataType : class
	{
		#region Private Fields
		private HttpClient _dataLoaderClient;
		private IGenericRepository _cardRepository;
		#endregion



		#region Constructor(s)
		public DataLoader()
		{
			_cardRepository = 
				DependencyManager.ResolveService<IGenericRepository>();
		}


		public DataLoader(string remoteContentURL = null, int requestTimeoutSeconds = 100, IEnumerable<(string Key, string Value)> defaultRequestHeaders = null)
		{
			if (remoteContentURL != null)
			{
				_dataLoaderClient = new HttpClient();
				_dataLoaderClient.BaseAddress = new Uri(remoteContentURL);
				_dataLoaderClient.Timeout = requestTimeoutSeconds > 0 ? TimeSpan.FromSeconds(requestTimeoutSeconds) : Timeout.InfiniteTimeSpan;
				defaultRequestHeaders?.ToList().ForEach(header => AddRequestHeader(header.Key, header.Value));
			}

			_cardRepository =
				DependencyManager.ResolveService<IGenericRepository>();
		}


		public DataLoader(HttpClient client)
		{
			_dataLoaderClient = client;

			_cardRepository =
				DependencyManager.ResolveService<IGenericRepository>();
		}
		#endregion



		#region Interface Implementations
		public virtual int ExpectedRecordCount => 0;

		public abstract Func<TDataType, int> KeySelector { get; }

		public virtual Func<TDataType, bool> RecordCountPredicate => entity => true;


		public abstract IEnumerable<TDataType> LoadDataIntoMemory();


		public void LoadDataIntoDatabase(IEnumerable<TDataType> payload)
		{
			int recordCount = 
				_cardRepository.RecordCount(RecordCountPredicate);

			if (recordCount == 0)
			{
				//If no entities have been loaded, load them into the database
				_cardRepository
					.PersistEntities
					(
						payload,
						KeySelector
					);
			}
			else if(recordCount != ExpectedRecordCount)
			{
				//If the number of entities does not match the expected value 
				//for the data loader, truncate the table before reloading
				_cardRepository.DeleteEntities<TDataType>(entity => true);

				_cardRepository
					.PersistEntities
					(
						payload,
						KeySelector
					);
			}
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
