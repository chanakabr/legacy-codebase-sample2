using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApiModule.Context;
using TVPApiModule.Objects;

namespace TVPApiModule.Services
{
    public class BaseService
    {
        #region Private

        private Object lockObject = new Object();

        #endregion

        #region Properties

        public object m_Module { get; set; }
        public string serviceKey { get; set; }
        
        public string m_wsUserName
        {
            get
            {
                string valueReturned = string.Empty;
                if (System.Web.HttpContext.Current.Items.Contains("m_wsUserName"))
                {
                    valueReturned = System.Web.HttpContext.Current.Items["m_wsUserName"].ToString();
                }
                
                return valueReturned;
            }
        }

        public string m_wsPassword
        {
            get
            {
                string valueReturned = string.Empty;
                if (System.Web.HttpContext.Current.Items.Contains("m_wsPassword"))
                {
                    valueReturned = System.Web.HttpContext.Current.Items["m_wsPassword"].ToString();
                }

                return valueReturned;
            }
        }
    
        public int m_groupID { get; set; }
        public PlatformType m_platform { get; set; }
        public int m_FailOverCounter { get; set; }
        public eService m_ServiceType { get; set; }

        #endregion

        #region Service Failover

        public object Execute(Func<object> funcToExecute)
        {
            object result = null;

        Operate:
            {
                try
                {
                    result = funcToExecute();
                    m_FailOverCounter = 0;
                    // TODO: Write a log that indicates the function succeeded running - get the function name
                }
                catch (TimeoutException timeout) // Catches services operations exceptions
                {
                    m_FailOverCounter++;
                    if (m_FailOverCounter < ServicesManager.Instance.FailOverLimit)
                    {
                        BaseService restartedService = null;
                        lock (this.lockObject)
                        {
                            restartedService = ServicesManager.Instance.RestartService(this);
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
                        // Reset Service
                        lock (this.lockObject)
                        {
                            ServicesManager.Instance.ResetService(this);
                        }
                    }
                }
                catch (Exception ex) // Catches logic exceptions from backend
                {
                    // TODO: Write log exception message
                }
            }

        End:
            return result;
        }

        #endregion
    }
}
