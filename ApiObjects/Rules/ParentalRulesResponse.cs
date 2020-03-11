using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class ParentalRulesResponse
    {
        public Status status
        {
            get;
            set;
        }

        public List<ParentalRule> rules
        {
            get;
            set;
        }

        public ParentalRulesResponse()
        {
            this.rules = new List<ParentalRule>();
        }
    }
}
