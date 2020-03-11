using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Base
{
    public class BaseSupportsNullable
    {
        public HashSet<string> NullableProperties { get; set; }

        public bool IsNullablePropertyExists(string str)
        {
            return NullableProperties != null && NullableProperties.Contains(str.ToLower());
        }
    }
}
