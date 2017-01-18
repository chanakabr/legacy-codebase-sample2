using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Response
{
    public class StatusErrorCodesResponse
    {
        public List<KeyValuePair> ErrorsDictionary { get; set; }

        public Status Status { get; set; }

        public StatusErrorCodesResponse()
        {
            ErrorsDictionary = new List<KeyValuePair>();
            Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
        }
    }
}
