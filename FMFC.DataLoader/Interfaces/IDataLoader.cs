using System;
using System.Collections.Generic;
using TGH.Common.Repository.Interfaces;

namespace FMDC.DataLoader.Interfaces
{
	public interface IDataLoader<TDataType>
		where TDataType : class
	{
		int ActualRecordCount { get; }
		int ExpectedRecordCount { get; }

		Func<TDataType, int> KeySelector { get; }

		Func<TDataType, bool> RecordCountPredicate { get; }


		IEnumerable<TDataType> LoadDataIntoMemory();


		void LoadDataIntoDatabase(IEnumerable<TDataType> payload);
	}
}
