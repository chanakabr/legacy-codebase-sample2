using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace Proxy
{
    /// <summary>
    /// Summary description for proxy
    /// </summary>
    public class proxy : IHttpHandler
    {
        private static string url = "https://stg.eu.tvinci.com/tvpapi_v3_4/gateways/jsonpostgw.aspx";
        public void ProcessRequest(HttpContext Context)
        {
            /* Create variables to hold the request and response. */
            HttpRequest Request = Context.Request;
            HttpResponse Response = Context.Response;
            string URI = null;

            /* Attempt to get the URI the proxy is to pass along or fail. */
            try
            {
                if (!url.StartsWith("https://"))
                    throw new Exception("URL is not HTTPS - aborting");

                URI = string.Format("{0}?m={1}", url, HttpContext.Current.Request["m"]);
            }
            catch (Exception Passless)
            {
                Response.StatusCode = 500;
                Response.StatusDescription = "Parameter Missing";
                Response.Write("The parameter that makes this proxy worthwhile and functioning was not given.");
                Response.End();
                return;
            }

            ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate,
             X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
                return true;
            };

            /* Create an HttpWebRequest to send the URI on and process results. */
            System.Net.HttpWebRequest ProxyRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(URI);

            /* Set the appropriate values to the request methods. */
            ProxyRequest.Method = Request.HttpMethod;
            ProxyRequest.ServicePoint.Expect100Continue = false;
            ProxyRequest.Referer = Request.Headers["referer"];

            /* Set the body of ProxyRequest for POST requests to the proxy. */
            if (Request.InputStream.Length > 0)
            {
                /* 
                 * Since we are using the same request method as the original request, and that is 
                 * a POST, the values to send on in the new request must be grabbed from the 
                 * original POSTed request.
                 */
                byte[] Bytes = new byte[Request.InputStream.Length];

                Request.InputStream.Read(Bytes, 0, (int)Request.InputStream.Length);

                ProxyRequest.ContentLength = Bytes.Length;

                string ContentType = Request.ContentType;

                if (String.IsNullOrEmpty(ContentType))
                {
                    ProxyRequest.ContentType = "application/x-www-form-urlencoded";
                }
                else
                {
                    ProxyRequest.ContentType = ContentType;
                }

                using (Stream OutputStream = ProxyRequest.GetRequestStream())
                {
                    OutputStream.Write(Bytes, 0, Bytes.Length);
                }
            }
            else
            {
                /*
                 * When the original request is a GET, things are much easier, as we need only to 
                 * pass the URI we collected earlier which will still have any parameters 
                 * associated with the request attached to it.
                 */
                ProxyRequest.Method = "GET";
            }

            System.Net.WebResponse ServerResponse = null;

            /* Send the proxy request to the remote server or fail. */
            try
            {
                ServerResponse = ProxyRequest.GetResponse();
            }
            catch (System.Net.WebException WebEx)
            {
                Response.StatusCode = (int) ((HttpWebResponse)WebEx.Response).StatusCode;
                Response.StatusDescription = ((HttpWebResponse)WebEx.Response).StatusCode.ToString();
                Response.End();
                return;
            }

            /* Set up the response to the client if there is one to set up. */
            if (ServerResponse != null)
            {
                Response.ContentType = ServerResponse.ContentType;
                using (Stream ByteStream = ServerResponse.GetResponseStream())
                {
                    /* What is the response type? */
                    if (ServerResponse.ContentType.Contains("text") ||
                            ServerResponse.ContentType.Contains("json") ||
                            ServerResponse.ContentType.Contains("xml"))
                    {
                        /* These "text" types are easy to handle. */
                        using (StreamReader Reader = new StreamReader(ByteStream))
                        {
                            string ResponseString = Reader.ReadToEnd();

                            /* 
                             * Tell the client not to cache the response since it 
                             * could easily be dynamic, and we do not want to mess
                             * that up!
                             */
                            Response.CacheControl = "no-cache";
                            Response.Write(ResponseString);
                        }
                    }
                    else
                    {
                        /* 
                         * Handle binary responses (image, layer file, other binary 
                         * files) differently than text.
                         */
                        BinaryReader BinReader = new BinaryReader(ByteStream);

                        byte[] BinaryOutputs = BinReader.ReadBytes((int)ServerResponse.ContentLength);

                        BinReader.Close();

                        /* 
                         * Tell the client not to cache the response since it could 
                         * easily be dynamic, and we do not want to mess that up!
                         */
                        Response.CacheControl = "no-cache";
                        /*
                         * Send the binary response to the client.
                         * (Note: if large images/files are sent, we could modify this to 
                         * send back in chunks instead...something to think about for 
                         * future.)
                         */
                        Response.OutputStream.Write(BinaryOutputs, 0, BinaryOutputs.Length);
                    }
                    ServerResponse.Close();
                }

                if (ServerResponse.Headers != null)
                {
                    foreach (var key in ServerResponse.Headers.AllKeys)
                    {
                        Response.Headers.Add(key, ServerResponse.Headers[key]);
                    }
                }
            }
            Response.End();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}