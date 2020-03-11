using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class InitializationObject
    {
        public InitializationObject() 
        {
            m_oPicObjects = null;
            m_oFileRequestObjects = null;
            m_oPlayerIMRequestObject = null;
            m_oUserIMRequestObject = null;
            m_oLanguageRequestObject = null;
            m_oExtraRequestObject = null;
            m_nGroupID = 0;
            m_nWatcherID = 0;
            m_nPlayerID = 0;
            m_sDevice = "";
            m_sAdminToken = "";
        }

        public void Initialize(PicObject[] oPicObjects ,
            FileRequestObject[] oFileRequestObjects ,
            PlayerIMRequestObject oPlayerIMRequestObject ,
            UserIMRequestObject oUserIMRequestObject ,
            LanguageRequestObject oLanguageRequestObject ,
            ExtraRequestObject oExtraRequestObject , 
            string sDevice ,
            string sAdminToken)
        {
            m_sAdminToken = sAdminToken;
            m_sDevice = sDevice;
            m_oPicObjects = oPicObjects;
            m_oFileRequestObjects = oFileRequestObjects;
            m_oPlayerIMRequestObject = oPlayerIMRequestObject;
            m_oUserIMRequestObject = oUserIMRequestObject;
            m_oLanguageRequestObject = oLanguageRequestObject;
            m_oExtraRequestObject = oExtraRequestObject;
            //InitializeGroupNPlayer();
        }

        public PicObject[] m_oPicObjects;
        public FileRequestObject[] m_oFileRequestObjects;
        public PlayerIMRequestObject m_oPlayerIMRequestObject;
        public UserIMRequestObject m_oUserIMRequestObject;
        public LanguageRequestObject m_oLanguageRequestObject;
        public ExtraRequestObject m_oExtraRequestObject;
        public string m_sDevice;
        public string m_sAdminToken;

        [System.Xml.Serialization.XmlIgnore]
        public Int32 m_nGroupID;

        [System.Xml.Serialization.XmlIgnore]
        public Int32 m_nWatcherID;

        [System.Xml.Serialization.XmlIgnore]
        public Int32 m_nPlayerID;
    }
}
