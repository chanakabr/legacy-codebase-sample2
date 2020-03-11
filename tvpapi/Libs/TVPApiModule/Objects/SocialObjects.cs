using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects
{
    public class SocialActivityDoc
    {
        [JsonProperty()]
        public string id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DocOwnerSiteGuid { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int SocialPlatform { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DocType { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long CreateDate { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long LastUpdate { get; set; }
        [JsonProperty()]
        public bool IsActive { get; set; }
        [JsonProperty()]
        public bool PermitSharing { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public SocialActivityObject ActivityObject { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public SocialActivitySubject ActivitySubject { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public SocialActivityVerb ActivityVerb { get; set; }

        public SocialActivityDoc()
        {
            DocType = "user_action";
            ActivityObject = new SocialActivityObject();
            ActivitySubject = new SocialActivitySubject();
            ActivityVerb = new SocialActivityVerb();
        }

        public SocialActivityDoc(ApiObjects.SocialActivityDoc source)
        {
            this.ActivityObject = new SocialActivityObject(source.ActivityObject);
            this.ActivitySubject = new SocialActivitySubject(source.ActivitySubject);
            this.ActivityVerb = new SocialActivityVerb(source.ActivityVerb);
            this.CreateDate = source.CreateDate;
            this.DocOwnerSiteGuid = source.DocOwnerSiteGuid;
            this.DocType = source.DocType;
            this.id = source.id;
            this.IsActive = source.IsActive;
            this.LastUpdate = source.LastUpdate;
            this.PermitSharing = source.PermitSharing;
            this.SocialPlatform = source.SocialPlatform;
        }
    }

    public class SocialActivityVerb
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SocialActionID { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int ActionType { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ActionName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int RateValue { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ActionProperties> ActionProperties { get; set; }

        public SocialActivityVerb()
        {
            ActionProperties = new List<ActionProperties>();
        }

        public SocialActivityVerb(ApiObjects.SocialActivityVerb source)
        {
            this.ActionName = source.ActionName;

            if (source.ActionProperties != null)
            {
                this.ActionProperties = source.ActionProperties.Select(o => new ActionProperties(o)).ToList();
            }

            this.ActionType = source.ActionType;
            this.RateValue = source.RateValue;
            this.SocialActionID = source.SocialActionID;
        }
    }

    public class SocialActivitySubject
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ActorSiteGuid { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ActorPicUrl { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ActorTvinciUsername { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int GroupID { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DeviceUdid { get; set; }

        public SocialActivitySubject()
        {
        }

        public SocialActivitySubject(ApiObjects.SocialActivitySubject source)
        {
            this.ActorPicUrl = source.ActorPicUrl;
            this.ActorSiteGuid = source.ActorSiteGuid;
            this.ActorTvinciUsername = source.ActorTvinciUsername;
            this.DeviceUdid = source.DeviceUdid;
            this.GroupID = source.GroupID;
        }
    }

    public class SocialActivityObject
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int AssetID { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ObjectID { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ApiObjects.eAssetType AssetType { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AssetName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PicUrl { get; set; }

        public SocialActivityObject()
        {
        }

        public SocialActivityObject(ApiObjects.SocialActivityObject source)
        {
            this.AssetID = source.AssetID;
            this.AssetName = source.AssetName;
            this.AssetType = source.AssetType;
            this.ObjectID = source.ObjectID;
            this.PicUrl = source.PicUrl;
        }
    }

    [Serializable]
    public class ActionProperties
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PropertyName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PropertyValue { get; set; }
        public ActionProperties(ApiObjects.ActionProperties source)
        {
            this.PropertyName = source.PropertyName;
            this.PropertyValue = source.PropertyValue;
        }
    }
}
