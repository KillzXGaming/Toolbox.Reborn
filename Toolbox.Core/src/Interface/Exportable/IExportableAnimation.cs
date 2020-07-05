using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.Animations;

namespace Toolbox.Core
{
    public interface IExportableAnimation
    {
        void Export(STAnimation model, string filePath);
    }
}
