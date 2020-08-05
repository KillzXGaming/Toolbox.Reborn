using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core.GUI
{
    public class SaveFileDialog
    {
        public static EventHandler RunDialog;

        public string[] FileNames { get; set; }
        public string FileName { get; set; }

        public enum Result
        {
            None,
            Cancel,
            OK,
        }

        public Result ShowDialog()
        {
            RunDialog?.Invoke(null, EventArgs.Empty);

            return Result.None;
        }
    }
}
