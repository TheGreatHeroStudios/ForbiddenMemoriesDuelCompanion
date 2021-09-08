using System;
using System.Collections.Generic;
using System.Text;

namespace FMDC.Data.DataLoader.Interfaces
{
	public interface IDataLoader<DataType>
	{
		IEnumerable<DataType> LoadDataIntoMemory();
		int LoadDataIntoDatabase(IEnumerable<DataType> data);
	}
}
