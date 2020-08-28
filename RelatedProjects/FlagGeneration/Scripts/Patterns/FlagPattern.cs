using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagGeneration
{
    public abstract class FlagMainPattern
    {
        protected List<CoatOfArms> CoatOfArms = new List<CoatOfArms>()
        {
            new PrefabCoa()
        };

        protected Random R;
        protected static float FlagWidth = FlagGenerator.FLAG_WIDTH;
        protected static float FlagHeight = FlagGenerator.FLAG_HEIGHT;
        protected static PointF FlagCenter = new PointF(FlagWidth / 2f, FlagHeight / 2f);

        public abstract void Apply(SvgDocument SvgDocument);

        public void DrawRectangle(SvgDocument SvgDocument, float startX, float startY, float width, float height, Color c)
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

        public int RandomRange(int min, int max)
        {
            return R.Next(max - min) + min;
        }
        public float RandomRange(float min, float max)
        {
            return (float)R.NextDouble() * (max - min) + min;
        }

        public CoatOfArms GetRandomCoatOfArms()
        {
            return CoatOfArms[R.Next(CoatOfArms.Count)];
        }
    }
}
