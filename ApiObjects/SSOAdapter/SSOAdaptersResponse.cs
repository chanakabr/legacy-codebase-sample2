using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiObjects.Response;

namespace ApiObjects.SSOAdapter
{
    public class SSOAdaptersResponse
    {
        public Status RespStatus { get; set; }
        public IEnumerable<SSOAdapter> SSOAdapters { get; set; }

        public SSOAdaptersResponse()
        {
            RespStatus = new Status((int) eResponseStatus.Error, eResponseStatus.Error.ToString());
            SSOAdapters = new List<SSOAdapter>();
        }
    }
}
