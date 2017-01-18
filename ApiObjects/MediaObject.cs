using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class MediaObject : BaseCacheObject
    {
        public MediaObject()
        {
            m_nMediaID = 0;
            m_sTitle = "";
            m_sBlockType = "";
            m_dPublishDate = DateTime.UtcNow;
            m_oMediaFiles = null;
            m_oPicPbjects = null;
            m_oMediaInfo = null;
            m_oMediaStatistics = null;
            m_oMediaPersonalStatistics = null;
            m_sOwnerGUID = "";
            m_oAvailableFileTypes = null;
        }

        public override string GetCacheKey(int nObjectID)
        {
            string sKey = this.GetType().ToString() + "_" + nObjectID.ToString();
            return sKey;
        }

        public void Initialize(Int32 nMediaID, string sTitle, string sBlockType, DateTime dPublishDate ,
            MediaFileObject[] oMediaFiles, PicObject[] oPicObjects, MediaInfoObject oMediaInfo ,
            MediaStatistics oMediaStatistics, MediaPersonalStatistics oMediaPersonalStatistics,
            string sOwnerGUID, FileTypeContainer[] oAvailableFileTypes)
        {
            m_oAvailableFileTypes = oAvailableFileTypes;
            m_nMediaID = nMediaID;
            m_sTitle = sTitle;
            m_sBlockType = sBlockType;
            m_dPublishDate = dPublishDate;
            m_oMediaFiles = oMediaFiles;
            m_oPicPbjects = oPicObjects;
            m_oMediaInfo = oMediaInfo;
            m_oMediaStatistics = oMediaStatistics;
            m_oMediaPersonalStatistics = oMediaPersonalStatistics;
            m_sOwnerGUID = sOwnerGUID;
        }

        public string m_sOwnerGUID;
        public Int32 m_nMediaID;
        public string m_sTitle;
        public string m_sBlockType;
        public DateTime m_dPublishDate;
        public MediaFileObject[] m_oMediaFiles;
        public PicObject[] m_oPicPbjects;
        public MediaInfoObject m_oMediaInfo;
        public MediaStatistics m_oMediaStatistics;
        public MediaPersonalStatistics m_oMediaPersonalStatistics;
        public FileTypeContainer[] m_oAvailableFileTypes;
    }
}
