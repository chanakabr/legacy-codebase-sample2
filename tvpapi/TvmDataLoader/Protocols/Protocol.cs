using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.IO;
using System.Configuration;
using System.Collections.Specialized;

namespace Tvinci.Data.TVMDataLoader.Protocols
{
    [Flags]
    public enum eBehaivor
    {
        None = 0,
        ForceNoCache = 2
    }

    [Serializable]
    public abstract class Protocol : IProtocol
    {
        protected virtual void PreSerialize()
        {
            return;
        }

		protected enum eProtocolType
		{
			Read,
			Write
		}

        public virtual bool IsTVMProProtocol()
        {
            return false;
        }

		protected abstract eProtocolType GetProtocolType();

        //protected abstract object GetProtocolObject();

        protected string PostSerialize(string serializedRequest)
        {
            return serializedRequest;
        }

        public virtual string PreResponseProcess(string originalResponse)
        {
            return originalResponse;
        }

        protected bool IsValidRequest()
        {
            return true;
        }

       
        
        public eBehaivor Behaivor { get; set; }

        public delegate string GetRequestLanguageDelegate();

        protected bool EnableTimers
        {
            get
            {
                if (m_GetTVMConfigurationMethod == null)
                    return false;

                return m_GetTVMConfigurationMethod().EnableTimer;
            }
        }

        [NonSerialized]
        private static GetRequestLanguageDelegate m_GetRequestLanguageMethod;

        public static GetRequestLanguageDelegate GetRequestLanguageMethod
        {
            get
            {
                return m_GetRequestLanguageMethod;
            }
            set
            {
                m_GetRequestLanguageMethod = value;
            }
        }

        static Protocol()
        {
            GetRequestLanguageMethod = dummyGetRequestLanguage;
        }

        private static string dummyGetRequestLanguage()
        {
            return string.Empty;
        }

        protected string getCacheValue(string currentValue)
        {
            if (currentValue != null && currentValue.Equals("1"))
            {
                return "1";
            }

            if ((Behaivor & eBehaivor.ForceNoCache) == eBehaivor.ForceNoCache)
            {
                return "1";
            }
            else
            {
                if (m_GetTVMConfigurationMethod == null)
                    return "0";

                return m_GetTVMConfigurationMethod().ForceUpdatedData ? "1" : "0";
            }
        }

        protected string getTVMUserValue()
        {
            if (m_GetTVMConfigurationMethod == null)
                return string.Empty;

            return m_GetTVMConfigurationMethod().User;
        }

        protected string getUseZipValue()
        {
            if (m_GetTVMConfigurationMethod == null)
                return "1";

            return m_GetTVMConfigurationMethod().UseZip ? "1" : "0";
        }

        protected string getTVMPasswordValue()
        {
            if (m_GetTVMConfigurationMethod == null)
                return string.Empty;

            return m_GetTVMConfigurationMethod().Password;
        }

        protected string getLanguageValue()
        {
            return GetRequestLanguageMethod();
        }

        public delegate TVMProtocolConfiguration GetTVMConfigurationDelegate();
        public static GetTVMConfigurationDelegate m_GetTVMConfigurationMethod;
        public static GetTVMConfigurationDelegate GetTVMConfigurationMethod
        {
            set
            {
                m_GetTVMConfigurationMethod = value;
            }
        }

        #region IProtocol Members

        bool IProtocol.IsValidRequest()
        {
            return IsValidRequest();
        }

        void IProtocol.PreSerialize()
        {
            PreSerialize();
        }

        string IProtocol.PostSerialize(string serializedRequest)
        {
            return PostSerialize(serializedRequest);
        }

        public bool ProtocolUseZip
        {
            get
            {
                if (m_GetTVMConfigurationMethod == null)
                    return true;

                return m_GetTVMConfigurationMethod().UseZip;
            }
        }

		public bool IsWriteProtocol
		{
			get
			{
				return GetProtocolType() == eProtocolType.Write;
			}
		}
        #endregion
	}

    public class TVMProtocolConfiguration
    {
        public TVMProtocolConfiguration(bool forceUpdatedData, bool enableTimer, string user, string password) :
            this(forceUpdatedData, enableTimer, user, password, true)
        {
        }

        public TVMProtocolConfiguration(bool forceUpdatedData, bool enableTimer, string user, string password, bool useZip)
        {
            UseZip = useZip;
            ForceUpdatedData = forceUpdatedData;
            EnableTimer = enableTimer;
            User = user;
            Password = password;
        }

        public bool ForceUpdatedData { get; set; }
        public bool EnableTimer { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public bool UseZip { get; set; }
    }
}
