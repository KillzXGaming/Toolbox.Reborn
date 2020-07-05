using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core
{
    /// <summary>
    /// Calls a dialog to open for a File Format when it is loaded.
    /// </summary>
    public interface IFileSelectRequest
    {
        /// <summary>
        /// The request to show in the dialog.
        /// </summary>
        string Request { get; }

        /// <summary>
        /// The event to run the open file dlalog
        /// </summary>
        EventHandler OpenDialog { get; }
    }
}
