using System;
using System.Collections.Generic;
using System.Text;

namespace FMDC.Data.DataLoader.Exceptions
{
	public class FileParsingAnomalyException : Exception
	{
		#region Properties
		public int FileLineNumber { get; set; }
		#endregion



		#region Constructor
		public FileParsingAnomalyException(string info, int lineNumber = -1) : base(info)
		{
			FileLineNumber = lineNumber;
		}
		#endregion
	}
}
