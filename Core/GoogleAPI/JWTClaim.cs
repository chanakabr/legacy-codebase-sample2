using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Tvinic.GoogleAPI
{
      
    #region "Notes"
    //Order  of DataMember is used (only) for easier unit testing with Google's demo page. There is no "required order".
    //Otherwise, the default datacontract datamember order is alphabetical making it impossible to test using Google demo page.
    //Google testing/demo page: https://sandbox.google.com/checkout/customer/gadget/inapp/demo.html

    //REF: http://self-issued.info/docs/draft-jones-json-web-token.html#anchor4
#endregion

    [DataContract()]
    public abstract class JWTClaim
    {
        /// <summary>
        /// The iss (issuer) claim identifies the principal that issued the JWT. 
        /// The processing of this claim is generally application specific. 
        /// The iss value is case sensitive. 
        /// This claim is OPTIONAL.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [DataMember(EmitDefaultValue=false, Order=1)]
        public virtual string iss { get; set; }

        /// <summary>
        /// The aud (audience) claim identifies the audience that the JWT is intended for. 
        /// The principal intended to process the JWT MUST be identified by the value of the audience claim. 
        /// If the principal processing the claim does not identify itself with the identifier in the aud claim value then the JWT MUST be rejected. 
        /// The interpretation of the contents of the audience value is generally application specific. 
        /// The aud value is case sensitive. This claim is OPTIONAL.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [DataMember(EmitDefaultValue = false, Order = 2)]
        public virtual string aud { get; set; }

        /// <summary>
        /// The typ (type) claim is used to declare a type for the contents of this JWT. 
        /// The typ value is case sensitive. 
        /// This claim is OPTIONAL.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [DataMember(EmitDefaultValue = false, Order = 3)]
        public virtual string typ { get; set; }

        /// <summary>
        /// The iat (issued at) claim identifies the UTC time at which the JWT was issued. 
        /// The processing of the iat claim requires that the current date/time MUST be after the issued date/time listed in the iat claim. 
        /// Implementers MAY provide for some small leeway, usually no more than a few minutes, to account for clock skew. 
        /// This claim is OPTIONAL.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [DataMember(EmitDefaultValue = false, Order = 4)]
        public virtual long iat { get; set; }

        /// <summary>
        /// The exp (expiration time) claim identifies the expiration time on or after which the token MUST NOT be accepted for processing. 
        /// The processing of the exp claim requires that the current date/time MUST be before the expiration date/time listed in the exp claim. 
        /// Implementers MAY provide for some small leeway, usually no more than a few minutes, to account for clock skew. 
        /// This claim is OPTIONAL.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [DataMember(EmitDefaultValue = false, Order = 5)]
        public virtual long? exp { get; set; }

        public JWTClaim()
        { }

    }
}
