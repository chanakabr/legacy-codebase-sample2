using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPVR
{
    public class NPVRRecordObj : NPVRParamsObj
    {
        public string SeriesId { get; set; }

        public int SeasonNumber { get; set; }

        public int SeasonSeed { get; set; }

        public int EpisodeSeed { get; set; }

        public List<string> LookupCriteria { get; set; }


        public NPVRRecordObj(string channelId, string domainID, string seriesId, int seasonSeed, int seasonNumber, int episodeSeed, List<string> lookupCriteria)
        {
            EpgChannelID = channelId;
            EntityID = domainID;
            SeriesId = seriesId;
            SeasonSeed = seasonSeed;
            SeasonNumber = seasonNumber;

            EpisodeSeed = episodeSeed;
            LookupCriteria = lookupCriteria;
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(String.Concat("NPVRRecordObj. Base Obj:", base.ToString()));
            sb.Append(String.Concat("SeriesId: ", SeriesId));
            sb.Append(String.Concat(" SeasonNumber: ", SeasonNumber));
            sb.Append(String.Concat(" SeasonSeed: ", SeasonSeed));
            sb.Append(String.Concat(" EpisodeSeed: ", EpisodeSeed));
            if (LookupCriteria != null && LookupCriteria.Count > 0)
            {
                sb.Append(String.Concat(" LookupCriteria: ", LookupCriteria != null ? string.Join(",", LookupCriteria) : string.Empty));
            }

            return sb.ToString();
        }
    }
}
