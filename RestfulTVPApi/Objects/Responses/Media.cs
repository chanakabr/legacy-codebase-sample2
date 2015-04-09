using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using TVPApiModule.Services;
using TVPApiModule.Manager;
using TVPApiModule.Context;
using TVPApiModule.Helper;
using RestfulTVPApi.Catalog;

/// <summary>
/// Summary description for Media
/// </summary>
/// 

namespace RestfulTVPApi.Objects.Responses
{
    public class ExtIDPair
    {
        public string key { get; set; }
        public string value { get; set; }
    }

    public class MediaFile
    {
        public string file_id { get; set; }
        public string url { get; set; }
        public string duration { get; set; }
        public string format { get; set; }
        public AdvertisingProvider pre_provider { get; set; }
        public AdvertisingProvider post_provider { get; set; }
        public AdvertisingProvider break_provider { get; set; }
        public AdvertisingProvider overlay_provider { get; set; }
        public string[] break_points { get; set; }
        public string[] overlay_points { get; set; }
        public string co_guid { get; set; }
    }

    public class Picture
    {
        public string pic_size { get; set; }
        public string url { get; set; }
    }

    public class TagMetaPair
    {
        public string key { get; set; }
        public string value { get; set; }

        public TagMetaPair(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }

    public class Media
    {
        #region Properties

        public string media_id { get; set; }
        public string media_name { get; set; }
        public string media_type_id { get; set; }
        public string media_type_name { get; set; }
        public double rating { get; set; }
        public int view_counter { get; set; }
        public string description { get; set; }
        public DateTime creation_date { get; set; }
        public DateTime? last_watch_date { get; set; }
        public DateTime start_date { get; set; }
        public DateTime catalog_start_date { get; set; }
        public string pic_url { get; set; }
        public string url { get; set; }
        public string mediaWeb_link { get; set; }
        public string duration { get; set; }
        public string file_id { get; set; }
        private List<TagMetaPair> m_tags;
        private List<TagMetaPair> m_metas;
        private List<MediaFile> m_files;
        private List<TagMetaPair> m_adParams;

        private List<Picture> m_pictures;

        private List<ExtIDPair> m_externalIDs;

        public string sub_duration { get; set; }
        public string sub_file_format { get; set; }
        public string sub_file_id { get; set; }
        public string sub_url { get; set; }
        public string geo_block { get; set; }
        public long total_items { get; set; }
        public int? like_counter { get; set; }


        public List<TagMetaPair> tags
        {
            get
            {
                if (m_tags == null)
                {
                    m_tags = new List<TagMetaPair>();
                }
                return m_tags;
            }
        }

        public List<TagMetaPair> metas
        {
            get
            {
                if (m_metas == null)
                {
                    m_metas = new List<TagMetaPair>();
                }
                return m_metas;
            }
        }

        public List<TagMetaPair> advertising_parameters
        {
            get
            {
                if (m_adParams == null)
                {
                    m_adParams = new List<TagMetaPair>();
                }
                return m_adParams;
            }
        }

        public List<MediaFile> files
        {
            get
            {
                if (m_files == null)
                {
                    m_files = new List<MediaFile>();
                }
                return m_files;
            }
        }

        public List<Picture> pictures
        {
            get
            {
                if (m_pictures == null)
                {
                    m_pictures = new List<Picture>();
                }
                return m_pictures;
            }
        }

        public List<ExtIDPair> external_ids
        {
            get
            {
                if (m_externalIDs == null)
                {
                    m_externalIDs = new List<ExtIDPair>();
                }
                return m_externalIDs;
            }
        }
        #endregion

        #region Constructors
        public Media()
        {

        }

        public Media(MediaObj mediaObj, string picSize, long totalItems, int groupID, PlatformType platform)//, bool withDynamic)
        {
            InitMediaObj(mediaObj, picSize, totalItems, groupID, platform);//, withDynamic);
        }

        #endregion

        #region private functions

        private string GetMediaWebLink(int groupID, PlatformType platform)
        {
            string retVal = string.Empty;
            string baseUrl = ConfigurationManager.AppSettings[string.Format("{0}_BaseURL", groupID.ToString())];
            if (!string.IsNullOrEmpty(baseUrl))
            {
                if (ConfigManager.GetInstance().GetConfig(groupID, platform).SiteConfiguration.Data.Features.FriendlyURL.SupportFeature)
                {
                    string sMediaName = media_name.Replace("/", "");

                    sMediaName = sMediaName.Replace(" ", "-");

                    sMediaName = HttpUtility.UrlEncode(sMediaName);

                    retVal = string.Format("{0}/{1}/{2}/{3}", baseUrl, media_type_name, sMediaName, media_id);
                }
                else
                {
                    retVal = string.Format("{0}/MediaPage.aspx?MediaID={1}&MediaType={2}", baseUrl, media_id, media_type_id);
                }
            }
            return retVal;
        }

        private void InitMediaObj(MediaObj mediaObj, string picSize, long totalItems, int groupID, PlatformType platform)//, bool withDynamic)
        {
            if (mediaObj != null)
            {
                media_id = mediaObj.m_nID.ToString();
                media_name = mediaObj.m_sName;
                media_type_id = mediaObj.m_oMediaType.m_nTypeID.ToString();
                media_type_name = mediaObj.m_oMediaType.m_sTypeName;
                rating = mediaObj.m_oRatingMedia.m_nRatingAvg;
                view_counter = mediaObj.m_oRatingMedia.m_nViwes;
                description = mediaObj.m_sDescription;
                creation_date = mediaObj.m_dCreationDate;
                last_watch_date = mediaObj.m_dLastWatchedDate;
                start_date = mediaObj.m_dStartDate;
                catalog_start_date = mediaObj.m_dCatalogStartDate;
                like_counter = mediaObj.m_nLikeCounter;

                total_items = totalItems;

                if (!string.IsNullOrEmpty(picSize))
                    pic_url = (from pic in mediaObj.m_lPicture where pic.m_sSize.ToLower() == picSize.ToLower() select pic.m_sURL).FirstOrDefault();


                //media.GeoBlock = No data...                

                //MediaWebLink
                mediaWeb_link = GetMediaWebLink(groupID, platform);

                //Files
                buildFiles(mediaObj.m_lFiles, mediaObj.m_lBranding, groupID, platform);

                //Metas & Tags
                BuildTagMetas(mediaObj.m_lMetas, mediaObj.m_lTags, groupID, platform);


                //ExternalIDs
                if (!string.IsNullOrEmpty(mediaObj.m_ExternalIDs))
                    external_ids.Add(new ExtIDPair() { key = "epg_id", value = mediaObj.m_ExternalIDs });

                //Pictures
                buildPictures(mediaObj.m_lPicture);
            }
        }

        private void buildFiles(List<FileMedia> mediaFiles, List<Branding> brandings, int groupID, PlatformType platform)
        {
            // Get file formats from configuration
            var techConfigFlashVars = ConfigManager.GetInstance().GetConfig(groupID, platform).TechnichalConfiguration.Data.TVM.FlashVars;
            string fileFormat = techConfigFlashVars.FileFormat;
            string subFileFormat = (techConfigFlashVars.SubFileFormat.Split(';')).FirstOrDefault();

            file_id = "0"; // default value

            if (mediaFiles != null && mediaFiles.Count > 0)
            {
                MediaFile mediaFile;
                foreach (FileMedia file in mediaFiles)
                {
                    mediaFile = new MediaFile();

                    mediaFile.file_id = file.m_nFileId.ToString();
                    mediaFile.url = file.m_sUrl;
                    mediaFile.duration = file.m_nDuration.ToString();
                    mediaFile.format = file.m_sFileFormat;
                    mediaFile.co_guid = file.m_sCoGUID;

                    if (file.m_sFileFormat.ToLower() == fileFormat.ToLower())
                    {
                        url = file.m_sUrl;
                        duration = file.m_nDuration.ToString();
                        file_id = file.m_nFileId.ToString();
                    }
                    if (file.m_sFileFormat.ToLower() == subFileFormat.ToLower())
                    {
                        sub_duration = file.m_nDuration.ToString();
                        subFileFormat = file.m_sFileFormat;
                        sub_file_id = file.m_nFileId.ToString();
                        sub_url = file.m_sUrl;
                    }

                    if (file.m_oPreProvider != null)
                        mediaFile.pre_provider = new AdvertisingProvider(file.m_oPreProvider.ProviderID, file.m_oPreProvider.ProviderName);

                    if (file.m_oPostProvider != null)
                        mediaFile.post_provider = new AdvertisingProvider(file.m_oPostProvider.ProviderID, file.m_oPostProvider.ProviderName);

                    if (file.m_oBreakProvider != null)
                    {
                        mediaFile.break_provider = new AdvertisingProvider(file.m_oBreakProvider.ProviderID, file.m_oBreakProvider.ProviderName);
                        if (!string.IsNullOrEmpty(file.m_sBreakpoints))
                            mediaFile.break_points = file.m_sBreakpoints.ToString().Split(';');
                    }

                    if (file.m_oOverlayProvider != null)
                    {
                        mediaFile.overlay_provider = new AdvertisingProvider(file.m_oOverlayProvider.ProviderID, file.m_oOverlayProvider.ProviderName);
                        if (!string.IsNullOrEmpty(file.m_sOverlaypoints))
                            mediaFile.overlay_points = file.m_sOverlaypoints.ToString().Split(';');
                    }

                    files.Add(mediaFile);
                }
            }

            if (brandings != null && brandings.Count > 0)
            {
                MediaFile mediaFile;
                foreach (Branding branding in brandings)
                {
                    mediaFile = new MediaFile();

                    mediaFile.file_id = branding.m_nFileId.ToString();
                    mediaFile.url = branding.m_sUrl;
                    mediaFile.duration = branding.m_nDuration.ToString();
                    mediaFile.format = branding.m_sFileFormat;
                    mediaFile.co_guid = branding.m_sCoGUID;

                    files.Add(mediaFile);
                }
            }
        }        

        private void buildPictures(List<RestfulTVPApi.Catalog.Picture> mediaPictures)
        {
            if (mediaPictures != null)
            {
                Picture picture;
                foreach (RestfulTVPApi.Catalog.Picture pic in mediaPictures)
                {
                    picture = new Picture();
                    picture.pic_size = pic.m_sSize;
                    picture.url = pic.m_sURL;
                    pictures.Add(picture);
                }
            }
        }

        private void BuildTagMetas(List<Metas> mediaMetas, List<Tags> mediaTags, int groupID, PlatformType platform)
        {
            TagMetaPair pair;

            string[] adMetas = ConfigManager.GetInstance().GetConfig(groupID, platform).MediaConfiguration.Data.TVM.AdvertisingValues.Metas.Split(';');

            if (mediaMetas != null)
            {
                foreach (Metas meta in mediaMetas)
                {
                    if (meta.m_oTagMeta.m_sName != "ID")
                    {
                        pair = new TagMetaPair(meta.m_oTagMeta.m_sName, meta.m_sValue);
                        metas.Add(pair);

                        if (adMetas.Contains(pair.key))
                            advertising_parameters.Add(pair);
                    }
                }
            }
            //Copy Tags

            if (mediaTags != null)
            {
                string[] adTags = ConfigManager.GetInstance().GetConfig(groupID, platform).MediaConfiguration.Data.TVM.AdvertisingValues.Tags.Split(';');

                foreach (Tags tag in mediaTags)
                {
                    if (tag.m_oTagMeta.m_sName != "ID")
                    {
                        foreach (string tagValue in tag.m_lValues)
                        {
                            TagMetaPair mediaTag = tags.Where(t => t.key == tag.m_oTagMeta.m_sName).FirstOrDefault();
                            if (mediaTag != null)
                            {
                                if (!string.IsNullOrEmpty(mediaTag.key) && !string.IsNullOrEmpty(mediaTag.value))
                                {
                                    mediaTag.value = (!String.IsNullOrEmpty(mediaTag.value.ToString())) ? string.Concat(mediaTag.value.ToString(), "|", tagValue) : tagValue;
                                }
                            }
                            else
                            {
                                pair = new TagMetaPair(tag.m_oTagMeta.m_sName, tagValue);
                                tags.Add(pair);

                                if (adTags.Contains(pair.key))
                                    advertising_parameters.Add(pair);
                            }
                                //}
                                //else
                                //{
                                //    mediaTag.value = (!String.IsNullOrEmpty(mediaTag.value.ToString())) ? string.Concat(mediaTag.value.ToString(), "|", tagValue) : tagValue;
                                //}
                            
                        }
                    }
                    //string[] adTags = ConfigManager.GetInstance().GetConfig(groupID, platform).MediaConfiguration.Data.TVM.AdvertisingValues.Tags.Split(';');

                    //foreach (TagMetaPair mediaTag in tags)
                    //{
                    //    if (adTags.Contains(mediaTag.key))
                    //        advertising_parameters.Add(mediaTag);
                    //}
                }
            }
        }


        private long GetMediaMark()
        {
            long retVal = 0;
            int groupID = WSUtils.GetGroupIDByMediaType(int.Parse(media_type_id));

            return retVal;
        }        

        #endregion
    }
}
