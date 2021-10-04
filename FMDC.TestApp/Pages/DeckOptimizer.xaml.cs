using FMDC.TestApp.Base;
using FMDC.TestApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FMDC.TestApp.Pages
{
	/// <summary>
	/// Interaction logic for DeckOptimizer.xaml
	/// </summary>
	public partial class DeckOptimizer : AutoBindingPage<DeckOptimizerViewModel>
	{
		#region Constructor(s)
		public DeckOptimizer()
		{
			InitializeComponent();
		}

		#endregion

		#region Event Handler(s)
		private void OptimalFusionInfoButton_Click(object sender, RoutedEventArgs e)
		{
			ViewModel
				.SetPropertyValue
				(
					nameof(ViewModel.OptimalFusionWindowOpen),
					true
				);
		}


		private void DismissOptimalFusionInfoButton_Click(object sender, RoutedEventArgs e)
		{
			ViewModel
				.SetPropertyValue
				(
					nameof(ViewModel.OptimalFusionWindowOpen),
					false
				);
		}
		#endregion
	}
}
