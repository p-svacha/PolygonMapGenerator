using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagGeneration
{
    public static class ColorManager
    {
        private static Dictionary<Color, int> Colors = new Dictionary<Color, int>
        {
            {Color.FromArgb(255,0,0), 100}, // Red
            {Color.FromArgb(255,255,255), 80}, // White
            {Color.FromArgb(0,0,255), 60}, // Blue
            {Color.FromArgb(0,0,0), 50}, // Black
        };
        private static Random RNG = new Random();

        public static Color GetRandomColor(List<Color> excludedColors = null)
        {
            if (excludedColors == null) excludedColors = new List<Color>();
            Dictionary<Color, int> colorCandidates = Colors.Where(x => !excludedColors.Contains(x.Key)).ToDictionary(x => x.Key, y => y.Value);

            int probabilitySum = colorCandidates.Sum(x => x.Value);
            int rng = RNG.Next(probabilitySum);
            int tmpSum = 0;
            foreach (KeyValuePair<Color, int> kvp in colorCandidates)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum) return kvp.Key;
            }
            return Color.Transparent;
        }
    }
}
