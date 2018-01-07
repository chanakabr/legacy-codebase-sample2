using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.SearchObjects
{
    public class Media
    {
        #region Data Members

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

        public Dictionary<string, Dictionary<long, string>> m_dTagValues;
        public Dictionary<string, string> m_dMeatsValues;

        /// <summary>
        /// Region IDs in which the media is available in
        /// </summary>
        public List<int> regions;

        /// <summary>
        /// Geo Block Rules ID that is applied on this media
        /// </summary>
        public int geoBlockRule;

        /// <summary>
        /// Media file types that are currently free to watch for everyone
        /// </summary>
        public List<int> freeFileTypes;

        /// <summary>
        /// If the media is free or not
        /// </summary>
        public bool isFree;

        /// <summary>
        /// EPG identifier (if this is a linear channel)
        /// </summary>
        public string epgIdentifier;

        #endregion

        #region Ctor

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
            geoBlockRule = 0;

            string sNow = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            string sMax = DateTime.MaxValue.ToString("yyyyMMddHHmmss");

            m_sUpdateDate = sNow;
            m_sStartDate = sNow;
            m_sCreateDate = sNow;

            m_sEndDate = sMax;
            m_sFinalEndDate = sMax;

            m_dMeatsValues = new Dictionary<string, string>();
            m_dTagValues = new Dictionary<string, Dictionary<long, string>>();
            regions = new List<int>();
            freeFileTypes = new List<int>();

            epgIdentifier = null;
        }

        #endregion

        #region Public Methods

        public Media Clone()
        {
            Media clone = new Media()
            {
                m_sName = this.m_sName,
                m_dRating = this.m_dRating,
                m_nDeviceRuleId = this.m_nDeviceRuleId,
                m_nWPTypeID = this.m_nWPTypeID,
                m_nVotes = this.m_nVotes,
                m_nViews = this.m_nViews,
                m_nMediaID = this.m_nMediaID,
                m_nMediaTypeID = this.m_nMediaTypeID,
                m_nLikeCounter = this.m_nLikeCounter,
                m_nIsActive = this.m_nIsActive,
                m_nGroupID = this.m_nGroupID,
                m_sCreateDate = this.m_sCreateDate,
                m_sDescription = this.m_sDescription,
                m_sEndDate = this.m_sEndDate,
                m_sMFTypes = this.m_sMFTypes,
                m_sFinalEndDate = this.m_sFinalEndDate,
                m_sStartDate = this.m_sStartDate,
                m_sUpdateDate = this.m_sUpdateDate,
                m_sUserTypes = this.m_sUserTypes,
                geoBlockRule = this.geoBlockRule,
                epgIdentifier = this.epgIdentifier,
                isFree = this.isFree
            };

            clone.m_dMeatsValues = (from meta in this.m_dMeatsValues select meta).ToDictionary(x => x.Key, x => x.Value);

            clone.m_dTagValues = new Dictionary<string, Dictionary<long, string>>();

            foreach (string tagName in this.m_dTagValues.Keys)
            {
                Dictionary<long, string> dTag = new Dictionary<long, string>();
                foreach (int tagID in this.m_dTagValues[tagName].Keys)
                {
                    dTag.Add(tagID, this.m_dTagValues[tagName][tagID]);
                }

                clone.m_dTagValues[tagName] = dTag;
            }

            clone.regions.AddRange(this.regions);

            clone.freeFileTypes = new List<int>();
            clone.freeFileTypes.AddRange(this.freeFileTypes);

            return clone;
        }

        #endregion
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
