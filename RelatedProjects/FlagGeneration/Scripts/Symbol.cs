﻿using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagGeneration
{
    public abstract class Symbol
    {
        public abstract void Draw(SvgDocument Svg, FlagMainPattern flag, PointF pos, float size, float angle, Color c, Random R);
    }
}
