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
    class Star : CoatOfArms
    {
        public override void Draw(SvgDocument Svg, PointF center, float size, Color c, Random R)
        {
            int numCorners = 5;
            int startAngle = 180;
            float outerRadius = size * 0.5f;
            float innerRadius = outerRadius * 0.4f;

            int numVertices = numCorners * 2;
            PointF[] vertices = new PointF[numVertices];

            // Create vertices
            float angleStep = 360f / numVertices;
            for (int i = 0; i < numVertices; i++)
            {
                float angle = startAngle + (i * angleStep);
                bool outerCorner = i % 2 == 0;
                float radius = outerCorner ? outerRadius : innerRadius;
                float x = center.X + (float)(radius * Math.Sin(DegreeToRadian(angle)));
                float y = center.Y + (float)(radius * Math.Cos(DegreeToRadian(angle)));
                vertices[i] = new PointF(x, y);
            }

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
            Svg.Children.Add(SvgPolygon);
        }
    }
}
