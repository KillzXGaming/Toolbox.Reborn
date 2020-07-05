using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using STLibrary.Forms;

namespace Toolbox.Winforms
{
    public partial class DropdownPanel : STDropDownPanel
    {
        public DropdownPanel()
        {
            InitializeComponent();
        }

        public void AddControl(Control control) {
            control.Dock = DockStyle.Fill;
            stPanel1.Controls.Add(control);
        }
    }
}
