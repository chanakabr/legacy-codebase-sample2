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
        #region Consts

        //private static char SPLITTER = '.';

        #endregion

        #region Static Members

        //private static ConcurrentDictionary<string, BaseService> _services = new ConcurrentDictionary<string, BaseService>();
        //private static ConcurrentDictionary<int, ConcurrentDictionary<PlatformType, ConcurrentDictionary<eService, BaseService>>> _services = new ConcurrentDictionary<int, ConcurrentDictionary<PlatformType, ConcurrentDictionary<eService, BaseService>>>();
        #endregion

        #region Properties

        public object m_Module { get; set; }
        //public string m_wsUserName { get; set; }
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

        #endregion

        #region Public Static

        public static BaseService Instance(int groupId, PlatformType platform, eService serviceType)
        {
            return null;
            ///////// Implement GetInstance Logic /////
            //string serviceTcmConfigurationKey = string.Format("{0}{1}{2}{3}{4}", groupId, SPLITTER, platform, SPLITTER, serviceType);
            //string serviceUrl = TCMClient.Settings.Instance.GetValue<string>(string.Format("{0}{1}{2}", serviceTcmConfigurationKey, SPLITTER, "URL"));



            //if (!string.IsNullOrEmpty(serviceUrl) && !_services.ContainsKey(serviceUrl))
            //{
            //    //string serviceUrl = TCMClient.Settings.Instance.GetValue<string>(string.Format("{0}{1}{2}", dictionaryMultiKey, SPLITTER, "URL"));
            //    //string serviceUser = TCMClient.Settings.Instance.GetValue<string>(string.Format("{0}{1}{2}", dictionaryMultiKey, SPLITTER, "USER"));
            //    //string servicePass = TCMClient.Settings.Instance.GetValue<string>(string.Format("{0}{1}{2}", dictionaryMultiKey, SPLITTER, "PASSWORD"));

            //    //if (!string.IsNullOrEmpty(serviceUrl) && !string.IsNullOrEmpty(serviceUser) && !string.IsNullOrEmpty(servicePass))
            //    //{
            //        BaseService serviceInserted = ServiceFactory.GetService(groupId, platform, serviceUrl, serviceType);

            //        if (serviceInserted != null)
            //        {
            //            _services.TryAdd(serviceUrl, serviceInserted);
            //        }
            //    //}
            //}

            //BaseService service = null;
            //_services.TryGetValue(serviceUrl, out service);

            //return service;            
        }

        #endregion

    }
}
