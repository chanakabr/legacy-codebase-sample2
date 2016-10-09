using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.DMS
{
    public class DMSGroupConfigurationGetResponse
    {
        public DMSStatusResponse Result { get; set; }

        public DMSGroupConfiguration GroupConfiguration { get; set; }
    }
}