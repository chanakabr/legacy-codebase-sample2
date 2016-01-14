using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using KLogMonitor;
using System.Reflection;

namespace Mailer
{
    public class Utils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        static public string SendXMLHttpReq(string sUrl, string sToSend, string sSoapHeader)
        {
            //Create the HTTP POST request and the authentication headers
            HttpWebRequest oWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(sUrl));
            oWebRequest.Method = "post";
            oWebRequest.ContentType = "text/xml; charset=utf-8";
            //oWebRequest.Headers["SOAPAction"] = sSoapHeader;

            byte[] encodedBytes = Encoding.UTF8.GetBytes(sToSend);
            //oWebRequest.ContentLength = encodedBytes.Length;
            //oWebRequest.AllowWriteStreamBuffering = true;

            //Send the request.
            Stream requestStream = oWebRequest.GetRequestStream();
            requestStream.Write(encodedBytes, 0, encodedBytes.Length);
            requestStream.Close();

            try
            {
                HttpWebResponse oWebResponse = (HttpWebResponse)oWebRequest.GetResponse();
                HttpStatusCode sCode = oWebResponse.StatusCode;
                Stream receiveStream = oWebResponse.GetResponseStream();

                StreamReader sr = new StreamReader(receiveStream);
                string resultString = sr.ReadToEnd();

                sr.Close();
                oWebRequest = null;
                oWebResponse = null;
                return resultString;
            }
            catch (WebException ex)
            {
                log.Error(string.Empty, ex);
                WebResponse errRsp = ex.Response;

                if (errRsp == null)
                {
                    return string.Empty;
                }

                using (StreamReader rdr = new StreamReader(errRsp.GetResponseStream()))
                {
                    return rdr.ReadToEnd();
                }

            }
        }

        public static string GetTcmConfigValue(string sKey)
        {
            string result = string.Empty;
            try
            {
                result = TCMClient.Settings.Instance.GetValue<string>(sKey);
            }
            catch (Exception ex)
            {
                result = string.Empty;
                log.Error("Mailer - Key=" + sKey + "," + ex.Message, ex);
            }
            return result;
        }

    }
}
