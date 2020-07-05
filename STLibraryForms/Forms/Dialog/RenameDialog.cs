using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace STLibrary.Forms
{
    public partial class RenameDialog : STForm
    {
        public RenameDialog()
        {
            InitializeComponent();

            StartPosition = FormStartPosition.CenterParent;
            button2.Select();
            CanResize = false;
        }
        public void SetString(string Default)
        {
            textBox1.Text = Default;
            button2.Select();
        }

        private void RenameDialog_Load(object sender, EventArgs e)
        {
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
         
            
        }
    }
}
