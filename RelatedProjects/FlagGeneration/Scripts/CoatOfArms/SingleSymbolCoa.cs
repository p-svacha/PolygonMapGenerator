using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlagGeneration
{
    class SingleSymbolCoa : CoatOfArms
    {
        public override void Draw(SvgDocument Svg, FlagMainPattern flag, PointF pos, float size, Color c, Random R)
        {
            Symbol symbol = flag.GetRandomSymbol();
            symbol.Draw(Svg, flag, pos, size, 0, c, R);
        }
    }
}
