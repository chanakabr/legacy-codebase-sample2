using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class MCResponseObject
    {
        public string status { get; set; }
        public int code { get; set; }
        public string name { get; set; }
        public string message { get; set; }
    }
}
