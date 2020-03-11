using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;
using KLogMonitor;
using System.Reflection;

namespace Tvinic.GoogleAPI
{
    public static class JWTHelpers
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).Ticks = 621355968000000000
        /// </summary>
        /// <remarks></remarks>
        const long UNIX_EPOCH_TICKS = 621355968000000000L;

        #region "JWT"

        /// <summary>
        /// Base64url encoder
        /// </summary>
        /// <param name="jsonString">String to Base64url encode</param>
        /// <returns>Base64url string</returns>
        /// <remarks></remarks>
        private static string jwtEncodeB64Url(string jsonString)
        {
            if (!string.IsNullOrEmpty(jsonString))
            {
                return convertToBase64url(System.Convert.ToBase64String(new UTF8Encoding(true, true).GetBytes(jsonString)));
            }
            else
            {
                throw new ArgumentNullException("jsonString", "String is required.");
            }

        }

        /// <summary>
        /// Base64url decoder
        /// </summary>
        /// <param name="base64UrlString">Base64url string to decode</param>
        /// <returns>Decoded string</returns>
        /// <remarks></remarks>
        public static string jwtDecodeB64Url(string base64UrlString)
        {
            if (!string.IsNullOrEmpty(base64UrlString))
            {
                try
                {
                    base64UrlString = base64UrlString.Replace("-", "+").Replace("_", "/");

                    int m = base64UrlString.Length % 4;
                    if (m != 0)
                    {
                        switch (m)
                        {
                            case 2:
                            case 3:
                                return new UTF8Encoding(true, true).GetString(System.Convert.FromBase64String(base64UrlString.PadRight(base64UrlString.Length + (4 - m), '=')));
                            default:
                                return string.Empty;
                        }
                    }
                    else
                    {
                        return new UTF8Encoding(true, true).GetString(System.Convert.FromBase64String(base64UrlString));
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("String is malformed/invalid base64 string.", "base64UrlString", ex);
                }

            }
            else
            {
                throw new ArgumentNullException("base64UrlStrings", "String is required.");
            }
        }

        /// <summary>
        /// Generate HMAC SHA-256 signature
        /// </summary>
        /// <param name="jwtHeaderAndClaim">Concatenated Base64url Header Input and Payload Input, delimited by period (.)</param>
        /// <param name="sellerSecret">Your Seller Secret</param>
        /// <returns>JWT signature</returns>
        /// <remarks></remarks>
        private static string jwtHMAC256(string jwtHeaderAndClaim, string sellerSecret)
        {
            if (!string.IsNullOrEmpty(jwtHeaderAndClaim) && !string.IsNullOrEmpty(sellerSecret))
            {
                UTF8Encoding _utf8 = new UTF8Encoding(true, true);
                byte[] _key = _utf8.GetBytes(sellerSecret);
                byte[] _btxt = _utf8.GetBytes(jwtHeaderAndClaim);
                string _str = string.Empty;
                using (HMACSHA256 hmac = new HMACSHA256(_key))
                {
                    _str = convertToBase64url(System.Convert.ToBase64String(hmac.ComputeHash(_btxt)));
                    hmac.Clear();
                }
                return _str;
            }
            else
            {
                throw new ArgumentNullException(string.IsNullOrEmpty(jwtHeaderAndClaim) ? "jwtHeaderAndClaim" : "hkey", "Argument cannot be null/empty");
            }
        }

        /// <summary>
        /// Build JWT
        /// </summary>
        /// <param name="HeaderObj">JWTHeaderObject</param>
        /// <param name="ClaimObj">InAppItemObject</param>
        /// <param name="sellerSecret">Your Seller Secret</param>
        /// <returns>JWT string</returns>
        /// <remarks></remarks>
        public static string buildJWT(JWTHeaderObject HeaderObj, InAppItemObject ClaimObj, string sellerSecret)
        {
            if (HeaderObj != null && ClaimObj != null && !string.IsNullOrEmpty(sellerSecret))
            {
                try
                {
                    string _jh = jwtEncodeB64Url(JSONHelpers.dataContractToJSON(HeaderObj));
                    string _jp = jwtEncodeB64Url(JSONHelpers.dataContractToJSON(ClaimObj));
                    string _sig = string.Empty;
                    switch (parseJWTHashEnum(HeaderObj.alg))
                    {
                        case JWTHeaderObject.JWTHash.HS256:
                            _sig = jwtHMAC256(String.Concat(_jh, ".", _jp), sellerSecret);
                            break;
                        case JWTHeaderObject.JWTHash.HS384:
                            _sig = jwtHMAC384(String.Concat(_jh, ".", _jp), sellerSecret);
                            break;
                        case JWTHeaderObject.JWTHash.HS512:
                            _sig = jwtHMAC512(String.Concat(_jh, ".", _jp), sellerSecret);
                            break;
                        default:
                            _sig = string.Empty;
                            break;
                    }
                    return string.Concat(_jh, ".", _jp, ".", _sig);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message.ToString());
                }
            }
            else
            {
                throw new ArgumentNullException(HeaderObj == null ? new JWTHeaderObject().ToString() : ClaimObj == null ? new InAppItemObject().ToString() : "sellerSecret", "Null Argument");
            }
        }
        public static string buildJWT(JWTHeaderObject HeaderObj, InAppItemSubscriptionObject ClaimObj, string sellerSecret)
        {
            if (HeaderObj != null && ClaimObj != null && !string.IsNullOrEmpty(sellerSecret))
            {
                try
                {
                    string _jh = jwtEncodeB64Url(JSONHelpers.dataContractToJSON(HeaderObj));
                    string _jp = jwtEncodeB64Url(JSONHelpers.dataContractToJSON(ClaimObj));
                    string _sig = string.Empty;
                    switch (parseJWTHashEnum(HeaderObj.alg))
                    {
                        case JWTHeaderObject.JWTHash.HS256:
                            _sig = jwtHMAC256(String.Concat(_jh, ".", _jp), sellerSecret);
                            break;
                        case JWTHeaderObject.JWTHash.HS384:
                            _sig = jwtHMAC384(String.Concat(_jh, ".", _jp), sellerSecret);
                            break;
                        case JWTHeaderObject.JWTHash.HS512:
                            _sig = jwtHMAC512(String.Concat(_jh, ".", _jp), sellerSecret);
                            break;
                        default:
                            _sig = string.Empty;
                            break;
                    }
                    return string.Concat(_jh, ".", _jp, ".", _sig);
                    //return "eyJhbGciOiJIUzI1NiIsImtpZCI6IjEiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiIwNjUxMTIxMDU0NjI5MTg5MTcxMyIsImF1ZCI6Ikdvb2dsZSIsInR5cCI6Imdvb2dsZVwvcGF5bWVudHNcL2luYXBwXC9zdWJzY3JpcHRpb25cL3YxIiwiaWF0IjoxMzYwMjQzMTEyLCJleHAiOjEzNjAyNDY3MDMsInJlcXVlc3QiOnsibmFtZSI6IlBpZWNlIG9mIENha2UiLCJkZXNjcmlwdGlvbiI6IkEgZGVsaWNpb3VzIHBpZWNlIG9mIHZpcnR1YWwgY2FrZSIsInNlbGxlckRhdGEiOiJZb3VyIERhdGEgSGVyZSIsImluaXRpYWxQYXltZW50Ijp7InByaWNlIjoiMTAuNTAiLCJjdXJyZW5jeUNvZGUiOiJVU0QiLCJwYXltZW50VHlwZSI6InByb3JhdGVkIn0sInJlY3VycmVuY2UiOnsicHJpY2UiOiI0Ljk5IiwiY3VycmVuY3lDb2RlIjoiVVNEIiwic3RhcnRUaW1lIjoiMTM2MDE3MTg1MiIsImZyZXF1ZW5jeSI6Im1vbnRobHkiLCJudW1SZWN1cnJlbmNlcyI6IjEyIn19fQ.RhU4hSpQk3QqxaNnJnNdy9JaQLlqBHWvPBxFHP6Bz3s";
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message.ToString());
                }
            }
            else
            {
                throw new ArgumentNullException(HeaderObj == null ? new JWTHeaderObject().ToString() : ClaimObj == null ? new InAppItemObject().ToString() : "sellerSecret", "Null Argument");
            }
        }

        /// <summary>
        /// Verify JWT (header test and signature matching only)
        /// </summary>
        /// <param name="jwtString">The raw JWT string to parse</param>
        /// <param name="sellerSecret">Your Seller Secret</param>
        /// <param name="jwtHeader">out: string for deserializing to JWTHeaderObject if TRUE</param>
        /// <param name="jwtClaim">out: string for deserializing to InAppItemObject if TRUE</param>
        /// <returns>Boolean</returns>
        /// <remarks></remarks>
        public static bool verifyJWT(string jwtString, string sellerSecret,
                           ref string jwtHeader, ref string jwtClaim)
        {
            string[] _arr = jwtString.Split('.');
            if (_arr.Length == 3)
            {
                jwtHeader = _arr[0];
                jwtClaim = _arr[1];
                string _sig = _arr[2];
                if (!string.IsNullOrEmpty(jwtHeader) && !string.IsNullOrEmpty(jwtClaim) && !string.IsNullOrEmpty(_sig))
                {
                    try
                    {
                        JWTHeaderObject header = JSONHelpers.dataContractJSONToObj(jwtHeader, new JWTHeaderObject()) as JWTHeaderObject;
                        //JWT header test and signature verification
                        //*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~
                        return (header != null && verifySignature(header.alg, jwtHeader, jwtClaim, _sig, sellerSecret));
                    }
                    catch (Exception ex)
                    {
                        log.Error("", ex);
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }



        /// <summary>
        ///  A stricter JWT verification routine. Verifies values in header and payload (not just signuature matching).
        /// </summary>       
        /// <param name="iss">Issuer claim: will be compared with JWT iss. Currently "Google" (case-sensitive).</param>
        /// <param name="aud">Audience claim: will be compared with JWT aud. Must match your Merchant/Seller ID (case-sensitive).</param>
        /// <param name="typ">Type Claim: will by compared with JWT typ. Currently "goog.payments.inapp.buyItem.v1.postback"</param>
        /// <param name="clockSkewMinutes">Minute(s) to adjust your clock. 0 for no adjustment. Behavior: add when verifying iat, subtract when verifying exp. Restrict to a few mintues only.</param>
        /// <param name="jwtString">The raw JWT string to parse</param>
        /// <param name="sellerSecret">Your Seller Secret</param>
        /// <param name="HeaderObj">out: JWTHeaderObject</param>
        /// <param name="ClaimObj">out: InAppItemObject</param>
        /// <returns>Boolean</returns>
        /// <remarks></remarks>
        public static bool verifyJWT(string iss, string aud, string typ, int clockSkewMinutes, string jwtString, string sellerSecret,
                           ref JWTHeaderObject HeaderObj, ref InAppItemObject ClaimObj)
        {
            string[] _arr = jwtString.Split('.');
            if (_arr.Length == 3)
            {
                string _jwth = _arr[0];
                string _jwtp = _arr[1];
                string _sig = _arr[2];
                if (!string.IsNullOrEmpty(_jwth) && !string.IsNullOrEmpty(_jwtp) && !string.IsNullOrEmpty(_sig))
                {
                    try
                    {
                        HeaderObj = JSONHelpers.dataContractJSONToObj(_jwth, new JWTHeaderObject()) as JWTHeaderObject;
                        //JWT header test and signature verification
                        //*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~
                        if (HeaderObj != null && verifySignature(HeaderObj.alg, _jwth, _jwtp, _sig, sellerSecret))
                        {
                            //If signature is verified, proceed with JWT payload verification
                            //*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~
                            ClaimObj = JSONHelpers.dataContractJSONToObj(_jwtp, new InAppItemObject()) as InAppItemObject;
                            //ClaimObj must have request and response
                            //*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~
                            if (ClaimObj != null && ClaimObj.request != null && ClaimObj.response != null)
                            {
                                bool _validexp = true;
                                long _myservertime = myServerClock();
                                if (ClaimObj.exp.HasValue)
                                {
                                    //exp must be > than current system clock time
                                    _validexp = _myservertime - (clockSkewMinutes * 60) < ClaimObj.exp.Value;
                                }
                                //iat must be < than current system clock time, exp must be valid, iss, aud and typ claims are verified
                                return (_validexp && ClaimObj.iat < _myservertime + (clockSkewMinutes * 60) && string.Equals(ClaimObj.iss, iss, StringComparison.Ordinal) && string.Equals(ClaimObj.aud, aud, StringComparison.Ordinal) && string.Equals(ClaimObj.typ, typ, StringComparison.Ordinal));
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("", ex);
                        return false;
                    }
                }

                else
                {


                    return false;
                }
            }
            else
            {


                return false;
            }
        }


        #endregion

        #region "Utils"

        /// <summary>
        /// Parses string to enum
        /// </summary>
        /// <param name="algString">String to parse</param>
        /// <returns>JWTHeaderObject.JWTHash enum (returns JWTHeaderObject.JWTHash.none if match is not found). Google In-App Payments currently only uses HS256 (06/2011)</returns>
        /// <remarks></remarks>
        public static JWTHeaderObject.JWTHash parseJWTHashEnum(string algString)
        {
            if (!string.IsNullOrEmpty(algString))
            {
                //bool boolFunc = Enum.TryParse(algString, out _ci);
                object d = Enum.Parse(typeof(JWTHeaderObject.JWTHash), algString, true);


                if (d != null)
                {
                    return (JWTHeaderObject.JWTHash)d;
                }
                else
                {
                    return JWTHeaderObject.JWTHash.none;
                }
            }
            else
            {
                return JWTHeaderObject.JWTHash.none;
            }
        }

        /// <summary>
        /// The number of seconds from 1970-01-01T0:0:0Z as measured in UTC until the desired date/time. 
        /// </summary>
        /// <returns>Integer</returns>
        /// <remarks>If by any chance this is still in use in 2038, revisit this before 1/19/2038 (Int32 to Int64)</remarks>
        public static long myServerClock()
        {
            return Convert.ToInt32(((DateTime.UtcNow.Ticks - UNIX_EPOCH_TICKS) / 10000000));
        }

        /// <summary>
        /// Converts base64 string to base64url
        /// </summary>
        /// <param name="s">base64 string</param>
        /// <returns>base64url string</returns>
        /// <remarks></remarks>
        private static string convertToBase64url(string s)
        {
            return s.Replace("=", String.Empty).Replace("+", "-").Replace("/", "_");
        }

        /// <summary>
        /// Signature verification
        /// </summary>
        /// <param name="alg">JWT Header alg string</param>
        /// <param name="jwtHeader">Raw JWT header claim</param>
        /// <param name="jwtClaim">Raw JWT payload claim</param>
        /// <param name="jwtSignature">Raw JWT signature</param>
        /// <param name="sellerSecret">YOUR SELLER KEY</param>
        /// <returns>Boolean</returns>
        /// <remarks></remarks>
        public static bool verifySignature(string alg, string jwtHeader, string jwtClaim, string jwtSignature, string sellerSecret)
        {
            switch (parseJWTHashEnum(alg))
            {
                case JWTHeaderObject.JWTHash.HS256:
                    return string.Equals(jwtSignature, jwtHMAC256(String.Concat(jwtHeader, ".", jwtClaim), sellerSecret), StringComparison.Ordinal);
                case JWTHeaderObject.JWTHash.HS384:
                    return string.Equals(jwtSignature, jwtHMAC384(String.Concat(jwtHeader, ".", jwtClaim), sellerSecret), StringComparison.Ordinal);
                case JWTHeaderObject.JWTHash.HS512:
                    return string.Equals(jwtSignature, jwtHMAC512(String.Concat(jwtHeader, ".", jwtClaim), sellerSecret), StringComparison.Ordinal);
                default:
                    return false;

            }
        }

        #endregion

        #region "Not used for Google In-App Payments"

        /// <summary>
        /// Generate HMAC SHA-384 signature. Not used for Google In-App Payments (06/2011).
        /// </summary>
        /// <param name="jwtHeaderAndClaim">Concatenated Base64url Header Input and Payload Input, delimited by period (.)</param>
        /// <param name="sellerSecret">Your Seller Secret</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private static string jwtHMAC384(string jwtHeaderAndClaim, string sellerSecret)
        {
            if (!string.IsNullOrEmpty(jwtHeaderAndClaim) && (!string.IsNullOrEmpty(sellerSecret)))
            {
                UTF8Encoding _utf8 = new UTF8Encoding(true, true);
                byte[] _key = _utf8.GetBytes(sellerSecret);
                byte[] _btxt = _utf8.GetBytes(jwtHeaderAndClaim);
                string _str = String.Empty;
                using (HMACSHA384 hmac = new HMACSHA384(_key))
                {
                    _str = System.Convert.ToBase64String(hmac.ComputeHash(_btxt)).Replace("=", String.Empty).Replace("+", "-").Replace("/", "_");
                    hmac.Clear();
                }
                Debug.WriteLine(string.Format("From hash: {0}", _str));
                return _str;
            }
            else
            {
                throw new ArgumentNullException(string.IsNullOrEmpty(jwtHeaderAndClaim) ? "jwtHeaderAndClaim" : "sellerSecret", "Argument cannot be null/empty");
            }
        }

        /// <summary>
        /// Generate HMAC SHA-512 signature. Not used for Google In-App Payments (06/2011).
        /// </summary>
        /// <param name="jwtHeaderAndClaim">Concatenated Base64url Header Input and Payload Input, delimited by period (.)</param>
        /// <param name="sellerSecret">Your Seller Secret</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private static string jwtHMAC512(string jwtHeaderAndClaim, string sellerSecret)
        {
            if (!string.IsNullOrEmpty(jwtHeaderAndClaim) && !string.IsNullOrEmpty(sellerSecret))
            {
                UTF8Encoding _utf8 = new UTF8Encoding(true, true);
                byte[] _key = _utf8.GetBytes(sellerSecret);
                byte[] _btxt = _utf8.GetBytes(jwtHeaderAndClaim);
                string _str = String.Empty;
                using (HMACSHA512 hmac = new HMACSHA512(_key))
                {
                    _str = System.Convert.ToBase64String(hmac.ComputeHash(_btxt)).Replace("=", String.Empty).Replace("+", "-").Replace("/", "_");
                    hmac.Clear();
                }
                Debug.WriteLine(string.Format("From hash: {0}", _str));
                return _str;
            }
            else
            {
                throw new ArgumentNullException(string.IsNullOrEmpty(jwtHeaderAndClaim) ? "jwtHeaderAndClaim" : "sellerSecret", "Argument cannot be null/empty");
            }

        }

        #endregion

    }

}