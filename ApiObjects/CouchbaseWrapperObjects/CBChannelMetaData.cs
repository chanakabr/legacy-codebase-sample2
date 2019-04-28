using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects.CouchbaseWrapperObjects
{
    public class CBChannelMetaData
    {
        public int Id { get; set; }
        public Dictionary<string, string> MetaData { get; set; }

        public enum eChannelType
        {
            Internal,
            External
        }

        public static string CreateChannelMetaDataKey(int id, eChannelType channelType)
        {
            return string.Format("channel_metadata_{0}_{1}", id, channelType);
        }
    }
}
