using System;
using System.Security.Cryptography;

namespace Toolbox.Core.Hashes
{
    public class Adler32Managed : HashAlgorithm
    {
        private ushort o_sum_1;
        private ushort o_sum_2;

        public Adler32Managed() {
            Initialize();
        }

        public static uint CalculateHash(byte[] data) {
            byte[] computed = new Adler32Managed().ComputeHash(data);
            return BitConverter.ToUInt32(computed,0);
        }

        public override int HashSize
        {
            get
            {
                return 32;
            }
        }

        public override void Initialize()
        {
            // reset the sum values
            o_sum_1 = 1;
            o_sum_2 = 0;
        }

        protected override void HashCore(byte[] p_array, int p_start_index, int p_count)
        {
            // process each byte in the array
            int endIndex = p_start_index + p_count;
            for (int i = p_start_index; i < endIndex; i++)
            {
                o_sum_1 = (ushort)((o_sum_1 + p_array[i]) % 65521);
                o_sum_2 = (ushort)((o_sum_1 + o_sum_2) % 65521);
            }
        }

        protected override byte[] HashFinal()
        {
            // concat the two 16 bit values to form
            // one 32-bit value
            uint x_concat_value = (uint)((o_sum_2 << 16) | o_sum_1);
            // use the bitconverter class to render the
            // 32-bit integer into an array of bytes
            return BitConverter.GetBytes(x_concat_value);
        }
    }
}