using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;


namespace RestAdaptersCommon
{
    public class BaseAdapterClient<T> where T : new()
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly T _AdapterProfile;

        public BaseAdapterClient(T profile)
        {
            _AdapterProfile = profile;
        }

        public virtual eAdapterStatus SetConfiguration() { return eAdapterStatus.OK; }

        public void ValidateAdapterResponse(AdapterStatus status, Action retryAction = null)
        {
            _Logger.Info($"Validating adapter response AdapterProfile:[{_AdapterProfile}], ResponseStatus:[{status}]");
            if (status == null || status.Code == (int)eAdapterStatus.Error)
            {
                LogErrorAndThrow($"Adapater returned error or null response AdapterProfile:[{_AdapterProfile}], responseStatus:[{status}");
            }

            if (status?.Code == (int)eAdapterStatus.SignatureMismatch)
            {
                LogErrorAndThrow($"Adapater returned signature mismatch error. AdapterProfile:[{_AdapterProfile}], responseStatus:[{status}");
            }

            if (status?.Code == (int)eAdapterStatus.NoConfigurationFound)
            {
                _Logger.Info($"Adapter responded with no configuration found, resetting configuration. AdapterProfile:[{_AdapterProfile}], ResponseStatus:[{status}]");

                var resetConfigResponse = SetConfiguration();
                if (resetConfigResponse == eAdapterStatus.OK)
                {
                    _Logger.Info($"Adapter reset configuration successful, retrying initial request. AdapterProfile:[{_AdapterProfile}], responseStatus:[{status}");
                    retryAction?.Invoke();
                }
                else
                {
                    LogErrorAndThrow($"Adapater reset configuration failed. AdapterProfile:[{_AdapterProfile}], responseStatus:[{status}");
                }
            }
        }

        private void LogErrorAndThrow(string msg)
        {
            _Logger.Error(msg);
            throw new Exception(msg);
        }
    }
}
