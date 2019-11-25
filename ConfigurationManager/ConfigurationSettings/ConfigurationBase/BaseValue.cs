
namespace ConfigurationManager.ConfigurationSettings.ConfigurationBase
{
    public class BaseValue<T>
    {
        internal string Key { get; }
        internal T DefaultValue { get; }
        internal bool ShouldAllowEmpty { get; } //seems we don't neet it anymore

        internal readonly string description;

        internal T ActualValue { get; set; }

        public T Value
        {
            get
            {
                return ActualValue == null ? DefaultValue : ActualValue;
            }
        }
        public BaseValue(string key, T defaultValue, bool ShouldAllowEmpty = true, string description = null)
        {
            this.Key = key;
            this.DefaultValue = defaultValue;
            this.ShouldAllowEmpty = ShouldAllowEmpty;
            this.description = description;
        }

  /*      public T Get()
        {
            return ActualValue == null ?  DefaultValue : ActualValue;
        }*/


    }
}
