using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Services;
using System.Web;
using System.Web.Services.Protocols;
using System.Threading;
using System.ServiceModel.Web;

using System.IO;
using KLogMonitor;
using System.Reflection;
using System.ServiceModel;

namespace ServiceExtensions
{

    public class WSExtension : SoapExtension
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public override object GetInitializer(LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute)
        {
            return methodInfo.Name;
        }

        public override void Initialize(object initializer)
        {
            if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
            {
                Thread.CurrentThread.Name = Guid.NewGuid().ToString();
            }
        }

        public override object GetInitializer(Type WebServiceType)
        {
            return WebServiceType.Name;
        }

        public override void ProcessMessage(SoapMessage message)
        {
            try
            {
                switch (message.Stage)
                {
                    case SoapMessageStage.BeforeSerialize:
                        break;
                    case SoapMessageStage.AfterSerialize:
                        break;
                    case SoapMessageStage.BeforeDeserialize:
                        break;
                    case SoapMessageStage.AfterDeserialize:
                        break;
                    default:
                        throw new Exception("invalid stage");
                }
            }
            catch (Exception ex)
            {
                log.Error("SOapExtension - Soap Exception :" + ex.Message, ex);
            }
        }
    }
}

