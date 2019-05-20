using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace M1BL
{
    internal static class Http
    {
        
        internal static string SendXMLHttpReq(string sUrl, string sToSend, string contentType = "text/xml; charset=utf-8", string sUsername = "", string sPassword = "", string sMethod = "post")
        {

            //Create the HTTP POST request and the authentication headers
            HttpWebRequest oWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(sUrl));
            oWebRequest.Method = (string.IsNullOrEmpty(sMethod) ? "post" : sMethod);
            oWebRequest.ContentType = (string.IsNullOrEmpty(contentType) ? "text/xml; charset=utf-8" : contentType);

            if (!string.IsNullOrEmpty(sUsername) && !string.IsNullOrEmpty(sPassword))
            {
                oWebRequest.Credentials = new NetworkCredential(sUsername, sPassword);
            }


            //foreach (string header in postHeaders.Keys)
            //{
            //    oWebRequest.Headers[header] = postHeaders[header];
            //}

            //if (!string.IsNullOrEmpty(sUsernameField) || !string.IsNullOrEmpty(sPasswordField) || !string.IsNullOrEmpty(sUsername) || !string.IsNullOrEmpty(sPassword))
            //{
            //    oWebRequest.Headers[sUsernameField] = sUsername;
            //    oWebRequest.Headers[sPasswordField] = sPassword;
            //}

            byte[] encodedBytes = Encoding.UTF8.GetBytes(sToSend);

            //Send the request
            if (string.Compare(oWebRequest.Method, "post", true) == 0)
            {
                using (Stream requestStream = oWebRequest.GetRequestStream())
                {
                    requestStream.Write(encodedBytes, 0, encodedBytes.Length);
                    requestStream.Close();
                }
            }

            try
            {
                HttpWebResponse oWebResponse = (HttpWebResponse)oWebRequest.GetResponse();
                HttpStatusCode sCode = oWebResponse.StatusCode;
                Stream receiveStream = oWebResponse.GetResponseStream();

                using (StreamReader sr = new StreamReader(receiveStream))
                {
                    string resultString = HttpUtility.HtmlDecode(sr.ReadToEnd());

                    sr.Close();

                    oWebRequest = null;
                    oWebResponse = null;

                    return resultString;
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex);
                WebResponse errRsp = ex.Response;

                if (errRsp == null)
                {
                    return string.Empty;
                }

                using (StreamReader rdr = new StreamReader(errRsp.GetResponseStream()))
                {
                    return HttpUtility.HtmlDecode(rdr.ReadToEnd());
                }
            }
        }

    }
}
