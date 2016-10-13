using System;

namespace WebAPI.Models.DMS
{
    [Serializable]
    public enum KalturaPlatform
    {
        Android = 0,
        iOS = 1,
        WindowsPhone = 2,
        Blackberry = 3,
        STB = 4,
        CTV = 5,
        Other = 6
    }

    public enum KalturaConfigurationType
    {        
        All,
        Default,
        NotDefault
    }
}