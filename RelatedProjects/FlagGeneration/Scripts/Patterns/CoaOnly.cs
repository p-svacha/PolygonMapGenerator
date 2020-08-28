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

        private const float MIN_COA_SIZE_REL = 0.5f;
        private const float MAX_COA_SIZE_REL = 0.9f;

        public CoaOnly(Random r)
        {
            R = r;
        }

        public override void Apply(SvgDocument SvgDocument)
        {
            Color bgColor = ColorManager.GetRandomColor();
            Color coaColor = ColorManager.GetRandomColor(new List<Color>() { bgColor });
            DrawRectangle(SvgDocument, 0, 0, FlagWidth, FlagHeight, bgColor);

            float minCoaSize = FlagHeight * MIN_COA_SIZE_REL;
            float maxCoaSize = FlagHeight * MAX_COA_SIZE_REL;

            float coaSize = RandomRange(minCoaSize, maxCoaSize);
            CoatOfArms coa = GetRandomCoatOfArms();
            coa.Draw(SvgDocument, FlagCenter, coaSize, coaColor, R);
        }
    }
}
