using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
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
        public eRuleLevel origin;

        #endregion

        #region Ctor

        public ParentalRule()
        {
            name = string.Empty;
            blockAnonymousAccess = false;
            isDefault = false;

            mediaTagValues = new List<string>();
            epgTagValues = new List<string>();

            origin = eRuleLevel.User;
        }

        /// <summary>
        /// Create a parental rule based on a WS_API rule object
        /// </summary>
        /// <param name="rule"></param>
        public ParentalRule(TVPPro.SiteManager.TvinciPlatform.api.ParentalRule rule)
        {
            this.id = rule.id;
            this.name = rule.name;
            this.description = rule.description;
            this.order = rule.order.Value;
            this.mediaTagTypeId = rule.mediaTagTypeId.Value;
            this.epgTagTypeId = rule.epgTagTypeId.Value;
            this.blockAnonymousAccess = rule.blockAnonymousAccess.Value;

            switch (rule.ruleType)
            {
                case TVPPro.SiteManager.TvinciPlatform.api.eParentalRuleType.All:
                {
                    this.ruleType = eParentalRuleType.All;
                    break;
                }
                case TVPPro.SiteManager.TvinciPlatform.api.eParentalRuleType.Movies:
                {
                    this.ruleType = eParentalRuleType.Movies;
                    break;
                }
                case TVPPro.SiteManager.TvinciPlatform.api.eParentalRuleType.TVSeries:
                {
                    this.ruleType = eParentalRuleType.TVSeries;
                    break;
                }
                default:
                {
                    this.ruleType = eParentalRuleType.All;
                    break;
                }
            }

            this.origin = ConvertRuleLevelEnum(rule.level);

            this.mediaTagValues = new List<string>(rule.mediaTagValues);
            this.epgTagValues = new List<string>(rule.epgTagValues);
            this.isDefault = rule.isDefault;
        }

        public static eRuleLevel ConvertRuleLevelEnum(TVPPro.SiteManager.TvinciPlatform.api.eRuleLevel? originLevel)
        {
            eRuleLevel outLevel = eRuleLevel.User;

            if (originLevel.HasValue)
            {
                switch (originLevel)
                {
                    case TVPPro.SiteManager.TvinciPlatform.api.eRuleLevel.User:
                    {
                        outLevel = eRuleLevel.User;
                        break;
                    }
                    case TVPPro.SiteManager.TvinciPlatform.api.eRuleLevel.Domain:
                    {
                        outLevel = eRuleLevel.Domain;
                        break;
                    }
                    case TVPPro.SiteManager.TvinciPlatform.api.eRuleLevel.Group:
                    {
                        outLevel = eRuleLevel.Group;
                        break;
                    }
                    default:
                    {
                        outLevel = eRuleLevel.User;
                        break;
                    }
                }
            }

            return outLevel;
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
