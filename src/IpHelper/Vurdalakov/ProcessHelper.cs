namespace Vurdalakov
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Security.Principal;

    public class ProcessHelper
    {
        public static Boolean RestartApplicationAsAdministrator()
        {
            if (IsAdministrator())
            {
                return false;
            }

            var processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;
            processStartInfo.WorkingDirectory = Environment.CurrentDirectory;
            processStartInfo.Arguments = Environment.CommandLine;

            RunApplicationAsAdministrator(processStartInfo);

            Process.GetCurrentProcess().CloseMainWindow();

            return true;
        }

        public static void RunApplicationAsAdministrator(ProcessStartInfo processStartInfo)
        {
            try
            {
                processStartInfo.UseShellExecute = true;
                processStartInfo.Verb = "runas";

                Process.Start(processStartInfo);
            }
            catch (Win32Exception ex)
            {
                if (1223 == ex.NativeErrorCode)
                {
                    throw new OperationCanceledException(); // //The operation was canceled by the user.
                }
                else
                {
                    throw;
                }
            }
        }

        public static Boolean IsAdministrator()
        {
            var windowsIdentity = WindowsIdentity.GetCurrent();
            var windowsPrincipal = new WindowsPrincipal(windowsIdentity);
            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
