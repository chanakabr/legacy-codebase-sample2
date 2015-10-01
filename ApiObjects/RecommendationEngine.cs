using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ApiObjects
{
    public class RecommendationEngine
    {
        #region Properties

        public int ID
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public bool IsDefault
        {
            get;
            set;
        }

        public int IsActive
        {
            get;
            set;
        }

        public string AdapterUrl
        {
            get;
            set;
        }
        
        public string StatusUrl
        {
            get;
            set;
        }

        public string ExternalIdentifier
        {
            get;
            set;
        }

        public string SharedSecret
        {
            get;
            set;
        }

        [XmlIgnore]
        public int Status
        {
            get;
            set;
        }

        [XmlIgnore]
        public int Selected
        {
            get;
            set;
        }

        public List<KeyValuePair> Settings
        {
            get;
            set;
        }

        #endregion

        #region Ctor

        public RecommendationEngine()
        {

        }

        public RecommendationEngine(RecommendationEngine clone)
        {
            this.ID = clone.ID;
            this.Name = clone.Name;
            this.IsDefault = clone.IsDefault;
            this.IsActive = clone.IsActive;
            this.AdapterUrl = clone.AdapterUrl;
            this.Status = clone.Status;
            this.StatusUrl = clone.StatusUrl;
            this.ExternalIdentifier = clone.ExternalIdentifier;
            this.SharedSecret = clone.SharedSecret;
            this.Selected = clone.Selected;
            this.Settings = new List<KeyValuePair>(clone.Settings);
        }

        #endregion
    }
}
