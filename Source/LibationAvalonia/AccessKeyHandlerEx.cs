using Avalonia;
using Avalonia.Input;
using System.Linq;

namespace LibationAvalonia
{
    internal class AccessKeyHandlerEx : AccessKeyHandler
    {
        public KeyModifiers KeyModifier { get; }
        private readonly Key[] ActivatorKeys;

        public AccessKeyHandlerEx(KeyModifiers menuKeyModifier)
        {
            KeyModifier = menuKeyModifier;
            ActivatorKeys = menuKeyModifier switch
            {
                KeyModifiers.Alt => new[] { Key.LeftAlt, Key.RightAlt },
                KeyModifiers.Control => new[] { Key.LeftCtrl, Key.RightCtrl },
                KeyModifiers.Meta => new[] { Key.LWin, Key.RWin },
                _ => throw new System.NotSupportedException($"{nameof(KeyModifiers)}.{menuKeyModifier} is not implemented"),
            };
        }

        protected override void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (ActivatorKeys.Contains(e.Key))
            {
                var newArgs = new KeyEventArgs
                {
                    Key = Key.LeftAlt,
                    Handled = e.Handled,
                    KeyModifiers = e.KeyModifiers,
                };
                base.OnPreviewKeyDown(sender, newArgs);
                e.Handled = newArgs.Handled;
            }
        }

        protected override void OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (ActivatorKeys.Contains(e.Key))
            {
                var newArgs = new KeyEventArgs()
                {
                    Key = Key.LeftAlt,
                    Handled = e.Handled,
                    KeyModifiers = e.KeyModifiers,
                };
                base.OnPreviewKeyUp(sender, newArgs);
                e.Handled = newArgs.Handled;
            }
        }

        protected override void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyModifiers.HasAllFlags(KeyModifier))
            {
                var newArgs = new KeyEventArgs
                {
                    Key = e.Key,
                    Handled = e.Handled,
                    KeyModifiers = KeyModifiers.Alt,
                };
                base.OnKeyDown(sender, newArgs);
                e.Handled = newArgs.Handled;
            }
        }
    }
}
