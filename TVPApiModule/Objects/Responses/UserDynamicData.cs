using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    [Serializable]
    public class UserDynamicData
    {
        public UserDynamicDataContainer[] user_data { get; set; }
    }

}
