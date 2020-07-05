using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Toolbox.Winforms
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Toolbox.Core.Runtime.ExecutableDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            STLibrary.Forms.FormThemes.ActivePreset = STLibrary.Forms.FormThemes.Preset.Dark;

            Application.Run(new MainForm());
        }
    }
}
