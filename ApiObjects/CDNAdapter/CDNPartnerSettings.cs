using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class CDNPartnerSettings
    {
        public int? DefaultAdapter { get; set; }

        public int? DefaultRecordingAdapter { get; set; }
    }

    public class CDNPartnerSettingsResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public CDNPartnerSettings CDNPartnerSettings { get; set; }

        public CDNPartnerSettingsResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            CDNPartnerSettings = new CDNPartnerSettings();
        }
    }
}
