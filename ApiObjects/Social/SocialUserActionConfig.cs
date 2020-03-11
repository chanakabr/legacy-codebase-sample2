
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Social
{
    [DataContract]
    public class SocialUserActionConfig : SocialConfig
    {
        [DataMember]
        List<ApiObjects.Social.ActionPermissionItem> ActionPermissionItemList { get; set; }
        [DataMember]
        ApiObjects.eSocialPrivacy ePrivacy { get; set; }
    }


    [KnownType(typeof(SocialUserActionConfig))]
    public class SocialConfig
    {
        public ApiObjects.Response.Status Status { get; set; }
        public ApiObjects.Social.PlatformConfig PlatformConfig { get; set; }

        public SocialConfig()
        {
            Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
        }
    }
}
