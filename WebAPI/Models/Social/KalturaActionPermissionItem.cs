using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Social
{
    public class KalturaActionPermissionItem : KalturaOTTObject
    {
        /// <summary>
        /// Social network 
        /// </summary>
        [DataMember(Name = "network")]
        [JsonProperty("network")]
        [XmlElement(ElementName = "network", IsNullable = true)]
        [SchemeProperty()]
        public KalturaSocialNetwork? Network { get; set; }

        /// <summary>
        /// Action privacy 
        /// </summary>
        [DataMember(Name = "actionPrivacy")]
        [JsonProperty("actionPrivacy")]
        [XmlElement(ElementName = "actionPrivacy")]
        [SchemeProperty()]
        public KalturaSocialActionPrivacy ActionPrivacy { get; set; }

        /// <summary>
        /// Social privacy
        /// </summary>
        [DataMember(Name = "privacy")] 
        [JsonProperty("privacy")]
        [XmlElement(ElementName = "privacy")]
        [SchemeProperty()]
        public KalturaSocialPrivacy Privacy { get; set; }

        /// <summary>
        /// Action - separated with comma
        /// </summary>
        [DataMember(Name = "action")]
        [JsonProperty("action")]
        [XmlElement(ElementName = "action")]
        [SchemeProperty(DynamicType = typeof(KalturaSocialActionType))]
        public string Action { get; set; }


        public List<KalturaSocialActionType> SocialAction()
        {
            List<KalturaSocialActionType> socialActionsList = null;
            if (!string.IsNullOrEmpty(this.Action))
            {
                string[] socialActions = this.Action.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                socialActionsList = new List<KalturaSocialActionType>();
                foreach (string action in socialActions)
                {
                    KalturaSocialActionType socialActionType;
                    if (Enum.TryParse<KalturaSocialActionType>(action.ToUpper(), out socialActionType))
                    {
                        socialActionsList.Add(socialActionType);
                    }
                }
            }

            return socialActionsList;
        }
    }
}