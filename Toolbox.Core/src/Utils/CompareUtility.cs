using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Reflection;

namespace Toolbox.Core
{
    public class CompareUtility
    {
        public static int SearchArray<T>(T[] haystack, T[] needle)
        {
            var len = needle.Length;
            var limit = haystack.Length - len;
            for (var i = 0; i <= limit; i++)
            {
                var k = 0;
                for (; k < len; k++)
                {
                    if (!needle[k].Equals(haystack[i + k])) break;
                }
                if (k == len) return i;
            }
            return -1;
        }
    }
}
