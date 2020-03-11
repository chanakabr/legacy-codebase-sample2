using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Rules
{
    public class GenericRuleResponse
    {
        public Status Status { get; set; }

        public List<GenericRule> Rules { get; set; }

        public GenericRuleResponse()
        {
            Rules = new List<GenericRule>();
        }
    }
}
