using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.MultiRequest
{
    /// <summary>
    /// Skips current request if an error occurs according to the selected skip option 
    /// </summary>
    public partial class KalturaSkipOnErrorCondition : KalturaSkipCondition
    {
        /// <summary>
        /// Indicates which error should be considered to skip the current request
        /// </summary>
        [DataMember(Name = "condition")]
        [JsonProperty("condition")]
        [XmlElement(ElementName = "condition")]
        public KalturaSkipOptions Condition { get; set; }

        protected override void Init()
        {
            base.Init();
            Condition = KalturaSkipOptions.No;
        }
    }
}