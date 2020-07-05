using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.ModelView;

namespace Toolbox.Core
{
    /// <summary>
    /// Represents an array of menu items 
    /// which can load custom editors in the main window.
    /// </summary>
    public interface ICustomEditor
    {
        ToolMenuItem[] ToolMenuItem { get; }
    }
}
