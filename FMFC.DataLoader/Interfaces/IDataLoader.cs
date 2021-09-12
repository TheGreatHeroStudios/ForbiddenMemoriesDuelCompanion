using System;
using System.Collections.Generic;

namespace FMDC.DataLoader.Interfaces
{
	public interface IDataLoader<TDataType>
		where TDataType : class
	{
		int ActualRecordCount { get; }
		int ExpectedRecordCount { get; }

		Func<TDataType, int> KeySelector { get; }

		Func<TDataType, bool> RecordRetrievalPredicate { get; }


		IEnumerable<TDataType> LoadDataIntoMemory();


		IEnumerable<TDataType> LoadDataIntoDatabase(IEnumerable<TDataType> payload);
	}
}
