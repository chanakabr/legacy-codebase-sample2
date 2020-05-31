using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Web;
using KLogMonitor;
using System.Reflection;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace ServiceExtensions
{
    public class ClientMessageInspector : IClientMessageInspector
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private ServiceEndpoint _Endpoint;
        public ClientMessageInspector(ServiceEndpoint endpoint)
        {
            _Endpoint = endpoint;
        }
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            // Implement this method to inspect/modify messages after a message
            // is received but prior to passing it back to the client 
            //Console.WriteLine("AfterReceiveReply called");
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            // Implement this method to inspect/modify messages before they 
            // are sent to the service

            // add request ID to message header
            if (request.Headers != null &&
                request.Headers.FindHeader(KLogMonitor.Constants.REQUEST_ID_KEY.ToString(), string.Empty) == -1)
            {
                if (KLogMonitor.KLogger.AppType == KLogEnums.AppType.WCF)
                {
                    if (OperationContext.Current != null)
                    {
                        object res = null;
                        if (OperationContext.Current.IncomingMessageProperties.TryGetValue(KLogMonitor.Constants.REQUEST_ID_KEY, out res))
                            request.Headers.Add(MessageHeader.CreateHeader(KLogMonitor.Constants.REQUEST_ID_KEY, string.Empty, res.ToString()));

                        if (OperationContext.Current.IncomingMessageProperties.TryGetValue(KLogMonitor.Constants.KS, out res))
                            request.Headers.Add(MessageHeader.CreateHeader(KLogMonitor.Constants.KS, string.Empty, res.ToString()));
                    }
                }
                else
                {
                    if (KLogger.LogContextData != null)
                    {
                        var requestId = KLogger.LogContextData[Constants.REQUEST_ID_KEY]?.ToString();
                        var ks = KLogger.LogContextData[Constants.KS]?.ToString();

                        // backward compatibility for phoenix on windows
                        if (string.IsNullOrEmpty(ks) && HttpContext.Current != null && HttpContext.Current.Items[KLogMonitor.Constants.KS] != null)
                        {
                            ks = HttpContext.Current.Items[KLogMonitor.Constants.KS].ToString();
                        }

                        if (!string.IsNullOrEmpty(requestId))
                            request.Headers.Add(MessageHeader.CreateHeader(KLogMonitor.Constants.REQUEST_ID_KEY, string.Empty, requestId));
                        else
                            log.Warn($"could not find request Id to send to WCF service: [{_Endpoint.Address.Uri}]");

                        if (!string.IsNullOrEmpty(ks))
                            request.Headers.Add(MessageHeader.CreateHeader(KLogMonitor.Constants.KS, string.Empty, ks));
                        else
                            log.Warn($"could not find Ks to send to WCF service: [{_Endpoint.Address.Uri}]");
                    }
                    else{
                        log.Warn($"Could not find LogContext, no requestId or ks sent to WC service [{_Endpoint.Address.Uri}]");
                    }
                }
            }

            return null;
        }
    }
}
