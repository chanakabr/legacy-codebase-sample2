using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.ServiceInterface
{
    public class UnknownGroupException : Exception
    {
        public UnknownGroupException()
            : base(string.Format("Unknown Group"))
        {
        }
    }
}