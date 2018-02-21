using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS_Lines
{
    public static class Extensions
    {
        public static bool IsOutOfTreshold(this double coord, double treshold)
        {
            if (coord > treshold) return true;
            return false;
        }
    }
}
