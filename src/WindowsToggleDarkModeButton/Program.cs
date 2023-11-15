using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace WindowsToggleDarkModeButton;

static class Program
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

    private const uint SPI_SETCLIENTAREAANIMATION = 0x1043;
    private const uint SPIF_UPDATEINIFILE = 0x01;
    private const uint SPIF_SENDCHANGE = 0x02;

    private const string ThemeKeyName =
        @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    private const string AppSettingName = "AppsUseLightTheme";
    private const string SystemSettingName = "SystemUsesLightTheme";
    private static NotifyIcon _trayIcon;

    private static DateTime _lastClick = DateTime.Now;

    [STAThread]
    static void Main()
    {
        _trayIcon = new NotifyIcon();
        _trayIcon.Icon = new Icon("img/sun.ico");
        _trayIcon.Text = "Toggle Dark Mode";

        object? appSetting = Registry.GetValue(ThemeKeyName, AppSettingName, null);
        if (appSetting != null)
        {
            SetIcon((int)appSetting == 1);
        }

        _trayIcon.Visible = true;

        _trayIcon.MouseClick += TrayIcon_MouseClick;

        Application.Run();
    }

    private static void TrayIcon_MouseClick(object sender, MouseEventArgs e)
    {
        if (_lastClick.AddSeconds(5) > DateTime.Now)
        {
            return;
        }

        if (e.Button == MouseButtons.Right)
        {
            _lastClick = DateTime.Now;

            ToggleDarkMode();
        }
    }

    private static void ToggleDarkMode()
    {
        object? appSetting = Registry.GetValue(ThemeKeyName, AppSettingName, null);
        if (appSetting != null)
        {
            int newSetting = (int)appSetting == 1 ? 0 : 1;
            SetIcon(newSetting == 1);
            Registry.SetValue(ThemeKeyName, AppSettingName, newSetting, RegistryValueKind.DWord);
        }

        var currentSetting = Registry.GetValue(ThemeKeyName, SystemSettingName, null);
        if (currentSetting != null)
        {
            int newSetting = (int)currentSetting == 1 ? 0 : 1;
            Registry.SetValue(ThemeKeyName, SystemSettingName, newSetting, RegistryValueKind.DWord);
        }

        SystemParametersInfo(SPI_SETCLIENTAREAANIMATION, 0, IntPtr.Zero, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        RestartFileExplorer();
    }

    private static void SetIcon(bool isCurrentModeLight)
    {
        _trayIcon.Icon = isCurrentModeLight ? new Icon("img/moon.ico") : new Icon("img/sun.ico");
    }

    private static void RestartFileExplorer()
    {
        foreach (var process in Process.GetProcessesByName("explorer"))
        {
            process.Kill();
        }

        //Process.Start("explorer.exe");
    }
}