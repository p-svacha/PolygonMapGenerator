using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagGeneration
{
    static class Geometry
    {
        public static float DegreeToRadian(float angle)
        {
            return (float)(Math.PI * angle / 180.0);
        }

        public static float RadianToDegree(float angle)
        {
            return (float)(angle * (180.0 / Math.PI));
        }
    }
}
