using FMDC.Model.Base;
using FMDC.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FMDC.Model.Models
{
	public class OptimizerSuggestion : ObservableModel
	{
		#region Public Propert(ies)
		public Card TargetCard { get; set; }
		public OptimizerDirection Direction { get; set; }
		public int Amount { get; set; }
		#endregion
	}
}
