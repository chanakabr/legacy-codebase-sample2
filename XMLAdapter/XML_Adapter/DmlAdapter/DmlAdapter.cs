using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XMLAdapter
{
    public sealed class DmlAdapter : BaseXMLAdapter
    {
        // handle the translation excel files and init base
        public override void Init()
        {
            base.Init(); // Init base
        }

        // Get GCD
        static int GCD(int a, int b)
        {
            int Remainder = 0;

            while (b != 0)
            {
                Remainder = a % b;
                a = b;
                b = Remainder;
            }

            return a;
        }

        // Find x y ratio
        public override string GetRatio(string sx, string sy)
        {
            int x = int.Parse(sx);
            int y = int.Parse(sy);
            return string.Format("{0}:{1}", x / GCD(x, y), y / GCD(x, y)); 
        }
    }
}
