using Microsoft.Win32;
using System.Diagnostics;
using System.Security;
using System.Security.Principal;

namespace WledSRServer
{
    internal static class AdminFunctions
    {
        private const string AppKey = "WLedSRServer";

        public static bool SetAutoRun(bool value)
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                try
                {
                    if (value)
                        registryKey.SetValue(AppKey, Application.ExecutablePath);
                    else
                        registryKey.DeleteValue(AppKey);
                }
                catch (SecurityException)
                {
                    if (IsElevated())
                        return false;

                    // Rerun app as admin 
                    var adminProcess = new ProcessStartInfo(Application.ExecutablePath)
                    {
                        WorkingDirectory = Environment.CurrentDirectory,
                        Arguments = $"-setAutoRun={value}",
                        Verb = "runas",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = true,
                    };
                    using (var process = Process.Start(adminProcess))
                        process?.WaitForExit();

                }
            }
            catch
            {
                return false;
            }

            return GetAutoRun() == value;
        }

        public static bool GetAutoRun()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);
            return registryKey?.GetValue(AppKey)?.ToString() == Application.ExecutablePath;
        }

        public static bool IsElevated()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

    }
}
