using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.ServiceModel
{
    public class MediaDTO
    {
        public string MediaID { get; set; }
        public string MediaName { get; set; }
        public string MediaTypeID { get; set; }
        public string MediaTypeName { get; set; }
        public double Rating { get; set; }
        public int ViewCounter { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastWatchDate { get; set; }
        public DateTime StartDate { get; set; }
        public string PicURL { get; set; }
        public string URL { get; set; }
        public string MediaWebLink { get; set; }
        public string Duration { get; set; }
        public string FileID { get; set; }

        public List<TagMetaPairDTO> Tags { get; set; }
        public List<TagMetaPairDTO> Metas { get; set; }
        public List<FileDTO> Files { get; set; }
        public List<TagMetaPairDTO> AdvertisingParameters { get; set; }
        public List<PictureDTO> Pictures { get; set; }
        public List<ExtIDPairDTO> ExternalIDs { get; set; }

        public DynamicDataDTO MediaDynamicData { get; set; }
        public string SubDuration { get; set; }
        public string SubFileFormat { get; set; }
        public string SubFileID { get; set; }
        public string SubURL { get; set; }
        public string GeoBlock { get; set; }
        public long TotalItems { get; set; }
        public int? like_counter { get; set; }

        public class ExtIDPairDTO
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        public class FileDTO
        {
            public string FileID { get; set; }
            public string URL { get; set; }
            public string Duration { get; set; }
            public string Format { get; set; }
            public AdvertisingProviderDTO PreProvider { get; set; }
            public AdvertisingProviderDTO PostProvider { get; set; }
            public AdvertisingProviderDTO BreakProvider { get; set; }
            public AdvertisingProviderDTO OverlayProvider { get; set; }
            public string[] BreakPoints { get; set; }
            public string[] OverlayPoints { get; set; }
        }

        public class PictureDTO
        {
            public string PicSize { get; set; }
            public string URL { get; set; }
        }
    }

    public class AdvertisingProviderDTO
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class TagMetaPairDTO
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class DynamicDataDTO
    {
        public bool IsFavorite { get; set; }
        public string Price { get; set; }
        public int MediaMark { get; set; }
        public PriceReasonDTO PriceType { get; set; }
        public bool Notification { get; set; }
        public DateTime ExpirationDate { get; set; }
    }

    public enum PriceReasonDTO
    {
        PPVPurchased,
        Free,
        ForPurchaseSubscriptionOnly,
        SubscriptionPurchased,
        ForPurchase,
        UnKnown,
        SubscriptionPurchasedWrongCurrency,
        PrePaidPurchased
    }

    [Serializable]
    public enum actionDTO
    {
        none,
        stop,
        finish,
        pause,
        play,
        first_play,
        load,
        bitrate_change
    }
}