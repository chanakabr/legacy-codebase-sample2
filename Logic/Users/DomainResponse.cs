using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users
{
    public class DomainResponse
    {
        public Domain Domain { get; set; }

        public ApiObjects.Response.Status Status { get; set; }
    }
}
