using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagGeneration
{
    class Stripes : FlagMainPattern
    {
        private Dictionary<int, int> NumStripes = new Dictionary<int, int>()
        {
            {2, 40 },
            {3, 60 },
            {4, 20 },
            {5, 30 },
            {6, 5 },
            {7, 10 },
            {9, 5 },
        };

        private const float HORIZONTAL_CHANCE = 0.5f;

        private const float ALTERNATE_CHANCE = 0.6f; // Chance that two colors alternate
        private const float UNEVEN_SYMMETRY_CHANCE = 0.8f; // Chance that colors are symmetrical when there is an uneven amount of stripes

        private const float WIDE_MID_STRIPE_CHANCE = 0.3f;
        private const float MIN_MID_STRIPE_SIZE = 0.2f;
        private const float MAX_MID_STRIPE_SIZE = 0.8f;

        // Overlays
        private const float EVEN_SQUARE_CHANCE = 0.3f;

        private const float LEFT_TRIANGLE_MIN_WIDTH = 0.2f; // Relative to flag width
        private const float LEFT_TRIANGLE_MAX_WIDTH = 0.6f; // Relative to flag width
        private const float LEFT_TRIANGLE_CHANCE = 0.3f;

        private const float ANTIGUA_OVERLAY_CHANCE = 0.05f;

        private const float COAT_OF_ARMS_CHANCE = 0.5f;
        private const float BIG_COA_CHANCE = 0.5f;

        

        public Stripes(Random r)
        {
            R = r;
        }

        public override void Apply(SvgDocument SvgDocument)
        {
            int numStripes = GetNumStripes();

            Color[] stripeColors = new Color[numStripes];

            bool horizontal = R.NextDouble() < HORIZONTAL_CHANCE;
            bool even = numStripes % 2 == 0;
            bool alternate = R.NextDouble() < ALTERNATE_CHANCE;

            

            // Get stripe colors
            if (alternate) // Alternating colored stripes
            {
                Color c1 = ColorManager.GetRandomColor();
                Color c2 = ColorManager.GetRandomColor(new List<Color>() { c1 });
                for (int i = 0; i < numStripes; i++)
                    stripeColors[i] = i % 2 == 0 ? c1 : c2;

            }
            else if (!even && R.NextDouble() < UNEVEN_SYMMETRY_CHANCE) // Symmetrical colored stripes
            {
                for (int i = 0; i < numStripes; i++)
                {
                    if (i < (numStripes + 1) / 2) stripeColors[i] = ColorManager.GetRandomColor(stripeColors.Where(x => x != null).ToList());
                    else stripeColors[i] = stripeColors[numStripes - 1 - i];
                }
            }
            else // All stripes different color
            {
                for (int i = 0; i < numStripes; i++)
                {
                    stripeColors[i] = ColorManager.GetRandomColor(stripeColors.Where(x => x != null).ToList());
                }
            }

            float[] stripeSizes = new float[numStripes]; // Stripe size (0-1)
            
            if (!even && R.NextDouble() < WIDE_MID_STRIPE_CHANCE)
            {
                float midStripeSize = RandomRange(MIN_MID_STRIPE_SIZE, MAX_MID_STRIPE_SIZE);
                float otherStripesSize = (1f - midStripeSize) / (numStripes - 1);
                for (int i = 0; i < numStripes; i++)
                {
                    if (i == numStripes / 2) stripeSizes[i] = midStripeSize;
                    else stripeSizes[i] = otherStripesSize;
                }
            }
            else
            {
                for (int i = 0; i < numStripes; i++)
                    stripeSizes[i] = 1f / numStripes;
            }

            // Draw stripes
            float curRel = 0;
            for(int i = 0; i < numStripes; i++)
            {
                float stripeSize = stripeSizes[i];
                
                DrawRectangle(SvgDocument,
                    horizontal ? 0 : curRel * FlagWidth,
                    horizontal ? curRel * FlagHeight : 0,
                    horizontal ? FlagWidth : FlagWidth * stripeSize,
                    horizontal ? FlagHeight * stripeSize : FlagHeight,
                    stripeColors[i]);
                curRel += stripeSize;
            }

            PointF CoaPosition = FlagCenter;
            float coaSize = 0;
            Color coaColor = Color.Transparent;

            // If even, can have a square top right
            if(even && R.NextDouble() < EVEN_SQUARE_CHANCE)
            {
                List<Color> forbiddenColors = new List<Color>();
                for (int i = 0; i <= numStripes / 2; i++) forbiddenColors.Add(stripeColors[i]);
                Color squareColor = ColorManager.GetRandomColor(forbiddenColors);
                DrawRectangle(SvgDocument, 0, 0, FlagWidth / 2, FlagHeight / 2, squareColor);

                // Coa
                CoaPosition = new PointF(FlagWidth / 4, FlagHeight / 4);
                float minCoaSize = FlagHeight / 4;
                float maxCoaSize = FlagHeight / 2;
                coaSize = RandomRange(minCoaSize, maxCoaSize);
                coaColor = ColorManager.GetRandomColor(new List<Color>() { squareColor });
            }

            else if(horizontal && R.NextDouble() < LEFT_TRIANGLE_CHANCE)
            {
                Color triangleColor = ColorManager.GetRandomColor(stripeColors.ToList());
                float triangleWidth = RandomRange(LEFT_TRIANGLE_MIN_WIDTH, LEFT_TRIANGLE_MAX_WIDTH);
                PointF[] vertices = new PointF[]
                {
                    new PointF(0,0), new PointF(triangleWidth * FlagWidth, FlagHeight/2), new PointF(0, FlagHeight)
                };
                DrawPolygon(SvgDocument, vertices, triangleColor);

                // Coa
                CoaPosition = new PointF((triangleWidth * 0.35f) * FlagWidth, FlagHeight / 2);
                float minCoaSize = (triangleWidth * 0.3f) * FlagWidth;
                float maxCoaSize = (triangleWidth * 0.6f) * FlagWidth;
                coaSize = RandomRange(minCoaSize, maxCoaSize);
                coaColor = ColorManager.GetRandomColor(new List<Color>() { triangleColor });
            }

            else if(R.NextDouble() < ANTIGUA_OVERLAY_CHANCE)
            {
                Color overlayColor = ColorManager.GetRandomColor(stripeColors.ToList());
                PointF[] triangle1 = new PointF[] { new PointF(0, 0), new PointF(FlagWidth / 2, FlagHeight), new PointF(0, FlagHeight) };
                PointF[] triangle2 = new PointF[] { new PointF(FlagWidth, 0), new PointF(FlagWidth / 2, FlagHeight), new PointF(FlagWidth, FlagHeight) };
                DrawPolygon(SvgDocument, triangle1, overlayColor);
                DrawPolygon(SvgDocument, triangle2, overlayColor);

                // Coa
                float height = RandomRange(0.2f, 0.4f);
                CoaPosition = new PointF(FlagWidth / 2, height * FlagHeight);
                float coaSizeRel = RandomRange(0.2f, Math.Min(0.5f, height * 2f));
                coaSize = coaSizeRel * FlagHeight;
                coaColor = ColorManager.GetRandomColor(stripeColors.ToList());
            }

            // Coat of arms
            if(R.NextDouble() < COAT_OF_ARMS_CHANCE)
            {
                List<Color> forbiddenColors = even ? new List<Color>() { stripeColors[numStripes / 2 - 1], stripeColors[numStripes / 2] } : new List<Color>() { stripeColors[numStripes / 2] };
                float coaSizeRel = even ? stripeSizes[0] * 2 : stripeSizes[numStripes / 2];
                if (!even && R.NextDouble() < BIG_COA_CHANCE)
                {
                    coaSizeRel = stripeSizes[(numStripes / 2) - 1] + stripeSizes[(numStripes / 2)] + stripeSizes[(numStripes / 2) + 1];
                    forbiddenColors.Add(stripeColors[(numStripes / 2) - 1]);
                    forbiddenColors.Add(stripeColors[(numStripes / 2) + 1]);
                }

                // Set size if not set yet
                if (coaSize == 0)
                {
                    coaSize = horizontal ? coaSizeRel * FlagHeight : coaSizeRel * FlagWidth;
                    coaSize = Math.Min(FlagHeight, coaSize);
                    coaSize *= 0.9f;
                }

                // Set color if not set yet
                if (coaColor == Color.Transparent)
                    coaColor = ColorManager.GetRandomColor(forbiddenColors);

                CoatOfArms coa = GetRandomCoa();

                coa.Draw(SvgDocument, this, CoaPosition, coaSize, coaColor, R);
            }
        }

        private int GetNumStripes()
        {
            int probabilitySum = NumStripes.Sum(x => x.Value);
            int rng = R.Next(probabilitySum);
            int tmpSum = 0;
            foreach (KeyValuePair<int, int> kvp in NumStripes)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum) return kvp.Key;
            }
            return 0;
        }
    }
}
