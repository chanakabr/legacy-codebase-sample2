using KLogMonitor;
using Microsoft.AspNetCore.Http;
using SoapCore.Extensibility;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace SoapAdaptersCommon.Middleware
{
    /// <summary>
    /// This Message Inspector will be invoked by SoapCore middelware
    /// It will extract the action, request id and ks from the soap message came from phoenix
    /// To access these properties use IAdapterRequestContextAccessor class
    /// </summary>
    public class AdapterRequestContextExtractor : IMessageInspector
    {
        private readonly KLogger _Log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly IHttpContextAccessor _HttpContexAccessor;

        public AdapterRequestContextExtractor(IHttpContextAccessor _httpContextAccessor)
        {
            _HttpContexAccessor = _httpContextAccessor;
        }

        public object AfterReceiveRequest(ref Message message)
        {
            var action = message.Headers.Action;
            action = action ?? _HttpContexAccessor.HttpContext.Request.Headers["SOAPAction"];
            action = action.Substring(action.LastIndexOf('/') + 1).TrimEnd('"');
            action = action ?? "null";

            var reqid = message.Headers.GetHeader<string>(Constants.REQUEST_ID_KEY, string.Empty);
            reqid = reqid ?? _HttpContexAccessor.HttpContext.TraceIdentifier;

            var ks = message.Headers.GetHeader<string>(Constants.KS, string.Empty);

            _HttpContexAccessor.HttpContext.Items[Constants.REQUEST_ID_KEY] = reqid;
            _HttpContexAccessor.HttpContext.Items[Constants.KS] = ks;
            _HttpContexAccessor.HttpContext.Items[Constants.ACTION] = action;
            _HttpContexAccessor.HttpContext.Items[Constants.CLIENT_TAG] = Dns.GetHostName();

            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
        }

        private XmlDocument GetDocument(Message request)
        {
            var document = new XmlDocument();
            using (var memoryStream = new MemoryStream())
            {
                var writer = XmlWriter.Create(memoryStream);
                request.WriteMessage(writer);
                writer.Flush();
                memoryStream.Position = 0;

                // load memory stream into a document
                document.Load(memoryStream);
            }

            return document;
        }
    }
}