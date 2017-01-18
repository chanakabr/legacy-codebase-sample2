using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users
{
    public class DomainStatusResponse
    {
        public DomainResponseObject DomainResponse { get; set; }

        public ApiObjects.Response.Status Status { get; set; }

    }
}
