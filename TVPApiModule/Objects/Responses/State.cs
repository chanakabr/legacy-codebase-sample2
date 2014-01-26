using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class State
    {
        public int objectID { get; set; }

        public string stateName { get; set; }

        public string stateCode { get; set; }

        public Country country { get; set; }
    }

}
