using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
namespace Tvinic.GoogleAPI
{
    #region "Notes"
    //Order Property of DataMember is used (only) for easier unit testing with Google's demo page. There is no "required order".
    //Otherwise, the default datacontract datamember order is alphabetical making it impossible to test using Google demo page.
    //Google testing/demo page: https://sandbox.google.com/checkout/customer/gadget/inapp/demo.html

    //REF: http://self-issued.info/docs/draft-jones-json-web-signature.html
    #endregion

    [DataContract]
    public abstract class JWTHeader
    {
        ///<summary>
        /// The alg (algorithm) header parameter identifies the cryptographic algorithm used to secure the JWS.
        /// The processing of the alg (algorithm) header parameter, if present, requires that the value of the alg header parameter MUST be one that is both supported and for which there exists a key for use with that algorithm associated with the signer of the content. 
        /// The alg parameter value is case sensitive. 
        /// This header parameter is REQUIRED.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>Note possible conflicting spec verbiage: Required vs. "if present"</remarks>
        [DataMember(EmitDefaultValue=true, IsRequired=true, Order=1)]
        public string alg { get; set; }
        /// <summary>
        /// The kid (key ID) header parameter is a hint indicating which specific key owned by the signer should be used to validate the signature. 
        /// This allows signers to explicitly signal a change of key to recipients. 
        /// Omitting this parameter is equivalent to setting it to an empty string. 
        /// The interpretation of the contents of the kid parameter is unspecified. 
        /// This header parameter is OPTIONAL.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [DataMember(EmitDefaultValue=false, IsRequired=false, Order=2)]
        public virtual string kid { get; set; }

        /// <summary>
        /// The typ (type) header parameter is used to declare the type of the signed content. 
        /// The typ value is case sensitive. 
        /// This header parameter is OPTIONAL.
        /// Possible values: "JWT" or "http://openid.net/specs/jwt/1.0".
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [DataMember(EmitDefaultValue = false, IsRequired = false, Order = 3)]
        public virtual string typ { get; set; }

        /// <summary>
        /// The jku (JSON Key URL) header parameter is a URL that points to JSON-encoded public keys that can be used to validate the signature. 
        /// The keys MUST be encoded as per the JSON Web Key (JWK) [JWK] specification. 
        /// This header parameter is OPTIONAL.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [DataMember(EmitDefaultValue=false, IsRequired=false)]
        public virtual Uri jku { get; set; }

        /// <summary>
        /// The x5u (X.509 URL) header parameter is a URL utilizing TLS RFC 5785 [RFC5785] that points to an X.509 public key certificate or certificate chain that can be used to validate the signature. 
        /// This certificate or certificate chain MUST use the PEM encoding RFC 1421 [RFC1421] and MUST conform to RFC 5280 [RFC5280]. 
        /// This header parameter is OPTIONAL.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [DataMember(EmitDefaultValue=false, IsRequired=false)]
        public virtual Uri x5u { get; set; }

        /// <summary>
        /// The x5t (x.509 certificate thumbprint) header parameter provides a base64url encoded SHA-1 thumbprint (a.k.a. digest) of the DER encoding of an X.509 certificate that can be used to match the certificate. 
        /// This header parameter is OPTIONAL.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [DataMember(EmitDefaultValue=false, IsRequired=false)]
        public virtual  string x5t { get; set; }

        public JWTHeader()
        { }
        
    }
}
