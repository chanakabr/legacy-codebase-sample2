using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Tvinci.Data.DataLoader
{
    [Serializable]
    public class LoaderParameters : ISerializable
    {
        private SortedDictionary<string, object> m_retrieveParameters = new SortedDictionary<string, object>();
        private SortedDictionary<string, object> m_filterParameters = new SortedDictionary<string, object>();

        public LoaderParameters()
        {

        }

        public void SetParameter<TValue>(eParameterType type, string identifier, TValue value)
        {
            if (type == eParameterType.Retrieve)
            {
                m_retrieveParameters[identifier] = value;
            }
            else
            {
                m_filterParameters[identifier] = value;
            }
        }

        public TReturnValue GetParameter<TReturnValue>(eParameterType type, string identifier, TReturnValue defaultValue)
        {
            TReturnValue result = defaultValue;
            object tempValue;
            if (type == eParameterType.Retrieve)
            {
                if (m_retrieveParameters.TryGetValue(identifier, out tempValue))
                {
                    if (tempValue is TReturnValue)
                    {
                        return (TReturnValue)tempValue;
                    }
                }
            }
            else
            {
                if (m_filterParameters.TryGetValue(identifier, out tempValue))
                {
                    if (tempValue is TReturnValue)
                    {
                        return (TReturnValue)tempValue;
                    }
                }
            }

            return defaultValue;
        }

        public string GetUniqueKey()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0};", m_retrieveParameters.Count);

            foreach (KeyValuePair<string, object> item in m_retrieveParameters)
            {
                sb.AppendFormat("{0}|{1};", item.Key, getParameterToken(item.Value));
            }

            return sb.ToString();
        }

        private string getParameterToken(object propertyValue)
        {
            string value = string.Empty;
            if (propertyValue != null)
            {
                if (!(propertyValue is string) && !(propertyValue is ValueType))
                {
                    if (propertyValue is ICustomParameterType)
                    {
                        StringBuilder sb = new StringBuilder();

                        foreach (object innerPropertyValue in ((ICustomParameterType)propertyValue).GetPropertiesValue())
                        {
                            sb.AppendFormat("{0}_", getParameterToken(innerPropertyValue));
                        }

                        value = sb.ToString();
                    }
                    else if ((propertyValue is Array))
                    {
                        StringBuilder sb = new StringBuilder();
                        Array array = (Array)propertyValue;

                        foreach (object val in array)
                        {
                            object key = (val == null) ? string.Empty : val.ToString();

                            sb.AppendFormat("{0}_", key);
                        }

                        value = sb.ToString();
                    }
                    else
                    {
                        throw new Exception(string.Format("The parameter '{0}' of type '{1}' is not supported. please impelement interface 'ICustomParameterType'", propertyValue, propertyValue.GetType()));
                    }
                }
                else
                {
                    value = propertyValue.ToString();
                }
            }

            return value;
        }


        #region ISerializable Members

        protected LoaderParameters(SerializationInfo info, StreamingContext context)
        {
            for (int counter = 1; counter <= info.GetInt32("RP_Count"); counter++)
            {
                string key = (string)info.GetValue(string.Format("RP_{0}_K", counter), typeof(string));
                object value = info.GetValue(string.Format("RP_{0}_V", counter), typeof(object));

                m_retrieveParameters.Add(key, value);
            }

            for (int counter = 1; counter <= info.GetInt32("FP_Count"); counter++)
            {
                string key = (string)info.GetValue(string.Format("FP_{0}_K", counter), typeof(string));
                object value = info.GetValue(string.Format("FP_{0}_V", counter), typeof(object));

                m_filterParameters.Add(key, value);
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            int counter;
            info.AddValue("RP_Count", m_retrieveParameters.Count);
            counter = 0;
            foreach (KeyValuePair<string, object> item in m_retrieveParameters)
            {
                counter++;
                info.AddValue(string.Format("RP_{0}_K", counter), item.Key);
                info.AddValue(string.Format("RP_{0}_V", counter), item.Value);
            }

            info.AddValue("FP_Count", m_filterParameters.Count);
            counter = 0;
            foreach (KeyValuePair<string, object> item in m_filterParameters)
            {
                counter++;
                info.AddValue(string.Format("FP_{0}_K", counter), item.Key);
                info.AddValue(string.Format("FP_{0}_V", counter), item.Value);
            }
        }

        #endregion
    }
}