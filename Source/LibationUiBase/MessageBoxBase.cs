using System.Threading.Tasks;

#nullable enable
namespace LibationUiBase.Forms;

public enum DialogResult
{
	/// <summary> Nothing is returned from the dialog box. This means that the modal dialog continues running. </summary>
	None = 0,
	/// <summary> The dialog box return value is OK (usually sent from a button labeled OK). </summary>
	OK = 1, //IDOK
	/// <summary> The dialog box return value is Cancel (usually sent from a button labeled Cancel). </summary>
	Cancel = 2, //IDCANCEL
	/// <summary> The dialog box return value is Abort (usually sent from a button labeled Abort). </summary>
	Abort = 3, //IDABORT
	/// <summary> The dialog box return value is Retry (usually sent from a button labeled Retry). </summary>
	Retry = 4, //IDRETRY
	/// <summary> The dialog box return value is Ignore (usually sent from a button labeled Ignore). </summary>
	Ignore = 5, //IDIGNORE
	/// <summary> The dialog box return value is Yes (usually sent from a button labeled Yes). </summary>
	Yes = 6, //IDYES
	/// <summary> The dialog box return value is No (usually sent from a button labeled No). </summary>
	No = 7, //IDNO
	/// <summary> The dialog box return value is Try Again (usually sent from a button labeled Try Again). </summary>
	TryAgain = 10, //IDTRYAGAIN
	/// <summary> The dialog box return value is Continue (usually sent from a button labeled Continue). </summary>
	Continue = 11 //IDCONTINUE
}

public enum MessageBoxIcon
{
	/// <summary> Specifies that the message box contain no symbols. </summary>
	None = 0x00000000,
	/// <summary> Specifies that the message box contains a hand symbol. </summary>
	Hand = 0x00000010, //MB_ICONHAND
	/// <summary> Specifies that the message box contains a question mark symbol. </summary>
	Question = 0x00000020, //MB_ICONQUESTION
	/// <summary> Specifies that the message box contains an exclamation symbol. </summary>
	Exclamation = 0x00000030, //MB_ICONEXCLAMATION
	/// <summary> Specifies that the message box contains an asterisk symbol. </summary>
	Asterisk = 0x00000040, //MB_ICONASTERISK
	/// <summary> Specifies that the message box contains a hand icon. This field is constant. </summary>
	Stop = Hand,
	/// <summary> Specifies that the message box contains a hand icon. </summary>
	Error = Hand,
	/// <summary> Specifies that the message box contains an exclamation icon. </summary>
	Warning = Exclamation,
	/// <summary> Specifies that the message box contains an asterisk icon. </summary>
	Information = Asterisk
}

public enum MessageBoxButtons
{
	/// <summary> Specifies that the message box contains an OK button. </summary>
	OK = 0x00000000, //MB_OK
	/// <summary> Specifies that the message box contains OK and Cancel buttons. </summary>
	OKCancel = 0x00000001, //MB_OKCANCEL
	/// <summary> Specifies that the message box contains Abort, Retry, and Ignore buttons. </summary>
	AbortRetryIgnore = 0x00000002, //MB_ABORTRETRYIGNORE
	/// <summary> Specifies that the message box contains Yes, No, and Cancel buttons. </summary>
	YesNoCancel = 0x00000003, //MB_YESNOCANCEL
	/// <summary> Specifies that the message box contains Yes and No buttons. </summary>
	YesNo = 0x00000004, //MB_YESNO
	/// <summary> Specifies that the message box contains Retry and Cancel buttons. </summary>
	RetryCancel = 0x00000005, //MB_RETRYCANCEL
	/// <summary> Specifies that the message box contains Cancel, Try Again, and Continue buttons. </summary>
	CancelTryContinue = 0x00000006 //MB_CANCELTRYCONTINUE
}

public enum MessageBoxDefaultButton
{
	/// <summary> Specifies that the first button on the message box should be the default button. </summary>
	Button1 = 0x00000000, //MB_DEFBUTTON1
	/// <summary> Specifies that the second button on the message box should be the default button. </summary>
	Button2 = 0x00000100, //MB_DEFBUTTON2
	/// <summary> Specifies that the third button on the message box should be the default button. </summary>
	Button3 = 0x00000200, //MB_DEFBUTTON3
	/// <summary> Specifies that the Help button on the message box should be the default button. </summary>
	Button4 = 0x00000300, //MB_DEFBUTTON4
}

/// <summary>
/// Displays a message box in front of the specified object and with the specified text, caption, buttons, icon, and default button.
/// </summary>
/// <param name="owner">An implementation of a GUI window that will own the modal dialog box</param>
/// <param name="message">The text to display in the message box</param>
/// <param name="caption">The text to display in the title bar of the message box</param>
/// <param name="buttons">One of the <see cref="MessageBoxButtons"/> values that specifies which buttons to disply in the message box</param>
/// <param name="icon">One of the <see cref="MessageBoxIcon"/> values that specifies which icon to disply in the message box</param>
/// <param name="defaultButton">One of the <see cref="MessageBoxDefaultButton"/> values that specifies the default button of the message box</param>
/// <param name="saveAndRestorePosition">A value indicating whether the message box's position should be saved and restored the next time it is shown</param>
/// <returns>One of the <see cref="DialogResult"/> values</returns>
public delegate Task<DialogResult> ShowAsyncDelegate(object? owner, string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, bool saveAndRestorePosition = true);

public static class MessageBoxBase
{
	private static ShowAsyncDelegate? s_ShowAsyncImpl;
	public static ShowAsyncDelegate ShowAsyncImpl
	{
		get => s_ShowAsyncImpl ?? DefaultShowAsyncImpl;
		set => s_ShowAsyncImpl = value;
	}

	private static Task<DialogResult> DefaultShowAsyncImpl(object? owner, string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, bool saveAndRestorePosition = true)
	{
		// default to a no-op impl
		Serilog.Log.Logger.Error("MessageBoxBase implementation not set. {@DebugInfo}", new { owner, message, caption, buttons, icon, defaultButton });
		return Task.FromResult(DialogResult.None);
	}

	public static Task<DialogResult> Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, bool saveAndRestorePosition = true)
		=> ShowAsyncImpl(null, text, caption, buttons, icon, defaultButton);
	public static Task<DialogResult> Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, bool saveAndRestorePosition = true)
		=> ShowAsyncImpl(null, text, caption, buttons, icon, MessageBoxDefaultButton.Button1, saveAndRestorePosition);
	public static Task<DialogResult> Show(string text, string caption, MessageBoxButtons buttons)
		=> ShowAsyncImpl(null, text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
	public static Task<DialogResult> Show(string text, string caption)
		=> ShowAsyncImpl(null, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
	public static Task<DialogResult> Show(string text)
		=> ShowAsyncImpl(null, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
	public static Task<DialogResult> Show(object? owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		=> ShowAsyncImpl(owner, text, caption, buttons, icon, defaultButton);
	public static Task<DialogResult> Show(object? owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		=> ShowAsyncImpl(owner, text, caption, buttons, icon, MessageBoxDefaultButton.Button1);
	public static Task<DialogResult> Show(object? owner, string text, string caption, MessageBoxButtons buttons)
		=> ShowAsyncImpl(owner, text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
	public static Task<DialogResult> Show(object? owner, string text, string caption)
		=> ShowAsyncImpl(owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
	public static Task<DialogResult> Show(object? owner, string text)
		=> ShowAsyncImpl(owner, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
}