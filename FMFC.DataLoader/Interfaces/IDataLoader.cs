using System.Collections.Generic;

namespace FMDC.DataLoader.Interfaces
{
	public interface IDataLoader<DataType>
	{
		IEnumerable<DataType> LoadDataIntoMemory();
		int LoadDataIntoDatabase(IEnumerable<DataType> data);
	}
}
