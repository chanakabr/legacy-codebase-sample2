using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    [Serializable]
    public class PinResponse
    {
        public Status status
        {
            get;
            set;
        }

        public string pin
        {
            get;
            set;
        }

        public eRuleLevel? level
        {
            get;
            set;
        }
    }
}
