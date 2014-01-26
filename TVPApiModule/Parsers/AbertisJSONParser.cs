using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.Services;
using ODBCWrapper;
using TVPPro.SiteManager.DataLoaders;
using TVPApiModule.Services;
using TVPPro.SiteManager.TvinciPlatform.Pricing;
using TVPApiModule.CatalogLoaders;
namespace TVPApi
{
    public class AbertisJSONParser : IParser
    {
        #region IParser Members

        public string Parse(object obj, int items, int index, int groupID, long totalItemsCount, PlatformType platform)
        {
            object objToParse = null;
            if (obj is Category)
            {
                objToParse = ParseCatToActivaCat((Category)obj);
            }
            if (obj is PageContext)
            {
                objToParse = ParsePageToActivaCategory((PageContext)obj);
            }
            if (obj is List<GalleryItem>)
            {
                objToParse = ParseGalleryItemsToActivaGallery((List<GalleryItem>)obj);
            }
            if (obj is List<Media>)
            {
                objToParse = ParseChannelToActivaChannel((List<Media>)obj, totalItemsCount, groupID, platform);
            }
            if (obj is GalleryItem)
            {
                objToParse = ParseGalleryItemToActivaChannel((GalleryItem)obj, items, index, groupID, platform);
            }
            StringBuilder sb = new StringBuilder();
            JavaScriptSerializer jsSer = new JavaScriptSerializer();
            jsSer.Serialize(objToParse, sb);
            return sb.ToString();
        }

        #endregion

        private void AdMetaToMedia(ActivaMedia activaMedia, Media media)
        {
            activaMedia.aspectRatio16_9 = true;
            if (media.metas != null)
            {
                foreach (TagMetaPair metaPair in media.metas)
                {
                    if (metaPair.key.Equals("short description"))
                    {
                        activaMedia.shortDescription = metaPair.value;
                    }
                    if (metaPair.key.Equals("medium description"))
                    {
                        activaMedia.midDescription = metaPair.value;
                    }
                    if (metaPair.key.Equals("Duration"))
                    {
                        activaMedia.assetDuration = int.Parse(metaPair.value);
                    }
                    if (metaPair.key.Equals("is premium"))
                    {
                        Boolean.TryParse(metaPair.value, out activaMedia.premium);
                    }
                    if (metaPair.key.Equals("Hour"))
                    {
                        activaMedia.assetHour = metaPair.value;
                    }
                    
                }
            }
            if (media.tags != null)
            {
                foreach (TagMetaPair tagPair in media.tags)
                {
                    if (tagPair.key.Equals("Channel"))
                    {
                        activaMedia.channelName = tagPair.value;
                    }
                    if (tagPair.key.Equals("Parental"))
                    {
                        activaMedia.tvParental = tagPair.value;
                    }
                    if (tagPair.key.Equals("Genre"))
                    {
                        activaMedia.genre = tagPair.value;
                    }

                }
            }
        }

        private ActivaGallery ParseGalleryItemsToActivaGallery(List<GalleryItem> items)
        {
            ActivaGallery retVal = new ActivaGallery();
            if (items != null)
            {
                foreach (GalleryItem item in items)
                {
                    ActivaGalleryItem activaItem = new ActivaGalleryItem();
                    activaItem.functionID = item.TVMChannelID.ToString();
                    activaItem.functionName = item.Title;
                    if (retVal.content == null)
                    {
                        retVal.content = new List<ActivaGalleryItem>();
                    }
                    retVal.content.Add(activaItem);
                }
            }
            return retVal;
        }

        private InitializationObject GetInitObj()
        {
            InitializationObject retVal = new InitializationObject();
            retVal.Platform = PlatformType.STB;
            return retVal;
        }

        private ActivaFile ParseMediaToActivaFile(Media media)
        {
            ActivaFile retVal = null;
            if (media != null)
            {
                retVal = new ActivaFile();
                string fileID = media.fileID;

            }
            return retVal;
        }

        private ActivaChannel ParseGalleryItemToActivaChannel(GalleryItem item, int items, int index, int groupID, PlatformType platform)
        {
            ActivaChannel retVal = new ActivaChannel();
            if (item != null)
            {
                string wsPass = string.Empty;
                string wsUser = string.Empty;
                GetWSUserPass(groupID, ref wsUser, ref wsPass);
                long mediaCount = 0;
                List<Media> mediaList = new APIChannelMediaLoader((int)item.TVMChannelID, groupID, platform, GetInitObj().UDID, SiteHelper.GetClientIP(), items, index, "full", GetInitObj().Locale.LocaleLanguage, null, Tvinci.Data.Loaders.TvinciPlatform.Catalog.CutWith.OR)
                {
                    UseStartDate = bool.Parse(ConfigManager.GetInstance().GetConfig(groupID, platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate)
                }.Execute() as List<Media>;
                retVal = ParseChannelToActivaChannel(mediaList, mediaCount, groupID, platform);
                retVal.sectionTitle = item.Title;
                retVal.test = "TestVal";
                //retVal.itemsCount = mediaCount;
                retVal.status = string.Format("{0} - {1}" ,groupID.ToString(), item.TVMChannelID);
            }
            return retVal;
        }

        private ActivaChannel ParseChannelToActivaChannel(List<Media> medias, long totalItemsCount, int groupID, PlatformType platform)
        {
            ActivaChannel retVal = new ActivaChannel();
            retVal.itemsCount = totalItemsCount;
            if (medias != null && medias.Count > 0)
            {
                Dictionary<string, ActivaMedia> filesDict = new Dictionary<string, ActivaMedia>();
                int[] filesArr = new int[medias.Count];
                int counter = 0;
                foreach (Media media in medias)
                {
                    ActivaMedia activaMedia = new ActivaMedia();
                    AdMetaToMedia(activaMedia, media);
                    activaMedia.assetName = media.mediaName;
                    activaMedia.midDescription = media.description;
                    activaMedia.assetDate = media.creationDate.ToString("dd/MM/yyyy");
                    activaMedia.unique_ID = media.mediaID;
                    activaMedia.videoURL = media.url;

                    if (!string.IsNullOrEmpty(activaMedia.videoURL))
                    {
                        activaMedia.videoHD = IsHD(activaMedia.videoURL);
                    }
                    if (!string.IsNullOrEmpty(media.duration))
                    {
                        activaMedia.assetDuration = int.Parse(media.duration);
                    }
                    if (!string.IsNullOrEmpty(media.picURL))
                    {
                        activaMedia.assetSmallThumbnail = GetSizedImage(media.picURL, "160X90"); //old "138X90"
                        activaMedia.assetMedThumbnail = GetSizedImage(media.picURL, "144X108"); //old "143X105"
                        activaMedia.assetBigThumbnail = GetSizedImage(media.picURL, "400X225"); //old 824X460
                    }

                    if (!string.IsNullOrEmpty(media.fileID) && media.fileID != "0")
                    {

                        filesArr[counter] = int.Parse(media.fileID);
                        string breakPoints = GetBreakPoints(int.Parse(media.fileID), ref activaMedia.preProvider, ref activaMedia.postProvider, ref activaMedia.cueProvider);
                        if (!string.IsNullOrEmpty(breakPoints))
                        {
                            int breakCount = 1;
                            string[] breakArr = breakPoints.Split(',');
                            foreach (string breakPoint in breakArr)
                            {
                                if (activaMedia.cuePoints == null)
                                {
                                    activaMedia.cuePoints = new List<CuePoints>();
                                }
                                CuePoints point = new CuePoints();
                                point.id = breakCount.ToString();
                                int tempPoint = 0;
                                Int32.TryParse(breakPoint, out tempPoint);
                                point.timeStamp = tempPoint;
                                activaMedia.cuePoints.Add(point);
                                breakCount++;
                            }
                        }
                        filesDict.Add(media.fileID, activaMedia);
                        counter++;
                        //Dictionary<int, TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.MediaFileItemPricesContainer> prices = ConditionalAccessService.Instance.GetItemsPrice(filesArr, false);
                    }
                    else
                    {
                        if (retVal.content == null)
                        {
                            retVal.content = new List<ActivaMedia>();
                        }
                        retVal.content.Add(activaMedia);
                    }
                }
                MediaFilePPVModule[] modules = new ApiPricingService(groupID, platform).GetPPVModuleListForMediaFiles(filesArr, string.Empty, string.Empty, string.Empty);
                if (modules != null && modules.Length > 0)
                {
                    foreach (MediaFilePPVModule module in modules)
                    {
                        if (module != null && module.m_oPPVModules != null && module.m_oPPVModules.Length > 0)
                        {
                            PPVModule ppvModule = module.m_oPPVModules[0];
                            if (filesDict.ContainsKey(module.m_nMediaFileID.ToString()))
                            {
                                filesDict[module.m_nMediaFileID.ToString()].ppvModuleName = module.m_oPPVModules[0].m_sDescription[0].m_sValue;
                                if (retVal.content == null)
                                {
                                    retVal.content = new List<ActivaMedia>();
                                }
                                retVal.content.Add(filesDict[module.m_nMediaFileID.ToString()]);
                            }
                        }
                        else
                        {
                            if (module != null && filesDict.ContainsKey(module.m_nMediaFileID.ToString()))
                            {
                                if (retVal.content == null)
                                {
                                    retVal.content = new List<ActivaMedia>();
                                }
                                retVal.content.Add(filesDict[module.m_nMediaFileID.ToString()]);
                            }
                        }
                    }

                }
            }
            return retVal;
        }

        private ActivaCategory ParsePageToActivaCategory(PageContext page)
        {
            ActivaCategory retVal = new ActivaCategory();
            if (page != null)
            {
                if (page.MainGalleries != null)
                {
                   
                    foreach (PageGallery pg in page.MainGalleries)
                    {
                        if (retVal.content == null)
                        {
                            retVal.content = new List<ActivaInnerCategory>();
                        }
                        string tvmUser = string.Empty;
                        string tvmPass = string.Empty;
                        ActivaInnerCategory innerRetVal = new ActivaInnerCategory();
                        innerRetVal.categoryName = pg.GroupTitle;
                        innerRetVal.categoryID = pg.GalleryID.ToString();
                        if (pg.GalleryItems != null)
                        {
                            string[] picsArr = new string[pg.GalleryItems.Count];
                            StringBuilder sb = new StringBuilder();
                            int picsCount = 0;
                            List<ActivaCategoryChannel> picGalleries = new List<ActivaCategoryChannel>();
                            foreach (GalleryItem gi in pg.GalleryItems)
                            {
                                if (string.IsNullOrEmpty(tvmPass))
                                {
                                    if (!string.IsNullOrEmpty(gi.TVMPass))
                                    {
                                        tvmPass = gi.TVMPass;
                                    }
                                }
                                if (string.IsNullOrEmpty(tvmUser))
                                {
                                    if (!string.IsNullOrEmpty(gi.TVMUser))
                                    {
                                        tvmUser = gi.TVMUser;
                                    }
                                }
                                if (innerRetVal.categoryItems == null)
                                {
                                    innerRetVal.categoryItems = new List<ActivaCategoryChannel>();
                                }
                                
                                ActivaCategoryChannel retChannel = new ActivaCategoryChannel();
                                retChannel.SectionID = gi.TVMChannelID.ToString();
                                retChannel.premium = gi.BooleanParam;
                                retChannel.SectionTitle = gi.Title;
                                if (!string.IsNullOrEmpty(gi.MainPic.ToString()) && gi.MainPic != 0)
                                {
                                    picsArr[picsCount] = gi.MainPic.ToString();
                                    if (!string.IsNullOrEmpty(sb.ToString()))
                                    {
                                        sb.Append(",");
                                    }
                                    sb.Append(gi.MainPic.ToString());
                                    retChannel.smallThumbnail = gi.MainPic.ToString();
                                    picGalleries.Add(retChannel);
                                    picsCount++;
                                }
                                else
                                {
                                    innerRetVal.categoryItems.Add(retChannel);
                                }
                            }
                            SerializableDictionary<string, string> picsDict = new PicLoader(picsArr, "120X90", tvmUser, tvmPass) { PicsIDStr = sb.ToString(), PageSize = pg.GalleryItems.Count }.Execute();
                            foreach (ActivaCategoryChannel gallItem in picGalleries)
                            {
                                if (picsDict.ContainsKey(gallItem.smallThumbnail))
                                {
                                    gallItem.smallThumbnail = picsDict[gallItem.smallThumbnail];
                                    innerRetVal.categoryItems.Add(gallItem);
                                }
                            }
                        }
                        retVal.content.Add(innerRetVal);
                    }
                }
            }
            return retVal;
        }

        private ActivaCategory ParseCatToActivaCat(Category cat)
        {
            ActivaCategory retVal = new ActivaCategory();
            if (cat != null)
            {
                if (cat.innerCategories != null)
                {
                    foreach (Category innerCat in cat.innerCategories)
                    {
                        if (retVal.content == null)
                        {
                            retVal.content = new List<ActivaInnerCategory>();
                        }
                        ActivaInnerCategory innerRetVal = new ActivaInnerCategory();
                        innerRetVal.categoryName = innerCat.title;
                        if (innerCat.channels != null)
                        {
                            foreach (Channel channel in innerCat.channels)
                            {
                                if (innerRetVal.categoryItems == null)
                                {
                                    innerRetVal.categoryItems = new List<ActivaCategoryChannel>();
                                }
                                ActivaCategoryChannel retChannel = new ActivaCategoryChannel();
                                retChannel.SectionID = channel.channelID.ToString();
                                retChannel.SectionTitle = channel.title;
                                innerRetVal.categoryItems.Add(retChannel);
                            }
                        }
                        retVal.content.Add(innerRetVal);
                    }
                }
            }
            return retVal;
        }

        private string GetBreakPoints(int fileID, ref string preProvider, ref string postProvider, ref string breakProvider)
        {
            string retVal = string.Empty;
            ConnectionManager connMngr = new ConnectionManager(121, PlatformType.STB, false);
            DataSetSelectQuery selectQuery = new DataSetSelectQuery(connMngr.GetTvinciConnectionString());
            selectQuery += " select COMMERCIAL_BREAK_POINTS, COMMERCIAL_TYPE_BREAK_ID, COMMERCIAL_TYPE_POST_ID, COMMERCIAL_TYPE_PRE_ID from media_files where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", fileID);
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    retVal = selectQuery.Table("query").DefaultView[0].Row["COMMERCIAL_BREAK_POINTS"].ToString();
                    if (!string.IsNullOrEmpty(selectQuery.Table("query").DefaultView[0].Row["Commercial_Type_Post_ID"].ToString()))
                    {
                        int postID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["Commercial_Type_Post_ID"].ToString());
                        int preID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["Commercial_Type_Pre_ID"].ToString());
                        int breakID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["Commercial_Type_Break_ID"].ToString());
                        if (postID > 0)
                        {
                            postProvider = "EyeWonder";
                        }
                        if (preID > 0)
                        {
                            preProvider = "EyeWonder";
                        }
                        if (breakID > 0)
                        {
                            breakProvider = "EyeWonder";
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return retVal;
        }

        private bool IsHD(string videoURL)
        {
            bool retVal = false;
            if (videoURL.Contains("16_9HD"))
            {
                retVal = true;
            }
            return retVal;
        }

        private void GetWSUserPass(int groupID, ref string wsUser, ref string wsPass)
        {
            if (groupID == 122)
            {
                wsUser = "Abertis-regular";
                wsPass = "Abertis-regular";
            }
            else if (groupID == 123)
            {
                wsUser = "Abertis-Fictivic";
                wsPass = "Abertis-Fictivic";
            }
            else if (groupID == 124)
            {
                wsUser = "Abertis-WL2";
                wsPass = "Abertis-WL2";
            }
        }

        private string GetSizedImage(string fullPic,string newSize)
        {
            return fullPic.Replace("full", newSize);
        }

        public class ActivaCategory
        {
            public string status;
            public List<ActivaInnerCategory> content;
        }

        public class ActivaInnerCategory
        {
            public string categoryName;
            public string categoryID;
            public bool premium;
            public List<ActivaCategoryChannel> categoryItems;
        }

        public class ActivaCategoryChannel
        {
            public string SectionTitle;
            public string smallThumbnail;
            public string SectionID;
            public bool premium;
        }

        public class ActivaGalleryItem
        {
            public string functionName;
            public string smallThumbnail;
            public string functionID;
        }

        public class ActivaGallery
        {
            public string status;
            public List<ActivaGalleryItem> content;
        }

        public class ActivaChannel
        {
            public string status;
            public string sectionTitle;
            public string test;
            public long itemsCount;
            public List<ActivaMedia> content;
        }

        public class ActivaMedia
        {
            public string assetName;
            public string assetDate;
            public string shortDescription;
            public string midDescription;
            public string assetSmallThumbnail;
            public string assetMedThumbnail;
            public string assetBigThumbnail;
            public string ppvModuleName;
            public int assetDuration;
            public bool aspectRatio16_9;
            public bool premium;
            public string channelName;
            public string unique_ID;
            public string videoURL;
            public string assetHour;
            public string tvParental;
            public string preProvider;
            public string postProvider;
            public string cueProvider;
            public bool videoHD;
            public string genre;
            public List<CuePoints> cuePoints;
        }

        public class ActivaFile
        {
            public string fileType;
            public string ppvModuleName;
            public string videoURL;
            public string duration;
            public string fileID;
        }

        public class AutoCompleteList
        {
            public List<AutoCompleteObj> content;
        }

        public class AutoCompleteObj
        {
            public string choice;
        }

        public class CuePoints
        {
            public string id;
            public int timeStamp;
        }
       
    }
}
