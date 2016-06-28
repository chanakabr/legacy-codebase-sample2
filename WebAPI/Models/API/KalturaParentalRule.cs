using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Parental rule
    /// </summary>
    public class KalturaParentalRule : KalturaParentalRuleProfile
    {
        public KalturaParentalRule(KalturaParentalRuleProfile parentalRule)
        {
            id = parentalRule.id;
            name = parentalRule.name;
            description = parentalRule.description;
            order = parentalRule.order;
            epgTagTypeId = parentalRule.epgTagTypeId;
            blockAnonymousAccess = parentalRule.blockAnonymousAccess;
            ruleType = parentalRule.ruleType;
            mediaTagValues = parentalRule.mediaTagValues;
            epgTagValues = parentalRule.epgTagValues;
            isDefault = parentalRule.isDefault;
            Origin = parentalRule.Origin;
        }
    }
}