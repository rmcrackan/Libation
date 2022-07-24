using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Data;

namespace LibationAvalonia.AvaloniaUI.Views.Dialogs.Login
{
	public partial class MfaDialog : DialogWindow
	{
		public string SelectedName { get; private set; }
		public string SelectedValue { get; private set; }
		private RbValues Values { get; } = new();

		public MfaDialog()
		{
			InitializeComponent();

			if (Design.IsDesignMode)
			{
				var mfaConfig = new AudibleApi.MfaConfig { Title = "My title" };
				mfaConfig.Buttons.Add(new() { Text = "Enter the OTP from the authenticator app", Name = "otpDeviceContext", Value = "aAbBcC=, TOTP" });
				mfaConfig.Buttons.Add(new() { Text = "Send an SMS to my number ending with 123", Name = "otpDeviceContext", Value = "dDeEfE=, SMS" });
				mfaConfig.Buttons.Add(new() { Text = "Call me on my number ending with 123", Name = "otpDeviceContext", Value = "dDeEfE=, VOICE" });

				loadRadioButtons(mfaConfig);
			}
		}

		public MfaDialog(AudibleApi.MfaConfig mfaConfig) : this()
		{
			loadRadioButtons(mfaConfig);
		}

		private void loadRadioButtons(AudibleApi.MfaConfig mfaConfig)
		{
			if (!string.IsNullOrWhiteSpace(mfaConfig.Title))
				Title = mfaConfig.Title;

			rbStackPanel = this.Find<StackPanel>(nameof(rbStackPanel));

			foreach (var conf in mfaConfig.Buttons)
			{
				var rb = new RbValue(conf);
				Values.AddButton(rb);

				RadioButton radioButton = new()
				{
					Content = new TextBlock { Text = conf.Text },
					Margin = new Thickness(0, 10, 0, 0),
				};

				radioButton.Bind(
					RadioButton.IsCheckedProperty,
					new Binding
					{
						Source = rb,
						Path = nameof(rb.IsChecked)
					});

				rbStackPanel.Children.Add(radioButton);
			}
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		protected override async Task SaveAndCloseAsync()
		{
			var selected = Values.CheckedButton;

			Serilog.Log.Logger.Information("Submit button clicked: {@DebugInfo}", new
			{
				text = selected?.Text,
				name = selected?.Name,
				value = selected?.Value
			});
			if (selected is null)
			{
				MessageBox.Show("No MFA option selected", "None selected", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			SelectedName = selected.Name;
			SelectedValue = selected.Value;

			await base.SaveAndCloseAsync();
		}

		public async void Submit_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> await SaveAndCloseAsync();


		private class RbValue : ViewModels.ViewModelBase
		{
			private bool _isChecked;
			public bool IsChecked
			{
				get => _isChecked;
				set => this.RaiseAndSetIfChanged(ref _isChecked, value);
			}
			public AudibleApi.MfaConfigButton MfaConfigButton { get; }
			public RbValue(AudibleApi.MfaConfigButton mfaConfig)
			{
				MfaConfigButton = mfaConfig;
			}
		}

		private class RbValues
		{
			private List<RbValue> ButtonValues { get; } = new();

			public AudibleApi.MfaConfigButton CheckedButton => ButtonValues.SingleOrDefault(rb => rb.IsChecked)?.MfaConfigButton;

			public void AddButton(RbValue rbValue)
			{
				if (ButtonValues.Contains(rbValue))
					return;

				rbValue.PropertyChanged += RbValue_PropertyChanged;
				rbValue.IsChecked = ButtonValues.Count == 0;
				ButtonValues.Add(rbValue);
			}

			private void RbValue_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
			{
				var button = sender as RbValue;

				if (button.IsChecked)
				{
					foreach (var rb in ButtonValues.Where(rb => rb != button))
						rb.IsChecked = false;
				}
			}
		}
	}
}
