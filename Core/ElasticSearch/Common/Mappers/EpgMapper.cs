using System;
using System.Collections.Generic;
using ApiObjects;

namespace ElasticSearch.Common.Mappers
{
    public static class EpgMapper
    {
        public static EpgEs MapEpg(EpgCB epg, bool doesGroupUsesTemplates)
        {
            return Map(epg, doesGroupUsesTemplates, true);
        }

        public static EpgEs MapRecording(EpgCB epg, bool isOpc)
        {
            return Map(epg, isOpc, false);
        }

        private static EpgEs Map(EpgCB epg, bool isOpc, bool withRouting)
        {
            var epgEs = new EpgEs();
            epgEs.EpgID = epg.EpgID;
            epgEs.GroupId = isOpc ? epg.ParentGroupID : epg.GroupID;
            epgEs.ChannelId = epg.ChannelID;
            epgEs.IsActive = epg.IsActive;
            epgEs.StartDate = epg.StartDate;
            epgEs.EndDate = epg.EndDate;
            epgEs.Name = epg.Name;
            epgEs.Description = epg.Description;
            epgEs.CacheDate = DateTime.UtcNow;
            epgEs.CreateDate = epg.CreateDate;
            epgEs.UpdateDate = epg.UpdateDate;
            epgEs.SearchEndDate = epg.SearchEndDate;
            epgEs.Crid = epg.Crid;
            epgEs.EpgIdentifier = epg.EpgIdentifier;
            epgEs.ExternalId = epg.EpgIdentifier;
            epgEs.DocumentId = epg.DocumentId;
            epgEs.IsAutoFill = epg.IsAutoFill;
            epgEs.EnableCDVR = epg.EnableCDVR;
            epgEs.EnableCatchUp = epg.EnableCatchUp;
            epgEs.Suppressed = epg.Suppressed;
            epgEs.Metas = epg.Metas;
            epgEs.Tags = epg.Tags;
            epgEs.Regions = epg.regions?.ToArray();
            epgEs.ExternalOfferIds = (epg.ExternalOfferIds ?? new List<string>()).ToArray();

            if (epg.LinearMediaId > 0)
            {
                epgEs.LinearMediaId = epg.LinearMediaId;
            }

            if (withRouting)
            {
                epgEs.DateRouting = epg.StartDate.ToUniversalTime();
            }

            return epgEs;
        }
    }
}