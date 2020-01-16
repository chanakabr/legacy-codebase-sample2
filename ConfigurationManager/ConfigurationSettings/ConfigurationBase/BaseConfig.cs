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

        public abstract string TcmKey { get; }

        public abstract string[] TcmPath { get; }

        protected const string BaseClassName = "BaseConfig";

        public JToken GetTcmToken()
        {
            JToken token;
            token = Settings.Instance.GetJsonString(TcmPath);
            return token;
        }

        protected void SetActualValue<TV>(BaseValue<TV> baseValue, TV actualvalue)
        {
            baseValue.ActualValue = actualvalue;
        }

        public virtual void SetActualValue<TV>(JToken token, BaseValue<TV> defaultData)
        {
            string loweredKey = defaultData.Key.ToLower();
            var path = (TcmPath == null ? loweredKey : string.Join(":", TcmPath) + $":{loweredKey}").ToLower();

            try
            {
                bool emptyValue = true;

                if (token != null)
                {
                    var tokenValue = token[loweredKey];
                    
                    if (tokenValue != null)
                    {
                        emptyValue = false;
                        defaultData.ActualValue = token[loweredKey].ToObject<TV>();
                    }
                }

                if (emptyValue)
                {
                    _Logger.Info($"Empty data in TCM under object:  [{GetType().Name}]  for key [{path}], setting default value as actual value");
                    defaultData.ActualValue = defaultData.DefaultValue;
                }
            }
            catch (Exception ex)
            {
                _Logger.Error($"Invalid data structure for key: {path} under object [{GetType().Name}]. Setting default value as actual value", ex);
                defaultData.ActualValue = defaultData.DefaultValue;
            }
        }

        protected static void Init(IBaseConfig baseConfig)
        {
            try
            {
                Type type = baseConfig.GetType();
                MethodInfo TcmMethod = type.GetMethod("GetTcmToken");
                JToken token = (JToken)TcmMethod.Invoke(baseConfig, null);
                IterateOverClassFields(baseConfig, token);
            }
            catch (Exception ex)
            {
                _Logger.Error($"Eror in configuration reading: {ex}");
                //todo
            }
        }

        protected static void IterateOverClassFields(IBaseConfig baseConfig, JToken token)
        {
            Type type = baseConfig.GetType();
            List<FieldInfo> fields = type.GetFields().ToList();

            foreach (var field in fields)
            {
                object baseValueData = field.GetValue(baseConfig);

                if (baseValueData == null)
                {
                    _Logger.Error($"Null objcet configuration under {baseConfig.GetType().Name}, please init object during compile time ");
                    throw new Exception("In test means the filed is not initiated");
                }

                if (field.FieldType.Name.StartsWith("BaseValue"))
                {
                    if (token == null)
                    {
                        _Logger.Info($"No data exists in TCM for {baseConfig.ToString()} configuration, will use the default configuration");
                    }

                    Type argu = field.FieldType.GetGenericArguments()[0];
                    MethodInfo methodInfo = type.GetMethod("SetActualValue");
                    var genericMethod = methodInfo.MakeGenericMethod(argu);
                    genericMethod.Invoke(baseConfig, new object[] { token, baseValueData });
                }
                else if (field.FieldType == type && field.Name == "Current")
                {
                    continue;
                }
                else if (IsBaseStartWithName(field.FieldType, BaseClassName) && field.FieldType.GetInterface("IBaseConfig") != null)
                {
                    var baseConfig2 = baseValueData as IBaseConfig;
                    Init(baseConfig2);

                    if (!baseConfig2.Validate())
                    {
                        _Logger.Error($"TCM Configuration Validation Error under object:  [{baseConfig2.GetType().Name}] ");
                    }
                }
                else
                {
                    throw new Exception($"Unkown object {field.FieldType } in confiugration - please handle");
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

        public virtual bool Validate()
        {
            return true;
        }
    }
}
