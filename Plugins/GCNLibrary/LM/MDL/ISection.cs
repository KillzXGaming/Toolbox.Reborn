using Toolbox.Core.IO;

namespace GCNLibrary.LM.MDL
{
    public interface ISection
    {
        void Read(FileReader reader);
        void Write(FileWriter writer);
    }
}
