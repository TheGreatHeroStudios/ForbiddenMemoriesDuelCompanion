using FMDC.Utility.PubSubUtility.PubSubEventTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FMDC.Utility.PubSubUtility
{
	public static class PubSubManager
	{
		#region Private Fields
		//The internal dictionary of event subscriptions keeps track of event types subscribed to,
		//and a collection of event handlers to execute when an event of that type is fired  
		private static Dictionary<Type, IList> _eventSubscriptions;
		#endregion



		#region Static Constructor
		static PubSubManager()
		{
			_eventSubscriptions = new Dictionary<Type, IList>();
		}
		#endregion



		#region Public Methods
		/// <summary>
		/// Allows the current context to subscribe to an event type.<br/>
		/// When an event of that type is published, the specified callback is fired and receives the published event.
		/// </summary>
		/// <typeparam name="EventType">
		/// The type of event to subscribe to.  This derives from <see cref="PubSubEventBase"/>
		/// </typeparam>
		/// <param name="callback">The method to fire when an event of the specified type is published</param>
		public static void Subscribe<EventType>(Action<EventType> callback)
			where EventType : PubSubEventBase
		{
			//Get the name of the file subscribing to the current event.  
			//This will be used later when trying to unsubscribe from the event.
			StackTrace stack = new StackTrace(true);
			string subscribingFile = stack.GetFrame(1).GetFileName() ?? "";

			if (_eventSubscriptions.ContainsKey(typeof(EventType)))
			{
				//If the specified event type exists in the dictionary, add the callback 
				//(and its associated file name) to the list of callbacks that are
				//subscribed to the event type.
				_eventSubscriptions[typeof(EventType)]?.Add((subscribingFile, callback));
			}
			else
			{
				//If the specified event type does not yet exist in the dictionary, create a new 
				//list of callbacks, add the callback (and its associated file name), and add the 
				//list as a new entry in the dictionary with the event type as its key.
				List<(string, Action<EventType>)> callbackList = new List<(string, Action<EventType>)>();
				callbackList.Add((subscribingFile, callback));
				_eventSubscriptions.Add(typeof(EventType), callbackList);
			}
		}


		/// <summary>
		/// Unsubscribes the current context from an event type.
		/// </summary>
		/// <typeparam name="EventType">
		/// The type of event being unsubscribed from.  This derives from <see cref="PubSubEventBase"/>
		/// </typeparam>
		public static void Unsubscribe<EventType>()
			where EventType : PubSubEventBase
		{
			if (_eventSubscriptions.ContainsKey(typeof(EventType)))
			{
				//Get the file name of the method that called 'Unsubscribe()'.  We want to remove the 
				//callback subscribed by this file for the specified event.
				StackTrace stackTrace = new StackTrace(true);
				string callingFile = stackTrace.GetFrame(1).GetFileName() ?? "";

				//Search the event's associated list of callbacks for any whose subscribing file name
				//matches that of the calling file and remove it from the list
				((IEnumerable<(string subscribingFile, Action<EventType> callback)>)_eventSubscriptions[typeof(EventType)])
					.Where(e => e.subscribingFile == callingFile)
					.ToList()
					.ForEach(e => _eventSubscriptions[typeof(EventType)].Remove(e));
			}
		}


		public static void Publish<EventType>(EventType e)
			where EventType : PubSubEventBase
		{
			//If the dictionary of subscriptions contains the event type being published
			//(meaning we have subscribers for it), loop through the list of subscriptions and
			//execute the registered callback for each
			if (_eventSubscriptions.ContainsKey(typeof(EventType)))
			{
				((IEnumerable<(string subscribingFile, Action<EventType> callback)>)_eventSubscriptions[typeof(EventType)])
					.ToList()
					.ForEach(subscription => subscription.callback?.Invoke(e));
			}
		}
		#endregion
	}
}
