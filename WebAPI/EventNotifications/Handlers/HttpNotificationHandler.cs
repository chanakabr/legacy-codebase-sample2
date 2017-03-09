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
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using WebAPI.Managers;

namespace WebAPI.EventNotifications
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class HttpNotificationHandler : NotificationAction
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Override Method

        internal override void Handle(EventManager.KalturaEvent kalturaEvent, KalturaNotification eventWrapper)
        {
            if (this.ValidHttpStatuses == null || this.ValidHttpStatuses.Count == 0)
            {
                this.ValidHttpStatuses = new List<int>() { 200, 201, 202, 203, 204, 205, 206, 207, 208, 226 };
            }

            this.SendRequest(
                new KalturaHttpNotification()
                {
                    eventObject = eventWrapper.eventObject,
                    eventObjectType = eventWrapper.eventObjectType,
                    eventType = eventWrapper.eventType,
                    objectType = eventWrapper.objectType
                }
                );
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

        [JsonProperty("valid_statuses")]
        public List<int> ValidHttpStatuses
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

        [JsonProperty("content_type")]
        public string ContentType
        {
            get;
            set;
        }

        #endregion

        #region Protected and private methods

        protected void SendRequest(KalturaHttpNotification phoenixObject)
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
                    this.SendHttpRequest(phoenixObject, "POST");
                    break;
                }
                case eHttpMethod.Put:
                {
                    this.SendHttpRequest(phoenixObject, "PUT");
                    break;
                }
                case eHttpMethod.Delete:
                {
                    this.SendHttpRequest(phoenixObject, "DELETE");
                    break;
                }
                default:
                break;
            }
        }

        #region HTTP requests

        public void SendHttpRequest(object phoenixObject, string method)
        {
            int statusCode = -1;

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(this.Url);
            webRequest.ContentType = "application/json";

            if (!string.IsNullOrEmpty(this.ContentType))
            {
                webRequest.ContentType = this.ContentType;
            }

            webRequest.Method = method;

            JsonManager jsonManager = JsonManager.GetInstance();
            string postBody = jsonManager.Serialize(phoenixObject);

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(postBody);
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
            webRequest.ClientCertificates.Add(new X509Certificate()); //add cert

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
            catch (HttpException ex)
            {
                log.Error("Error in SendPostHttpReq HttpException", ex);
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse)
                {
                    statusCode = (int)((HttpWebResponse)(ex.Response)).StatusCode;
                }

                //if (ex.Response
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

            if (!this.ValidHttpStatuses.Contains(statusCode))
            {
                throw new Exception("HTTP handler: received unwanted status code");
            }
        }

        public void Post(object phoenix)
        {
            try
            {
                // Initialize handler and client
                using (HttpClientHandler handler = new HttpClientHandler()
                {
                    Credentials = new NotificationCredentials(this)
                })
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    // Set basic parameters
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    httpClient.BaseAddress = new Uri(this.Url);
                    httpClient.DefaultRequestHeaders.Accept.Clear();

                    // serialize and code object
                    string postBody = JsonConvert.SerializeObject(phoenix, Newtonsoft.Json.Formatting.None);
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(postBody);
                    HttpContent content = new ByteArrayContent(bytes);

                    if (this.CustomHeaders != null)
                    {
                        // Set headers
                        foreach (var header in this.CustomHeaders)
                        {
                            content.Headers.Add(header.key, header.value);
                        }
                    }

                    // Send request and wait until it finishes
                    Task<HttpResponseMessage> task = httpClient.PostAsync(this.Url, content);
                    task.Wait();

                    // Result is here
                    var response = task.Result;

                    string responseString = response.ToString();

                    log.DebugFormat("Http POST request response is {0}", responseString);
                }
            }
            catch (HttpRequestException e)
            {
            }
            catch (Exception ex)
            {
            }
            finally
            {

            }
        }

        public void SendGetHttpReq()
        {
            HttpWebResponse webResponse = null;
            Stream receiveStream = null;

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(this.Url);
                Int32 statusCode = -1;
                Encoding encoding = new UTF8Encoding(false);

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