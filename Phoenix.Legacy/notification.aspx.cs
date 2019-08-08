using System;
using System.IO;
using System.Web;
using TVinciShared;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using KLogMonitor;
using System.Reflection;

namespace WS_API
{
    public partial class notification : System.Web.UI.Page
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected void Page_Load(object sender, EventArgs e)
        {
            log.DebugFormat("Got new notification");

            Response.Clear();

            var queryStatusCode = HttpContext.Current.Request.QueryString["status"];

            if (queryStatusCode != null && int.TryParse(queryStatusCode, out int statusCode))
            {
                Response.StatusCode = statusCode;
            }

            string requestBody = RequestBody();

            Task.Run(() =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(requestBody))
                    {
                        JObject json = JObject.Parse(requestBody);

                        var idToken = json.SelectToken("object.id");

                        string nowString = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

                        string idString = nowString;

                        if (idToken != null)
                        {
                            idString = idToken.ToString();
                        }

                        var objectTypeJson = json["eventObjectType"];
                        var actionJson = json["eventType"];

                        string objectType = string.Empty;
                        string action = string.Empty;

                        if (objectTypeJson != null)
                        {
                            objectType = objectTypeJson.ToString();
                        }

                        if (actionJson != null)
                        {
                            action = actionJson.ToString();
                        }

                        string jsonRequestBody = json.ToString();

                        string esURL = WS_Utils.GetTcmConfigValue("ES_URL_V2");
                        if (string.IsNullOrEmpty(esURL))
                        {
                            esURL = WS_Utils.GetTcmConfigValue("ES_URL");
                        }

                        string documentId = string.Format("{0}_{1}_{2}", objectType, action, idString);

                        string postURL = string.Format("{0}/events/notifications/{1}", esURL, documentId);
                        string postResponse = string.Empty;
                        string postErrorMsg = string.Empty;
                        int httpStatus = 0;

                        ElasticSearch.Common.ElasticSearchApi api = new ElasticSearch.Common.ElasticSearchApi(esURL);
                        var postRequestResult = api.SendPostHttpReq(postURL, ref httpStatus, string.Empty, string.Empty, requestBody, true);
                        log.DebugFormat("Finished successfully");
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error in Page_Load Exception", ex);
                    Response.StatusCode = 400;
                }
            }
            );
        }

        public static string RequestBody()
        {
            var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
            bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
            var bodyText = bodyStream.ReadToEnd();
            return bodyText;
        }
    }
}