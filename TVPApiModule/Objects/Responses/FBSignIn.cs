using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVPApiModule.Objects.Responses
{
    public class FBSignIn
    {        
        public string status { get; set; }
        public string message { get; set; }
        public UserResponseObject user { get; set; }
    }
}
