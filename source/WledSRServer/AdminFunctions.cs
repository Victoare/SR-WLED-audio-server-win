using Microsoft.Win32;
using System.Diagnostics;
using System.Security;
using System.Security.Principal;

namespace WledSRServer
{
    internal static class AdminFunctions
    {
        private const string AppKey = "WLedSRServer";
        private const string RunKeyPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

        public static bool SetAutoRun(bool value)
        {
            try
            {
                try
                {
                    using var registryKey = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
                    if (registryKey == null)
                        return false;

                    if (value)
                        registryKey.SetValue(AppKey, Application.ExecutablePath);
                    else
                        registryKey.DeleteValue(AppKey, false);
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
            using var registryKey = Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
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
