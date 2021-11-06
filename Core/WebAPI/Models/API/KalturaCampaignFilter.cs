using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.Base;
using System;
using ApiLogic.Users.Managers;
using System.Collections.Generic;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Campaign filter (same as KalturaCampaignSearchFilter with no parameters)
    /// </summary>
    [Serializable]
    public partial class KalturaCampaignFilter : KalturaFilter<KalturaCampaignOrderBy>
    {
        public override KalturaCampaignOrderBy GetDefaultOrderByValue()
        {
            return KalturaCampaignOrderBy.START_DATE_DESC;
        }
    }
}