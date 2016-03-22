namespace Vurdalakov.IpHelperDotNet
{
    using System;
    using System.Diagnostics;
    using System.Windows.Forms;

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                if (!Debugger.IsAttached && ProcessHelper.RestartApplicationAsAdministrator())
                {
                    return;
                }
            }
            catch { }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
