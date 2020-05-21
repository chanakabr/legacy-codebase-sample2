
using KLogMonitor;
using System;
using System.Reflection;

namespace ConfigurationManager.ConfigurationSettings.ConfigurationBase
{
    public class BaseValue<T> : IBaseValue<T>
    {
        protected static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        internal string Key { get; }
        internal T DefaultValue { get; }

        internal bool MustBeOverwriteInTcm { get; }

        internal readonly string description;

        internal T ActualValue { get; set; }

        public T Value
        {
            get
            {
                if (MustBeOverwriteInTcm && (typeof(string) == typeof(T) && ActualValue.ToString() == TcmObjectKeys.Stub || ActualValue == null))
                {
                    string message = $"key [{Key}] must be set in TCM.";
                    var ex = new MissingFieldException(message);
                    _Logger.Warn(message, ex);
                    throw ex;
                }

                return ActualValue == null ? DefaultValue : ActualValue;
            }
        }

        public BaseValue(string key, T defaultValue, bool mustBeOverwriteInTcm = false, string description = null)
        {
            this.Key = key;
            this.DefaultValue = defaultValue;
            this.MustBeOverwriteInTcm = mustBeOverwriteInTcm;
            this.description = description;
        }

        internal BaseValue<T> Clone()
        {
            return new BaseValue<T>(this.Key, this.DefaultValue, this.MustBeOverwriteInTcm, this.description);
        }
    }
}
