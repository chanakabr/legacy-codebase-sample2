using Newtonsoft.Json;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Define base profile response -  optional configurations
    /// </summary>
    [JsonObject]
    public abstract partial class KalturaBaseResponseProfile : KalturaOTTObject
    {
        
    }
}