using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using STLibrary.Forms;

namespace Toolbox.Winforms
{
    public partial class ProgressWindow : STForm
    {
        private static ProgressWindow ProgressForm;

        private Thread Thread;

        public ProgressWindow()
        {
            InitializeComponent();

            FormClosing += (sender, args) => { if (Thread != null) Thread.Abort(); };
            TopMost = true;
            CanResize = false;
        }

        public static void Start(ThreadStart action, Form parent = null) {
            if (ProgressForm == null || ProgressForm.IsDisposed)
            {
                ProgressForm = new ProgressWindow();
                ProgressForm.StartPosition = FormStartPosition.CenterScreen;
                ProgressForm.Show();
            }
            ProgressForm.Thread = new Thread(action) { IsBackground = true };
            ProgressForm.Thread.Start();
        }

        public static void Update(string text, int amount, bool continuous = false) {
            if (ProgressForm.InvokeRequired)
            {
                ProgressForm.Invoke((MethodInvoker)delegate {
                    ProgressForm.UpdateProgressBar(text, amount, continuous);
                });
            }
            else
                ProgressForm.UpdateProgressBar(text, amount, continuous);
        }

        public static void CloseProgressBar()
        {
            if (ProgressForm == null) return;

            if (ProgressForm.InvokeRequired)
            {
                ProgressForm.Invoke((MethodInvoker)delegate {
                    ProgressForm.Close();
                    ProgressForm = null;
                });
            }
            else
            {
                ProgressForm.Close();
                ProgressForm = null;
            }
        }

        public static void Wait(int value)
        {
            Thread.Sleep(value);
        }

        void UpdateProgressBar(string text, int amount, bool continuous = false)
        {
            progressLabel.Text = text;
            progressBar1.Value = amount;
            progressBar1.Style = ProgressBarStyle.Blocks;
            if (continuous)
            {
                progressBar1.MarqueeAnimationSpeed = 30;
                progressBar1.Style = ProgressBarStyle.Marquee;
            }
            else
                progressBar1.MarqueeAnimationSpeed = 0;

            if (!continuous && amount >= 100)
                CloseProgressBar();
        }

        private void stPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
