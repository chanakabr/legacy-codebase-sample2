
using System;

namespace ConfigurationManager.ConfigurationSettings.ConfigurationBase
{
    public class BaseValue<T> : IBaseValue<T>
    {
        internal string Key { get; }
        internal T DefaultValue { get; }

        internal bool MustBeOverwriteInTcm { get; }

        internal readonly string description;

        internal T ActualValue { get; set; }

        public T Value
        {
            get
            {
                if(MustBeOverwriteInTcm && typeof(string) == typeof(T) && ActualValue.ToString() == TcmObjectKeys.Stub)
                {
                    throw new MissingFieldException($"key [{Key}] must be set in DCM ");
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
    }
}
