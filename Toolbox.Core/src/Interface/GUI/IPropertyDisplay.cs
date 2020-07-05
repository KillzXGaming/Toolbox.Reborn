using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core
{
    /// <summary>
    /// Displays properties to the GUI if this object is selected
    /// </summary>
    public interface IPropertyDisplay
    {
        object PropertyDisplay { get; }
    }
}
