using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    [Serializable]
    public class State
    {
        public int object_id { get; set; }

        public string state_name { get; set; }

        public string state_code { get; set; }

        public Country country { get; set; }
    }

}
