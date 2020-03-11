using CommonWithSL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonWithSL
{
    public class Media : IItemTemplate
    {
        public string ID { get; set; }
        public string MediaTypeID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageLink { get; set; }
        public bool IsLive { get; set; }
        public bool IsBlackout { get; set; }
        public bool HasVOD { get; set; }
        public bool HasTrailer { get; set; }
        public string AddedDate { get; set; }
        public string MediaTemplate { get; set; }
        public int Rating { get; set; }
        public string Controller { get; set; }
        public string EpgId { get; set; }
        public string Number { get; set; }
        public string CustomData { get; set; }
        public string PictureSize { get; set; }
        public List<MediaFile> Files { get; set; }
        public string EpisodeNumber { get; set; }
        public string SeasonNumber { get; set; }
        public List<Forcast> Forcast { get; private set; }
        public Dictionary<string, string> Metas { get; private set; }
        public Dictionary<string, string> Tags { get; private set; }
        public bool IsAnonymousFreeContent
        {
            get
            {
                if (Tags != null && Tags.ContainsKey("Product type") && Tags["Product type"] == CommonWithSL.Enums.ProductType.FVOD)
                {
                    return true;
                }
                return false;
            }
        }
        public bool IsRecordOnly
        {
            get
            {
                bool isRecordOnly = false;
                if (Tags != null && Tags.ContainsKey("RecordOnly") && bool.TryParse(Tags["RecordOnly"], out isRecordOnly))
                {
                    return isRecordOnly;
                }
                return isRecordOnly;
            }
        }

        public Media()
        {
            Metas = new Dictionary<string, string>();
            Tags = new Dictionary<string, string>();
            Files = new List<MediaFile>();
            Forcast = new List<Forcast>();
        }

        public string TemplateName
        {
            get
            {
                return MediaTemplate;
            }
            set
            {
                MediaTemplate = value;
            }
        }
    }
}
