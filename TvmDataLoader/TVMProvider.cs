using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using Tvinci.Data.TVMDataLoader.Protocols;
using System.Xml;
using Tvinci.Performance;
using ICSharpCode.SharpZipLib.Zip;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Tvinci.Data.TVMDataLoader.Protocols.TVMMenu;
using KLogMonitor;
using System.Reflection;


namespace Tvinci.Data.TVMDataLoader
{
    public class TVMProvider : LoaderProvider<ITVMAdapter>
    {
        #region Constructor
        static TVMProvider()
        {
			GetTVMUrlMethod = delegate(bool IsWriteProtocol) { return @"http://platform-us.tvinci.com/api.aspx"; };
        }
        #endregion

        #region Fields
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static Encoding responseEncoding = Encoding.UTF8;

        public static GetTVMUrlDelegate GetTVMUrlMethod { get; set; }

        public static GetProxyCredentials GetProxyCredentialsMethod { get; set; }

        public string TVMAltURL { get; set; }

        Match requestMatch = null;
        #endregion

        #region Delegates
		public delegate string GetTVMUrlDelegate(bool IsWriteProtocol);

        public delegate NetworkCredential GetProxyCredentials(); 
        #endregion

        #region Public Methods
        public override object GetDataFromSource(ITVMAdapter adapter)
        {
            object result;
            DateTime allTimer = DateTime.Now;

            IProtocol request = adapter.CreateProtocol();

            if (request == null)
            {
                return null;
            }

            Guid requestGuid = Guid.NewGuid();

            string serializedRequest = getSerializedRequest(request);
            string serializedResponse;
           // using (TvinciStopwatch timer = new TvinciStopwatch(ePerformanceSource.Site, string.Concat("TVM Request - ", requestGuid.ToString())))
            {
                serializedResponse = getResponse(serializedRequest, request.ProtocolUseZip, request.IsWriteProtocol, request.GetType().ToString());
            }

            serializedResponse = request.PreResponseProcess(serializedResponse);

           /* if (logger.IsDebugEnabled)
            {
                logger.Debug(string.Format("Request - {3}{0}{1}{0}{0}Response{0}{2} ", "\r\n", serializedRequest, serializedResponse, requestGuid.ToString()));
            }*/

            // de-serialize response instance
            XmlSerializer xs = new XmlSerializer(request.GetType());

            string response = serializedResponse;
            if (!(((Protocol)request)).IsTVMProProtocol())
            {
                response = string.Format("{0}{1}</{2}>", requestMatch.Groups["pre"].Value, serializedResponse, request.GetType().Name);
            }
            //response = Regex.Replace(response, "xmlns:xsi", "xmlns:tns=\"SharedElements\" xmlns:xsi");

            
            result = xs.Deserialize(new StringReader(response));

            //performanceLogger.InfoFormat("TVM TOTAL request time - {0}", DateTime.Now.Subtract(allTimer));

            return result;
        }
        #endregion

        #region Methods
		private string getResponse(string serializedRequest, bool isZip)
		{
			return getResponse(serializedRequest, isZip, false);
		}

        private string getRequestURI(bool isWriteProtocol)
        {
            return TVMAltURL ?? GetTVMUrlMethod(isWriteProtocol);
        }

        private string getResponse(string serializedRequest, bool isZip, bool isWriteProtocol, string sQueryStr)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            StreamReader readStream = null;
            string result = string.Empty;

            try
            {
                if (GetTVMUrlMethod == null)
                {
                    throw new NullReferenceException("Static member 'GetTVMUrlMethod' must be assigned");
                }
                string sUrl = getRequestURI(isWriteProtocol);
                if (!string.IsNullOrEmpty(sQueryStr))
                {
                    sUrl = string.Format(sUrl + "?t={0}", sQueryStr);
                }
                request = (HttpWebRequest)WebRequest.Create(sUrl);
                request.KeepAlive = false;
                request.Method = "POST";
                request.ContentType = "application/xml";

                if (GetProxyCredentialsMethod != null)
                {
                    try
                    {
                        NetworkCredential creds = GetProxyCredentialsMethod();
                        if (creds != null)
                        {
                            request.Proxy.Credentials = creds;
                        }
                    }
                    catch (Exception) { }
                }

                using (StreamWriter sw = new StreamWriter(request.GetRequestStream(), new UTF8Encoding(false)))
                {
                    sw.Write(serializedRequest);
                    sw.Close();
                }

                try
                {
                    using (response = (HttpWebResponse)request.GetResponse())
                    {

                        Stream receiveStream = response.GetResponseStream();

                        MemoryStream a = new MemoryStream();
                        byte[] temp = new byte[4096];
                        int readCount;
                        while ((readCount = receiveStream.Read(temp, 0, temp.Length)) > 0)
                        {
                            a.Write(temp, 0, readCount);
                        }

                        // Check if request uses zip
                        a.Position = 0;
                        bool succeedUnZip = false;
                        if (isZip && response.ContentType == "application/x-gzip-compressed")
                        {
                            string res;
                            if (TryUnZipRequest(a, out res))
                            {
                                succeedUnZip = true;
                                result = res;
                            }
                        }

                        if (!succeedUnZip)
                        {
                            a.Position = 0;
                            using (readStream = new StreamReader(a, responseEncoding))
                            {
                                result = readStream.ReadToEnd();
                                readStream.Close();
                            }
                        }

                        response.Close();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("", ex);
                }
                finally
                {

                }
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        private string getResponse(string serializedRequest, bool isZip, bool isWriteProtocol)
        {
            return getResponse(serializedRequest, isZip, isWriteProtocol, string.Empty);
            //HttpWebRequest request = null;
            //HttpWebResponse response = null;
            //StreamReader readStream = null;
            //string result = string.Empty;

            //try
            //{
            //    if (GetTVMUrlMethod == null)
            //    {
            //        throw new NullReferenceException("Static member 'GetTVMUrlMethod' must be assigned");
            //    }
            //    request = (HttpWebRequest)WebRequest.Create(getRequestURI(isWriteProtocol));
            //    request.KeepAlive = false;
            //    request.Method = "POST";
            //    request.ContentType = "application/xml";

            //    if (GetProxyCredentialsMethod != null)
            //    {
            //        try
            //        {
            //            NetworkCredential creds = GetProxyCredentialsMethod();
            //            if (creds != null)
            //            {
            //                request.Proxy.Credentials = creds;
            //            }
            //        }
            //        catch (Exception) { }
            //    }

            //    using (StreamWriter sw = new StreamWriter(request.GetRequestStream(), new UTF8Encoding(false)))
            //    {
            //        sw.Write(serializedRequest);
            //        sw.Close();
            //    }

            //    try
            //    {
            //        using (response = (HttpWebResponse)request.GetResponse())
            //        {

            //            Stream receiveStream = response.GetResponseStream();

            //            MemoryStream a = new MemoryStream();
            //            byte[] temp = new byte[4096];
            //            int readCount;
            //            while ((readCount = receiveStream.Read(temp, 0, temp.Length)) > 0)
            //            {
            //                a.Write(temp, 0, readCount);
            //            }

            //            // Check if request uses zip
            //            a.Position = 0;
            //            bool succeedUnZip = false;
            //            if (isZip && response.ContentType == "application/x-gzip-compressed")
            //            {
            //                string res;
            //                if (TryUnZipRequest(a, out res))
            //                {
            //                    succeedUnZip = true;
            //                    result = res;
            //                }
            //            }

            //            if (!succeedUnZip)
            //            {
            //                a.Position = 0;
            //                using (readStream = new StreamReader(a, responseEncoding))
            //                {
            //                    result = readStream.ReadToEnd();
            //                    readStream.Close();
            //                }
            //            }

            //            response.Close();
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.Error(ex);
            //    }
            //    finally
            //    {

            //    }
            //}
            //catch (Exception)
            //{
            //    throw;
            //}

            //return result;
        }

        private string getSerializedRequest(IProtocol request)
        {
            if (request == null)
            {
                throw new Exception("problem");
            }

            request.PreSerialize();

            XmlSerializer xs = new XmlSerializer(request.GetType());

            string result;

            using (StringWriter sw = new StringWriter())
            {
                xs.Serialize(sw, request);
                result = sw.ToString();
            }

            if (!(((Protocol)request).IsTVMProProtocol()))
            {
                result = handleRequest(result);
            }
            result = request.PostSerialize(result);

            return result;
        }

        private string handleRequest(string result)
        {
            requestMatch = Regex.Match(result, "(?<pre>.*?)(?<content><root>.*?</root>)(?<post>.*)", RegexOptions.Singleline);

            return requestMatch.Groups["content"].Value;
        }

        private bool TryUnZipRequest(Stream inputStream, out string result)
        {
            result = string.Empty;

            try
            {
                // Try unzip response
                ZipInputStream zStream = new ZipInputStream(inputStream);
                
                ZipEntry entry = zStream.GetNextEntry();

                if (entry == null)
                    return false;

                byte[] readBuffer = new byte[zStream.Length];

                zStream.Read(readBuffer, 0, readBuffer.Length);

                result = Encoding.UTF8.GetString(readBuffer);

				zStream.Close();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion
    }
}


