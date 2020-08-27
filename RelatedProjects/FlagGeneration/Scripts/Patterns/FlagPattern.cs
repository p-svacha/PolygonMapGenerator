using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagGeneration
{
    abstract class FlagMainPattern
    {
        public abstract void Apply(SvgDocument SvgDocument);

        public void DrawRectangle(SvgDocument SvgDocument, int startX, int startY, int width, int height, Color c)
        {
            SvgRectangle svgRectangle = new SvgRectangle()
            {
                X = startX,
                Y = startY,
                Width = width,
                Height = height,
                Fill = new SvgColourServer(c)
            };
            SvgDocument.Children.Add(svgRectangle);
        }
    }
}
