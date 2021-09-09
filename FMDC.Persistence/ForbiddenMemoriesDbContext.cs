using FMDC.Model;
using Microsoft.EntityFrameworkCore;
using System;
using TGH.Common.Persistence.Contexts;

namespace FMDC.Persistence
{
	public class ForbiddenMemoriesDbContext : SqliteDbContext
	{
		#region Constructor(s)
		public ForbiddenMemoriesDbContext(string targetDatabaseFilePath) : 
			base
			(
				targetDatabaseFilePath,
				PersistenceConstants.SQLITE_TEMPLATE_FILEPATH
			)
		{

		}
		#endregion
	}
}
