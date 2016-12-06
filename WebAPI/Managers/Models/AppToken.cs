using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Managers.Models
{
    public class AppToken
    {
        /// <summary>
        /// The id of the application token
        /// </summary>
        [DataMember(Name = "app_token_id")]
        [JsonProperty("app_token_id")]
        [XmlElement(ElementName = "app_token_id")]
        public string AppTokenId { get; set; }

        /// <summary>
        /// Expiry time of current token (unix timestamp in seconds)
        /// </summary>
        [DataMember(Name = "expiry")]
        [JsonProperty("expiry")]
        [XmlElement(ElementName = "expiry")]
        public int Expiry { get; set; }

        /// <summary>
        /// Partner identifier
        /// </summary>
        [DataMember(Name = "partnerId")]
        [JsonProperty("partnerId")]
        [XmlElement(ElementName = "partnerId")]
        public int PartnerId { get; set; }

        /// <summary>
        /// Expiry duration of KS (Kaltura Session) that created using the current token (in seconds)
        /// </summary>
        [DataMember(Name = "sessionDuration")]
        [JsonProperty("sessionDuration")]
        [XmlElement(ElementName = "sessionDuration")]
        public int SessionDuration { get; set; }

        /// <summary>
        /// The hash type of the token
        /// </summary>
        [DataMember(Name = "hashType")]
        [JsonProperty("hashType")]
        [XmlElement(ElementName = "hashType")]
        public KalturaAppTokenHashType HashType { get; set; }

        /// <summary>
        /// Comma separated privileges to be applied on KS (Kaltura Session) that created using the current token
        /// </summary>
        [DataMember(Name = "sessionPrivileges")]
        [JsonProperty("sessionPrivileges")]
        [XmlElement(ElementName = "sessionPrivileges")]
        public string SessionPrivileges { get; set; }

        /// <summary>
        /// Type of KS (Kaltura Session) that created using the current token
        /// </summary>
        [DataMember(Name = "sessionType")]
        [JsonProperty("sessionType")]
        [XmlElement(ElementName = "sessionType")]
        public KalturaSessionType SessionType { get; set; }

        /// <summary>
        /// Application token status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        public KalturaAppTokenStatus Status { get; set; }

        /// <summary>
        /// The application token
        /// </summary>
        [DataMember(Name = "token")]
        [JsonProperty("token")]
        [XmlElement(ElementName = "token")]
        public string Token { get; set; }

        /// <summary>
        /// User id of KS (Kaltura Session) that created using the current token
        /// </summary>
        [DataMember(Name = "sessionUserId")]
        [JsonProperty("sessionUserId")]
        [XmlElement(ElementName = "sessionUserId")]
        public string SessionUserId { get; set; }

        public AppToken()
        {
        }

        public AppToken(KalturaAppToken kalturaAppToken)
        {
            AppTokenId = kalturaAppToken.Id;
            Expiry = kalturaAppToken.getExpiry();
            PartnerId = kalturaAppToken.getPartnerId();
            SessionDuration = kalturaAppToken.getSessionDuration();
            HashType = kalturaAppToken.HashType;
            SessionType = kalturaAppToken.SessionType;
            Status = kalturaAppToken.Status;
            Token = kalturaAppToken.Token;
            SessionUserId = kalturaAppToken.SessionUserId;
            SessionPrivileges = kalturaAppToken.SessionPrivileges;
        }

        /// <summary>
        /// calculates the hash of the ks + token string based on the token hash type
        /// </summary>
        /// <returns></returns>
        internal string CalcHash()
        {
            string response = null;

            string stringToHash = KS.GetFromRequest().ToString() + Token;

            switch (HashType)
            {
                case KalturaAppTokenHashType.SHA1:
                    {
                        byte[] hashed = Utils.EncryptionUtils.HashSHA1(stringToHash);
                        if (hashed != null && hashed.Length > 0)
                        {
                            response = hashed.Aggregate(string.Empty, (x, y) => x + y.ToString("X2").ToLower());
                        }
                    }
                    break;
                case KalturaAppTokenHashType.SHA256:
                    response = Utils.EncryptionUtils.HashSHA256(stringToHash);
                    break;
                case KalturaAppTokenHashType.SHA512:
                    response = Utils.EncryptionUtils.HashSHA512(stringToHash);
                    break;
                case KalturaAppTokenHashType.MD5:
                    response = Utils.EncryptionUtils.HashMD5(stringToHash);
                    break;
                default:
                    break;
            }

            return response;
        }
    }
}