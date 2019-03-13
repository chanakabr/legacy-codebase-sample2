using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Web;
using KLogMonitor;
using log4net;
using log4net.Util;

namespace KlogMonitorHelper
{
    internal static class Helpers
    {
        internal static string TryGetStringValueSafe<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV defaultVal = default(TV))
        {
            TV retVal;
            if (dict.TryGetValue(key, out retVal))
            {
                return retVal?.ToString();
            }

            return defaultVal?.ToString();
        }

        internal static string TryGetStringValueSafe(this IDictionary dict, object key, object defaultVal = null)
        {
            object retVal;
            if (dict.Contains(key))
            {
                return dict[key]?.ToString();
            }

            return defaultVal?.ToString();
        }
    }

    public class ContextData
    {
        private const string DEFAULT_LOG4_NET_CONFIG_FILE = "log4net.config";

        private static readonly KLogger _Log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public static readonly LogicalThreadContextProperties _ContextData = KLogger.LogContextData;

        private readonly MessageProperties _WCFContextData = OperationContext.Current?.IncomingMessageProperties;
        private readonly IDictionary _HttpContextData = HttpContext.Current?.Items;

        public void Load(string log4NetConfigFile = null)
        {
            if (log4NetConfigFile == null)
                log4NetConfigFile = DEFAULT_LOG4_NET_CONFIG_FILE;

            try
            {
                KLogger.Configure(log4NetConfigFile, KMonitor.AppType);
                KMonitor.Configure(log4NetConfigFile, KMonitor.AppType);

                TransferLoggingContextDataToLogicalThreadContext();

            }
            catch (Exception ex)
            {
                _Log.Error("Error while trying to load context data to a new thread", ex);
            }
        }

        private void TransferLoggingContextDataToLogicalThreadContext()
        {
            try
            {
                switch (KMonitor.AppType)
                {
                    case KLogEnums.AppType.WCF:

                        if (_WCFContextData != null)
                        {
                            _ContextData[Constants.CLIENT_TAG] = _WCFContextData.TryGetStringValueSafe(Constants.CLIENT_TAG);
                            _ContextData[Constants.HOST_IP] = _WCFContextData.TryGetStringValueSafe(Constants.HOST_IP);
                            _ContextData[Constants.REQUEST_ID_KEY] = _WCFContextData.TryGetStringValueSafe(Constants.REQUEST_ID_KEY);
                            _ContextData[Constants.GROUP_ID] = _WCFContextData.TryGetStringValueSafe(Constants.GROUP_ID);
                            _ContextData[Constants.ACTION] = _WCFContextData.TryGetStringValueSafe(Constants.ACTION);
                            _ContextData[Constants.USER_ID] = _WCFContextData.TryGetStringValueSafe(Constants.USER_ID);
                            _ContextData[Constants.TOPIC] = _WCFContextData.TryGetStringValueSafe(Constants.TOPIC);
                            _ContextData[Constants.KS] = _WCFContextData.TryGetStringValueSafe(Constants.KS);
                        }

                        break;

                    case KLogEnums.AppType.WS:
                    default:

                        if (_HttpContextData != null)
                        {
                            _ContextData[Constants.CLIENT_TAG] = _HttpContextData.TryGetStringValueSafe(Constants.CLIENT_TAG);
                            _ContextData[Constants.HOST_IP] = _HttpContextData.TryGetStringValueSafe(Constants.HOST_IP);
                            _ContextData[Constants.REQUEST_ID_KEY] = _HttpContextData.TryGetStringValueSafe(Constants.REQUEST_ID_KEY);
                            _ContextData[Constants.GROUP_ID] = _HttpContextData.TryGetStringValueSafe(Constants.GROUP_ID);
                            _ContextData[Constants.ACTION] = _HttpContextData.TryGetStringValueSafe(Constants.ACTION);
                            _ContextData[Constants.USER_ID] = _HttpContextData.TryGetStringValueSafe(Constants.USER_ID);
                            _ContextData[Constants.TOPIC] = _HttpContextData.TryGetStringValueSafe(Constants.TOPIC);
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                _Log.Error("Error while trying to copy context data to a new thread", ex);
            }
        }

    }
}
