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
            {Color.FromArgb(199,4,44), 100}, // Red
            {Color.FromArgb(255,255,255), 90}, // White
            {Color.FromArgb(0,85,164), 80}, // Blue
            {Color.FromArgb(0,140,69), 70}, // Green
            {Color.FromArgb(255,205,0), 60}, // Yellow
            {Color.FromArgb(0,0,0),50}, // Black
            {Color.FromArgb(117,170,219), 40}, // Light Blue
            {Color.FromArgb(141,21,58), 30}, // Violet-Red
            {Color.FromArgb(235,116,0), 30}, // Orange
            {Color.FromArgb(0,83,78), 30}, // Jungle Green
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
