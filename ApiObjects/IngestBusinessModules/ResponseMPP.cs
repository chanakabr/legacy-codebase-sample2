using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.IngestBusinessModules
{
    public class ResponseMPP
    {
        public List<MultiPricePlanResponse> mpp { get; set; }
        public Status Status { get; set; }
         
        public ResponseMPP()
        {
            Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
        }

        
    }
}
