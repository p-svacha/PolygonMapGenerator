using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagGeneration
{
    class CoaOnly : FlagMainPattern
    {
        public enum Style
        {
            Plain,
            Frame,
            Diamond
        }

        private Dictionary<Style, int> Styles = new Dictionary<Style, int>()
        {
            {Style.Plain, 100 },
            {Style.Frame, 80 },
            {Style.Diamond, 60 }
        };

        public CoaOnly(Random r)
        {
            R = r;
        }

        private const float MIN_COA_SIZE_REL = 0.5f;

        private const float MIN_FRAME_SIZE = 0.05f; // relative to flag height
        private const float MAX_FRAME_SIZE = 0.3f; // relative to flag height
        private const float MAX_DIAMOND_FRAME_SIZE = 0.3f; // relative to flag height

        public override void Apply(SvgDocument SvgDocument)
        {
            Style style = GetRandomStyle();

            Color bgColor, secColor, coaColor = Color.Transparent;
            float minCoaSizeRel = 0, maxCoaSizeRel = 0;
            switch(style)
            {
                case Style.Plain:
                    bgColor = ColorManager.GetRandomColor();
                    coaColor = ColorManager.GetRandomColor(new List<Color>() { bgColor });
                    minCoaSizeRel = 0.6f;
                    maxCoaSizeRel = 0.95f;
                    DrawRectangle(SvgDocument, 0, 0, FlagWidth, FlagHeight, bgColor);
                    break;

                case Style.Frame:
                    bgColor = ColorManager.GetRandomColor();
                    secColor = ColorManager.GetRandomColor(new List<Color>() { bgColor });
                    coaColor = ColorManager.GetRandomColor(new List<Color>() { secColor });
                    float frameHeightRel = RandomRange(MIN_FRAME_SIZE, MAX_FRAME_SIZE);
                    float frameSize = frameHeightRel * FlagHeight;
                    DrawRectangle(SvgDocument, 0, 0, FlagWidth, FlagHeight, bgColor);
                    // Frame
                    DrawRectangle(SvgDocument, frameSize, frameSize, FlagWidth - 2*frameSize, FlagHeight - 2*frameSize, secColor);

                    minCoaSizeRel = 0.3f;
                    maxCoaSizeRel = 1f - (2f * frameHeightRel);
                    break;

                case Style.Diamond:
                    bgColor = ColorManager.GetRandomColor();
                    secColor = ColorManager.GetRandomColor(new List<Color>() { bgColor });
                    coaColor = ColorManager.GetRandomColor(new List<Color>() { secColor });
                    float frameSizeXRel = RandomRange(0f, MAX_DIAMOND_FRAME_SIZE);
                    float frameSizeYRel = RandomRange(0f, MAX_DIAMOND_FRAME_SIZE);
                    float frameSizeX = frameSizeXRel * FlagWidth;
                    float frameSizeY = frameSizeYRel * FlagHeight;
                    DrawRectangle(SvgDocument, 0, 0, FlagWidth, FlagHeight, bgColor);
                    // Frame
                    PointF[] vertices = new PointF[]
                    {
                        new PointF(FlagWidth/2, frameSizeY),
                        new PointF(FlagWidth - frameSizeX, FlagHeight / 2),
                        new PointF(FlagWidth/2, FlagHeight - frameSizeY),
                        new PointF(frameSizeX, FlagHeight/2)
                    };
                    DrawPolygon(SvgDocument, vertices, secColor);

                    minCoaSizeRel = 0.3f;
                    maxCoaSizeRel = 1f - (3 * Math.Max(frameSizeXRel, frameSizeYRel));
                    break;
            }

            

            float minCoaSize = FlagHeight * minCoaSizeRel;
            float maxCoaSize = FlagHeight * maxCoaSizeRel;
            float coaSize = RandomRange(minCoaSize, maxCoaSize);

            CoatOfArms coa = GetRandomCoa();
            coa.Draw(SvgDocument, this, FlagCenter, coaSize, coaColor, R);
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
            return Style.Plain;
        }
    }
}
