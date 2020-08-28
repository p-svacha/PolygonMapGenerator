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

        private const float ALTERNATE_CHANCE = 0.4f; // Chance that two colors alternate
        private const float EVEN_SYMMETRY_CHANCE = 0.5f; // Chance that colors are symmetrical when there is an uneven amount of stripes

        private const float WIDE_MID_STRIPE_CHANCE = 0.3f;
        private const float MAX_MID_STRIPE_SIZE = 0.8f;

        private const float COAT_OF_ARMS_CHANCE = 0.5f;
        private const float BIG_COA_CHANCE = 0.5f;

        public Stripes(Random r)
        {
            R = r;
        }

        public override void Apply(SvgDocument SvgDocument)
        {
            int numStripes = GetNumStripes();

            Color[] colors = new Color[numStripes];

            bool horizontal = R.NextDouble() < HORIZONTAL_CHANCE;
            bool even = numStripes % 2 == 0;
            bool alternate = R.NextDouble() < ALTERNATE_CHANCE;

            

            // Get stripe colors
            if (alternate) // Alternating colored stripes
            {
                Color c1 = ColorManager.GetRandomColor();
                Color c2 = ColorManager.GetRandomColor(new List<Color>() { c1 });
                for (int i = 0; i < numStripes; i++)
                    colors[i] = i % 2 == 0 ? c1 : c2;

            }
            else if (!even && R.NextDouble() < EVEN_SYMMETRY_CHANCE) // Symmetrical colored stripes
            {
                for (int i = 0; i < numStripes; i++)
                {
                    if (i < (numStripes + 1) / 2) colors[i] = ColorManager.GetRandomColor(colors.Where(x => x != null).ToList());
                    else colors[i] = colors[numStripes - 1 - i];
                }
            }
            else // All stripes different color
            {
                for (int i = 0; i < numStripes; i++)
                {
                    colors[i] = ColorManager.GetRandomColor(colors.Where(x => x != null).ToList());
                }
            }

            float[] stripeSizes = new float[numStripes]; // Stripe size (0-1)
            
            if (!even && R.NextDouble() < WIDE_MID_STRIPE_CHANCE)
            {
                float midStripeSize = (float)R.NextDouble() * MAX_MID_STRIPE_SIZE;
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
                    colors[i]);
                curRel += stripeSize;
            }

            // Coat of arms
            if(R.NextDouble() < COAT_OF_ARMS_CHANCE)
            {
                List<Color> forbiddenColors = even ? new List<Color>() { colors[numStripes / 2 - 1], colors[numStripes / 2] } : new List<Color>() { colors[numStripes / 2] };
                float coaSizeRel = even ? stripeSizes[0] * 2 : stripeSizes[numStripes/2];
                if(!even && R.NextDouble() < BIG_COA_CHANCE)
                {
                    coaSizeRel *= 3f;
                    forbiddenColors.Add(colors[(numStripes / 2) - 1]);
                    forbiddenColors.Add(colors[(numStripes / 2) + 1]);
                }
                float coaSize = horizontal ? coaSizeRel * FlagHeight : coaSizeRel * FlagWidth;
                coaSize = Math.Min(FlagHeight, coaSize);
                coaSize *= 0.9f;
                Color coaColor = ColorManager.GetRandomColor(forbiddenColors);
                CoatOfArms coa = GetRandomCoatOfArms();
                coa.Draw(SvgDocument, FlagCenter, coaSize, coaColor, R);
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
