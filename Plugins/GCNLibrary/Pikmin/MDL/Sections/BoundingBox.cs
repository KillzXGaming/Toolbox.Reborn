using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace GCNLibrary.Pikmin1.Model
{
    public struct BoundingBox
    {
        public Vector3 Min { get; set; }
        public Vector3 Max { get; set; }
    }
}
