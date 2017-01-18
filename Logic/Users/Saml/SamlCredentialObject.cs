using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users.Saml
{
    public class SamlCredentialObject
    {
        public string Status { get; set; }
        public string Error { get; set; }
        public string Customer_ID { get; set; }
        public string entity_ID { get; set; }
    }
}
