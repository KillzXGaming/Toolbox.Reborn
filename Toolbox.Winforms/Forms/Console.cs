using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Toolbox.Core;
using STLibrary.Forms;

namespace Toolbox.Winforms
{
    public partial class ConsoleWindow : STForm
    {
        public ConsoleWindow()
        {
            InitializeComponent();

            richTextBox1.BackColor = FormThemes.BaseTheme.ConsoleEditorBackColor;
            richTextBox1.ForeColor = FormThemes.BaseTheme.TextForeColor;
            richTextBox1.Multiline = true;
            richTextBox1.ReadOnly = true;
            richTextBox1.DataBindings.Add("Text", STConsole.Instance, "Value", false, DataSourceUpdateMode.OnPropertyChanged);
        }
    }
}
