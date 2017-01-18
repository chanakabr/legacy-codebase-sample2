using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Pricing
{
    public class PPVModuleDataResponse
    {
        public PPVModule PPVModule { get; set; }

        public ApiObjects.Response.Status Status { get; set; }
    }
}
