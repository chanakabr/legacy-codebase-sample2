using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class BusinessModuleResponse
    {
        public int Id { get; set; }

        public Status status { get; set; }

        public BusinessModuleResponse()
        {
            status = new Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Id = 0;
        }

        public BusinessModuleResponse(int Id, Status status)
        {
            this.Id = Id;
            this.status = status;
        }
    }


}
