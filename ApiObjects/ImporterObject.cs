using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    [Serializable]
    public class ImporterObject
    {

        public enum ImporterObjectStatus
        {
            OK = 0,
            FAILED = 1,
        }

        public enum ImporterObjectType
        {
            MEDIA = 0,
            CHANNEL = 1,
        }

        public class IportMediaObject
        {
            public IportMediaObject()
            {
                m_sCoGUID = string.Empty;
                m_nMediaID = 0;
                m_sMessage = string.Empty;
                m_Status = ImporterObjectStatus.OK;
                m_Type = ImporterObjectType.MEDIA;
            }

            public void Initialize(string sCoGuid, Int32 nMID, string sMessage, bool status)
            {
                m_sCoGUID = sCoGuid;
                m_nMediaID = nMID;
                m_sMessage = sMessage;

                if (status == false)
                {
                    m_Status = ImporterObjectStatus.FAILED;
                }
            }

            public string m_sCoGUID;
            public Int32 m_nMediaID;
            public string m_sMessage;
            public ImporterObjectStatus m_Status;
            public ImporterObjectType m_Type;
        }

        public class IportChannelObject
        {
            public IportChannelObject()
            {
                m_sCoGUID = string.Empty;
                m_nChannelID = 0;
                m_sMessage = string.Empty;
                m_Status = ImporterObjectStatus.OK;
                m_Type = ImporterObjectType.CHANNEL;
            }

            public void Initialize(string sCoGuid, Int32 nChannelID, string sMessage, bool status)
            {
                m_sCoGUID = sCoGuid;
                m_nChannelID = nChannelID;
                m_sMessage = sMessage;

                if (status == false)
                {
                    m_Status = ImporterObjectStatus.FAILED;
                }
            }

            public string m_sCoGUID;
            public Int32 m_nChannelID;
            public string m_sMessage;
            public ImporterObjectStatus m_Status;
            public ImporterObjectType m_Type;
        }


        public List<IportMediaObject> m_MediaList;
        public List<IportChannelObject> m_ChannelList;
        public string m_sNotifyXML;


        public ImporterObject()
        {
            m_sNotifyXML = string.Empty;
            m_MediaList = new List<IportMediaObject>();
            m_ChannelList = new List<IportChannelObject>();
        }

        public void UpdateNotifyXML(string res)
        {
            m_sNotifyXML += res;
        }

        public void AddMediaObject(IportMediaObject imo)
        {
            m_MediaList.Add(imo);
        }

        public void AddChannelObject(IportChannelObject ico)
        {
            m_ChannelList.Add(ico);
        }

    }
}
