using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Catalog
{
    [DataContract]
    public class ProgramObj : BaseObject
    {
        [DataMember]
        public ApiObjects.EPGChannelProgrammeObject m_oProgram;

        /*
        [DataMember]
        public int m_nEpgChannelID;
        [DataMember]
        public int m_nLikeCounter;

        [DataMember]
        public string m_sName;
        [DataMember]
        public string m_sDescription;

        [DataMember]
        public DateTime m_dCreationDate;
        [DataMember]
        public DateTime m_dPublishDate;
        [DataMember]
        public DateTime m_dStartDate;
        [DataMember]
        public DateTime m_dEndDate;

        [DataMember]
        public List<Metas> m_lMetas;
        [DataMember]
        public List<Tags> m_lTags;

        [DataMember]
        public List<Picture> m_lPicture;
        */

        public ProgramObj()
            : base()
        {
            this.AssetType = ApiObjects.eAssetTypes.EPG;
            this.m_oProgram = new ApiObjects.EPGChannelProgrammeObject();
        }
    }
}
