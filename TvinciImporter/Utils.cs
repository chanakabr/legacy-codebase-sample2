using KLogMonitor;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.ServiceModel;

namespace TvinciImporter
{
    public class Utils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static byte[] Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                return sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            }
        }

        public static string HttpPost(string uri, string parameters, string contentType = null)
        {
            try
            {
                WebRequest request = WebRequest.Create(uri);

                request.Method = "POST";
                string postData = parameters;
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                if (contentType == null)
                    request.ContentType = "application/x-www-form-urlencoded";
                else
                    request.ContentType = contentType;

                request.ContentLength = byteArray.Length;

                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                WebResponse response = request.GetResponse();
                dataStream = response.GetResponseStream();

                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

                reader.Close();
                dataStream.Close();
                response.Close();

                return responseFromServer;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error on post request. URL: {0}, Parameter: {1}. Error: {2}", uri, parameters, ex);
            }
            return null;
        }

    }
}

namespace TvinciImporter.WS_ConditionalAccess
{
    // adding request ID to header
    public partial class module
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(uri);
            KlogMonitorHelper.MonitorLogsHelper.AddHeaderToWebService(request);
            return request;
        }
    }
}
