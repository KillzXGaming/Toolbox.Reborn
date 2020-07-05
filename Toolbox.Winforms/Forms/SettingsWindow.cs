using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core.ModelView;
using Toolbox.Core.GUI;

namespace Toolbox.Winforms
{
    public class SettingsWindow : Controls.TabControl
    {
        public SettingsWindow() {
            Pages.Add(new GeneralSettings());
            Pages.Add(new View3DSettings());
            Pages.Add(new PathSettings());
        }
    }

    public class GeneralSettings : Controls.TabPage
    {
        [Controls.TextBox()]
        public string OpenGLVersion { get; set; }
    }

    public class View3DSettings : Controls.TabPage
    {
    }

    public class PathSettings : Controls.TabPage
    {
    }
}
