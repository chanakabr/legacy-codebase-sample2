using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Response
{
    /// <summary>
    /// An exception message that is used to easily return a status code without using Status object:
    /// Just throw the KalturaException and the GetResponse method will catch it and return a Status object.
    /// </summary>
    public class KalturaException : Exception
    {
        public KalturaException(string message, int statusCode)
            : base(message)
        {
            this.Data["StatusCode"] = statusCode;
        }
    }
}
