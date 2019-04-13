using RestAdaptersCommon;
using System;
using System.Collections.Generic;
using System.Text;

namespace IngestTransformationAdapterClient.Models
{
    class SetConfigurationRequest : BaseAdapterRequest
    {
        public Dictionary<string,string> Configuration { get; set; }
    }
}
