using HangoverBase;

namespace HangoverWinForms;

internal static class HangoverMutationConfirm
{
	public static bool Confirm(IWin32Window owner, string actionDescription)
		=> MessageBox.Show(
			owner,
			HangoverDbMutation.BuildConfirmMessage(actionDescription),
			HangoverDbMutation.ConfirmTitle,
			MessageBoxButtons.YesNo,
			MessageBoxIcon.Warning) == DialogResult.Yes;
}
