using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class UserResponseObject
    {
        public eResponseStatus resp_status { get; set; }

        public User user { get; set; }

        public string user_instance_id { get; set; }

    }
}
