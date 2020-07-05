using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core
{
    public class TriangleConverter
    {
        public static List<uint> ConvertTriangleStripsToTriangles(List<uint> faces)
        {
            List<uint> f = new List<uint>();

            int startDirection = 1;
            int p = 0;
            uint f1 = faces[p++];
            uint f2 = faces[p++];
            int faceDirection = startDirection;
            uint f3;
            do
            {
                f3 = faces[p++];
                if (f3 == 0xFFFF)
                {
                    f1 = faces[p++];
                    f2 = faces[p++];
                    faceDirection = startDirection;
                }
                else
                {
                    faceDirection *= -1;
                    if ((f1 != f2) && (f2 != f3) && (f3 != f1))
                    {
                        if (faceDirection > 0)
                        {
                            f.Add(f3);
                            f.Add(f2);
                            f.Add(f1);
                        }
                        else
                        {
                            f.Add(f2);
                            f.Add(f3);
                            f.Add(f1);
                        }
                    }
                    f1 = f2;
                    f2 = f3;
                }
            } while (p < faces.Count);

            return f;
        }
    }
}
