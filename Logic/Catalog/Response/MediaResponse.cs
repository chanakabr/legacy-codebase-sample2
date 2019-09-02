using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.Catalog;
using Core.Catalog.CatalogManagement;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TVinciShared;

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
            m_sName = mediaAsset.Name != null ? mediaAsset.Name : string.Empty;
            Name = mediaAsset.NamesWithLanguages != null ? mediaAsset.NamesWithLanguages.ToArray() : new LanguageContainer[] { };
            m_sDescription = mediaAsset.Description != null ? mediaAsset.Description : string.Empty;
            Description = mediaAsset.DescriptionsWithLanguages != null ? mediaAsset.DescriptionsWithLanguages.ToArray() : new LanguageContainer[] { };
            EntryId = mediaAsset.EntryId != null ? mediaAsset.EntryId : string.Empty;
            CoGuid = mediaAsset.CoGuid != null ? mediaAsset.CoGuid : string.Empty;
            m_oMediaType = new MediaType(mediaAsset.MediaType.m_sTypeName, mediaAsset.MediaType.m_nTypeID);
            m_dCreationDate = mediaAsset.CreateDate.Value.TruncateMilliSeconds();
            m_dFinalDate = mediaAsset.FinalEndDate != null && mediaAsset.FinalEndDate.HasValue ? 
                mediaAsset.FinalEndDate.Value.TruncateMilliSeconds() : DateTime.MaxValue;
            m_dStartDate = mediaAsset.StartDate != null && mediaAsset.StartDate.HasValue ?
                mediaAsset.StartDate.Value.TruncateMilliSeconds() : DateTime.MaxValue;
            m_dEndDate = mediaAsset.EndDate != null && mediaAsset.EndDate.HasValue ?
                mediaAsset.EndDate.Value.TruncateMilliSeconds() : DateTime.MaxValue;
            m_dCatalogStartDate = mediaAsset.CatalogStartDate != null && mediaAsset.CatalogStartDate.HasValue ?
                mediaAsset.CatalogStartDate.Value.TruncateMilliSeconds() : DateTime.MaxValue;
            AssetType = eAssetTypes.MEDIA;
            IsActive = mediaAsset.IsActive ?? false;
            m_dUpdateDate = mediaAsset.UpdateDate ?? DateTime.MinValue;
            m_lMetas = mediaAsset.Metas != null ? new List<Metas>(mediaAsset.Metas) : new List<Metas>();
            m_lTags = mediaAsset.Tags != null ? new List<Tags>(mediaAsset.Tags) : new List<Tags>();
            GeoblockRule = mediaAsset.GeoBlockRuleId.HasValue ? TvmRuleManager.GetGeoBlockRuleName(groupId, mediaAsset.GeoBlockRuleId.Value) : null;
            DeviceRule = mediaAsset.DeviceRuleId.HasValue ? TvmRuleManager.GetDeviceRuleName(groupId, mediaAsset.DeviceRuleId.Value) : null;
            m_lFiles = FileManager.ConvertFiles(mediaAsset.Files, groupId);
            m_lPicture = Core.Catalog.CatalogManagement.ImageManager.ConvertImagesToPictures(mediaAsset.Images, groupId);
            m_ExternalIDs = mediaAsset.FallBackEpgIdentifier != null ? mediaAsset.FallBackEpgIdentifier : string.Empty;
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

