using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users.Saml
{
    public class SAML
    {
        public string entityID { get; set; }
        public string keyInfo { get; set; }
        public string singleSignOnService { get; set; }
        public string x509 { get; set; }
        public string signatureValue { get; set; }
        public string privateKey { get; set;  }
    }
}
