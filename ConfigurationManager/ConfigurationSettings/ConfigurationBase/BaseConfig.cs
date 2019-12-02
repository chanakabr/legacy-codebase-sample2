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
        protected static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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
            var path = TcmPath == null ? defaultData.Key : string.Join(":", TcmPath) + $":{defaultData.Key}";
            try
            {
                if (token == null || token[defaultData.Key] == null)
                {
                    _Logger.Info($"Empty data in TCM under object:  [{GetType().Name}]  for key [{path}], setting default value as actual value");
                    defaultData.ActualValue = defaultData.DefaultValue;
                }
                else
                {
                    defaultData.ActualValue = token[defaultData.Key].ToObject<TV>();
                }
                if (!Validate())
                {
                    _Logger.Error($"TCM Configuration Validation Error under object:  [{GetType().Name}]  for key [{path}], setting default value as actual value");
                }
            }
            catch(Exception ex)
            {
                _Logger.Error($"Invalid data structure for key: {path} under object [{GetType().Name}]. Setting default value as actual value", ex);
                defaultData.ActualValue = defaultData.DefaultValue;
            }
        }

        protected static void Init(IBaseConfig baseConfig)
        {
            Type type = baseConfig.GetType();
            MethodInfo TcmMethod = type.GetMethod("GetTcmToken");
            JToken token = (JToken)TcmMethod.Invoke(baseConfig, null);
            IterateOverClassFields(baseConfig,  token);
        }

        protected static void IterateOverClassFields(IBaseConfig baseConfig,  JToken token)
        {
            Type type = baseConfig.GetType();
            List<FieldInfo> fields = type.GetFields().ToList();
            foreach (var field in fields)
            {
                object baseValueData = field.GetValue(baseConfig);
                if (baseValueData == null)
                {
                    _Logger.Error($"Null objcet configuration under {baseConfig.GetType().Name}, please init object during compile time ");
                    throw new Exception("In test means the filed is not initiate");
                }
                if (field.FieldType.Name.StartsWith("BaseValue"))
                {
                    if (token == null)
                    {
                        _Logger.Info($"No data exist in TCM for {baseConfig.ToString()} configuration, will use the default configuration");
                    }
                    Type argu = field.FieldType.GetGenericArguments()[0];
                    MethodInfo methodInfo = type.GetMethod("SetActualValue");
                    var genericMethod = methodInfo.MakeGenericMethod(argu);
                    genericMethod.Invoke(baseConfig, new object[] { token, baseValueData });
                }
                else if (field.FieldType == typeof(Dictionary<string, AdapterConfiguration>) ||
                    field.FieldType == typeof(ConsumerSettings) ||
                    field.FieldType == typeof(Dictionary<string, CouchbaseBucketConfig> ))
                {
                    MethodInfo methodInfo = type.GetMethod("SetValues");
                    methodInfo.Invoke(baseConfig, new object[] { token, baseValueData });
                }
                else if (field.FieldType == type && field.Name == "Current")
                {
                    continue;
                }
                else if (IsBaseStartWithName(field.FieldType, BaseClassName) &&
                    field.FieldType.GetInterface("IBaseConfig") != null)
                {
                    Init(baseValueData as IBaseConfig);

                }
                else
                {
                    throw new Exception("Do somthing - ToDo");
                }
            }
        }

        private static bool IsBaseStartWithName(Type fieldType, string typeName)
        {
            while (fieldType != typeof(object))
            {
                if (fieldType.Name.StartsWith(typeName))
                {
                    return true;
                }
                fieldType = fieldType.BaseType;
            }
            return false;
        }

        protected virtual bool Validate()
        {
            return true;
        }

    }
}
