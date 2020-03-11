using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users.Saml
{
    public class SamlResponseObject
    {
        public int Provider_ID { get; set; }
        public string Tvinci_ID { get; set; }
        public string Customer_ID { get; set; }
        public string Guid_ID { get; set; }        
        public int Domain_ID { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }
        public string Scope { get; set; }
        public string Name_ID { get; set; }
        public string Session_ID { get; set; }
        public string Assertion_ID { get; set; }
       
    }

    public enum eSamlStatus
    {
        Error = 0,
        Success = 1
    }

    // Summary:
    //     The XML namespace URIs.
    public static class NamespaceURIs
    {
        // Summary:
        //     The SAML assertion namespace URI.
        public const string Assertion = "urn:oasis:names:tc:SAML:2.0:assertion";
        //
        // Summary:
        //     The SAML metadata namespace URI.
        public const string Metadata = "urn:oasis:names:tc:SAML:2.0:metadata";
        //
        // Summary:
        //     The SAML protocol namespace URI.
        public const string Protocol = "urn:oasis:names:tc:SAML:2.0:protocol";
    }

    // Summary:
    //     The XML prefixes.
    public static class Prefixes
    {
        // Summary:
        //     The SAML metadata prefix.
        public const string MD = "md";
        //
        // Summary:
        //     The SAML prefix.
        public const string SAML = "saml";
        //
        // Summary:
        //     The SAML protocol prefix.
        public const string SAMLP = "samlp";
    }
}
