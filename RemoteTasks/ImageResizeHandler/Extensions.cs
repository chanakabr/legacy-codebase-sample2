using ConfigurationManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ImageResizeHandler
{
    public static class Extensions
    {
        public static byte[] ToByteArray(this Stream stream)
        {
            using (stream)
            {
                using (MemoryStream memStream = new MemoryStream())
                {
                    stream.CopyTo(memStream);
                    return memStream.ToArray();
                }
            }
        }

        public static byte[] ToByteArray(this Uri uri)
        {
            HttpWebResponse httpWebResponse = null;

            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(uri);

                string proxyAddress = ApplicationConfiguration.ImageResizerConfiguration.ProxyAddress.Value;

                if (!string.IsNullOrEmpty(proxyAddress))
                {
                    string username = ApplicationConfiguration.ImageResizerConfiguration.ProxyUsername.Value;
                    string password = ApplicationConfiguration.ImageResizerConfiguration.ProxyPassword.Value;

                    WebProxy webProxy = new WebProxy();

                    webProxy.Address = new Uri(proxyAddress);
                    webProxy.Credentials = new NetworkCredential(username, password);

                    httpWebRequest.Proxy = webProxy;
                }

                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (Stream stream = httpWebResponse.GetResponseStream())
                {
                    return stream.ToByteArray();
                }
            }
            catch (Exception ex)
            {
                //Write Log

                throw ex;
            }
            finally
            {
                if (httpWebResponse != null)
                    httpWebResponse.Close();
            }
        }
    }
}
