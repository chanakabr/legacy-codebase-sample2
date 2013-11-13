using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TVPWebApi.Models
{
    public class UnknownGroupException :Exception
    {
        public UnknownGroupException()
            : base(string.Format("Unknown Group."))
        {
        }

        public UnknownGroupException(string message)
            : base(string.Format("Unknown Group. {0}", message))
        {
        }

        public UnknownGroupException(string message, Exception inner)
            : base(string.Format("Unknown Group. {0}", message), inner)
        {
        }

        public UnknownGroupException(string sWSName, string sModuleName, string sUN, string sPass, string sIP)
            : base(string.Format("Unknown Group. sWSName: {0}, sModuleName: {1}, sUN: {2}, sPass: {3}, sIP: {4},", sWSName, sModuleName, sUN, sPass, sIP))
        {  
        }
    }
}