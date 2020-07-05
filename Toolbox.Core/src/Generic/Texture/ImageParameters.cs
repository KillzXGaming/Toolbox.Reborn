﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Core
{
    public class ImageParameters
    {
        public STRotateFlipType Rotation;

        //Flip the image on the Y axis
        public bool FlipY { get; set; }

        //Dont swap the red and green channels
        public bool DontSwapRG { get; set; }
    }
}
