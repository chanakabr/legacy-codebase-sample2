using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects.SearchObjects
{
    [DataContract]
    public class UnifiedSearchResult : SearchResult
    {
        [DataMember]
        public AssetType type
        {
            get;
            set;
        }

        /// <summary>
        /// Parses a string to an enum, regardless of upper/lowercase issues
        /// </summary>
        /// <param name="typeString"></param>
        /// <returns></returns>
        public static AssetType ParseType(string typeString)
        {
            AssetType typeEnum = AssetType.Unknown;

            if (typeString.ToLower().StartsWith("media"))
            {
                typeEnum = AssetType.Media;
            }
            else if (typeString.ToLower().StartsWith("epg"))
            {
                typeEnum = AssetType.Epg;
            }

            return (typeEnum);
        }
    }

    /// <summary>
    /// Asset types that are stored in ES
    /// </summary>
    public enum AssetType
    {
        Unknown = -1,
        Media = 1,
        Epg = 2
    }
}
