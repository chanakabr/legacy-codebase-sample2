using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects;

namespace Core.Catalog.Response
{
    [DataContract]
    [KnownType(typeof(RecordingSearchResult))]
    public class UnifiedSearchResult : BaseObject
    {
        #region Static Methods

        /// <summary>
        /// Parses a string to an enum, regardless of upper/lowercase issues
        /// </summary>
        /// <param name="typeString"></param>
        /// <returns></returns>
        public static eAssetTypes ParseType(string typeString)
        {
            eAssetTypes typeEnum = eAssetTypes.UNKNOWN;

            if (typeString.ToLower().StartsWith("media"))
            {
                typeEnum = eAssetTypes.MEDIA;
            }
            else if (typeString.ToLower().StartsWith("epg"))
            {
                typeEnum = eAssetTypes.EPG;
            }
            else if (typeString.ToLower().StartsWith("recording"))
            {
                typeEnum = eAssetTypes.NPVR;
            }

            return typeEnum;
        }

        #endregion
    }

    [DataContract]
    public class RecordingSearchResult : UnifiedSearchResult
    {
        [DataMember]
        public string EpgId
        {
            get;
            set;
        }
    }
}
