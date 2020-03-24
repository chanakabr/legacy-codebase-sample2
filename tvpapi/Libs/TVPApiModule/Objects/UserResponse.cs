using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects
{
    public class UserResponse
    {
        public ResponseStatus ResponseStatus { get; set; }
        public string StatusCode { get; set; }
        public string Message { get; set; }
    }
}
