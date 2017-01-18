using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class LinkResult
    {
        public string Url { get; set; }

        public string ProviderStatusCode { get; set; }

        public string ProviderStatusMessage { get; set; }
    }
}
