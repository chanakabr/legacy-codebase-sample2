using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class ExternalChannel
    {
        public long id;
        public string externalId;
        public string name;
        public int groupId;
        public List<ExternalChannelEnrichment> enrichments;

        /// <summary>
        /// KSQL expression with personalized filtering
        /// </summary>
        public string filterExpression;

        public int recommendationEngineId;
    }

    public enum ExternalChannelEnrichment : int
    {
        ClientLocation = 1,
        UserId = 2,
        HouseholdId = 4,
        DeviceId = 8,
        DeviceType = 16,
        UTCOffset = 32
    }
}
