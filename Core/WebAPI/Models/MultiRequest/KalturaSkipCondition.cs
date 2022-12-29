using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Models.General;

namespace WebAPI.Models.MultiRequest
{
    /// <summary>
    /// Skip current request according to skip condition
    /// </summary>
    public abstract partial class KalturaSkipCondition : KalturaOTTObject
    {
       
    }
}