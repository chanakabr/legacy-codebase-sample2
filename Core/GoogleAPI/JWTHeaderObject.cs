using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Tvinic.GoogleAPI
{
  
    [DataContract()]
    public class JWTHeaderObject : JWTHeader
    {
        /// <summary>
        /// When using this constructor, refer to property Intellisense documentation/information/hints for requirements.
        /// </summary>
        /// <remarks></remarks>
        public JWTHeaderObject()
        {}

        /// <summary>
        /// Alg only constructor.
        /// </summary>
        /// <param name="alg">Google In-App Payments only supports "HS256".</param>
        /// <remarks></remarks>
        public JWTHeaderObject(string alg)
        {
            base.alg = JWTHelpers.parseJWTHashEnum(alg).ToString();
        }

        /// <summary>
        /// Alg only constructor.
        /// </summary>
        /// <param name="alg">Google In-App Payments only supports JWTHash.HS256 (HS256)</param>
        /// <remarks></remarks>
        public JWTHeaderObject(JWTHash alg)
        {
            base.alg = alg.ToString();
            }

        /// <summary>
        /// Alg and Kid constructor.
        /// </summary>
        /// <param name="alg">Google In-App Payments only supports "HS256"</param>
        /// <param name="kid">At this time your Google account only has one key so if you will use this consturctor: "1"</param>
        /// <remarks></remarks>
        public JWTHeaderObject(string alg, string kid)
        {
            base.alg = JWTHelpers.parseJWTHashEnum(alg).ToString();
            base.kid = kid;
        }

        /// <summary>
        /// Alg and Kid constructor.
        /// </summary>
        /// <param name="alg">Google In-App Payments only supports JWTHash.HS256 (HS256)</param>
        /// <param name="kid">At this time your Google account only has one key so if you will use this consturctor: "1"</param>
        /// <remarks></remarks>
        public JWTHeaderObject(JWTHash alg, string kid)
        {
            base.alg = alg.ToString();
            base.kid = kid;
        }

        /// <summary>
        /// Alg, Kid and Typ constructor.
        /// </summary>
        /// <param name="alg">Google In-App Payments only supports "HS256"</param>
        /// <param name="kid">At this time your Google account only has one key so if you will use this consturctor: "1"</param>
        /// <param name="typ">Set to "JWT"</param>
        /// <remarks></remarks>
        public JWTHeaderObject(string alg, string kid, string typ)
        {
            base.alg = JWTHelpers.parseJWTHashEnum(alg).ToString();
            base.kid = kid;
            base.typ = typ;
        }

        /// <summary>
        /// Alg, Kid and Typ constructor.
        /// </summary>
        /// <param name="alg">Google In-App Payments only supports JWTHash.HS256 (HS256)</param>
        /// <param name="kid">At this time your Google account only has one key so if you will use this consturctor: "1"</param>
        /// <param name="typ">Set to "JWT"</param>
        /// <remarks></remarks>
        public JWTHeaderObject(JWTHash alg, string kid, string typ)
        {
            base.alg = alg.ToString();
            base.kid = kid;
            base.typ = typ;
        }

        /// <summary>
        /// 06/21/2011: In-App Payments API currently uses JWT v1 and HMAC SHA-256 encryption
        /// </summary>
        /// <remarks>http://sites.google.com/site/inapppaymentsapi/reference#jwt</remarks>
        public enum JWTHash
        {
            //HMAC using SHA
            HS256,
            HS384,
            HS512,

            //not supported for Google In-App Payments
            none,

            //RSA using SHA: none of these are currently used in this library (futures)
            //RS256
            //RS384
            //RS512

            //ECDSA using P-256 curve and SHA: none of these are currently used in this library (futures)
            //ES256
            //ES384
            //ES512
        }

    }
}
