using LibationUiBase.Forms;
using System;
using LibationUiBase.Resources;

namespace LibationAvalonia.ViewModels.Dialogs;

public class MessageBoxViewModel
{
	public string? Message { get => field; set => field = value; }
	public string Caption { get; } = Strings.MessageBoxViewModel_MessageBox;
	private readonly MessageBoxButtons _button;
	private readonly MessageBoxIcon _icon;
	private readonly MessageBoxDefaultButton _defaultButton;

	public MessageBoxDefaultButton DefaultButton => _defaultButton;
	public MessageBoxButtons Buttons => _button;

	public bool IsAsterisk => _icon == MessageBoxIcon.Asterisk;
	public bool IsError => _icon == MessageBoxIcon.Error;
	public bool IsQuestion => _icon == MessageBoxIcon.Question;
	public bool IsExclamation => _icon == MessageBoxIcon.Exclamation;

	public bool HasButton3 => !string.IsNullOrEmpty(Button3Text);
	public bool HasButton2 => !string.IsNullOrEmpty(Button2Text);

	public int WindowHeight { get; private set; }
	public int WindowWidth { get; private set; }

	public string Button1Text => _button switch
	{
		MessageBoxButtons.OK => Strings.MessageBoxViewModel_OK,
		MessageBoxButtons.OKCancel => Strings.MessageBoxViewModel_OK,
		MessageBoxButtons.AbortRetryIgnore => Strings.MessageBoxViewModel_Abort,
		MessageBoxButtons.YesNoCancel => Strings.MessageBoxViewModel_Yes,
		MessageBoxButtons.YesNo => Strings.MessageBoxViewModel_Yes,
		MessageBoxButtons.RetryCancel => Strings.MessageBoxViewModel_Retry,
		MessageBoxButtons.CancelTryContinue => Strings.MessageBoxViewModel_Cancel,
		_ => string.Empty,
	};

	public string Button2Text => _button switch
	{
		MessageBoxButtons.OKCancel => Strings.MessageBoxViewModel_Cancel,
		MessageBoxButtons.AbortRetryIgnore => Strings.MessageBoxViewModel_Retry,
		MessageBoxButtons.YesNoCancel => Strings.MessageBoxViewModel_No,
		MessageBoxButtons.YesNo => Strings.MessageBoxViewModel_No,
		MessageBoxButtons.RetryCancel => Strings.MessageBoxViewModel_Cancel,
		MessageBoxButtons.CancelTryContinue => Strings.MessageBoxViewModel_Try,
		_ => string.Empty,
	};

	public string Button3Text => _button switch
	{
		MessageBoxButtons.AbortRetryIgnore => Strings.MessageBoxViewModel_Ignore,
		MessageBoxButtons.YesNoCancel => Strings.MessageBoxViewModel_Cancel,
		MessageBoxButtons.CancelTryContinue => Strings.MessageBoxViewModel_Continue,
		_ => string.Empty,
	};

	public int TextBlockMinWidth { get; }

	public double FormHeightFromTboxHeight(double tboxHeight) => tboxHeight + 65;
	public double FormWidthFromTboxWidth(double tboxWidth)
	{
		int iconWidth = _icon is MessageBoxIcon.None ? 0 : 42;
		return tboxWidth + 30 + iconWidth;
	}

	public MessageBoxViewModel() { }
	public MessageBoxViewModel(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultBtn)
	{

		Message = message;
		Caption = caption;
		_button = buttons;
		_icon = icon;
		_defaultButton = defaultBtn;

		int numBtns = HasButton3 ? 3 : HasButton2 ? 2 : 1;
		int iconWidth = icon is MessageBoxIcon.None ? 0 : 42;
		int formMinWidth = Math.Max(85 * numBtns + 10, 71 + iconWidth + 20);
		TextBlockMinWidth = formMinWidth - 30 - iconWidth;
	}
}
