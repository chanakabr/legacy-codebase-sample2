using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using WebAPI.Models.General;

namespace WebAPI.EventNotifications
{
    [Serializable]
    public class HttpNotificationHandler : NotificationEventHandler
    {
        internal override void Handle(EventManager.KalturaEvent kalturaEvent, object phoenixObject)
        {
            
        }

        [JsonProperty("url")]
        public string Url
        {
            get;
            set;
        }

        [JsonProperty("method")]
        public eHttpMethod method
        {
            get;
            set;
        }

        [JsonProperty("data")]
        public object data
        {
            get;
            set;
        }

        [JsonProperty("timeout")]
        public int Timeout
        {
            get;
            set;
        }


        [JsonProperty("connection_timeout")]
        public int ConnectionTimeout
        {
            get;
            set;
        }

        [JsonProperty("username")]
        public string Username
        {
            get;
            set;
        }

        [JsonProperty("password")]
        public string Password
        {
            get;
            set;
        }

        [JsonProperty("authentication_method")]
        public eAuthenticationMethod AuthetnticationMethod
        {
            get;
            set;
        }

        [JsonProperty("ssl_version")]
        public eSslVersion SslVersion
        {
            get;
            set;
        }

        [JsonProperty("ssl_certificate")]
        public string SslCertificate
        {
            get;
            set;
        }

        [JsonProperty("ssl_certificate_type")]
        public eCertificateType SslCertificateType
        {
            get;
            set;
        }

        [JsonProperty("ssl_certificate_password")]
        public string SslCertificatePassword
        {
            get;
            set;
        }

        [JsonProperty("ssl_engine")]
        public string SslEngine
        {
            get;
            set;
        }

        [JsonProperty("ssl_engine_default")]
        public string SslEngineDefault
        {
            get;
            set;
        }

        [JsonProperty("ssl_key_type")]
        public eSslKeyType SslKeyTpe
        {
            get;
            set;
        }

        [JsonProperty("ssk_key")]
        public string SslKey
        {
            get;
            set;
        }

        [JsonProperty("ssk_password")]
        public string SslKeyPassword
        {
            get;
            set;
        }

        [JsonProperty("custom_headers")]
        public List<KalturaKeyValue> CustomHeaders
        {
            get;
            set;
        }
    }

    #region Enums

    public enum eHttpMethod
    {
        Get,
        Post,
        Put,
        Delete
    }

    public enum eAuthenticationMethod
    {
        Anysafe,
        Any,
        Basic,
        Digest,
        GSSNegotiate,
        NTLM
    }

    public enum eSslVersion
    {
        V2,
        V3
    }

    public enum eCertificateType
    {
        DER,
        ENG,
        PEM
    }

    public enum eSslKeyType
    {
        DER,
        ENG,
        PEM
    }

    #endregion
}