using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects
{
    public class DomainFilter
    {
        public string ExternalIdEqual { get; set; }

        public override string ToString()
        {
            return $"ExternalIdEqual:{ExternalIdEqual}.";
        }
    }
}
