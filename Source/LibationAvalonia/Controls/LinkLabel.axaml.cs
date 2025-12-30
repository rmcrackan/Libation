using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Windows.Input;

namespace LibationAvalonia.Controls
{
	public partial class LinkLabel : TextBlock, ICommandSource
	{
		protected override Type StyleKeyOverride => typeof(LinkLabel);

		public static readonly StyledProperty<ICommand> CommandProperty =
			AvaloniaProperty.Register<LinkLabel, ICommand>(nameof(Command), enableDataValidation: true);

		public static readonly StyledProperty<object> CommandParameterProperty =
		   AvaloniaProperty.Register<LinkLabel, object>(nameof(CommandParameter));

		public static readonly StyledProperty<IBrush> ForegroundVisitedProperty =
		   AvaloniaProperty.Register<LinkLabel, IBrush>(nameof(ForegroundVisited));

		public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
			RoutedEvent.Register<Button, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble);

		public ICommand Command
		{
			get => GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		public object CommandParameter
		{
			get => GetValue(CommandParameterProperty);
			set => SetValue(CommandParameterProperty, value);
		}

		public IBrush ForegroundVisited
		{
			get => GetValue(ForegroundVisitedProperty);
			set => SetValue(ForegroundVisitedProperty, value);
		}

		public event EventHandler<RoutedEventArgs> Click
		{
			add => AddHandler(ClickEvent, value);
			remove => RemoveHandler(ClickEvent, value);
		}

		private static readonly Cursor HandCursor = new Cursor(StandardCursorType.Hand);
		private bool _commandCanExecute = true;
		public LinkLabel()
		{
			InitializeComponent();
			Tapped += LinkLabel_Tapped;
		}

		private void LinkLabel_Tapped(object? sender, TappedEventArgs e)
		{
			Foreground = ForegroundVisited;
			if (IsEffectivelyEnabled)
			{

				var args = new RoutedEventArgs(ClickEvent);
				RaiseEvent(args);

				if (!args.Handled && Command?.CanExecute(CommandParameter) == true)
				{
					Command.Execute(CommandParameter);
					args.Handled = true;
				}
			}
		}

		protected override void OnPointerEntered(PointerEventArgs e)
		{
			this.Cursor = HandCursor;
			base.OnPointerEntered(e);
		}
		protected override void OnPointerExited(PointerEventArgs e)
		{
			this.Cursor = Cursor.Default;
			base.OnPointerExited(e);
		}
		protected override bool IsEnabledCore => base.IsEnabledCore && _commandCanExecute;

		protected override void UpdateDataValidation(AvaloniaProperty property, BindingValueType state, Exception? error)
		{
			base.UpdateDataValidation(property, state, error);
			if (property == CommandProperty)
			{
				var canExecure = !state.HasFlag(BindingValueType.HasError);
				if (canExecure != _commandCanExecute)
				{
					_commandCanExecute = canExecure;
					UpdateIsEffectivelyEnabled();
				}
			}
		}

		public void CanExecuteChanged(object sender, EventArgs e)
		{
			var canExecute = Command == null || Command.CanExecute(CommandParameter);

			if (canExecute != _commandCanExecute)
			{
				_commandCanExecute = canExecute;
				UpdateIsEffectivelyEnabled();
			}
		}
	}
}
