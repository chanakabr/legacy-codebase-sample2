using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft;
using Newtonsoft.Json;

namespace TVPPro.SiteManager.Helper
{
    public class WebRequestHelper
    {
        public static T SendRequest<T>(string url, string postData,int timeOut=-1)
        {
            Stream dataStream = null;
            StreamReader reader = null;
            WebResponse response = null;
            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                request.Timeout = (timeOut > 0) ? timeOut : request.Timeout;

                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                request.ContentLength = byteArray.Length;

                dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                response = request.GetResponse();
                dataStream = response.GetResponseStream();

                reader = new StreamReader(dataStream);

                if (typeof(T) == typeof(String))
                {
                    string resposne = reader.ReadToEnd().Trim(new char[] { '\"' });
                    return (T)Convert.ChangeType(resposne, typeof(T));
                }

                return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());

            }
            finally
            {
                // Clean up the streams.
                if (reader != null) reader.Close();
                if (dataStream != null) dataStream.Close();
                if (response != null) response.Close();
            }
        }
    }
}
