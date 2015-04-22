using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.Clients.Utils
{
    public class BaseClient
    {
        #region Private

        private Object lockObject = new Object();

        #endregion

        #region Properties

        public object Module { get; set; }
        public string ServiceKey { get; set; }

        public string WSUserName { get; set; }

        public string WSPassword { get; set; }
        
        public int TimeoutRetry { get; set; }
        public RestfulTVPApi.Objects.Enums.Client ClientType { get; set; }

        #endregion

        #region Service Failover

        public object Execute(Func<object> funcToExecute)
        {
            object result = null;
            BaseLog executionLog = new BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
        Operate:
            {
                executionLog.Method = funcToExecute.Method.Name;
                executionLog.UserAgent = HttpContext.Current.Request.UserAgent;
                executionLog.IP = HttpContext.Current.Request.UserHostAddress;
                try
                {
                    result = funcToExecute();
                    TimeoutRetry = 0;
                    executionLog.Info(string.Format("Function {0} execution succeeded", executionLog.Method), true);
                }
                catch (TimeoutException timeout) // Catches services operations exceptions
                {
                    executionLog.Error(string.Format("Service failed to operate {0} due to error: {1}", executionLog.Method, timeout.Message), true);
                    TimeoutRetry++;
                    if (TimeoutRetry <ClientsManager.Instance.MaxRetries)
                    {
                        BaseClient restartedService = null;
                        lock (this.lockObject)
                        {
                            restartedService = ClientsManager.Instance.RestartClient(this);
                        }

                        if (restartedService != null)
                        {
                            goto Operate;
                        }
                        else
                        {
                            goto End;
                        }
                    }
                    else
                    {
                        // TODO: Write Log that we reached the limit of failures
                        executionLog.Error(string.Format("Service reached the limit of attemps. Service reset is invoked"), true);

                        // Reset Service
                        lock (this.lockObject)
                        {
                            ClientsManager.Instance.ResetClient(this);
                        }
                    }
                }
                catch (Exception ex) // Catches logic exceptions from backend
                {
                    executionLog.Error(string.Format("Error occured in {0} call due to the following error: {1}", executionLog.Method, ex.Message), true);
                }
            }

        End:
            return result;
        }

        #endregion
    }
}