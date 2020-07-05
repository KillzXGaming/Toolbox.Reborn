using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Toolbox.Core
{
    public class Utils
    {
        public static void DelayAction(int millisecond, Action action)
        {

        }

        public static string RenameDuplicateString(string oldString, List<string> strings, int index = 0, int numDigits = 1)
        {
            if (strings.Contains(oldString))
            {
                string key = $"{index++}";
                if (numDigits == 2)
                    key = string.Format("{0:00}", key);

                string NewString = $"{oldString}_{key}";
                if (strings.Contains(NewString))
                    return RenameDuplicateString(oldString, strings, index, numDigits);
                else
                    return NewString;
            }

            return oldString;
        }

        public static string GetExtension(string FileName)
        {
            return Path.GetExtension(FileName).ToLower();
        }

        public static int FloatToIntClamp(float r)
        {
            return Clamp((int)(r * 255), 0, 255);
        }

        public static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }
    }
}