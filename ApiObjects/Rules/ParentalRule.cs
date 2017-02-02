using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ApiObjects
{
    [Serializable]
    [JsonObject(Id = "id")]
    public class ParentalRule
    {
        #region Data Members

        [JsonProperty()]
        public long id;

        [JsonProperty()]
        public string name;

        [JsonProperty()]
        public string description;

        /// <summary>
        /// Rule order within the full list of rules
        /// </summary>
        [JsonProperty()]
        public int order;

        [JsonProperty()]
        public int mediaTagTypeId;

        [JsonProperty()]
        public int epgTagTypeId;

        /// <summary>
        /// Content that correspond to this rule is not available for guests
        /// </summary>
        [JsonProperty()]
        public bool blockAnonymousAccess;

        /// <summary>
        /// Rule type – Movies, TV series or both
        /// </summary>
        [JsonProperty()]
        public eParentalRuleType ruleType;
        
        [JsonProperty()]
        public List<string> mediaTagValues;

        [JsonProperty()]
        public List<string> epgTagValues;

        [JsonProperty()]
        public bool isDefault;

        [JsonProperty()]
        public eRuleLevel level;

        public string mediaTagType;

        
        public string epgTagType;

        #endregion

        #region Ctor

        public ParentalRule()
        {
            name = string.Empty;
            blockAnonymousAccess = false;
            isDefault = false;

            mediaTagValues = new List<string>();
            epgTagValues = new List<string>();

            level = eRuleLevel.User;
        }

        #endregion
    }

    /// <summary>
    /// Rule type – Movies, TV series or both
    /// </summary>
    public enum eParentalRuleType
    {
        All = 0,
        Movies = 1,
        TVSeries = 2
    }

    /// <summary>
    /// Distinction if rule was defined at account, domain or user level
    /// </summary>
    public enum eRuleLevel
    {
        User = 1,
        Domain = 2,
        Group = 3
    }
}
