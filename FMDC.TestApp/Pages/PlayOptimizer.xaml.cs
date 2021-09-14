using FMDC.Model;
using FMDC.Model.Models;
using FMDC.Persistence;
using FMDC.TestApp.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using TGH.Common.Patterns.IoC;
using TGH.Common.Persistence.Interfaces;
using TGH.Common.Repository.Implementations;
using TGH.Common.Repository.Interfaces;

namespace FMDC.TestApp.Pages
{
	/// <summary>
	/// Interaction logic for PlayOptimizer.xaml
	/// </summary>
	public partial class PlayOptimizer : Page
	{
		#region Non-Public Member(s)
		private PlayOptimizerViewModel _fusionOptimizerViewModel;
		#endregion



		#region Constructor(s)
		public PlayOptimizer()
		{
			InitializeComponent();

			RegisterDependencies();

			_fusionOptimizerViewModel =
				DependencyManager.ResolveService<PlayOptimizerViewModel>(ServiceScope.Singleton);

			DataContext = _fusionOptimizerViewModel;
		}
		#endregion



		#region Event Handler(s)
		private void CardSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				ComboBox comboBoxControl = sender as ComboBox;

				string controlName = comboBoxControl.Name;
				int controlIndex;
				bool handCardUpdated = false;

				if (controlName.StartsWith("FieldCard"))
				{
					controlIndex = int.Parse(controlName.Substring(9, 1)) - 1;
				}
				else
				{
					controlIndex = int.Parse(controlName.Substring(8, 1)) - 1;
					handCardUpdated = true;
				}

				_fusionOptimizerViewModel
					.UpdateCardSelection
					(
						comboBoxControl.SelectedItem as Card,
						handCardUpdated,
						controlIndex
					);
			}
			catch (Exception ex)
			{
				_ = MessageBox.Show
				(
					string.Format
					(
						"Error Occurred while attempting to update card selection: \n{0}",
						ex.Message
					),
					"Error Occurred",
					MessageBoxButton.OK
				);
			}
		}


		private void GenerateOptimalPlayButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				_fusionOptimizerViewModel.DetermineOptimalPlay();
			}
			catch (Exception ex)
			{
				_ = MessageBox.Show
				(
					string.Format
					(
						"Error Occurred while generating optimal play: \n{0}",
						ex.Message
					),
					"Error Occurred",
					MessageBoxButton.OK
				);
			}
		}


		private void AcceptFusionButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				_fusionOptimizerViewModel.AcceptFusion();
			}
			catch (Exception ex)
			{
				_ = MessageBox.Show
				(
					string.Format
					(
						"Error Occurred while attempting to accept fusion: \n{0}",
						ex.Message
					),
					"Error Occurred",
					MessageBoxButton.OK
				);
			}
		}


		private void ClearCardDataButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				MessageBoxResult result =
					MessageBox.Show
					(
						"This will clear all card data entered for the hand and the field.  Are you sure you want to proceed?",
						"Confirm Clear Card Data",
						MessageBoxButton.YesNo
					);

				if (result == MessageBoxResult.Yes)
				{
					_fusionOptimizerViewModel.ClearCardData();
				}
			}
			catch (Exception ex)
			{
				_ = MessageBox.Show
				(
					string.Format
					(
						"Error Occurred while attempting to clear card data: \n{0}",
						ex.Message
					),
					"Error Occurred",
					MessageBoxButton.OK
				);
			}
		}
		#endregion



		#region Non-Public Method(s)
		private static void RegisterDependencies()
		{
			DependencyManager
				.RegisterService<IDatabaseContext, ForbiddenMemoriesDbContext>
				(
					() =>
						new ForbiddenMemoriesDbContext
						(
							PersistenceConstants.SQLITE_DB_TARGET_FILEPATH
						),
					ServiceScope.Singleton
				);

			DependencyManager
				.RegisterService<IGenericRepository, GenericRepository>
				(
					ServiceScope.Singleton
				);

			DependencyManager
				.RegisterService<PlayOptimizerViewModel, PlayOptimizerViewModel>
				(
					ServiceScope.Singleton
				);
		}
		#endregion
	}
}
