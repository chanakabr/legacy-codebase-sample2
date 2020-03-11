using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Social
{
    public class SocialPrivacySettingsResponse
    {
        public SocialPrivacySettings settings { get; set; }
        public ApiObjects.Response.Status Status { get; set; }

        public SocialPrivacySettingsResponse()
        {
            Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
        }
    }
}
