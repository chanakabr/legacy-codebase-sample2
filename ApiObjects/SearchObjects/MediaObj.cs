using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.SearchObjects
{
    public class Media
    {
        public int m_nMediaID;
        public int m_nMediaTypeID;
        public int m_nWPTypeID;
        public int m_nGroupID;
        public int m_nIsActive;
        public int m_nDeviceRuleId;
        public int m_nLikeCounter;
        public int m_nViews;
        public double m_dRating;
        public int m_nVotes;

        public string m_sStartDate;
        public string m_sEndDate;
        public string m_sFinalEndDate;
        public string m_sCreateDate;
        public string m_sUpdateDate;

        public string m_sMFTypes;

        public string m_sName;
        public string m_sDescription;
        public string m_sUserTypes;

        public Dictionary<string, string> m_oMeatsValues;
        public Dictionary<string, string> m_oTagsValues;

        

        public Media()
        {
            m_sName = string.Empty;
            m_sDescription = string.Empty;
            m_sMFTypes = string.Empty;
            m_sUserTypes = string.Empty;

            m_nMediaID = 0;
            m_nWPTypeID = 0;
            m_nMediaTypeID = 0;
            m_nGroupID = 0;
            m_nIsActive = 0;
            m_nDeviceRuleId = 0;
            m_nLikeCounter = 0;
            m_nViews = 0;
            m_dRating = 0.0;
            m_nVotes = 0;

            string sNow = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            string sMax = DateTime.MaxValue.ToString("yyyyMMddHHmmss");

            m_sUpdateDate = sNow;
            m_sStartDate = sNow;
            m_sCreateDate = sNow;

            m_sEndDate = sMax;
            m_sFinalEndDate = sMax;

            m_oMeatsValues = new Dictionary<string, string>();
            m_oTagsValues = new Dictionary<string, string>();

        }
    }

    public class MetaObject
    {
        public MetaType m_eType;
        public string m_sName;

        public MetaObject()
        {
            m_eType = MetaType.STR;
            m_sName = string.Empty;
        }

        public MetaObject(MetaType mt, string name)
        {
            m_eType = mt;
            m_sName = name;
        }
    }
    public enum MetaType
    {
        STR = 1,
        NUM = 2,
        BOOL = 3
    }
}
