using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Toolbox.Core.IO;
using Toolbox.Core;

namespace GCNLibrary.LM
{
    public class BAS_Parser
    {
        public BAS_Parser() { }

        public BAS_Parser(Stream stream)
        {
            Read(new FileReader(stream));
        }

        public void Save(Stream stream)
        {
            Write(new FileWriter(stream));
        }

        private void Read(FileReader reader)
        {
        }

        private void Write(FileWriter reader)
        {
        }
    }
}
