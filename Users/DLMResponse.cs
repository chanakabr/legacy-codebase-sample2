using ApiObjects;
using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Users
{
    public class DLMResponse
    {
        public StatusObject resp { get; set; }

        public LimitationsManager dlm { get; set; }

        public DLMResponse()
        {
            resp = new StatusObject((int)eResponseStatus.InternalError, string.Empty);
            dlm = new LimitationsManager();
        }

        public DLMResponse(StatusObject eResp, LimitationsManager oDlm)
        {
            this.resp = eResp;
            this.dlm = oDlm;            
        }
    }
}
