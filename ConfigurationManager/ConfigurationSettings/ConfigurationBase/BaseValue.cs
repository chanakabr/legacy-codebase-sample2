
namespace ConfigurationManager.ConfigurationSettings.ConfigurationBase
{
    public class BaseValue<T> : IBaseValue<T>
    {
        internal string Key { get; }
        internal T DefaultValue { get; }
        internal bool MustBeOverwriteInTcm { get; } //seems we don't neet it anymore

        internal readonly string description;

        internal T ActualValue { get; set; }

        public T Value
        {
            get
            {
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

        public BaseValue<T> DeepCopy()
        {
            BaseValue<T> res = new BaseValue<T>(Key, Value, MustBeOverwriteInTcm, description);
            
            return res;
        }

    }
}
