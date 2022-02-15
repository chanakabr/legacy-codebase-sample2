using Phx.Lib.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace RestAdaptersCommon
{
    public class IgnoreFromSignatureHash : Attribute { }

    public class BaseAdapterRequest
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string SEPERATOR_FOR_ENUMRABLE_VALUES = "";

        public int IngestProfileId { get; set; }
        public int GroupId { get; set; }

        [IgnoreFromSignatureHash]
        public string Signature { get; set; }
    }
}
