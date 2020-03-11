using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearch.Searcher
{
    public class EpgChannelsFilterCompositeType : BaseFilterCompositeTypeDecorator
    {
        protected List<long> epgChannelIDs;

        public EpgChannelsFilterCompositeType(BaseFilterCompositeType filterCompositeType, List<long> epgChannelIDs) :
            base(filterCompositeType)
        {
            if (epgChannelIDs == null || epgChannelIDs.Count == 0)
                throw new Exception("IPNOEpgFilterCompositeType constructor. Input list is empty");
            this.epgChannelIDs = epgChannelIDs;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("\"and\": [");
            sb.Append(String.Concat(ToStringHelper(), ",{"));
            sb.Append(OriginalFilterCompositeType.ToString());
            sb.Append("}]");

            return sb.ToString();


        }

        private string ToStringHelper()
        {
            StringBuilder sb = new StringBuilder("{\"terms\": { \"epg_channel_id\": [");
            int length = epgChannelIDs.Count;
            for (int i = 0; i < length; i++)
            {
                sb.Append(String.Concat(i == 0 ? string.Empty : ",", epgChannelIDs[i]));
            }
            sb.Append("]}}");

            return sb.ToString();
        }
    }
}
