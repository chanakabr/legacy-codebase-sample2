using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.Social
{
    public partial class KalturaSocialActionRate : KalturaSocialAction
    {
        /// <summary>
        /// The value of the rating
        /// </summary>
        [DataMember(Name = "rate")]
        [JsonProperty("rate")]
        [XmlElement(ElementName = "rate")]
        public int Rate { get; set; }

        public KalturaSocialActionRate(int value) : base(null)
        {
            Rate = value;
        }

        protected override void Init()
        {
            base.Init();
            ActionType = KalturaSocialActionType.RATE;
        }
    }
}
