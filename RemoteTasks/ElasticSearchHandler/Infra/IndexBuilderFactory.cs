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

            switch (objectType)
            {
                case ApiObjects.eObjectType.Channel:
                    {
                        result = new ChannelIndexBuilderV2(groupID);
                        break;
                    }
                case ApiObjects.eObjectType.Media:
                    {
                        result = new MediaIndexBuilderV2(groupID);

                        break;
                    }
                case ApiObjects.eObjectType.EPG:
                    {

                        result = new EpgIndexBuilderV2(groupID);

                        break;
                    }
                case ApiObjects.eObjectType.Recording:
                    {

                        result = new RecordingIndexBuilderV2(groupID);
                        break;
                    }
                case ApiObjects.eObjectType.Tag:
                {

                        result = new TagsIndexBuilder(groupID);
                    break;
                }
                case ApiObjects.eObjectType.ChannelMetadata:
                    {

                        result = new ChannelMetadataIndexBuilder(groupID);
                        break;
                    }
                default:
                    break;
            }

            return result;
        }
    }
}
