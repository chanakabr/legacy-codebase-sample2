using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Upload
{
    /// <summary>
    /// indicates the object type in the bulk file
    /// </summary>
    public abstract partial class KalturaBulkUploadObjectData : KalturaOTTObject
    {
    }
}