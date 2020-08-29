using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.WebSockets;

namespace FlagGeneration
{
    class Diagonal : FlagMainPattern
    {
        public enum Style
        {
            Split,
            Cross
        }

        private Dictionary<Style, int> Styles = new Dictionary<Style, int>()
        {
            {Style.Split, 100 },
            {Style.Cross, 100 },
        };

        public Diagonal(Random r)
        {
            R = r;
        }

        private const float DOUBLE_SPLIT_CHANCE = 0.25f;

        private const float SPLIT_COA_CHANCE = 0.5f;
        private const float TOP_RIGHT_COA_CHANCE = 0.3f;

        private const float MIN_CROSS_WIDTH = 0.02f;
        private const float MAX_CROSS_WIDTH = 0.25f;
        private const float INNER_CROSS_CHANCE = 0.25f;
        private const float CROSS_DIFFERENT_SIDE_COLORS_CHANCE = 0.25f;

        public override void Apply(SvgDocument SvgDocument)
        {
            Color coaColor = Color.Transparent;
            float minCoaSize = 0.5f;
            float maxCoaSize = 0.95f;
            float coaSize = RandomRange(minCoaSize * FlagHeight, maxCoaSize * FlagHeight);
            PointF CoaPosition = FlagCenter;

            float coaChance = SPLIT_COA_CHANCE;

            switch (GetRandomStyle())
            {
                case Style.Split:
                    Color c1 = ColorManager.GetRandomColor();
                    Color c2 = ColorManager.GetRandomColor(new List<Color>() { c1 });
                    coaColor = ColorManager.GetRandomColor(new List<Color>() { c1, c2 });

                    PointF[] triangle1 = new PointF[] { new PointF(0, 0), new PointF(FlagWidth, 0), new PointF(0, FlagHeight) };
                    PointF[] triangle2 = new PointF[] { new PointF(0, FlagHeight), new PointF(FlagWidth, 0), new PointF(FlagWidth, FlagHeight) };
                    DrawPolygon(SvgDocument, triangle1, c1);
                    DrawPolygon(SvgDocument, triangle2, c2);

                    // Double Split
                    if(R.NextDouble() < DOUBLE_SPLIT_CHANCE)
                    {
                        Color c3 = ColorManager.GetRandomColor(new List<Color>() { c1, c2 });
                        float minSplit2Start = 0.2f;
                        float maxSplit2Start = 0.6f;
                        float split2Start = RandomRange(minSplit2Start, maxSplit2Start);
                        PointF[] triangle3 = new PointF[] { new PointF(FlagWidth * split2Start, FlagHeight), new PointF(FlagWidth, FlagHeight * split2Start), new PointF(FlagWidth, FlagHeight) };
                        DrawPolygon(SvgDocument, triangle3, c3);

                        coaColor = ColorManager.GetRandomColor(new List<Color>() { c1 });
                        minCoaSize = 0.2f;
                        maxCoaSize = 0.5f;
                        coaSize = RandomRange(FlagHeight * minCoaSize, FlagHeight * maxCoaSize);
                        CoaPosition = new PointF(50 + coaSize/2, 50 + coaSize/2);
                    }

                    // Top right coa
                    if(R.NextDouble() < TOP_RIGHT_COA_CHANCE)
                    {
                        coaColor = ColorManager.GetRandomColor(new List<Color>() { c1 });
                        minCoaSize = 0.2f;
                        maxCoaSize = 0.5f;
                        coaSize = RandomRange(FlagHeight * minCoaSize, FlagHeight * maxCoaSize);
                        CoaPosition = new PointF(50 + coaSize / 2, 50 + coaSize / 2);
                    }

                    break;

                case Style.Cross:
                    Color bg = ColorManager.GetRandomColor();
                    Color crossColor = ColorManager.GetRandomColor(new List<Color>() { bg });
                    DrawRectangle(SvgDocument, 0, 0, FlagWidth, FlagHeight, bg);
                    float crossWidth = RandomRange(MIN_CROSS_WIDTH, MAX_CROSS_WIDTH);
                    float crossWidthAbsX = crossWidth * FlagWidth;
                    float crossWidthAbsY = crossWidth * FlagHeight;

                    if (R.NextDouble() < CROSS_DIFFERENT_SIDE_COLORS_CHANCE)
                    {
                        Color sideColor = ColorManager.GetRandomColor(new List<Color>() { bg, crossColor });
                        PointF[] leftTriangle = new PointF[] { new PointF(0, 0), FlagCenter, new PointF(0, FlagHeight) };
                        PointF[] rightTriangle = new PointF[] { new PointF(FlagWidth, 0), FlagCenter, new PointF(FlagWidth, FlagHeight) };
                        DrawPolygon(SvgDocument, leftTriangle, sideColor);
                        DrawPolygon(SvgDocument, rightTriangle, sideColor);
                    }

                    PointF[] cross1Vertices = new PointF[] { new PointF(0, 0), new PointF(crossWidthAbsX, 0), new PointF(FlagWidth, FlagHeight - crossWidthAbsY), new PointF(FlagWidth, FlagHeight), new PointF(FlagWidth - crossWidthAbsX, FlagHeight), new PointF(0, crossWidthAbsY) };
                    PointF[] cross2Vertices = new PointF[] { new PointF(0, FlagHeight), new PointF(0, FlagHeight - crossWidthAbsY), new PointF(FlagWidth -  crossWidthAbsX, 0), new PointF(FlagWidth, 0), new PointF(FlagWidth, crossWidthAbsY), new PointF(crossWidthAbsX, FlagHeight) };
                    DrawPolygon(SvgDocument, cross1Vertices, crossColor);
                    DrawPolygon(SvgDocument, cross2Vertices, crossColor);

                    if(R.NextDouble() < INNER_CROSS_CHANCE)
                    {
                        Color innerCrossColor = ColorManager.GetRandomColor(new List<Color>() { crossColor });
                        float innerCrossWidth = RandomRange(crossWidth * 0.2f, crossWidth * 0.8f);
                        float innerCrossWidthAbsX = innerCrossWidth * FlagWidth;
                        float innerCrossWidthAbsY = innerCrossWidth * FlagHeight;
                        PointF[] innerCross1Vertices = new PointF[] { new PointF(0, 0), new PointF(innerCrossWidthAbsX, 0), new PointF(FlagWidth, FlagHeight - innerCrossWidthAbsY), new PointF(FlagWidth, FlagHeight), new PointF(FlagWidth - innerCrossWidthAbsX, FlagHeight), new PointF(0, innerCrossWidthAbsY) };
                        PointF[] innerCross2Vertices = new PointF[] { new PointF(0, FlagHeight), new PointF(0, FlagHeight - innerCrossWidthAbsY), new PointF(FlagWidth - innerCrossWidthAbsX, 0), new PointF(FlagWidth, 0), new PointF(FlagWidth, innerCrossWidthAbsY), new PointF(innerCrossWidthAbsX, FlagHeight) };
                        DrawPolygon(SvgDocument, innerCross1Vertices, innerCrossColor);
                        DrawPolygon(SvgDocument, innerCross2Vertices, innerCrossColor);
                    }
                    coaChance = 0;
                    break;
            }

            if(R.NextDouble() < coaChance)
            {
                CoatOfArms coa = GetRandomCoa();

                coa.Draw(SvgDocument, this, CoaPosition, coaSize, coaColor, R);
            }
        }

        private void SetTopRightCornerCoaParams()
        {

        }

        public Style GetRandomStyle()
        {
            int probabilitySum = Styles.Sum(x => x.Value);
            int rng = R.Next(probabilitySum);
            int tmpSum = 0;
            foreach (KeyValuePair<Style, int> kvp in Styles)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum) return kvp.Key;
            }
            return Style.Split;
        }
    }
}
