using FMDC.TestApp.Enums;
using FMDC.TestApp.Pages;
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

		private TrunkViewModel _trunkViewModel;
		private PlayOptimizerViewModel _playOptimizerViewModel;
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
		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			CacheViewModelReferences();
		}


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
			_playOptimizerViewModel.RefreshDeckList(_trunkViewModel.CardCounts);
		}
		#endregion



		#region Non-Public Method(s)
		private void CacheViewModelReferences()
		{
			_trunkViewModel =
				(TrunkPage.Content as Trunk)?.DataContext as TrunkViewModel;

			_playOptimizerViewModel = 
				(PlayOptimizerPage.Content as PlayOptimizer)?.DataContext as PlayOptimizerViewModel;
		}
		#endregion
	}
}
