using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.IngestBusinessModules
{
    public class BusinessModuleResponse
    {
        public Status Status { get; set; }
        public string Code { get; set; }

        public BusinessModuleResponse()
        {
            Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
        }

        public BusinessModuleResponse(Status Status, string Code)
        {
            this.Status = Status;
            this.Code = Code;
        }
    }
}
