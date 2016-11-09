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

            string urlV1 = ElasticSearchTaskUtils.GetTcmConfigValue("ES_URL_V1");
            string urlV2 = ElasticSearchTaskUtils.GetTcmConfigValue("ES_URL_V2");

            switch (eType)
            {
                case ApiObjects.eObjectType.Channel:
                {
                    if (!string.IsNullOrEmpty(urlV2))
                    {
                        if (!string.IsNullOrEmpty(urlV1))
                        {
                            result = new DualChannelUpdater(nGroupID, urlV1, urlV2);
                        }
                        else
                        {
                            result = new ChannelUpdaterV2(nGroupID);
                        }
                    }
                    else
                    {
                        result = new ChannelUpdaterV1(nGroupID);
                    }

                    break;
                }
                case ApiObjects.eObjectType.Media:
                {
                    if (!string.IsNullOrEmpty(urlV2))
                    {
                        if (!string.IsNullOrEmpty(urlV1))
                        {
                            result = new DualMediaUpdater(nGroupID, urlV1, urlV2);
                        }
                        else
                        {
                            result = new MediaUpdaterV2(nGroupID);
                        }
                    }
                    else
                    {
                        result = new MediaUpdaterV1(nGroupID);
                    }

                    break;
                }
                case ApiObjects.eObjectType.EPG:
                {
                    if (!string.IsNullOrEmpty(urlV2))
                    {
                        if (!string.IsNullOrEmpty(urlV1))
                        {
                            result = new DualEpgUpdater(nGroupID, urlV1, urlV2);
                        }
                        else
                        {
                            result = new EpgUpdaterV2(nGroupID);
                        }
                    }
                    else
                    {
                        result = new EpgUpdaterV1(nGroupID);
                    }

                    break;
                }
                case ApiObjects.eObjectType.Recording:
                {
                    if (!string.IsNullOrEmpty(urlV2))
                    {
                        if (!string.IsNullOrEmpty(urlV1))
                        {
                            result = new DualRecordingUpdater(nGroupID, urlV1, urlV2);
                        }
                        else
                        {
                            result = new RecordingUpdaterV2(nGroupID);
                        }
                    }
                    else
                    {
                        result = new RecordingUpdaterV1(nGroupID);
                    }

                    break;
                }
                default:
                break;
            }

            return result;
        }
    }
}
