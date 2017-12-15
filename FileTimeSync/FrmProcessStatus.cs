using System;
using System.Threading;
using System.Windows.Forms;

namespace FileTimeSync
{
    public partial class FrmProcessStatus : Form
    {
        private bool _isClose;
        private Thread _thread;
        private readonly Func<bool> _checkIsToClose;

        //指定透明度和消息
        /// <summary>
        /// 指定透明度和消息
        /// </summary>
        /// <param name="alpha">透明度 10表示10%</param>
        /// <param name="msg">消息</param>
        /// <param name="checkIsToClose">检查是否需要关闭窗体的方法</param>
        public FrmProcessStatus(int alpha, string msg, Func<bool> checkIsToClose)
        {
            InitializeComponent();

            _checkIsToClose = checkIsToClose;

            Opacity = (double)alpha / 100;

            lblInfo.Text = msg;
            _isClose = false;
        }

        private void FrmProcessStatus_Shown(object sender, EventArgs e)
        {
            _thread = new Thread(ResetLabel);
            _thread.Start();
        }

        private void ResetLabel()
        {
            while (true)
            {
                if (_isClose)
                    return;
                if (_checkIsToClose != null && _checkIsToClose())
                {
                    CloseForm();
                    break;
                }

                Thread.Sleep(500);

                SetInfoVisible();
            }
        }

        private delegate void SetInfoVisibleHandler();

        private void SetInfoVisible()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new SetInfoVisibleHandler(SetInfoVisible));
            }
            else
            {
                lblInfo.Visible = !lblInfo.Visible;
            }
        }

        private delegate void CloseFormHandler();

        private void CloseForm()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new CloseFormHandler(CloseForm));
            }
            else
            {
                Close();
            }
        }

        private void FrmProcessStatus_FormClosing(object sender, FormClosingEventArgs e)
        {
            _isClose = true;
            if (_thread != null)
                _thread.Abort();
        }
    }
}
