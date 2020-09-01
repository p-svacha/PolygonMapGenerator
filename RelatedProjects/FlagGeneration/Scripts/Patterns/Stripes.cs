using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace FlagGeneration
{
    class Stripes : FlagMainPattern
    {
        public enum OverlayType
        {
            None,
            LeftTriangle,
            TopRightSquare,
            Antigua
        }

        public enum StripeDirectionType
        {
            Horizontal,
            Vertical
        }

        private Dictionary<int, int> NumStripesDictionary = new Dictionary<int, int>()
        {
            {2, 40 },
            {3, 60 },
            {4, 20 },
            {5, 30 },
            {6, 5 },
            {7, 10 },
            {9, 5 },
        };

        private Dictionary<StripeDirectionType, int> StripeDirections = new Dictionary<StripeDirectionType, int>()
        {
            {StripeDirectionType.Horizontal, 60 },
            {StripeDirectionType.Vertical, 40 }
        };

        private Dictionary<OverlayType, int> Overlays = new Dictionary<OverlayType, int>()
        {
            {OverlayType.None, 100 },
            {OverlayType.TopRightSquare, 50 },
            {OverlayType.LeftTriangle, 50 },
            {OverlayType.Antigua, 5 }
        };

        private const float ALTERNATING_COLORS_CHANCE = 0.6f; // Chance that two colors alternate
        private const float UNEVEN_SYMMETRY_CHANCE = 0.8f; // Chance that colors are symmetrical when there is an uneven amount of stripes

        private const float WIDE_MID_STRIPE_CHANCE = 0.3f;
        private const float MIN_MID_STRIPE_SIZE = 0.2f;
        private const float MAX_MID_STRIPE_SIZE = 0.8f;

        // Overlays
        private const float MID_STRIPE_SYMBOLS_CHANCE = 0.5f; // Chance that instead of a coat of arms, the mid stripe has multiple instances of symbols
        private const int MID_STRIPE_SYMBOLS_MIN_AMOUNT = 2;
        private const int MID_STRIPE_SYMBOLS_MAX_AMOUNT = 5;

        private const float LEFT_TRIANGLE_MIN_WIDTH = 0.2f; // Relative to flag width
        private const float LEFT_TRIANGLE_MAX_WIDTH = 0.6f; // Relative to flag width

        private const float COAT_OF_ARMS_CHANCE = 0.5f;
        private const float BIG_COA_CHANCE = 0.5f;

        // Active values
        public int NumStripes;
        public bool EvenStripes;
        public StripeDirectionType StripeDirection;

        public Stripes(Random r)
        {
            R = r;
        }

        public override void Apply(SvgDocument SvgDocument)
        {
            NumStripes = GetNumStripes();

            Color[] stripeColors = new Color[NumStripes];

            StripeDirection = GetStripeDirection();
            EvenStripes = NumStripes % 2 == 0;
            bool alternate = R.NextDouble() < ALTERNATING_COLORS_CHANCE;

            // Get stripe colors
            if (alternate) // Alternating colored stripes
            {
                Color c1 = ColorManager.GetRandomColor();
                Color c2 = ColorManager.GetRandomColor(new List<Color>() { c1 });
                for (int i = 0; i < NumStripes; i++)
                    stripeColors[i] = i % 2 == 0 ? c1 : c2;

            }
            else if (!EvenStripes && R.NextDouble() < UNEVEN_SYMMETRY_CHANCE) // Symmetrical colored stripes
            {
                for (int i = 0; i < NumStripes; i++)
                {
                    if (i < (NumStripes + 1) / 2) stripeColors[i] = ColorManager.GetRandomColor(stripeColors.Where(x => x != null).ToList());
                    else stripeColors[i] = stripeColors[NumStripes - 1 - i];
                }
            }
            else // All stripes different color
            {
                for (int i = 0; i < NumStripes; i++)
                {
                    stripeColors[i] = ColorManager.GetRandomColor(stripeColors.Where(x => x != null).ToList());
                }
            }

            float[] stripeSizes = new float[NumStripes]; // Stripe size (0-1)
            
            if (!EvenStripes && R.NextDouble() < WIDE_MID_STRIPE_CHANCE)
            {
                float midStripeSize = RandomRange(MIN_MID_STRIPE_SIZE, MAX_MID_STRIPE_SIZE);
                float otherStripesSize = (1f - midStripeSize) / (NumStripes - 1);
                for (int i = 0; i < NumStripes; i++)
                {
                    if (i == NumStripes / 2) stripeSizes[i] = midStripeSize;
                    else stripeSizes[i] = otherStripesSize;
                }
            }
            else
            {
                for (int i = 0; i < NumStripes; i++)
                    stripeSizes[i] = 1f / NumStripes;
            }

            // Draw stripes
            float curRel = 0;
            for(int i = 0; i < NumStripes; i++)
            {
                float stripeSize = stripeSizes[i];
                
                DrawRectangle(SvgDocument,
                    StripeDirection == StripeDirectionType.Horizontal ? 0 : curRel * FlagWidth,
                    StripeDirection == StripeDirectionType.Horizontal ? curRel * FlagHeight : 0,
                    StripeDirection == StripeDirectionType.Horizontal ? FlagWidth : FlagWidth * stripeSize,
                    StripeDirection == StripeDirectionType.Horizontal ? FlagHeight * stripeSize : FlagHeight,
                    stripeColors[i]);
                curRel += stripeSize;
            }

            CoatOfArmsChance = COAT_OF_ARMS_CHANCE;

            float minCoaSize = 0, maxCoaSize = 0; // Absolute size

            switch(GetOverlayType())
            {
                case OverlayType.None:
                    if (!EvenStripes && R.NextDouble() < MID_STRIPE_SYMBOLS_CHANCE)
                    {
                        CoatOfArmsChance = 0f;
                        int numSymbols = RandomRange(MID_STRIPE_SYMBOLS_MIN_AMOUNT, MID_STRIPE_SYMBOLS_MAX_AMOUNT + 1);

                        Color symbolColor = ColorManager.GetRandomColor(new List<Color>() { stripeColors[NumStripes / 2] });

                        float midStripeWidth = stripeSizes[(NumStripes / 2)];
                        float minSymbolRelSize = Math.Min(midStripeWidth, 0.1f);
                        float maxSymbolRelSize = Math.Min(midStripeWidth, 1f / (numSymbols+1));
                        float symbolRelSize = RandomRange(minSymbolRelSize, maxSymbolRelSize);

                        Symbol symbol = GetRandomSymbol();
                        float relStepSize = 1f / (numSymbols + 1);
                        for(int i = 0; i < numSymbols; i++)
                        {
                            PointF position = new PointF(StripeDirection == StripeDirectionType.Horizontal ? (i + 1) * relStepSize * FlagWidth : FlagCenter.X, StripeDirection == StripeDirectionType.Horizontal ? FlagCenter.Y : (i + 1) * relStepSize * FlagHeight);
                            symbol.Draw(SvgDocument, this, position, StripeDirection == StripeDirectionType.Horizontal ? symbolRelSize * FlagHeight : symbolRelSize * FlagHeight, 0, symbolColor);
                        }

                    }

                    else // Coat of arms
                    {
                        CoatOfArmsPosition = FlagCenter;
                        List<Color> forbiddenCoaColors = EvenStripes ? new List<Color>() { stripeColors[NumStripes / 2 - 1], stripeColors[NumStripes / 2] } : new List<Color>() { stripeColors[NumStripes / 2] };
                        float coaSizeRel = EvenStripes ? stripeSizes[0] * 2 : stripeSizes[NumStripes / 2];
                        if (!EvenStripes && R.NextDouble() < BIG_COA_CHANCE)
                        {
                            coaSizeRel = stripeSizes[(NumStripes / 2) - 1] + stripeSizes[(NumStripes / 2)] + stripeSizes[(NumStripes / 2) + 1];
                            forbiddenCoaColors.Add(stripeColors[(NumStripes / 2) - 1]);
                            forbiddenCoaColors.Add(stripeColors[(NumStripes / 2) + 1]);
                        }

                        CoatOfArmsSize = StripeDirection == StripeDirectionType.Horizontal ? coaSizeRel * FlagHeight : coaSizeRel * FlagWidth;
                        CoatOfArmsSize = Math.Min(FlagHeight, CoatOfArmsSize);
                        CoatOfArmsSize *= 0.9f;
                        CoatOfArmsColor = ColorManager.GetRandomColor(forbiddenCoaColors);
                    }
                    break;

                case OverlayType.TopRightSquare:
                    List<Color> forbiddenColors = new List<Color>();
                    for (int i = 0; i <= NumStripes / 2; i++) forbiddenColors.Add(stripeColors[i]);
                    Color squareColor = ColorManager.GetRandomColor(forbiddenColors);
                    DrawRectangle(SvgDocument, 0, 0, FlagWidth / 2, FlagHeight / 2, squareColor);

                    // Coa
                    CoatOfArmsPosition = new PointF(FlagWidth / 4, FlagHeight / 4);
                    minCoaSize = FlagHeight / 4;
                    maxCoaSize = FlagHeight / 2;
                    CoatOfArmsColor = ColorManager.GetRandomColor(new List<Color>() { squareColor });
                    break;

                case OverlayType.LeftTriangle:
                    Color triangleColor = ColorManager.GetRandomColor(stripeColors.ToList());
                    float triangleWidth = RandomRange(LEFT_TRIANGLE_MIN_WIDTH, LEFT_TRIANGLE_MAX_WIDTH);
                    PointF[] vertices = new PointF[]
                    {
                    new PointF(0,0), new PointF(triangleWidth * FlagWidth, FlagHeight/2), new PointF(0, FlagHeight)
                    };
                    DrawPolygon(SvgDocument, vertices, triangleColor);

                    // Coa
                    CoatOfArmsPosition = new PointF((triangleWidth * 0.35f) * FlagWidth, FlagHeight / 2);
                    minCoaSize = (triangleWidth * 0.3f) * FlagWidth;
                    maxCoaSize = (triangleWidth * 0.6f) * FlagWidth;
                    CoatOfArmsColor = ColorManager.GetRandomColor(new List<Color>() { triangleColor });
                    break;

                case OverlayType.Antigua:
                    Color overlayColor = ColorManager.GetRandomColor(stripeColors.ToList());
                    PointF[] triangle1 = new PointF[] { new PointF(0, 0), new PointF(FlagWidth / 2, FlagHeight), new PointF(0, FlagHeight) };
                    PointF[] triangle2 = new PointF[] { new PointF(FlagWidth, 0), new PointF(FlagWidth / 2, FlagHeight), new PointF(FlagWidth, FlagHeight) };
                    DrawPolygon(SvgDocument, triangle1, overlayColor);
                    DrawPolygon(SvgDocument, triangle2, overlayColor);

                    // Coa
                    float height = RandomRange(0.2f, 0.4f);
                    CoatOfArmsPosition = new PointF(FlagWidth / 2, height * FlagHeight);
                    minCoaSize = 0.2f * FlagHeight;
                    maxCoaSize = Math.Min(0.5f, height * 2f) * FlagHeight;
                    CoatOfArmsColor = ColorManager.GetRandomColor(stripeColors.ToList());
                    break;
            }

            if(CoatOfArmsSize == 0) CoatOfArmsSize = RandomRange(minCoaSize, maxCoaSize);

            ApplyCoatOfArms(SvgDocument);
        }

        private int GetNumStripes()
        {
            int probabilitySum = NumStripesDictionary.Sum(x => x.Value);
            int rng = R.Next(probabilitySum);
            int tmpSum = 0;
            foreach (KeyValuePair<int, int> kvp in NumStripesDictionary)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum) return kvp.Key;
            }
            return 0;
        }

        private StripeDirectionType GetStripeDirection()
        {
            int probabilitySum = Overlays.Sum(x => x.Value);
            int rng = R.Next(probabilitySum);
            int tmpSum = 0;
            foreach (KeyValuePair<StripeDirectionType, int> kvp in StripeDirections)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum) return kvp.Key;
            }
            return StripeDirectionType.Horizontal;
        }

        private OverlayType GetOverlayType()
        {
            Dictionary<OverlayType, int> overlayCandidates = Overlays.Where(x => CanApplyOverlayType(x.Key)).ToDictionary(x => x.Key, x => x.Value);
            int probabilitySum = overlayCandidates.Sum(x => x.Value);
            int rng = R.Next(probabilitySum);
            int tmpSum = 0;
            foreach (KeyValuePair<OverlayType, int> kvp in overlayCandidates)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum) return kvp.Key;
            }
            return OverlayType.None;
        }
        private bool CanApplyOverlayType(OverlayType type)
        {
            switch(type)
            {
                case OverlayType.None:
                    return true;

                case OverlayType.LeftTriangle:
                    return StripeDirection == StripeDirectionType.Horizontal;

                case OverlayType.TopRightSquare:
                    return EvenStripes;

                case OverlayType.Antigua:
                    return true;

                default:
                    throw new Exception("Overlaytype not handled");
            }
        }
    }
}
