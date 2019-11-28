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
        }

        public void Init()
        {
            Type type = typeof(T);
            Init(type, this);
        }


        private void Init(Type type, IBaseConfig baseConfig)
        {
            List<FieldInfo> fields = type.GetFields().ToList();
            MethodInfo TcmMethod = type.GetMethod("GetTcmToken");

            JToken token = (JToken)TcmMethod.Invoke(baseConfig, null);

            foreach (var field in fields)
            {
                object baseValueData = field.GetValue(baseConfig);
                if (baseValueData == null)
                {
                    //throw new Exception("In test means the filed is not initiate");
                }
                
                if (field.FieldType.Name.StartsWith("BaseValue"))
                {
                    if (token == null)
                    {
                        _Logger.Info($"No data exist in TCM for {baseConfig.ToString()} configuration");
                    }
                    else
                    {
                        Type argu = field.FieldType.GetGenericArguments()[0];
                        MethodInfo methodInfo = type.GetMethod("SetActualValue");
                        var genericMethod = methodInfo.MakeGenericMethod(argu);
                        genericMethod.Invoke(baseConfig, new object[] { token, baseValueData });
                    }

                }
                else if(field.FieldType == typeof(Dictionary<string, AdapterConfiguration>))
                {
                    Type argu = field.FieldType.GetGenericArguments()[0];
                    MethodInfo methodInfo = type.GetMethod("SetValues");
                    methodInfo.Invoke(baseConfig, new object[] { token, baseValueData });

                }
                else if (field.FieldType == type && field.Name == "Current")
                {
                    continue;
                }
                else if (field.FieldType.BaseType.Name.StartsWith("BaseConfig") && field.FieldType.GetInterface("IBaseConfig") != null)
                {
                    Init(field.FieldType, baseValueData as IBaseConfig);
                }
                else
                {
                    throw new Exception("Do somthing - ToDo");
                }
            }
        }

    }
}
