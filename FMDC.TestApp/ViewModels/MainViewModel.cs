using FMDC.TestApp.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace FMDC.TestApp.ViewModels
{
	public class MainViewModel : INotifyPropertyChanged
	{
		#region Public Propert(ies)
		private FeatureSelection _currentFeature;
		public FeatureSelection CurrentFeature 
		{
			get => _currentFeature;
			set
			{
				_currentFeature = value;
				RaisePropertyChanged(nameof(CurrentFeature));
			}
		}
		#endregion



		#region 'INotifyPropertyChanged' Implementation
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion



		#region Private Method(s)
		private void RaisePropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}
}
