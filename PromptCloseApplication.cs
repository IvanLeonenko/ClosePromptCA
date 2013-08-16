using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ClosePromptCA
{
    public class PromptCloseApplication : IDisposable
    {
        private readonly string _productName;
        private readonly string _processName;
        private readonly string _displayName;
        private System.Threading.Timer _timer;
        private Form _form;
        private IntPtr _mainWindowHanle;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public PromptCloseApplication(string productName, string processName, string displayName)
        {
            _productName = productName;
            _processName = processName;
            _displayName = displayName;
        }

        public bool Prompt()
        {
            if (IsRunning(_processName))
            {
                _form = new ClosePromptForm(String.Format("Please close running instances of {0} before running {1} setup.", _displayName, _productName));
                _mainWindowHanle = FindWindow(null, _productName + " Setup");
                if (_mainWindowHanle == IntPtr.Zero)
                    _mainWindowHanle = FindWindow("#32770", _productName);

                _timer = new System.Threading.Timer(TimerElapsed, _form, 200, 200);

                return ShowDialog();
            }
            return true;
        }

        bool ShowDialog()
        {
            if (_form.ShowDialog(new WindowWrapper(_mainWindowHanle)) == DialogResult.OK)
                return !IsRunning(_processName) || ShowDialog();
            return false;
        }

        private void TimerElapsed(object sender)
        {
            if (_form == null || IsRunning(_processName) || !_form.Visible)
                return;
            _form.DialogResult = DialogResult.OK;
            _form.Close();
        }

        static bool IsRunning(string processName)
        {
            return Process.GetProcessesByName(processName).Length > 0;
        }

        public void Dispose()
        {
            if (_timer != null)
                _timer.Dispose();
            if (_form != null && _form.Visible)
                _form.Close();
        }
    }
}
