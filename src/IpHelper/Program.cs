using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Vurdalakov.IpHelperDotNet
{
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
                if (ProcessHelper.RestartApplicationAsAdministrator())
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
