using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    [Serializable]
    public class UserDynamicDataContainer
    {
        public string data_type { get; set; }

        public string value { get; set; }
    }
}
