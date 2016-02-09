using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteGenerator
{
    public partial class Progress : Form, ILogger
    {
        public Progress()
        {
            InitializeComponent();
        }

        public void Log(string text, bool appendLine = false)
        {
            AppendText(text, appendLine, Color.Black);
        }

        public void Log(string text, params object[] format)
        {
            AppendText(string.Format(text, format), false, Color.Black);
        }

        public void Log(string text, bool appendLine, params object[] format)
        {
            AppendText(string.Format(text, format), appendLine, Color.Black);
        }

        public void Clear()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)delegate { txtOutput.Text = string.Empty; });
                return;
            }

            txtOutput.Text = string.Empty;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void AppendText(string text, bool appendLine, Color color)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)delegate
                {
                    AppendText(text, appendLine, color);
                });
                return;
            }

            txtOutput.SelectionStart = txtOutput.TextLength;
            txtOutput.SelectionLength = 0;

            txtOutput.SelectionColor = color;
            txtOutput.AppendText(text);
            txtOutput.SelectionColor = txtOutput.ForeColor;

            if (appendLine)
                txtOutput.AppendText(Environment.NewLine);
        }
    }
}