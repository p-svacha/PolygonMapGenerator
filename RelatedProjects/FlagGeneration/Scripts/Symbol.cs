using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagGeneration
{
    public abstract class Symbol
    {
        Random R;
        protected Symbol(Random r)
        {
            R = r;
        }
        public abstract void Draw(SvgDocument Svg, FlagMainPattern flag, PointF pos, float size, float angle, Color c);

        public int RandomRange(int min, int max)
        {
            return R.Next(max - min) + min;
        }
        public float RandomRange(float min, float max)
        {
            return (float)R.NextDouble() * (max - min) + min;
        }
    }
}
