using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Social
{   
    [DataContract]
    public class ActionPermissionItem
    {
        [DataMember]
        public ApiObjects.eUserAction  Action { get; set; }
        [DataMember]
        public SocialPlatform Platform { get; set; }      
    }
}
