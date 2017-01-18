using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class TimeShiftedTvPartnerSettingsResponse
    {

        public ApiObjects.Response.Status Status { get; set; }
        public TimeShiftedTvPartnerSettings Settings { get; set; }

        public TimeShiftedTvPartnerSettingsResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Settings = null;
        }

        public TimeShiftedTvPartnerSettingsResponse(ApiObjects.Response.Status resp, TimeShiftedTvPartnerSettings settings)
        {
            this.Status = resp;
            this.Settings = settings;
        }

    }
}