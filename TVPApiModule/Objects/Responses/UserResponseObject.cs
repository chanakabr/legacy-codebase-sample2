using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class UserResponseObject
    {
        public ResponseStatus respStatus { get; set; }

        public User user { get; set; }

        public string userInstanceID { get; set; }

    }
}
