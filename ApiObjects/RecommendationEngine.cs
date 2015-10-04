using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ApiObjects
{
    public class RecommendationEngine : RecommendationEngineBase
    {
        #region Properties
       
        public bool IsDefault
        {
            get;
            set;
        }

        public bool IsActive
        {
            get;
            set;
        }

        public string AdapterUrl
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

      

        public List<RecommendationEngineSettings> Settings
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
            this.ExternalIdentifier = clone.ExternalIdentifier;
            this.SharedSecret = clone.SharedSecret;
            this.Settings = new List<RecommendationEngineSettings>(clone.Settings);
        }

        #endregion
    }
}
