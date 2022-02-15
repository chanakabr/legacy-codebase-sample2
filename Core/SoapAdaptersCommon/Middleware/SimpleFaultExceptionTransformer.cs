using Phx.Lib.Log;
using SoapCore.Extensibility;
using System;
using System.Reflection;
using System.ServiceModel.Channels;

namespace SoapAdaptersCommon.Middleware
{
    public class SimpleFaultExceptionTransformer : IFaultExceptionTransformer
    {
        private readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public Message ProvideFault(Exception exception, MessageVersion messageVersion)
        {
            _Logger.Error("Error while executing SOAP service", exception);
            return Message.CreateMessage(messageVersion, "", exception);
        }
    }
}