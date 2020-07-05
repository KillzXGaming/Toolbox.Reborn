using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.GUI;

namespace Toolbox.Core
{
    /// <summary>
    /// Displays properties to the GUI if this object is selected
    /// </summary>
    public interface IEditorDisplay
    {
        object PropertyDisplay { get; }
    }
}
