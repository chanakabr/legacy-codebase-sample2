using ConfigurationManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearchHandler.IndexBuilders
{
    public static class IndexBuilderFactory
    {
        public static AbstractIndexBuilder CreateIndexBuilder(int groupID, ApiObjects.eObjectType objectType)
        {
            AbstractIndexBuilder result = null;

            string urlV1 = ApplicationConfiguration.ElasticSearchConfiguration.URLV1.Value;
            string urlV2 = ApplicationConfiguration.ElasticSearchConfiguration.URLV2.Value;

            switch (objectType)
            {
                case ApiObjects.eObjectType.Channel:
                {
                    if (!string.IsNullOrEmpty(urlV2))
                    {
                        if (!string.IsNullOrEmpty(urlV1))
                        {
                            result = new DualChannelIndexBuilder(groupID, urlV1, urlV2);
                        }
                        else
                        {
                            result = new ChannelIndexBuilderV2(groupID);
                        }
                    }
                    else
                    {
                        result = new ChannelIndexBuilderV1(groupID);
                    }

                    break;
                }
                case ApiObjects.eObjectType.Media:
                {
                    if (!string.IsNullOrEmpty(urlV2))
                    {
                        if (!string.IsNullOrEmpty(urlV1))
                        {
                            result = new DualMediaIndexBuilder(groupID, urlV1, urlV2);
                        }
                        else
                        {
                            result = new MediaIndexBuilderV2(groupID);
                        }
                    }
                    else
                    {
                        result = new MediaIndexBuilderV1(groupID);
                    }

                    break;
                }
                case ApiObjects.eObjectType.EPG:
                {
                    if (!string.IsNullOrEmpty(urlV2))
                    {
                        if (!string.IsNullOrEmpty(urlV1))
                        {
                            result = new DualEPGIndexBuilder(groupID, urlV1, urlV2);
                        }
                        else
                        {
                            result = new EpgIndexBuilderV2(groupID);
                        }
                    }
                    else
                    {
                        result = new EpgIndexBuilderV1(groupID);
                    }

                    break;
                }
                case ApiObjects.eObjectType.Recording:
                {
                    if (!string.IsNullOrEmpty(urlV2))
                    {
                        if (!string.IsNullOrEmpty(urlV1))
                        {
                            result = new DualRecordingIndexBuilder(groupID, urlV1, urlV2);
                        }
                        else
                        {
                            result = new RecordingIndexBuilderV2(groupID);
                        }
                    }
                    else
                    {
                        result = new RecordingIndexBuilderV1(groupID);
                    }

                    break;
                }
                case ApiObjects.eObjectType.Tag:
                {
                    if (!string.IsNullOrEmpty(urlV2))
                    {
                        result = new TagsIndexBuilder(groupID);
                    }

                    break;
                }
                case ApiObjects.eObjectType.ChannelMetadata:
                {
                    if (!string.IsNullOrEmpty(urlV2))
                    {
                        result = new ChannelMetadataIndexBuilder(groupID);
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
