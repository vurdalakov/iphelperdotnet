namespace Vurdalakov
{
    using System;
    using System.Windows.Forms;

    public static class MsgBox
    {
        public static void Info(String format, params Object[] parameters)
        {
            Show(MessageBoxIcon.Information, MessageBoxButtons.OK, format, parameters);
        }

        public static void Warning(String format, params Object[] parameters)
        {
            Show(MessageBoxIcon.Warning, MessageBoxButtons.OK, format, parameters);
        }

        public static void Error(String format, params Object[] parameters)
        {
            Show(MessageBoxIcon.Error, MessageBoxButtons.OK, format, parameters);
        }

        public static Boolean YesNo(String format, params Object[] parameters)
        {
            return DialogResult.Yes == Show(MessageBoxIcon.Question, MessageBoxButtons.YesNoCancel, format, parameters);
        }

        private static DialogResult Show(MessageBoxIcon icon, MessageBoxButtons buttons, String format, params Object[] parameters)
        {
            var mainForm = Application.OpenForms[0];
            var message = String.Format(format, parameters);
            return MessageBox.Show(mainForm, message, mainForm.Text, buttons, icon);
        }
    }
}
