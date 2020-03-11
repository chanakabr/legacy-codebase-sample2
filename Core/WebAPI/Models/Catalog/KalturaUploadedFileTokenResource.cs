using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaUploadedFileTokenResource : KalturaContentResource
    {
        /// <summary>
        /// Token that returned from uploadToken.add action
        /// </summary>
        [DataMember(Name = "token")]
        [JsonProperty(PropertyName = "token")]
        [XmlElement(ElementName = "token")]
        public string Token { get; set; }

        public override string GetUrl(int groupId)
        {
            var ut = UploadTokenManager.GetUploadToken(Token, groupId);
            return ut.FileUrl;
        }
    }
}