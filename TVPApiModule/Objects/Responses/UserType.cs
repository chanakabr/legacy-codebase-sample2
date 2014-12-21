using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    [Serializable]
    public class UserType
    {
        public int? id { get; set; }

        public string description { get; set; }

        public bool is_default { get; set; }
    }
}
