using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Rules
{
    public class GenericRule
    {
        public long Id { get; set; }

        public RuleType RuleType { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
