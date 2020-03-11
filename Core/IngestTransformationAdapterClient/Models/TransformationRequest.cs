using RestAdaptersCommon;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdapterClients.IngestTransformation.Models
{
    class TransformationRequest : BaseAdapterRequest
    {
        public string FileUrl { get; set; }
    }
}
