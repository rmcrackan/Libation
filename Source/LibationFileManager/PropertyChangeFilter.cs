using Dinah.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace LibationFileManager
{
	#region Useage

	/*
	 * USEAGE
	 
		*************************
		*						*
		*   Event Filter Mode   *
		*						*
		*************************

	 
	 	propertyChangeFilter.PropertyChanged += MyPropertiesChanged;

		[PropertyChangeFilter("MyProperty1")]
		[PropertyChangeFilter("MyProperty2")]
		void MyPropertiesChanged(object sender, PropertyChangedEventArgsEx e)
		{
			// Only properties whose names match either "MyProperty1"
			// or "MyProperty2" will fire this event handler.
		}

	******
	* OR *
	******
	
		propertyChangeFilter.PropertyChanged +=
			[PropertyChangeFilter("MyProperty1")]
			[PropertyChangeFilter("MyProperty2")]
			(_, _) => 
			{
				// Only properties whose names match either "MyProperty1"
				// or "MyProperty2" will fire this event handler.
			};


		*************************
		*						*
		*    Observable Mode	*
		*						*
		*************************
		
		using var cancellation = propertyChangeFilter.ObservePropertyChanging<int>("MyProperty", MyPropertyChanging);
		
        void MyPropertyChanging(int oldValue, int newValue)
        {
			// Only the property whose name match
			// "MyProperty" will fire this method.
        }
	
		//The observer is delisted when cancellation is disposed

	******
	* OR *
	******
	
		using var cancellation = propertyChangeFilter.ObservePropertyChanged<bool>("MyProperty", s =>
			{
				// Only the property whose name match
				// "MyProperty" will fire this action.
			});
		
		//The observer is delisted when cancellation is disposed

	 */

	#endregion

	public abstract class PropertyChangeFilter
	{
		private readonly Dictionary<string, List<Delegate>> propertyChangedActions = new();
		private readonly Dictionary<string, List<Delegate>> propertyChangingActions = new();
		
		private readonly List<(PropertyChangedEventHandlerEx subscriber, PropertyChangedEventHandlerEx wrapper)> changedFilters = new();
		private readonly List<(PropertyChangingEventHandlerEx subscriber, PropertyChangingEventHandlerEx wrapper)> changingFilters = new();

		public PropertyChangeFilter()
		{
			PropertyChanging += Configuration_PropertyChanging;
			PropertyChanged += Configuration_PropertyChanged;
		}

		#region Events

		protected void OnPropertyChanged(string propertyName, object newValue)
			=> _propertyChanged?.Invoke(this, new(propertyName, newValue));
		protected void OnPropertyChanging(string propertyName, object oldValue, object newValue)
			=> _propertyChanging?.Invoke(this, new(propertyName, oldValue, newValue));

		private PropertyChangedEventHandlerEx _propertyChanged;
		private PropertyChangingEventHandlerEx _propertyChanging;

		public event PropertyChangedEventHandlerEx PropertyChanged
		{
			add
			{
				var attributes = getAttributes<PropertyChangeFilterAttribute>(value.Method);

				if (attributes.Any())
				{
					var matches = attributes.Select(a => a.PropertyName).ToArray();

					void filterer(object s, PropertyChangedEventArgsEx e)
					{
						if (e.PropertyName.In(matches)) value(s, e);
					}

					changedFilters.Add((value, filterer));

					_propertyChanged += filterer;
				}
				else
					_propertyChanged += value;
			}
			remove
			{
				var del = changedFilters.LastOrDefault(d => d.subscriber == value);
				if (del == default)
					_propertyChanged -= value;
				else
				{
					_propertyChanged -= del.wrapper;
					changedFilters.Remove(del);
				}
			}
		}

		public event PropertyChangingEventHandlerEx PropertyChanging
		{
			add
			{
				var attributes = getAttributes<PropertyChangeFilterAttribute>(value.Method);

				if (attributes.Any())
				{
					var matches = attributes.Select(a => a.PropertyName).ToArray();

					void filterer(object s, PropertyChangingEventArgsEx e)
					{
						if (e.PropertyName.In(matches)) value(s, e);
					}

					changingFilters.Add((value, filterer));

					_propertyChanging += filterer;

				}
				else
					_propertyChanging += value;
			}
			remove
			{
				var del = changingFilters.LastOrDefault(d => d.subscriber == value);
				if (del == default)
					_propertyChanging -= value;
				else
				{
					_propertyChanging -= del.wrapper;
					changingFilters.Remove(del);
				}
			}
		}

		private static T[] getAttributes<T>(MethodInfo methodInfo) where T : Attribute
			=> Attribute.GetCustomAttributes(methodInfo, typeof(T)) as T[];

		#endregion

		#region Observables

		/// <summary>
		/// Clear all subscriptions to Property<b>Changed</b> for <paramref name="propertyName"/>
		/// </summary>
		public void ClearChangedSubscriptions(string propertyName)
		{
			if (propertyChangedActions.ContainsKey(propertyName)
				&& propertyChangedActions[propertyName] is not null)
				propertyChangedActions[propertyName].Clear();
		}

		/// <summary>
		/// Clear all subscriptions to Property<b>Changing</b> for <paramref name="propertyName"/>
		/// </summary>
		public void ClearChangingSubscriptions(string propertyName)
		{
			if (propertyChangingActions.ContainsKey(propertyName)
				&& propertyChangingActions[propertyName] is not null)
				propertyChangingActions[propertyName].Clear();
		}

		/// <summary>
		/// Add an action to be executed when a property's value has changed
		/// </summary>
		/// <typeparam name="T">The <paramref name="propertyName"/>'s <see cref="Type"/></typeparam>
		/// <param name="propertyName">Name of the property whose change triggers the <paramref name="action"/></param>
		/// <param name="action">Action to be executed with the NewValue as a parameter</param>
		/// <returns>A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.</returns>
		public IDisposable ObservePropertyChanged<T>(string propertyName, Action<T> action)
		{
			validateSubscriber<T>(propertyName, action);

			if (!propertyChangedActions.ContainsKey(propertyName))
				propertyChangedActions.Add(propertyName, new List<Delegate>());

			var actionlist = propertyChangedActions[propertyName];

			if (!actionlist.Contains(action))
				actionlist.Add(action);

			return new Unsubscriber(actionlist, action);
		}

		/// <summary>
		/// Add an action to be executed when a property's value is changing
		/// </summary>
		/// <typeparam name="T">The <paramref name="propertyName"/>'s <see cref="Type"/></typeparam>
		/// <param name="propertyName">Name of the property whose change triggers the <paramref name="action"/></param>
		/// <param name="action">Action to be executed with OldValue and NewValue as parameters</param>
		/// <returns>A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.</returns>
		public IDisposable ObservePropertyChanging<T>(string propertyName, Action<T, T> action)
		{
			validateSubscriber<T>(propertyName, action);

			if (!propertyChangingActions.ContainsKey(propertyName))
				propertyChangingActions.Add(propertyName, new List<Delegate>());

			var actionlist = propertyChangingActions[propertyName];

			if (!actionlist.Contains(action))
				actionlist.Add(action);

			return new Unsubscriber(actionlist, action);
		}

		private void validateSubscriber<T>(string propertyName, Delegate action)
		{
			ArgumentValidator.EnsureNotNullOrWhiteSpace(propertyName, nameof(propertyName));
			ArgumentValidator.EnsureNotNull(action, nameof(action));

			var propertyInfo = GetType().GetProperty(propertyName);

			if (propertyInfo is null)
				throw new MissingMemberException($"{nameof(Configuration)}.{propertyName} does not exist.");

			if (propertyInfo.PropertyType != typeof(T))
				throw new InvalidCastException($"{nameof(Configuration)}.{propertyName} is {propertyInfo.PropertyType}, but parameter is {typeof(T)}.");
		}

		private void Configuration_PropertyChanged(object sender, PropertyChangedEventArgsEx e)
		{
			if (propertyChangedActions.ContainsKey(e.PropertyName))
			{
				foreach (var action in propertyChangedActions[e.PropertyName])
				{
					action.DynamicInvoke(e.NewValue);
				}
			}
		}

		private void Configuration_PropertyChanging(object sender, PropertyChangingEventArgsEx e)
		{
			if (propertyChangingActions.ContainsKey(e.PropertyName))
			{
				foreach (var action in propertyChangingActions[e.PropertyName])
				{
					action.DynamicInvoke(e.OldValue, e.NewValue);
				}
			}
		}

		private class Unsubscriber : IDisposable
		{
			private List<Delegate> _observers;
			private Delegate _observer;

			internal Unsubscriber(List<Delegate> observers, Delegate observer)
			{
				_observers = observers;
				_observer = observer;
			}

			public void Dispose()
			{
				if (_observers.Contains(_observer))
					_observers.Remove(_observer);
			}
		}

		#endregion
	}

	public delegate void PropertyChangedEventHandlerEx(object sender, PropertyChangedEventArgsEx e);
	public delegate void PropertyChangingEventHandlerEx(object sender, PropertyChangingEventArgsEx e);

	public class PropertyChangedEventArgsEx : PropertyChangedEventArgs
	{
		public object NewValue { get; }

		public PropertyChangedEventArgsEx(string propertyName, object newValue) : base(propertyName)
		{
			NewValue = newValue;
		}
	}

	public class PropertyChangingEventArgsEx : PropertyChangingEventArgs
	{
		public object OldValue { get; }
		public object NewValue { get; }

		public PropertyChangingEventArgsEx(string propertyName, object oldValue, object newValue) : base(propertyName)
		{
			OldValue = oldValue;
			NewValue = newValue;
		}
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class PropertyChangeFilterAttribute : Attribute
	{
		public string PropertyName { get; }
		public PropertyChangeFilterAttribute(string propertyName)
		{
			PropertyName = propertyName;
		}
	}
}
