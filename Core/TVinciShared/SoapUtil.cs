#if NETCOREAPP3_1

using System;
using System.Collections.Generic;
using System.Text;


namespace System.Web.Services
{
    public sealed class WebMethodAttribute : Attribute
    {
        public bool EnableSession { get; set; }
    }

    public class WebService
    {

    }
}

#endif