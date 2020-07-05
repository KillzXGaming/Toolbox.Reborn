using System.Linq;

namespace Toolbox.Core.Hashes
{
    public class SimpleHash
    {
        public static uint Create(string input, uint magic, uint hashCount)
        {
            return Create(input, magic) % hashCount;
        }

        public static uint Create(string input, uint magic)
        {
            return input.Aggregate(0u, (hash, c) => hash * magic + c);
        }
    }
}
