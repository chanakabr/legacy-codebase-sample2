using ConfigurationManager.Types;
using KLogMonitor;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TCMClient;

namespace ConfigurationManager.ConfigurationSettings.ConfigurationBase
{
    public abstract class BaseConfig<T> : IBaseConfig
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public abstract string TcmKey { get;  }

        public abstract string [] TcmPath { get; }

        public const string BaseClassName = "BaseConfig";
        public JToken GetTcmToken()
        {
            JToken token;
            token = Settings.Instance.GetJsonString(TcmPath);
            return token;
        }


        public virtual void SetActualValue<TV>(JToken token, BaseValue<TV> defaultData) 
        {
            defaultData.ActualValue = token[defaultData.Key] == null ?
                defaultData.DefaultValue : token[defaultData.Key].ToObject<TV>();

            if (!Validate())
            {
                _Logger.Error($"TCM Configuration Validation Error. key:[{TcmKey}], actual value:[{defaultData.ActualValue}]");
            }
        }

        protected virtual bool Validate()
        {
            return true;
        }

        

    }
}
