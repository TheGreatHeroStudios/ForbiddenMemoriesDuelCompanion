using FMDC.Model.Models;
using FMDC.TestApp.Base;
using FMDC.TestApp.ViewModels;
using System.Windows.Controls;

namespace FMDC.TestApp.Pages
{
	/// <summary>
	/// Interaction logic for Trunk.xaml
	/// </summary>
	public partial class Trunk : AutoBindingPage<TrunkViewModel>
	{
		#region Constructor(s)
		public Trunk()
		{
			InitializeComponent();
		}
		#endregion



		#region Event Handler(s)
		private void RemoveFromDeckButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			CardCount targetCardCount =
				(sender as Button).DataContext as CardCount;

			if(targetCardCount.NumberInDeck > 0)
			{
				targetCardCount.SetPropertyValue
				(
					nameof(targetCardCount.NumberInDeck),
					targetCardCount.NumberInDeck - 1
				);

				targetCardCount.SetPropertyValue
				(
					nameof(targetCardCount.NumberInTrunk),
					targetCardCount.NumberInTrunk + 1
				);
			}

			ViewModel.RaisePropertyChanged(nameof(ViewModel.DeckCount));
		}


		private void AddToDeckButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			CardCount targetCardCount =
				(sender as Button).DataContext as CardCount;

			if
			(
				targetCardCount.NumberInTrunk > 0 &&
				targetCardCount.NumberInDeck < 3 &&
				ViewModel.DeckCount < 40
			)
			{
				targetCardCount.SetPropertyValue
				(
					nameof(targetCardCount.NumberInDeck),
					targetCardCount.NumberInDeck + 1
				);

				targetCardCount.SetPropertyValue
				(
					nameof(targetCardCount.NumberInTrunk),
					targetCardCount.NumberInTrunk - 1
				);
			}

			ViewModel.RaisePropertyChanged(nameof(ViewModel.DeckCount));
		}


		private void AddToTrunkButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			CardCount targetCardCount =
				(sender as Button).DataContext as CardCount;

			if (targetCardCount.NumberInTrunk < 255)
			{
				targetCardCount.SetPropertyValue
				(
					nameof(targetCardCount.NumberInTrunk),
					targetCardCount.NumberInTrunk + 1
				);
			}

			ViewModel.RaisePropertyChanged(nameof(ViewModel.DeckCount));
		}


		private void RemoveFromTrunkButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			CardCount targetCardCount =
				(sender as Button).DataContext as CardCount;

			if (targetCardCount.NumberInTrunk > 0)
			{
				targetCardCount.SetPropertyValue
				(
					nameof(targetCardCount.NumberInTrunk),
					targetCardCount.NumberInTrunk - 1
				);
			}

			ViewModel.RaisePropertyChanged(nameof(ViewModel.DeckCount));
		}
		#endregion
	}
}
