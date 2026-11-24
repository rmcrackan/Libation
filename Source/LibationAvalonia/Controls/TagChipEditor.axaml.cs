using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibationAvalonia.Controls;

/// <summary>Chip-style tag editor; <see cref="Tags"/> is space-separated, matching UserDefinedItem.Tags.</summary>
public partial class TagChipEditor : UserControl
{
	public static readonly StyledProperty<string?> TagsProperty =
		AvaloniaProperty.Register<TagChipEditor, string?>(nameof(Tags), "", defaultBindingMode: BindingMode.TwoWay);

	private readonly List<string> _tags = [];
	private bool _updating;

	public string? Tags
	{
		get => GetValue(TagsProperty);
		set => SetValue(TagsProperty, value);
	}

	public TextBox InputTextBox => Input;

	public TagChipEditor()
	{
		InitializeComponent();
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);
		if (change.Property == TagsProperty && !_updating)
		{
			_tags.Clear();
			foreach (var t in (Tags ?? "").Split((char[]?)null, System.StringSplitOptions.RemoveEmptyEntries).Select(Normalize))
				if (t.Length > 0 && !_tags.Contains(t))
					_tags.Add(t);
			Rebuild();
		}
	}

	// Mirrors UserDefinedItem's sanitiser: lower case, hyphen to underscore, letters/digits/underscore only.
	private static string Normalize(string input)
	{
		var sb = new StringBuilder();
		foreach (var c in input.Trim().ToLowerInvariant().Replace('-', '_'))
			if (char.IsLetterOrDigit(c) || c == '_')
				sb.Append(c);
		return sb.ToString();
	}

	public void CommitPending()
	{
		if (string.IsNullOrWhiteSpace(Input.Text))
			return;
		Commit(Input.Text);
		Input.Text = "";
	}

	private void Commit(string text)
	{
		var tag = Normalize(text);
		if (tag.Length > 0 && !_tags.Contains(tag))
		{
			_tags.Add(tag);
			Publish();
		}
	}

	private void Publish()
	{
		_updating = true;
		Tags = string.Join(" ", _tags);
		_updating = false;
		Rebuild();
	}

	private void Rebuild()
	{
		for (var i = Panel.Children.Count - 2; i >= 0; i--)
			Panel.Children.RemoveAt(i);

		for (var i = 0; i < _tags.Count; i++)
			Panel.Children.Insert(i, CreateChip(_tags[i]));
	}

	private Control CreateChip(string tag)
	{
		var remove = new Button
		{
			Content = "×",
			Width = 18,
			Height = 18,
			Padding = new Thickness(0, 0, 0, 2),
			FontSize = 14,
			CornerRadius = new CornerRadius(999),
			Background = Brushes.Transparent,
			BorderThickness = new Thickness(0),
			HorizontalContentAlignment = HorizontalAlignment.Center,
			VerticalContentAlignment = VerticalAlignment.Center,
			Focusable = false
		};
		remove.Bind(ForegroundProperty, remove.GetResourceObservable("ChipForegroundBrush"));
		ToolTip.SetTip(remove, $"Remove tag {tag}");
		remove.Click += (_, _) =>
		{
			_tags.Remove(tag);
			Publish();
		};

		var label = new TextBlock
		{
			Text = tag,
			FontSize = 13,
			FontWeight = FontWeight.Medium,
			VerticalAlignment = VerticalAlignment.Center
		};
		label.Bind(TextBlock.ForegroundProperty, label.GetResourceObservable("ChipForegroundBrush"));

		var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4, Children = { label, remove } };
		var chip = new Border
		{
			CornerRadius = new CornerRadius(999),
			Padding = new Thickness(10, 2, 4, 2),
			Margin = new Thickness(0, 2, 6, 2),
			Child = panel
		};
		chip.Bind(Border.BackgroundProperty, chip.GetResourceObservable("ChipBackgroundBrush"));
		return chip;
	}

	private void Input_KeyDown(object? sender, KeyEventArgs e)
	{
		if (e.Key is Key.Enter or Key.Space && !string.IsNullOrWhiteSpace(Input.Text))
		{
			Commit(Input.Text);
			Input.Text = "";
			e.Handled = true;
		}
		else if (e.Key is Key.Enter or Key.Space)
		{
			e.Handled = e.Key == Key.Space;
		}
		else if (e.Key == Key.Back && string.IsNullOrEmpty(Input.Text) && _tags.Count > 0)
		{
			_tags.RemoveAt(_tags.Count - 1);
			Publish();
			e.Handled = true;
		}
	}

	private void Input_TextChanged(object? sender, TextChangedEventArgs e)
	{
		var text = Input.Text;
		if (text is null || !text.Contains(','))
			return;

		var parts = text.Split(',');
		foreach (var part in parts[..^1])
			Commit(part);
		Input.Text = parts[^1];
		Input.CaretIndex = Input.Text.Length;
	}

	private void Input_LostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		if (!string.IsNullOrWhiteSpace(Input.Text))
		{
			Commit(Input.Text);
			Input.Text = "";
		}
	}
}
