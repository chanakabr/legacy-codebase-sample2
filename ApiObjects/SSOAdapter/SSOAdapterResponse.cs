using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiObjects.Response;

namespace ApiObjects.SSOAdapter
{
    public class SSOAdapterResponse
    {
        public Status RespStatus { get; set; }
        public SSOAdapter SSOAdapter { get; set; }

        public SSOAdapterResponse()
        {
            RespStatus = new Status((int) eResponseStatus.Error, eResponseStatus.Error.ToString());
            SSOAdapter = new SSOAdapter();
        }
    }
}
