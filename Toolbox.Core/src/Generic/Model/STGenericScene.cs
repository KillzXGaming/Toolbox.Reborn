using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core
{
    /// <summary>
    /// Represents a scene which contains data such as models or animations.
    /// </summary>
    public class STGenericScene
    {
        /// <summary>
        /// The name of the resource.
        /// </summary>
        public string Name { get; set; }

        public List<STGenericModel> Models = new List<STGenericModel>();

        public List<STGenericTexture> Textures = new List<STGenericTexture>();
    }
}