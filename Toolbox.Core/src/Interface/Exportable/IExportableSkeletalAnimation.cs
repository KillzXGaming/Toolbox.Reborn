using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.Animations;

namespace Toolbox.Core
{
    public interface IExportableSkeletalAnimation
    {
        void Export(STAnimation model, STSkeleton skeleton, string filePath);
    }
}
