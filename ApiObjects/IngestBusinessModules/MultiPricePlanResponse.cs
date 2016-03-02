using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.IngestBusinessModules
{
    public class MultiPricePlanResponse
    {
        public Status Status { get; set; }
        public string Code { get; set; }

        public MultiPricePlanResponse()
        {
        }

        public MultiPricePlanResponse(Status Status, string Code)
        {
            this.Status = Status;
            this.Code = Code;
        }
    }
}
