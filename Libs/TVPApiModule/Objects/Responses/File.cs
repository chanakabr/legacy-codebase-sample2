//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace TVPApiModule.Objects.Responses
//{
//    public class File
//    {
//        [JsonProperty(PropertyName = "id")]
//        public int Id { get; set; }

//        [JsonProperty(PropertyName = "duration")]
//        public double Duration { get; set; }

//        [JsonProperty(PropertyName = "format")]
//        public string Format { get; set; }

//        [JsonProperty(PropertyName = "url")]
//        public string Url { get; set; }

//        [JsonProperty(PropertyName = "billing_type")]
//        public string BillingType { get; set; }

//        [JsonProperty(PropertyName = "cdn_id")]
//        public int CdnId { get; set; }

//        [JsonProperty(PropertyName = "pre_provider")]
//        public AdProvider PreProvider { get; set; }

//        [JsonProperty(PropertyName = "break_provider")]
//        public AdProvider BreakProvider { get; set; }

//        [JsonProperty(PropertyName = "overlay_provider")]
//        public AdProvider OverlayProvider { get; set; }

//        [JsonProperty(PropertyName = "post_provider")]
//        public AdProvider PostProvider { get; set; }

//        [JsonProperty(PropertyName = "break_points")]
//        public string Breakpoints { get; set; }

//        [JsonProperty(PropertyName = "overlay_points")]
//        public string Overlaypoints { get; set; }

//        [JsonProperty(PropertyName = "is_pre_skip_enabled")]
//        public bool IsPreSkipEnabled { get; set; }

//        [JsonProperty(PropertyName = "is_post_skip_enabled")]
//        public bool IsPostSkipEnabled { get; set; }

//        [JsonProperty(PropertyName = "co_guid")]
//        public string CoGuid { get; set; }

//        [JsonProperty(PropertyName = "language")]
//        public string Language { get; set; }

//        [JsonProperty(PropertyName = "is_default_language")]
//        public bool IsDefaultLanguage { get; set; }

//        [JsonProperty(PropertyName = "alt_url")]
//        public string AltUrl { get; set; }

//        [JsonProperty(PropertyName = "alt_cdn_id")]
//        public int AltCdnID { get; set; }

//        [JsonProperty(PropertyName = "alt_co_guid")]
//        public string AltCoGuid { get; set; }

//        [JsonProperty(PropertyName = "media_id")]
//        public int MediaID { get; set; }

//        public File(Tvinci.Data.Loaders.TvinciPlatform.Catalog.FileMedia file)
//        {
//            if (file != null)
//            {
//                Id = file.m_nFileId;
//                Duration = file.m_nDuration;
//                Format = file.m_sFileFormat;
//                Url = file.m_sUrl;
//                BillingType = file.m_sBillingType;
//                CdnId = file.m_nCdnID;
//                Breakpoints = file.m_sBreakpoints;
//                Overlaypoints = file.m_sOverlaypoints;
//                IsPreSkipEnabled = file.m_bIsPreSkipEnabled;
//                IsPostSkipEnabled = file.m_bIsPostSkipEnabled;
//                CoGuid = file.m_sCoGUID;
//                Language = file.m_sLanguage;
//                IsDefaultLanguage = file.m_nIsDefaultLanguage == 1 ? true : false;
//                AltUrl = file.m_sAltUrl;
//                AltCdnID = file.m_nAltCdnID;
//                AltCoGuid = file.m_sAltCoGUID;
//                MediaID = file.m_nMediaID;

//                if (file.m_oPreProvider != null)
//                {
//                    PreProvider = new AdProvider(file.m_oPreProvider);
//                }
//                if (file.m_oBreakProvider != null)
//                {
//                    BreakProvider = new AdProvider(file.m_oBreakProvider);
//                }
//                if (file.m_oBreakProvider != null)
//                {
//                    OverlayProvider = new AdProvider(file.m_oBreakProvider);
//                }
//                if (file.m_oPostProvider != null)
//                {
//                    PostProvider = new AdProvider(file.m_oPostProvider);
//                }
//            }
//        }
//    }
//}
