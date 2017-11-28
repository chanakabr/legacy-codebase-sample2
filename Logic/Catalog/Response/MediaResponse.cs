using ApiObjects;
using Core.Catalog.CatalogManagement;
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

        public MediaObj(int groupId, MediaAsset mediaAsset)
            : base()
        {
            AssetId = mediaAsset.Id.ToString();
            m_sName = string.Copy(mediaAsset.Name);
            Name = mediaAsset.NamesWithLanguages.ToArray();
            m_sDescription = string.Copy(mediaAsset.Description);
            Description = mediaAsset.DescriptionsWithLanguages.ToArray();
            EntryId = string.Copy(mediaAsset.EntryId);
            CoGuid = string.Copy(mediaAsset.CoGuid);
            m_oMediaType = new MediaType(mediaAsset.MediaType.m_sTypeName, mediaAsset.MediaType.m_nTypeID);
            m_dCreationDate = mediaAsset.CreateDate.Value;
            m_dFinalDate = mediaAsset.FinalEndDate.HasValue ? mediaAsset.FinalEndDate.Value : DateTime.MaxValue;
            // TODO: Lior - Ask Ira about value of publish date
            m_dPublishDate = mediaAsset.CreateDate.Value;
            m_dStartDate = mediaAsset.StartDate.Value;
            m_dEndDate = mediaAsset.EndDate.HasValue ? mediaAsset.EndDate.Value : DateTime.MaxValue;
            m_dCatalogStartDate = mediaAsset.CatalogStartDate.Value;
            AssetType = eAssetTypes.MEDIA;
            IsActive = mediaAsset.IsActive.Value;
            m_dUpdateDate = mediaAsset.UpdateDate.Value;
            m_lMetas = new List<Metas>(mediaAsset.Metas);
            m_lTags = new List<Tags>(mediaAsset.Tags);
            GeoblockRule = mediaAsset.GeoBlockRuleId.HasValue ? Core.Catalog.CatalogLogic.GetGeoBlockRuleName(groupId, mediaAsset.GeoBlockRuleId.Value) : null;
            DeviceRule = mediaAsset.DeviceRuleId.HasValue ? Core.Catalog.CatalogLogic.GetDeviceRuleName(groupId, mediaAsset.DeviceRuleId.Value) : null;
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

