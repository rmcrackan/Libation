using System;

public static class Theme
{
    [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
    private static extern bool ShouldSystemUseDarkMode();

    public static bool IsDarkMode => SystemInformation.HighContrast || ShouldSystemUseDarkMode();
}
