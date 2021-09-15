using FMDC.TestApp.Enums;
using FMDC.TestApp.ViewModels;
using System.Windows;

namespace FMDC.TestApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		#region Non-Public Member(s)
		private MainViewModel _mainViewModel;
		#endregion



		#region Constructor(s)
		public MainWindow()
		{
			_mainViewModel = new MainViewModel();
			DataContext = _mainViewModel;

			InitializeComponent();
		}
		#endregion



		#region Event Handler(s)
		private void CardChestButton_Click(object sender, RoutedEventArgs e)
		{
			_mainViewModel.CurrentFeature = FeatureSelection.Trunk;
		}

		private void DeckOptimizerButton_Click(object sender, RoutedEventArgs e)
		{
			_mainViewModel.CurrentFeature = FeatureSelection.DeckOptimizer;
		}

		private void PlayOptimizerButton_Click(object sender, RoutedEventArgs e)
		{
			_mainViewModel.CurrentFeature = FeatureSelection.PlayOptimizer;
		}
		#endregion
	}
}
