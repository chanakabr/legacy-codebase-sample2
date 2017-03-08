using ApiObjects;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
namespace Core.Catalog.Response
{
    [DataContract]
    public class MediaResponse : BaseResponse
    {
        public MediaResponse()
        {
        }
    }

    [DataContract]
    public class MediaObj : BaseObject
    {
        [DataMember]
        public string m_sName;
        [DataMember]
        public string m_sDescription;
        [DataMember]
        public MediaType m_oMediaType;
        [DataMember]
        public DateTime m_dCreationDate;
        [DataMember]
        public DateTime m_dFinalDate;
        [DataMember]
        public DateTime m_dPublishDate;
        [DataMember]
        public DateTime m_dCatalogStartDate;
        [DataMember]
        public DateTime m_dStartDate;
        [DataMember]
        public DateTime m_dEndDate;
        [DataMember]
        public List<Metas> m_lMetas;
        [DataMember]
        public List<Tags> m_lTags;
        [DataMember]
        public List<FileMedia> m_lFiles;
        [DataMember]
        public List<Branding> m_lBranding;
        [DataMember]
        public List<Picture> m_lPicture;
        [DataMember]
        public string m_ExternalIDs;
        [DataMember]
        public Int32 m_nLikeCounter;
        [DataMember]
        public RatingMedia m_oRatingMedia;
        [DataMember]
        public DateTime? m_dLastWatchedDate;
        [DataMember]
        public string m_sLastWatchedDevice;
        [DataMember]
        public string m_sSiteUserGuid;
        [DataMember]
        public string EntryId;
        [DataMember]
        public string CoGuid;
        [DataMember]
        public bool IsActive;

        [DataMember]
        public bool EnableCDVR;
        [DataMember]
        public bool EnableCatchUp;
        [DataMember]
        public bool EnableStartOver;
        [DataMember]
        public bool EnableTrickPlay;
        [DataMember]
        public long CatchUpBuffer;
        [DataMember]
        public long TrickPlayBuffer;
        [DataMember]
        public bool EnableRecordingPlaybackNonEntitledChannel;

        [DataMember]
        public string WatchPermissionRule;
        [DataMember]
        public string GeoblockRule;
        [DataMember]
        public string DeviceRule;

        [DataMember]
        public LanguageContainer[] Name;
        [DataMember]
        public LanguageContainer[] Description;


        public MediaObj()
            : base()
        {
            EnableCDVR = false;
            EnableCatchUp = false;
            EnableStartOver = false;
            EnableTrickPlay = false;
            CatchUpBuffer = 0;
            TrickPlayBuffer = 0;
            EnableRecordingPlaybackNonEntitledChannel = false;
        }
    }


    public class RatingMedia
    {
        public Int32 m_nRatingSum;
        public Int32 m_nRatingCount;
        public double m_nRatingAvg;
        public Int32 m_nViwes;
        public Int32 m_nVotesLoCnt;
        public Int32 m_nVotesUpCnt;
        public Int32 m_nVote1Count;
        public Int32 m_nVote2Count;
        public Int32 m_nVote3Count;
        public Int32 m_nVote4Count;
        public Int32 m_nVote5Count;

        public RatingMedia()
        {
            m_nRatingSum = 0;
            m_nRatingCount = 0;
            m_nRatingAvg = 0.0;
            m_nViwes = 0;
            m_nVotesLoCnt = 0;
            m_nVotesUpCnt = 0;
            m_nVote1Count = 0;
            m_nVote2Count = 0;
            m_nVote3Count = 0;
            m_nVote4Count = 0;
            m_nVote5Count = 0;

        }

        public RatingMedia(Int32 nRatingSum, Int32 nRatingCount, double nRatingAvg, Int32 nViwes, Int32 nVotesLoCnt, Int32 nVotesUpCnt, Int32 nVote1Count,
                            Int32 nVote2Count, Int32 nVote3Count, Int32 nVote4Count, Int32 nVote5Count)
        {
            m_nRatingSum = nRatingSum;
            m_nRatingCount = nRatingCount;
            m_nRatingAvg = nRatingAvg;
            m_nViwes = nViwes;
            m_nVotesLoCnt = nVotesLoCnt;
            m_nVotesUpCnt = nVotesUpCnt;
            m_nVote1Count = nVote1Count;
            m_nVote2Count = nVote2Count;
            m_nVote3Count = nVote3Count;
            m_nVote4Count = nVote4Count;
            m_nVote5Count = nVote5Count;
        }
    }

}

