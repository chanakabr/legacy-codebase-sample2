using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Phoenix.Context
{
    public class PhoenixResponseContext
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public const string PHOENIX_RESPONSE_CONTEXT_KEY = "PHOENIX_RESPONSE_CONTEXT";

        public int StatusCode { get; set; }
    }
}
