using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Notification
{
    public class UserDevice
    {
        public string Udid { get; set; }
        public long SignInAtSec { get; set; }
    }
}
