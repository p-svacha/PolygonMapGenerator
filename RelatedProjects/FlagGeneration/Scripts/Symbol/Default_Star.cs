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
    class Default_Star : Symbol
    {
        public Default_Star(Random R) : base(R) { }

        public override void Draw(SvgDocument Svg, FlagMainPattern flag, PointF center, float size, float angle, Color c)
        {
            int numCorners = 5;
            float startAngle = angle + 180;
            float outerRadius = size * 0.5f;
            float innerRadius = outerRadius * 0.4f;

            int numVertices = numCorners * 2;
            PointF[] vertices = new PointF[numVertices];

            // Create vertices
            float angleStep = 360f / numVertices;
            for (int i = 0; i < numVertices; i++)
            {
                float curAngle = startAngle + (i * angleStep);
                bool outerCorner = i % 2 == 0;
                float radius = outerCorner ? outerRadius : innerRadius;
                float x = center.X + (float)(radius * Math.Sin(DegreeToRadian(curAngle)));
                float y = center.Y + (float)(radius * Math.Cos(DegreeToRadian(curAngle)));
                vertices[i] = new PointF(x, y);
            }

            flag.DrawPolygon(Svg, vertices, c);
        }
    }
}
