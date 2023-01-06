using Dinah.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace LibationFileManager
{
	public partial class Configuration : INotifyPropertyChanging, INotifyPropertyChanged
	{
		public event PropertyChangingEventHandler PropertyChanging;
		public event PropertyChangedEventHandler PropertyChanged;
		private readonly Dictionary<string, List<MulticastDelegate>> propertyChangedActions = new();
		private readonly Dictionary<string, List<MulticastDelegate>> propertyChangingActions = new();

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
		/// <param name="action">Action to be executed with parameters: <paramref name="propertyName"/> and <strong>NewValue</strong></param>
		/// <returns>A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.</returns>
		public IDisposable SubscribeToPropertyChanged<T>(string propertyName, Action<string, T> action)
		{
			validateSubscriber<T>(propertyName, action);

			if (!propertyChangedActions.ContainsKey(propertyName))
				propertyChangedActions.Add(propertyName, new List<MulticastDelegate>());

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
		/// <param name="action">Action to be executed with parameters: <paramref name="propertyName"/>, <b>OldValue</b>, and <b>NewValue</b></param>
		/// <returns>A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.</returns>
		public IDisposable SubscribeToPropertyChanging<T>(string propertyName, Action<string, T, T> action)
		{
			validateSubscriber<T>(propertyName, action);

			if (!propertyChangingActions.ContainsKey(propertyName))
				propertyChangingActions.Add(propertyName, new List<MulticastDelegate>());

			var actionlist = propertyChangingActions[propertyName];

			if (!actionlist.Contains(action))
				actionlist.Add(action);

			return new Unsubscriber(actionlist, action);
		}

		private void validateSubscriber<T>(string propertyName, MulticastDelegate action)
		{
			ArgumentValidator.EnsureNotNullOrWhiteSpace(propertyName, nameof(propertyName));
			ArgumentValidator.EnsureNotNull(action, nameof(action));

			var propertyInfo = GetType().GetProperty(propertyName);

			if (propertyInfo is null)
				throw new MissingMemberException($"{nameof(Configuration)}.{propertyName} does not exist.");

			if (propertyInfo.PropertyType != typeof(T))
				throw new InvalidCastException($"{nameof(Configuration)}.{propertyName} is {propertyInfo.PropertyType}, but parameter is {typeof(T)}.");
		}

		private void Configuration_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e is PropertyChangedEventArgsEx args && propertyChangedActions.ContainsKey(args.PropertyName))
			{
				foreach (var action in propertyChangedActions[args.PropertyName])
				{
					action.DynamicInvoke(args.PropertyName, args.NewValue);
				}
			}
		}

		private void Configuration_PropertyChanging(object sender, PropertyChangingEventArgs e)
		{
			if (e is PropertyChangingEventArgsEx args && propertyChangingActions.ContainsKey(args.PropertyName))
			{
				foreach (var action in propertyChangingActions[args.PropertyName])
				{
					action.DynamicInvoke(args.PropertyName, args.OldValue, args.NewValue);
				}
			}
		}

		private class PropertyChangingEventArgsEx : PropertyChangingEventArgs
		{
			public object OldValue { get; }
			public object NewValue { get; }

			public PropertyChangingEventArgsEx(string propertyName, object oldValue, object newValue) : base(propertyName)
			{
				OldValue = oldValue;
				NewValue = newValue;
			}
		}

		private class PropertyChangedEventArgsEx : PropertyChangedEventArgs
		{
			public object NewValue { get; }

			public PropertyChangedEventArgsEx(string propertyName, object newValue) : base(propertyName)
			{
				NewValue = newValue;
			}
		}

		private class Unsubscriber : IDisposable
		{
			private List<MulticastDelegate> _observers;
			private MulticastDelegate _observer;

			internal Unsubscriber(List<MulticastDelegate> observers, MulticastDelegate observer)
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
	}
}
