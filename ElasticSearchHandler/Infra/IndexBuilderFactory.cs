using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearchHandler.IndexBuilders
{
    public static class IndexBuilderFactory
    {
        public static AbstractIndexBuilder CreateIndexBuilder(int nGroupID, ApiObjects.eObjectType eType)
        {
            AbstractIndexBuilder result = null;

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
                            result = new DualChannelIndexBuilder(nGroupID, urlV1, urlV2);
                        }
                        else
                        {
                            result = new ChannelIndexBuilderV2(nGroupID);
                        }
                    }
                    else
                    {
                        result = new ChannelIndexBuilderV1(nGroupID);
                    }

                    break;
                }
                case ApiObjects.eObjectType.Media:
                {
                    if (!string.IsNullOrEmpty(urlV2))
                    {
                        if (!string.IsNullOrEmpty(urlV1))
                        {
                            result = new DualMediaIndexBuilder(nGroupID, urlV1, urlV2);
                        }
                        else
                        {
                            result = new MediaIndexBuilderV2(nGroupID);
                        }
                    }
                    else
                    {
                        result = new MediaIndexBuilderV1(nGroupID);
                    }

                    break;
                }
                case ApiObjects.eObjectType.EPG:
                {
                    if (!string.IsNullOrEmpty(urlV2))
                    {
                        if (!string.IsNullOrEmpty(urlV1))
                        {
                            result = new DualEPGIndexBuilder(nGroupID, urlV1, urlV2);
                        }
                        else
                        {
                            result = new EpgIndexBuilderV2(nGroupID);
                        }
                    }
                    else
                    {
                        result = new EpgIndexBuilderV1(nGroupID);
                    }

                    break;
                }
                case ApiObjects.eObjectType.Recording:
                {
                    if (!string.IsNullOrEmpty(urlV2))
                    {
                        if (!string.IsNullOrEmpty(urlV1))
                        {
                            result = new DualRecordingIndexBuilder(nGroupID, urlV1, urlV2);
                        }
                        else
                        {
                            result = new RecordingIndexBuilderV2(nGroupID);
                        }
                    }
                    else
                    {
                        result = new RecordingIndexBuilderV1(nGroupID);
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
