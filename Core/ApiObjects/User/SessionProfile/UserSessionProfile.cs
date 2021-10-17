using System;
using ApiObjects.Rules;
using Newtonsoft.Json;

namespace ApiObjects.User.SessionProfile
{
    public interface IUserSessionConditionScope :
        IDeviceBrandConditionScope,
        IDeviceFamilyConditionScope,
        ISegmentsConditionScope,
        IDeviceManufacturerConditionScope,
        IDeviceModelConditionScope,
        IDynamicKeysConditionScope,
        IDeviceDynamicDataConditionScope
    { }

    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public interface IUserSessionProfileExpression
    {
        bool Evaluate(IUserSessionConditionScope scope);
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class UserSessionProfile
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public IUserSessionProfileExpression Expression { get; set; }
        public long CreateDate { get; set; }
        public long UpdateDate { get; set; }
    }
}