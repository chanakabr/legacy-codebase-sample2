using ApiObjects.MediaMarks;
using Core.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Users
{
    public class ConcurrencyResponse
    {
        public DomainResponseStatus Status { get; set; }
        public DevicePlayData ConcurrencyData { get; set; }

        public ConcurrencyResponse()
        {
            this.Status = DomainResponseStatus.OK;
        }
    }
}
