using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FlagGeneration.Geometry;

namespace FlagGeneration
{
    class Circle : Symbol
    {
        public Circle(Random R) : base(R) { }

        public override void Draw(SvgDocument Svg, FlagMainPattern flag, PointF center, float size, float angle, Color c)
        {
            flag.DrawCircle(Svg, center, size / 2, c);
        }
    }
}
