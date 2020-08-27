using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagGeneration
{
    class HorizontalStripes : FlagMainPattern
    {
        public override void Apply(SvgDocument SvgDocument)
        {
            Color topColor = ColorManager.GetRandomColor();
            Color bottomColor = ColorManager.GetRandomColor(new List<Color>() { topColor });

            DrawRectangle(SvgDocument, 0, 0, FlagGenerator.FLAG_WIDTH, FlagGenerator.FLAG_HEIGHT / 2, topColor);
            DrawRectangle(SvgDocument, 0, FlagGenerator.FLAG_HEIGHT / 2, FlagGenerator.FLAG_WIDTH, FlagGenerator.FLAG_HEIGHT / 2, bottomColor);
        }
    }
}
