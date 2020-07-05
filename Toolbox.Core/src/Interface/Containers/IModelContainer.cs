using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core
{
    /// <summary>
    /// A container of models.
    /// </summary>
    public interface IModelContainer
    {
        IEnumerable<IModelFormat> ModelList { get; }
    }
}
