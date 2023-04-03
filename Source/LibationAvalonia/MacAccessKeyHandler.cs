using Avalonia;
using Avalonia.Input;

namespace LibationAvalonia
{
    internal class MacAccessKeyHandler : AccessKeyHandler
    {
        protected override void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
			if (e.Key is Key.LWin or Key.RWin)
            {
                var newArgs = new KeyEventArgs { Key = Key.LeftAlt, Handled = e.Handled };
                base.OnPreviewKeyDown(sender, newArgs);
                e.Handled = newArgs.Handled;
            }
            else if (e.Key is not Key.LeftAlt and not Key.RightAlt)
				base.OnPreviewKeyDown(sender, e);
		}

        protected override void OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.LWin or Key.RWin)
            {
                var newArgs = new KeyEventArgs { Key = Key.LeftAlt,  Handled = e.Handled };
                base.OnPreviewKeyUp(sender, newArgs);
                e.Handled = newArgs.Handled;
			}
			else if (e.Key is not Key.LeftAlt and not Key.RightAlt)
				base.OnPreviewKeyDown(sender, e);
		}

        protected override void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyModifiers.HasAllFlags(KeyModifiers.Meta))
            {
                var newArgs = new KeyEventArgs { Key = e.Key, Handled = e.Handled, KeyModifiers = KeyModifiers.Alt };
                base.OnKeyDown(sender, newArgs);
                e.Handled = newArgs.Handled;
			}
			else if (!e.KeyModifiers.HasFlag(KeyModifiers.Alt))
				base.OnPreviewKeyDown(sender, e);
		}
    }
}
