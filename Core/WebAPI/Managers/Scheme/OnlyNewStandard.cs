using System;

namespace WebAPI.Managers.Scheme
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OnlyNewStandardAttribute : Attribute
    {
        public string SinceVersion { get; set; }

        #region ctor
        public OnlyNewStandardAttribute()
        {
            SinceVersion = null;
        }

        public OnlyNewStandardAttribute(string sinceVersion)
        {
            SinceVersion = sinceVersion;
        }
        #endregion

        public static bool IsNew(string version, Version current = null)
        {
            if (current == null)
            {
                OldStandardAttribute.getCurrentRequestVersion();
                if (current == null)
                {
                    return true;
                }
            }

            Version deprecationVersion = new Version(version);
            return current.CompareTo(deprecationVersion) >= 0;
        }
    }
}