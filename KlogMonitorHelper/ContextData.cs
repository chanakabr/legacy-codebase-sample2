using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Web;
using KLogMonitor;

namespace KlogMonitorHelper
{
    public class ContextData
    {
        private OperationContext wcfContext = null;
        private HttpContext wsContext = null;

        public ContextData()
        {
            switch (KMonitor.AppType)
            {
                case KLogEnums.AppType.WCF:

                    if (OperationContext.Current != null)
                        this.wcfContext = OperationContext.Current;
                    break;

                case KLogEnums.AppType.WS:
                default:

                    if (HttpContext.Current != null)
                        this.wsContext = HttpContext.Current;
                    break;
            }
        }

        public void Load()
        {
            switch (KMonitor.AppType)
            {
                case KLogEnums.AppType.WCF:

                    if (this.wcfContext != null)
                        OperationContext.Current = this.wcfContext;
                    break;

                case KLogEnums.AppType.WS:
                default:

                    if (this.wsContext != null)
                        HttpContext.Current = this.wsContext;
                    break;
            }
        }
    }
}
