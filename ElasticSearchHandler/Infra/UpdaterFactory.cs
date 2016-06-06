using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearchHandler.Updaters
{
    public class UpdaterFactory
    {
        public static IElasticSearchUpdater CreateUpdater(int nGroupID, ApiObjects.eObjectType eType)
        {
            IElasticSearchUpdater result = null;

            switch (eType)
            {
                case ApiObjects.eObjectType.Media:
                    result = new MediaUpdaterV1(nGroupID);
                    break;
                case ApiObjects.eObjectType.Channel:
                    result = new ChannelUpdaterV1(nGroupID);
                    break;
                case ApiObjects.eObjectType.EPG:
                    result = new EpgUpdaterV1(nGroupID);
                    break;
                case ApiObjects.eObjectType.EpgChannel:
                    result = new EpgChannelUpdaterV1(nGroupID);
                    break;
                case ApiObjects.eObjectType.Recording:
                    result = new RecordingUpdaterV1(nGroupID);
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}
