using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagGeneration
{
    class PrefabCoa : CoatOfArms
    {
        public override void Draw(SvgDocument Svg, FlagMainPattern flag, PointF pos, float size, Color c, Random R)
        {
            string[] files = Directory.GetFiles(Path.GetDirectoryName(Program.SavePath) + "/../../../RelatedProjects/FlagGeneration/Resources/CoatOfArms");
            string chosenPath = files[R.Next(files.Length)];

            SvgDocument prefab = SvgDocument.Open(chosenPath);
            prefab.Width = size;
            prefab.Height = size;
            prefab.X = pos.X - size / 2;
            prefab.Y = pos.Y - size / 2;
            SvgColourServer colServ = new SvgColourServer(c);
            prefab.Fill = colServ;
            prefab.Stroke = colServ;
            foreach (SvgElement elem in prefab.Children) FillElement(elem, colServ);

            Svg.Children.Add(prefab);
        }

        private void FillElement(SvgElement elem, SvgColourServer c)
        {
            if(!elem.Fill.Equals(new SvgColourServer(Color.Transparent))) elem.Fill = c;
            elem.Stroke = c;
            foreach (SvgElement elem2 in elem.Children) FillElement(elem2, c);
        }
    }
}
