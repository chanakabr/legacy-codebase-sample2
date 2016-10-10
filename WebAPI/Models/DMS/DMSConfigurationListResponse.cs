using System.Collections.Generic;

namespace WebAPI.Models.DMS
{
    public class DMSConfigurationListResponse
    {
        public DMSStatusResponse Result { get; set; }

        public List<DMSAppVersion> Configurations { get; set; }
    }
}