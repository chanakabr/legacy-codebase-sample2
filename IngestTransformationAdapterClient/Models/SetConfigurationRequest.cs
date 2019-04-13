using RestAdaptersCommon;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdapterClients.IngestTransformation.Models
{
    public class SetConfigurationRequest : BaseAdapterRequest
    {
        public Dictionary<string,string> Configuration { get; set; }
    }
}
