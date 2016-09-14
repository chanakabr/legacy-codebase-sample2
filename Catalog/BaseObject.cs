using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using ApiObjects;
using Catalog.Response;

namespace Catalog
{
    /*
     * Base Object to all kind of return Object 
     * Media, Pictures ,(and for Future needs like Channels etc
     */
    [KnownType(typeof(MediaObj))]
    [KnownType(typeof(PicObj))]
    [KnownType(typeof(ProgramObj))]
    [KnownType(typeof(MediaFileObj))]
    [KnownType(typeof(UnifiedSearchResult))]
    [KnownType(typeof(RecordingSearchResult))]
    [DataContract]
    [Serializable]
    public class BaseObject
    {
        [DataMember]
        public string AssetId;

        [DataMember]
        public DateTime m_dUpdateDate;

        [DataMember]
        public eAssetTypes AssetType { get; set; }

        public BaseObject()
        {
            m_dUpdateDate = DateTime.MinValue;
            AssetType = eAssetTypes.MEDIA;
        }
    }
}
