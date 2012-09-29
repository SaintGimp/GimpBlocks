using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public static class GimpMath
    {
        public static double TriLerp(double x, double y, double z, double q000, double q001, double q010, double q011, double q100, double q101, double q110, double q111, double x1, double x2, double y1, double y2, double z1, double z2)
        {
            double x00 = Lerp(x, x1, x2, q000, q100);
            double x10 = Lerp(x, x1, x2, q010, q110);
            double x01 = Lerp(x, x1, x2, q001, q101);
            double x11 = Lerp(x, x1, x2, q011, q111);
            double r0 = Lerp(y, y1, y2, x00, x01);
            double r1 = Lerp(y, y1, y2, x10, x11);
            return Lerp(z, z1, z2, r0, r1);
        }

        public static double Lerp(double x, double x1, double x2, double q00, double q01)
        {
            return ((x2 - x) / (x2 - x1)) * q00 + ((x - x1) / (x2 - x1)) * q01;
        }
    }
}
