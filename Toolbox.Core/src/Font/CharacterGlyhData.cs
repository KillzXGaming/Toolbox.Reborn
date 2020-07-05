using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core
{
    public class CharacterGlyhData
    {
        public ushort CharacterID { get; set; }

        public char Character
        {
            get { return (char)CharacterID; }
            set { CharacterID = value; }
        }

        public uint LeftSpacing { get; set; }
        public uint GlyphWidth { get; set; }
        public uint CharacterWidth { get; set; }
    }
}
