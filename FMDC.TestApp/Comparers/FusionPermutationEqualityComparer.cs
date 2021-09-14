using FMDC.Model.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace FMDC.TestApp.Comparers
{
	public class FusionPermutationEqualityComparer : IEqualityComparer<List<Card>>
	{
		#region 'IEqualityComparer' Implementation
		public bool Equals([AllowNull] List<Card> x, [AllowNull] List<Card> y)
		{
			return
				GetPermutationIdString(x)
					.Equals(GetPermutationIdString(y));
		}


		public int GetHashCode([DisallowNull] List<Card> obj)
		{
			return
				GetPermutationIdString(obj)
					.GetHashCode();
		}
		#endregion



		#region Public Method(s)
		public static string GetPermutationIdString(List<Card> fusionPermutation)
		{
			return
				fusionPermutation?
					.Aggregate
					(
						new StringBuilder(),
						(builder, card) => builder.Append($", {card.CardId}"),
						builder => builder.ToString()
					) ??
					string.Empty;
		}
		#endregion
	}
}
