using FMDC.Model.Models;
using FMDC.TestApp.Base;
using FMDC.TestApp.Enums;
using FMDC.TestApp.ViewModels;
using System;
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
		private void SortMethodButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			string sortMethodString =
				(sender as Button).Name
					.Replace("SortBy", "")
					.Replace("Button", "");

			SortMethod sortMethod = Enum.Parse<SortMethod>(sortMethodString);

			ViewModel.SetSortMethod(sortMethod);
		}


		private void InspectCardButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Card targetCard =
				(((sender as Button)?.Parent as Grid)?.DataContext as CardCount)?.Card;
			
			if(targetCard != null)
			{
				ViewModel
					.SetPropertyValue
					(
						nameof(ViewModel.InspectedCard),
						targetCard
					);

				ViewModel.RaisePropertyChanged(nameof(ViewModel.CardInspectorOpen));
			}
		}


		private void DismissInspectorButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			ViewModel
				.SetPropertyValue
				(
					nameof(ViewModel.InspectedCard),
					(Card)null
				);

			ViewModel.RaisePropertyChanged(nameof(ViewModel.CardInspectorOpen));
		}


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
			ViewModel.RaisePropertyChanged(nameof(ViewModel.TrunkCount));
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
			ViewModel.RaisePropertyChanged(nameof(ViewModel.TrunkCount));
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
			ViewModel.RaisePropertyChanged(nameof(ViewModel.TrunkCount));
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
			ViewModel.RaisePropertyChanged(nameof(ViewModel.TrunkCount));
		}
		#endregion
	}
}
