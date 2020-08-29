using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FlagGeneration.Geometry;

namespace FlagGeneration
{
    class FramedCoa : CoatOfArms
    {
        private enum FrameType
        {
            Circle,
            Star
        }

        private readonly Dictionary<FrameType, int> FrameTypes = new Dictionary<FrameType, int>()
        {
            { FrameType.Circle, 100 },
            {FrameType.Star, 50 }
        };

        public override void Draw(SvgDocument Svg, FlagMainPattern flag, PointF pos, float size, Color c, Random R)
        {

            switch(GetRandomFrameType(R))
            {
                case FrameType.Circle:
                    flag.DrawCircle(Svg, pos, size/2, c);
                    Color coaColor = ColorManager.GetRandomColor(new List<Color>() { c });
                    CoatOfArms coa = flag.GetRandomCoa();
                    coa.Draw(Svg, flag, pos, size * 0.8f, coaColor, R);
                    break;

                case FrameType.Star:
                    int numCorners = R.Next(17) + 8;
                    float startAngle = 180;
                    float outerRadius = size * 0.5f;
                    float innerRadius = outerRadius * flag.RandomRange(0.6f, 0.95f);

                    int numVertices = numCorners * 2;
                    PointF[] vertices = new PointF[numVertices];

                    // Create vertices
                    float angleStep = 360f / numVertices;
                    for (int i = 0; i < numVertices; i++)
                    {
                        float curAngle = startAngle + (i * angleStep);
                        bool outerCorner = i % 2 == 0;
                        float radius = outerCorner ? outerRadius : innerRadius;
                        float x = pos.X + (float)(radius * Math.Sin(DegreeToRadian(curAngle)));
                        float y = pos.Y + (float)(radius * Math.Cos(DegreeToRadian(curAngle)));
                        vertices[i] = new PointF(x, y);
                    }

                    flag.DrawPolygon(Svg, vertices, c);

                    coaColor = ColorManager.GetRandomColor(new List<Color>() { c });
                    coa = flag.GetRandomCoa();
                    coa.Draw(Svg, flag, pos, innerRadius * 2* 0.8f, coaColor, R);
                    break;
            }
        }

        private FrameType GetRandomFrameType(Random R)
        {
            int probabilitySum = FrameTypes.Sum(x => x.Value);
            int rng = R.Next(probabilitySum);
            int tmpSum = 0;
            foreach (KeyValuePair<FrameType, int> kvp in FrameTypes)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum) return kvp.Key;
            }
            return FrameType.Circle;
        }
    }
}
