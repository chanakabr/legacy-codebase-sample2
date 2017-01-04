using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using WebAPI.Models.General;
using KLogMonitor;
using KlogMonitorHelper;
using System.Reflection;
using WebAPI.Managers.Models;
using System.Security.Cryptography.X509Certificates;

namespace WebAPI.EventNotifications
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class HttpNotificationHandler : NotificationAction
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Override Method
        
        internal override void Handle(EventManager.KalturaEvent kalturaEvent, object phoenixObject)
        {
            this.SendRequest(phoenixObject);
            //WebRequest request = WebRequest.Create(this.Url);
            
            //int statusCode = -1;

            //request.Method = this.Method.ToString().ToUpper();
            //request.ContentType = "application/json";ֲ

            //string phoenixString = string.Empty;

            //byte[] byteArray = Encoding.UTF8.GetBytes(phoenixString);

            //Stream dataStream = request.GetRequestStream();
            //dataStream.Write(byteArray, 0, byteArray.Length);
            //dataStream.Close();

            //WebResponse response = request.GetResponse();
            //dataStream = response.GetResponseStream();

            //StreamReader reader = new StreamReader(dataStream);
            //string responseFromServer = reader.ReadToEnd();

            //reader.Close();
            //dataStream.Close();
            //response.Close();

            //WebClient webClient = new WebClient();

            //webClient.BaseAddress = this.Url;
            //webClient.Credentials = new NotificationCredentials(this);

            //if (this.CustomHeaders != null)
            //{
            //    foreach (var header in this.CustomHeaders)
            //    {
            //        webClient.Headers.Add(header.key, header.value);
            //    }
            //}

            //var stream = webClient.OpenRead(this.Url);

            //using (StreamReader sr = new StreamReader(stream))
            //{
            //    var page = sr.ReadToEnd();
            //}

            //client.

            //webClient.Dispose();
        }

        #endregion 

        #region Properties

        [JsonProperty("url")]
        public string Url
        {
            get;
            set;
        }

        [JsonProperty("method")]
        public eHttpMethod Method
        {
            get;
            set;
        }

        [JsonProperty("data")]
        public object Data
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

        #endregion

        #region Protected and private methods

        protected void SendRequest(object phoenixObject)
        {
            try
            {
                int statusCode = -1;

                System.Net.ServicePointManager.CertificatePolicy = new KalturaPolicy();

                switch (this.Method)
                {
                    case eHttpMethod.Get:
                    {
                        this.SendGetHttpReq();
                        break;
                    }
                    case eHttpMethod.Post:
                    {
                        this.SendPostHttpReq(phoenixObject);
                        break;
                    }
                    case eHttpMethod.Put:
                    break;
                    case eHttpMethod.Delete:
                    break;
                    default:
                    break;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error in HttpNotificationHandler Exception", ex);
            }
        }

        #region HTTP requests

        public void SendPostHttpReq(object phoenixObject)
        {
            int statusCode = -1;

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(this.Url);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(phoenixObject.ToString());
            webRequest.ContentLength = bytes.Length;

            // custom headers
            if (this.CustomHeaders != null)
            {
                foreach (var header in this.CustomHeaders)
                {
                    webRequest.Headers.Add(header.key, header.value);
                }
            }

            webRequest.Credentials = new NetworkCredential(this.Username, this.Password);

            using (System.IO.Stream os = webRequest.GetRequestStream())
            {
                os.Write(bytes, 0, bytes.Length);
            }

            if (this.Timeout > 0)
            {
                webRequest.Timeout = this.Timeout;
            }

            string res = string.Empty;

            try
            {
                string requestGuid = Guid.NewGuid().ToString();

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_ELASTIC, null, null, null, null)
                {
                    Database = this.Url,
                    Table = requestGuid
                })
                {

                    HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                    HttpStatusCode httpStatusCode = webResponse.StatusCode;
                    statusCode = GetResponseCode(httpStatusCode);
                    StreamReader sr = null;
                    try
                    {
                        sr = new StreamReader(webResponse.GetResponseStream());
                        res = sr.ReadToEnd();
                    }
                    finally
                    {
                        if (sr != null)
                            sr.Close();
                    }
                }
            }
            catch (WebException ex)
            {
                log.Error("Error in SendPostHttpReq WebException", ex);
                StreamReader errorStream = null;
                try
                {
                    errorStream = new StreamReader(ex.Response.GetResponseStream());
                    res = errorStream.ReadToEnd();
                }
                finally
                {
                    if (errorStream != null)
                        errorStream.Close();
                }
            }
            catch (Exception ex)
            {
                log.Error("Error in SendPostHttpReq Exception", ex);
            }
        }

        public void SendGetHttpReq()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(this.Url);
            HttpWebResponse webResponse = null;
            Stream receiveStream = null;
            Int32 statusCode = -1;
            Encoding encoding = new UTF8Encoding(false);

            try
            {
                webRequest.Credentials = new NetworkCredential(this.Username, this.Password);

                if (this.Timeout > 0)
                {
                    webRequest.Timeout = this.Timeout;
                }

                webResponse = (HttpWebResponse)webRequest.GetResponse();
                HttpStatusCode sCode = webResponse.StatusCode;
                statusCode = GetResponseCode(sCode);
                receiveStream = webResponse.GetResponseStream();

                StreamReader sr = new StreamReader(receiveStream, encoding);
                string resultString = sr.ReadToEnd();

                sr.Close();

                webResponse.Close();
                webRequest = null;
                webResponse = null;
            }
            catch (Exception ex)
            {
                log.Debug("Error in SendGettHttpReq WebException:" + ex.Message + " to: " + this.Url);

                if (webResponse != null)
                {
                    webResponse.Close();
                }

                if (receiveStream != null)
                {
                    receiveStream.Close();
                }
            }
        }

        //public string SendDeleteHttpReq(string sUrl, ref Int32 nStatus, string sUserName, string sPassword, string sParams, bool isFirstTry)
        //{
        //    Int32 nStatusCode = -1;

        //    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(sUrl);
        //    webRequest.ContentType = "application/x-www-form-urlencoded";
        //    webRequest.Method = "DELETE";
        //    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(sParams);
        //    webRequest.ContentLength = bytes.Length;
        //    System.IO.Stream os = webRequest.GetRequestStream();
        //    os.Write(bytes, 0, bytes.Length);
        //    os.Close();

        //    string res = string.Empty;
        //    try
        //    {
        //        HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
        //        HttpStatusCode sCode = webResponse.StatusCode;
        //        nStatusCode = GetResponseCode(sCode);
        //        StreamReader sr = null;
        //        try
        //        {
        //            sr = new StreamReader(webResponse.GetResponseStream());
        //            res = sr.ReadToEnd();
        //        }
        //        finally
        //        {
        //            if (sr != null)
        //                sr.Close();
        //        }

        //    }
        //    catch (WebException ex)
        //    {
        //        StreamReader errorStream = null;
        //        try
        //        {
        //            errorStream = new StreamReader(ex.Response.GetResponseStream());
        //            res = errorStream.ReadToEnd();
        //        }
        //        finally
        //        {
        //            if (errorStream != null)
        //                errorStream.Close();
        //        }
        //    }

        //    //retry alternative URL if this is the original (=first) call, the result was not OK and there is an alternative URL
        //    if (isFirstTry && nStatusCode != 200 && !string.IsNullOrEmpty(ALT_ES_URL))
        //    {
        //        string sAlternativeURL = sUrl.Replace(ES_URL, ALT_ES_URL);
        //        res = SendDeleteHttpReq(sAlternativeURL, ref nStatus, sUserName, sPassword, sParams, false);
        //    }

        //    nStatus = nStatusCode;
        //    return res;
        //}

        public Int32 GetResponseCode(HttpStatusCode theCode)
        {
            if (theCode == HttpStatusCode.OK || theCode == HttpStatusCode.Created || theCode == HttpStatusCode.Accepted)
                return (int)HttpStatusCode.OK;
            if (theCode == HttpStatusCode.NotFound)
                return (int)HttpStatusCode.NotFound;
            return (int)HttpStatusCode.InternalServerError;

        }

        #endregion
        #endregion
    }

    public class NotificationCredentials : ICredentials
    {
        private HttpNotificationHandler handler;

        public NotificationCredentials(HttpNotificationHandler handler)
        {
            this.handler = handler;
        }

        public NetworkCredential GetCredential(Uri uri, string authType)
        {
            NetworkCredential credentials = new NetworkCredential(handler.Username, handler.Password);
            
            return credentials;
        }
    }

    public class KalturaPolicy : ICertificatePolicy
    {
        #region ICertificatePolicy Members

        public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem)
        {
            return true;
        }

        #endregion
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