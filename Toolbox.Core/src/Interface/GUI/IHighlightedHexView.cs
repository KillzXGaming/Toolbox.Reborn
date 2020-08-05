using System.Drawing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core
{
    public interface IHighlightedHexView
    {
        List<HighlightedHexRegion> Selections { get; set; }
    }

    public class HighlightedHexRegion
    {
        public uint Offset { get; set; }
        public uint Size { get; set; }
        public Color ForeColor { get; set; }
        public Color BackColor { get; set; }
    }
}
