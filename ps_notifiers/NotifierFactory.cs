using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ps_notifiers
{
    public static class NotifierFactory
    {
        public static BaseMediaNotifier GetMediaIngestNotifier(int groupID)
        {
            switch (groupID)
            {
                // MediaCorp 
                case 147:
                    return new MediaCorpMediaNotifier(groupID);

                default:
                    return null;
            }
        }
    }
}
