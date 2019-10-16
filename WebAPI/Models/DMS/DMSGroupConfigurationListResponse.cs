using System.Collections.Generic;

namespace WebAPI.Models.DMS

{
    public class DMSGroupConfigurationListResponse
    {
        public DMSStatusResponse Result { get; set; }

        public List<DMSGroupConfiguration> GroupConfigurations { get; set; }
       
    }
}