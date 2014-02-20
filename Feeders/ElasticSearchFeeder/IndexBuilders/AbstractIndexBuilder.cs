using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchFeeder.IndexBuilders
{
    public abstract class AbstractIndexBuilder
    {
        protected static readonly string EPG = "epg";
        protected static readonly string MEDIA = "media";
        
        public static AbstractIndexBuilder GetIndexBuilder(int nGroupID, eESFeederType builderType)
        {
            AbstractIndexBuilder builder = null;

            switch (builderType)
            {
                case eESFeederType.MEDIA:
                    builder = new MediaIndexBuilder(nGroupID);
                    break;
                case eESFeederType.EPG:
                    builder = new EpgIndexBuilder(nGroupID);
                    break;
                default:
                    break;
            }

            return builder;
        }

        public DateTime dStartDate { get; set; }
        public DateTime dEndDate { get; set; }
        public abstract Task<bool> BuildIndex();
        public bool bSwitchIndex { get; set; }

    }
}
