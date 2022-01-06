using Phx.Lib.Appconfig;
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
                case ApiObjects.eObjectType.Channel:
                    {
                        result = new ChannelUpdaterV2(nGroupID);
                        break;
                    }
                case ApiObjects.eObjectType.Media:
                    {
                            result = new MediaUpdaterV2(nGroupID);

                        break;
                    }
                case ApiObjects.eObjectType.EPG:
                    {
                        result = new EpgUpdaterV2(nGroupID);
                        break;
                    }
                case ApiObjects.eObjectType.Recording:
                    {
                        result = new RecordingUpdaterV2(nGroupID);
                        break;
                    }
                case ApiObjects.eObjectType.Tag:
                    {
                        result = new TagUpdater(nGroupID);
                        break;
                    }
                case ApiObjects.eObjectType.ChannelMetadata:
                    {
                        result = new ChannelMetadataUpdater(nGroupID);
                        break;
                    }
                default:
                    break;
            }

            return result;
        }
    }
}
