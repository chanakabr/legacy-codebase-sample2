using ApiObjects.Epg;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ApiObjects
{

    public struct EPGDictionary
    {
        public string Key;
        public string Value;

    }

    public enum EPGUnit
    {
        Days,
        Hours,
        Current
    }

    [XmlType("EPGChannelProgrammeObject", Namespace = "http://api.tvinci.com/schemas/EPGChannelProgrammeObject1")]
    public class EPGChannelProgrammeObject
    {

        public long EPG_ID;
        public string EPG_CHANNEL_ID;
        public string EPG_IDENTIFIER;
        public string NAME;
        public string DESCRIPTION;
        public string START_DATE;
        public string END_DATE;
        public string PIC_URL;
        public int PIC_ID;
        public string STATUS;
        public string IS_ACTIVE;
        public string GROUP_ID;
        public string UPDATER_ID;
        public string UPDATE_DATE;
        public string PUBLISH_DATE;
        public string CREATE_DATE;
        public int LIKE_COUNTER;

        public List<EPGDictionary> EPG_TAGS;
        public List<EPGDictionary> EPG_Meta;

        public List<EpgPicture> EPG_PICTURES;

        public string media_id;

        // the linear media of the epg_channel
        public long LINEAR_MEDIA_ID;
        public int ENABLE_CDVR;
        public int ENABLE_CATCH_UP;
        public long CHANNEL_CATCH_UP_BUFFER;
        public int ENABLE_START_OVER;
        public int ENABLE_TRICK_PLAY;

        public string CRID;

        public void Initialize(long nEPG_ID, string nEPG_CHANNEL_ID, string nEPG_IDENTIFIER, string nNAME, string nDESCRIPTION, string nSTART_DATE, string nEND_DATE, string nPIC_URL,
            string nSTATUS, string nIS_ACTIVE, string nGROUP_ID, string nUPDATER_ID, string nUPDATE_DATE, string nPUBLISH_DATE, string nCREATE_DATE, List<EPGDictionary> nEPG_TAGS,
            List<EPGDictionary> nEPG_META, string nmedia_id, int nLikeCounter)
        {
            EPG_ID = nEPG_ID;
            EPG_CHANNEL_ID = nEPG_CHANNEL_ID;
            EPG_IDENTIFIER = nEPG_IDENTIFIER;
            NAME = nNAME;
            DESCRIPTION = nDESCRIPTION;
            START_DATE = nSTART_DATE;
            END_DATE = nEND_DATE;
            PIC_URL = nPIC_URL;
            STATUS = nSTATUS;
            IS_ACTIVE = nIS_ACTIVE;
            GROUP_ID = nGROUP_ID;
            UPDATER_ID = nUPDATER_ID;
            UPDATE_DATE = nUPDATE_DATE;
            PUBLISH_DATE = nPUBLISH_DATE;
            CREATE_DATE = nCREATE_DATE;
            EPG_TAGS = nEPG_TAGS;
            EPG_Meta = nEPG_META;
            media_id = nmedia_id;
            LIKE_COUNTER = nLikeCounter;
            EPG_PICTURES = new List<EpgPicture>();
        }
        public void Initialize(long nEPG_ID, string nEPG_CHANNEL_ID, string nEPG_IDENTIFIER, string nNAME, string nDESCRIPTION, string nSTART_DATE, string nEND_DATE, string nPIC_URL,
            string nSTATUS, string nIS_ACTIVE, string nGROUP_ID, string nUPDATER_ID, string nUPDATE_DATE, string nPUBLISH_DATE, string nCREATE_DATE, List<EPGDictionary> nEPG_TAGS,
            List<EPGDictionary> nEPG_META, string nmedia_id, int nLikeCounter, List<EpgPicture> epgPictures)
        {
            Initialize(nEPG_ID, nEPG_CHANNEL_ID, nEPG_IDENTIFIER, nNAME, nDESCRIPTION, nSTART_DATE, nEND_DATE, nPIC_URL, nSTATUS, nIS_ACTIVE, nGROUP_ID, nUPDATER_ID, nUPDATE_DATE,
                nPUBLISH_DATE, nCREATE_DATE, nEPG_TAGS, nEPG_META, nmedia_id, nLikeCounter);
            EPG_PICTURES = epgPictures;
        }

        public void Initialize(long nEPG_ID, string nEPG_CHANNEL_ID, string nEPG_IDENTIFIER, string nNAME, string nDESCRIPTION, string nSTART_DATE, string nEND_DATE, string nPIC_URL,
           string nSTATUS, string nIS_ACTIVE, string nGROUP_ID, string nUPDATER_ID, string nUPDATE_DATE, string nPUBLISH_DATE, string nCREATE_DATE, List<EPGDictionary> nEPG_TAGS,
           List<EPGDictionary> nEPG_META, string nmedia_id, int nLikeCounter, List<EpgPicture> epgPictures, int ENABLE_CDVR, int ENABLE_CATCH_UP, int ENABLE_START_OVER, int ENABLE_TRICK_PLAY)
        {
            Initialize(nEPG_ID, nEPG_CHANNEL_ID, nEPG_IDENTIFIER, nNAME, nDESCRIPTION, nSTART_DATE, nEND_DATE, nPIC_URL, nSTATUS, nIS_ACTIVE, nGROUP_ID, nUPDATER_ID, nUPDATE_DATE,
                nPUBLISH_DATE, nCREATE_DATE, nEPG_TAGS, nEPG_META, nmedia_id, nLikeCounter, epgPictures);
            this.ENABLE_CATCH_UP = ENABLE_CATCH_UP;
            this.CHANNEL_CATCH_UP_BUFFER = 0;
            this.ENABLE_CDVR = ENABLE_CDVR;
            this.ENABLE_START_OVER = ENABLE_START_OVER;
            this.ENABLE_TRICK_PLAY = ENABLE_TRICK_PLAY;
        }
        public void Initialize(long nEPG_ID, string nEPG_CHANNEL_ID, string nEPG_IDENTIFIER, string nNAME, string nDESCRIPTION, string nSTART_DATE, string nEND_DATE, string nPIC_URL,
           string nSTATUS, string nIS_ACTIVE, string nGROUP_ID, string nUPDATER_ID, string nUPDATE_DATE, string nPUBLISH_DATE, string nCREATE_DATE, List<EPGDictionary> nEPG_TAGS,
           List<EPGDictionary> nEPG_META, string nmedia_id, int nLikeCounter, List<EpgPicture> epgPictures, int ENABLE_CDVR, int ENABLE_CATCH_UP, int ENABLE_START_OVER, int ENABLE_TRICK_PLAY,
            string Crid)
        {
            Initialize(nEPG_ID, nEPG_CHANNEL_ID, nEPG_IDENTIFIER, nNAME, nDESCRIPTION, nSTART_DATE, nEND_DATE, nPIC_URL, nSTATUS, nIS_ACTIVE, nGROUP_ID, nUPDATER_ID, nUPDATE_DATE, nPUBLISH_DATE, nCREATE_DATE,
                nEPG_TAGS, nEPG_META, nmedia_id, nLikeCounter, epgPictures, ENABLE_CDVR, ENABLE_CATCH_UP, ENABLE_START_OVER, ENABLE_TRICK_PLAY);

            this.CRID = Crid;
          
        }


        public class EPGChannelProgrammeObjectStartDateComparer : IComparer<EPGChannelProgrammeObject>
        {

            public int Compare(EPGChannelProgrammeObject x, EPGChannelProgrammeObject y)
            {
                return x.START_DATE.CompareTo(y.START_DATE);
            }
        }
    }

    public class EPGMultiChannelProgrammeObject
    {
        public string EPG_CHANNEL_ID;
        public List<EPGChannelProgrammeObject> EPGChannelProgrammeObject;
        public void Initialize(string nEPG_CHANNEL_ID, List<EPGChannelProgrammeObject> oEPGChannelProgrammeObject)
        {
            EPG_CHANNEL_ID = nEPG_CHANNEL_ID;
            EPGChannelProgrammeObject = oEPGChannelProgrammeObject;
        }
    }

    public class RecordedEPGChannelProgrammeObject : EPGChannelProgrammeObject
    {
        public string RecordingID;
        public bool IsAssetProtected;
        public string ChannelName;
        public string RecordSource;
    }

    [Serializable]
    [DataContract]
    public class RecordedEPGOrderObj
    {
        [DataMember]
        public RecordedEPGOrderBy m_eOrderBy;
        [DataMember]
        public RecordedEPGOrderDir m_eOrderDir;

    }

    [Serializable]
    [DataContract]
    public enum RecordedEPGOrderBy
    {
        [EnumMember]
        StartTime = 0,
        [EnumMember]
        Name = 1,
        [EnumMember]
        ChannelID = 2
    }

    [Serializable]
    [DataContract]
    public enum RecordedEPGOrderDir
    {
        [EnumMember]
        DESC = 0,
        [EnumMember]
        ASC = 1
    }

    public class RecordedSeriesObject
    {
        public string recordingID;
        public string epgChannelID;
        public string seriesID;
        public string seriesName;

    }
}
