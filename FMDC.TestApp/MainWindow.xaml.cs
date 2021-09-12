using FMDC.Model;
using FMDC.Model.Models;
using FMDC.Persistence;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TGH.Common.Patterns.IoC;
using TGH.Common.Persistence.Interfaces;
using TGH.Common.Repository.Implementations;
using TGH.Common.Repository.Interfaces;

namespace FMDC.TestApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		#region Non-Public Member(s)
		private FusionOptimizerViewModel _fusionOptimizerViewModel;
		#endregion



		#region Constructor(s)
		public MainWindow()
		{
			InitializeComponent();

			RegisterDependencies();

			_fusionOptimizerViewModel =
				DependencyManager.ResolveService<FusionOptimizerViewModel>(ServiceScope.Singleton);

			DataContext = _fusionOptimizerViewModel;
		}
		#endregion



		#region Event Handler(s)
		private void CardSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			string controlName = (sender as Control).Name;
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
					e.AddedItems.Cast<Card>().First(),
					handCardUpdated,
					controlIndex
				);
		}


		private void GenerateOptimalFusionButton_Click(object sender, RoutedEventArgs e)
		{
			_fusionOptimizerViewModel
				.GenerateOptimalFusion();
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
				.RegisterService<FusionOptimizerViewModel, FusionOptimizerViewModel>
				(
					ServiceScope.Singleton
				);
		}
		#endregion
	}
}
