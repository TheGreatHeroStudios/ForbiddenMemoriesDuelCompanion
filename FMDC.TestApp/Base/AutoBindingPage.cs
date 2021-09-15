using System.Windows.Controls;
using TGH.Common.Patterns.IoC;

namespace FMDC.TestApp.Base
{
	public abstract class AutoBindingPage<TViewModelType> : Page
		where TViewModelType : class
	{
		#region Non-Public Member(s)
		private TViewModelType _viewModel;
		#endregion



		#region Public Propertie(s)
		public TViewModelType ViewModel => _viewModel;
		#endregion



		#region Constructor(s)
		/// <summary>
		///		Constructs the page by resolving and binding a  
		///		registered <typeparamref name="TViewModelType"/> 
		///		instance out of the <seealso cref="DependencyManager"/>
		/// </summary>
		public AutoBindingPage()
		{
			_viewModel = DependencyManager.ResolveService<TViewModelType>();
			DataContext = _viewModel;
		}


		/// <summary>
		///		Constructs the page by binding an explicitly provided 
		///		instance of <typeparamref name="TViewModelType"/>.
		/// </summary>
		/// <param name="viewModel">
		///		The instance of the <typeparamref name="TViewModelType"/>
		///		to use for binding to the page view.
		/// </param>
		public AutoBindingPage(TViewModelType viewModel)
		{
			_viewModel = viewModel;
			DataContext = _viewModel;
		}
		#endregion
	}
}
