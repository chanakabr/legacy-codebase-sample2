using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users.Saml
{
    public class SamlProviderObject
    {
        public int ID { get; set; }
        public int GroupID { get; set; }
        public string Name { get; set; }      
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string UrlCreds { get; set; }
        public string LogOutURL { get; set; }   
        public string ReturnURL { get; set; }
        public string Issuer { get; set; }
        public string Scope { get; set; }

    }
}
