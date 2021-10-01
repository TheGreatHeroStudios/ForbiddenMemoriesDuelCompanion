using FMDC.Model.Models;
using FMDC.TestApp.Enums;
using FMDC.TestApp.Pages;
using FMDC.TestApp.ViewModels;
using System.Collections.Generic;
using System.Windows;
using TGH.Common.Patterns.IoC;

namespace FMDC.TestApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		#region Non-Public Member(s)
		private App _currentAppInstance;

		private MainViewModel _mainViewModel;
		private TrunkViewModel _trunkViewModel;
		private PlayOptimizerViewModel _playOptimizerViewModel;
		private DeckOptimizerViewModel _deckOptimizerViewModel;
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
			_currentAppInstance = DependencyManager.ResolveService<App>();
			CacheViewModelReferences();
		}


		private void CardChestButton_Click(object sender, RoutedEventArgs e)
		{
			if (_mainViewModel.CurrentFeature != FeatureSelection.Trunk)
			{
				_mainViewModel.CurrentFeature = FeatureSelection.Trunk;
			}
		}


		private void DeckOptimizerButton_Click(object sender, RoutedEventArgs e)
		{
			if (_mainViewModel.CurrentFeature != FeatureSelection.DeckOptimizer)
			{
				_mainViewModel.CurrentFeature = FeatureSelection.DeckOptimizer;
				_deckOptimizerViewModel.RefreshAvailableCards(_trunkViewModel.CardCounts);
			}
		}


		private void PlayOptimizerButton_Click(object sender, RoutedEventArgs e)
		{
			if (_mainViewModel.CurrentFeature != FeatureSelection.PlayOptimizer)
			{
				_mainViewModel.CurrentFeature = FeatureSelection.PlayOptimizer;
				_playOptimizerViewModel.RefreshDeckList(_trunkViewModel.CardCounts);
			}
		}


		private void SaveCommand_Executed(object sender, RoutedEventArgs e)
		{
			_mainViewModel
				.SaveCardConfiguration
				(
					_trunkViewModel.CardCounts,
					_currentAppInstance.CardList
				);
		}


		private void OpenCommand_Executed(object sender, RoutedEventArgs e)
		{
			IEnumerable<CardCount> loadedCardCounts =
				_mainViewModel.LoadCardConfiguration(_currentAppInstance.CardList);

			if(loadedCardCounts != null)
			{
				//If card counts were successfully loaded from a file,
				//use them to reload card counts on the trunk view model.
				_trunkViewModel.LoadCardCounts(loadedCardCounts);

				//Then, refresh the deck list for the play optimizer view
				_playOptimizerViewModel.RefreshDeckList(_trunkViewModel.CardCounts);
			}
		}


		private void RandomizeMenuItem_Click(object sender, RoutedEventArgs e)
		{
			//Randomize the cards in the player's trunk and deck
			_trunkViewModel.RandomizeCardCounts();

			//Then, refresh the deck list for the play optimizer view
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

			_deckOptimizerViewModel =
				(DeckOptimizerPage.Content as DeckOptimizer)?.DataContext as DeckOptimizerViewModel;
		}
		#endregion
	}
}
