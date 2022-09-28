using ApiObjects;
using Core.Api;
using Core.GroupManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IngestHandler.Common.Infrastructure
{
    public class EpgIngestPartnerResolver
    {
        public static IEnumerable<int> GetRelevantPartnerIds()
        {
            var partnerIdsFromEnv = Environment.GetEnvironmentVariable("OTT_TEST_INGEST_CONSUMER_PARTNER_IDS");
            if (!string.IsNullOrEmpty(partnerIdsFromEnv))
            {
                var partnerIdsToConsume = partnerIdsFromEnv.Split(',').Select(id => int.Parse(id.Trim()));
                return partnerIdsToConsume;
            }
            var partnersWithEpgV2OrV3 = GroupSettingsManager.Instance.GetPartnersByEpgFeatureVersion(EpgFeatureVersion.V2, EpgFeatureVersion.V3);
            return partnersWithEpgV2OrV3;

        }
    }
}
