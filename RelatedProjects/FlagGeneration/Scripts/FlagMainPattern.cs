using Svg;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagGeneration
{
    public abstract class FlagMainPattern
    {
        // General chances of coat of arms and symbol types appearing
        protected Dictionary<CoatOfArms, int> CoatOfArms = new Dictionary<CoatOfArms, int>()
        {
            { new PrefabCoa(), 100 },
            { new SingleSymbolCoa(), 100 },
            { new FramedCoa(), 50 },
        };
        protected Dictionary<string, int> Symbols = new Dictionary<string, int>()
        {
            { "DefaultStar", 80 },
            { "Circle", 50 },
            { "SpecialStar", 60 },
        };

        // Flag Attributes
        protected Random R;
        protected static float FlagWidth = FlagGenerator.FLAG_WIDTH;
        protected static float FlagHeight = FlagGenerator.FLAG_HEIGHT;
        protected static PointF FlagCenter = new PointF(FlagWidth / 2f, FlagHeight / 2f);

        // Coat of Arms
        protected float CoatOfArmsChance;       // (0-1)
        protected Color CoatOfArmsColor;
        protected PointF CoatOfArmsPosition;    // Absolute position
        protected float CoatOfArmsSize;         // Absolute size

        public abstract void Apply(SvgDocument SvgDocument);

        protected void ApplyCoatOfArms(SvgDocument SvgDocument)
        {
            if (R.NextDouble() < CoatOfArmsChance)
            {
                CoatOfArms coa = GetRandomCoa();
                coa.Draw(SvgDocument, this, CoatOfArmsPosition, CoatOfArmsSize, CoatOfArmsColor, R);
            }
        }

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

        public void DrawPolygon(SvgDocument SvgDocument, PointF[] vertices, Color c)
        {
            SvgPointCollection points = new SvgPointCollection();
            foreach (PointF p in vertices)
            {
                points.Add(new SvgUnit(p.X));
                points.Add(new SvgUnit(p.Y));
            }

            SvgPolygon SvgPolygon = new SvgPolygon()
            {
                Points = points,
                Fill = new SvgColourServer(c),
                StrokeWidth = 0
            };
            SvgDocument.Children.Add(SvgPolygon);
        }

        public void DrawCircle(SvgDocument SvgDocument, PointF center, float radius, Color c)
        {
            SvgCircle SvgCircle = new SvgCircle()
            {
                CenterX = center.X,
                CenterY = center.Y,
                Radius = radius,
                Fill = new SvgColourServer(c)
            };
            SvgDocument.Children.Add(SvgCircle);
        }

        public int RandomRange(int min, int max)
        {
            return R.Next(max - min) + min;
        }
        public float RandomRange(float min, float max)
        {
            return (float)R.NextDouble() * (max - min) + min;
        }

        public CoatOfArms GetRandomCoa()
        {
            int probabilitySum = CoatOfArms.Sum(x => x.Value);
            int rng = R.Next(probabilitySum);
            int tmpSum = 0;
            foreach (KeyValuePair<CoatOfArms, int> kvp in CoatOfArms)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum) return kvp.Key;
            }
            return null;
        }

        public Symbol GetRandomSymbol()
        {
            int probabilitySum = Symbols.Sum(x => x.Value);
            int rng = R.Next(probabilitySum);
            int tmpSum = 0;
            foreach (KeyValuePair<string, int> kvp in Symbols)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum)
                {
                    switch(kvp.Key)
                    {
                        case "DefaultStar":
                            return new Default_Star(R);

                        case "Circle":
                            return new Circle(R);

                        case "SpecialStar":
                            return new Special_Star(R);
                    }
                }
            }
            return null;
        }
    }
}
