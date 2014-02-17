using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace M1BL
{
    public class M1Response
    {
        public M1Response()
        {
          
        }

        public M1Response(string r, bool se)
        {
            reason = r;
            is_succeeded = se;
        }

        public M1Response(string r, bool se, string desc)
        {
            reason = r;
            is_succeeded = se;
            description = desc;
        }

        public string reason { get; set; }
        public bool is_succeeded { get; set; }
        public string description { get; set; }
    }
}
