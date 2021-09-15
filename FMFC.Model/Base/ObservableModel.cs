using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using TGH.Common.Extensions;

namespace FMDC.Model.Base
{
	public class ObservableModel : INotifyPropertyChanged
	{
		#region Class-Specific Constant(s)
		private const string ERROR_INVALID_PROPERTY =
			"The specified property does not exist or can not be set dynamically.  " +
			"Ensure that the specified property exists, is a public instance property," +
			"and has a publicly exposed setter.";

		private const string ERROR_UNASSIGNABLE_VALUE =
			"The specified value '{0}' of type '{1}' can not be assigned " +
			"to property '{2}' of type '{3}'.";
		#endregion



		#region 'INotifyPropertyChanged' Implementation
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion



		#region Public Method(s)
		/// <summary>
		///		Sets the value of the property named <paramref name="propertyName"/>
		///		on this instance of the model to <paramref name="newValue"/>.
		/// </summary>
		/// <typeparam name="TPropertyType">
		///		The type of the property being set.
		/// </typeparam>
		/// <param name="propertyName">
		///		The name of the property for which to set a new value
		/// </param>
		/// <param name="newValue">
		///		The new value to assign to the property.
		/// </param>
		/// <exception cref="ArgumentException">
		///		The specified property does not exist,
		///		is static, does not have a public setter,
		///		or the provided value is not assignable to it.
		/// </exception>
		public void SetPropertyValue<TPropertyType>
		(
			string propertyName,
			TPropertyType newValue
		)
		{
			PropertyInfo targetProperty =
				GetType()
					.GetProperty
					(
						propertyName,
						BindingFlags.Public | BindingFlags.Instance
					);

			if(targetProperty == null || targetProperty.GetSetMethod() == null)
			{
				throw new ArgumentException(ERROR_INVALID_PROPERTY);
			}
			else if(!typeof(TPropertyType).IsAssignableTo(targetProperty.PropertyType))
			{
				throw new ArgumentException
				(
					string.Format
					(
						ERROR_UNASSIGNABLE_VALUE,
						newValue.ToString(),
						typeof(TPropertyType).Name,
						propertyName,
						targetProperty.PropertyType
					)
				);
			}

			targetProperty.SetValue(this, newValue);
			RaisePropertyChanged(propertyName);
		}


		/// <summary>
		///		Raises the <seealso cref="PropertyChanged"/> event for  
		///		the property specified by <paramref name="propertyName"/>
		/// </summary>
		/// <param name="propertyName">
		///		The name of the property for which to raise the event.
		/// </param>
		public void RaisePropertyChanged(string propertyName)
		{
			PropertyChanged?
				.Invoke
				(
					this,
					new PropertyChangedEventArgs(propertyName)
				);
		}
		#endregion
	}
}
