using Svg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagGeneration
{
    public class FlagGenerator
    {
        public const int FLAG_WIDTH = 1200;
        public const int FLAG_HEIGHT = 800;

        private List<FlagMainPattern> MainPatterns = new List<FlagMainPattern>()
        {
            new HorizontalStripes()
        };

        public SvgDocument GenerateFlag()
        {
            Random r = new Random();

            SvgDocument SvgDoc = new SvgDocument()
            {
                Width = FLAG_WIDTH,
                Height = FLAG_HEIGHT
            };

            FlagMainPattern mainPattern = MainPatterns[r.Next(MainPatterns.Count)];
            mainPattern.Apply(SvgDoc);

            return SvgDoc;
        }
    }
}
