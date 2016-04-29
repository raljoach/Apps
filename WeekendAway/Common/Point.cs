﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Point
    {
        public double X;
        public double Y;
        private GeoLocation start;

        public Point(GeoLocation start)
        {
            this.start = start;
        }

        public Point(double x, double y) { this.X = x; this.Y = y; }
    }
}
