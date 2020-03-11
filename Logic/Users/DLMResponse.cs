using ApiObjects;
using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users
{
    public class DLMResponse
    {
        public ApiObjects.Response.Status resp { get; set; }

        public LimitationsManager dlm { get; set; }

        public DLMResponse()
        {
            resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            dlm = new LimitationsManager();
        }

        public DLMResponse(ApiObjects.Response.Status eResp, LimitationsManager oDlm)
        {
            this.resp = eResp;
            this.dlm = oDlm;            
        }
    }
}
