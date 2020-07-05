using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.Animations;

namespace Toolbox.Core
{
    /// <summary>
    /// A container of animations.
    /// </summary>
    public interface IAnimationContainer
    {
        IEnumerable<STAnimation> AnimationList { get; }
    }
}
