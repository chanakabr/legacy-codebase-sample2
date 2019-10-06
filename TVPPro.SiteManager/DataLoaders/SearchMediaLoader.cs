using System;
using System.Collections.Generic;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.Search;
using TVPPro.Configuration.Media;
using TVPPro.Configuration.Technical;
using TVPPro.SiteManager.Context;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.Helper;
using System.Data;
using TVPPro.Configuration.Site;
using System.Configuration;
using TVPPro.SiteManager.Manager;
using Core.Catalog.Request;
using Core.Catalog.Response;
using ApiObjects;
using ApiObjects.Response;
using TVPPro.SiteManager.Services;
using KLogMonitor;
using System.Reflection;
using ConfigurationManager;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class SearchMediaLoader : TVMAdapter<dsItemInfo>
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private TVPPro.SiteManager.CatalogLoaders.SearchMediaLoader m_oCatalogSearchLoader;
        

        #region Enum
        public enum eOrderDirection
        {
            Asc,
            Desc
        }

        public enum eCutType
        {
            Or,
            And
        }
        #endregion

        #region Members
        protected Dictionary<string, string> m_dictTags = null;
        #endregion Members

        #region Public Properties
        public bool WithInfo
        {
            get
            {
                return Parameters.GetParameter<bool>(eParameterType.Retrieve, "WithInfo", true);
            }
            set
            {
                Parameters.SetParameter<bool>(eParameterType.Retrieve, "WithInfo", value);
            }
        }

        public bool IsPosterPic
        {
            get
            {
                return Parameters.GetParameter<bool>(eParameterType.Retrieve, "IsPosterPic", false);
            }
            set
            {
                Parameters.SetParameter<bool>(eParameterType.Retrieve, "IsPosterPic", value);
            }
        }

        public string PictureSize
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "PictureSize", null);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "PictureSize", value);
            }
        }

        public string SiteGuid
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "SiteGuid", null);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "SiteGuid", value);
            }
        }

        public Enums.eOrderBy OrderBy
        {
            get
            {
                return Parameters.GetParameter<Enums.eOrderBy>(eParameterType.Retrieve, "Order", Enums.eOrderBy.None);
            }
            set
            {
                Parameters.SetParameter<Enums.eOrderBy>(eParameterType.Retrieve, "Order", value);
            }
        }

        public string OrderByMeta
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "OrderByMeta", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "OrderByMeta", value);
            }
        }

        public eOrderDirection OrderDirection
        {
            get
            {
                return Parameters.GetParameter<eOrderDirection>(eParameterType.Retrieve, "OrderDirection", eOrderDirection.Asc);
            }
            set
            {
                Parameters.SetParameter<eOrderDirection>(eParameterType.Retrieve, "OrderDirection", value);
            }
        }

        public eCutType CutType
        {
            get
            {
                return Parameters.GetParameter<eCutType>(eParameterType.Retrieve, "CutType", eCutType.Or);
            }
            set
            {
                Parameters.SetParameter<eCutType>(eParameterType.Retrieve, "CutType", value);
            }
        }

        public int? MediaType
        {
            get
            {
                return Parameters.GetParameter<int?>(eParameterType.Retrieve, "MediaType", null);
            }
            set
            {
                Parameters.SetParameter<int?>(eParameterType.Retrieve, "MediaType", value);
            }
        }

        /// <summary>
        /// A unique token which gets from tags search querystring: "{TagType}={TagValue}|{TagType}={TagValue}| ..."
        /// </summary>
        public string SearchTokenSignature
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "SearchTokenSignature", null);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "SearchTokenSignature", value);
            }
        }

        public string Name
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "Name", null);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "Name", value);
            }
        }

        public bool ExactSearch
        {
            get
            {
                return Parameters.GetParameter<bool>(eParameterType.Retrieve, "ExactSearch", false);
            }
            set
            {
                Parameters.SetParameter<bool>(eParameterType.Retrieve, "ExactSearch", value);
            }
        }

        public bool SideSearch
        {
            get
            {
                return Parameters.GetParameter<bool>(eParameterType.Retrieve, "SideSearch", false);
            }
            set
            {
                Parameters.SetParameter<bool>(eParameterType.Retrieve, "SideSearch", value);
            }
        }

        public string MetaValues // To cache dictMetas, should include dictMetas values with ';' seperator
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "MetaValues", null);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "MetaValues", value);
            }
        }

        public string UseFinalEndDate
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "UseFinalEndDate", "false");
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "UseFinalEndDate", value);
            }
        }

        public List<string> ExcludeMediaIDs
        {
            get
            {
                return Parameters.GetParameter<List<string>>(eParameterType.Filter, "ExcludeMediaIDs", new List<string>());
            }
            set
            {
                Parameters.SetParameter<List<string>>(eParameterType.Filter, "ExcludeMediaIDs", value);
            }
        }

        public Dictionary<string, string> dictMetas
        {
            get;
            set;
        }

        protected string TvmUser
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "TvmUser", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "TvmUser", value);
            }

        }
        protected string TvmPass
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "TvmPass", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "TvmPass", value);
            }

        }
        public string DeviceUDID
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Filter, "DeviceUDID", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Filter, "DeviceUDID", value);
            }
        }

        public Enums.ePlatform Platform
        {
            get
            {
                return Parameters.GetParameter<Enums.ePlatform>(eParameterType.Retrieve, "Platform", Enums.ePlatform.Unknown);
            }
            set
            {
                Parameters.SetParameter<Enums.ePlatform>(eParameterType.Retrieve, "Platform", value);
            }
        }

        public string Language
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "Language", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "Language", value);
            }
        }

        public int UserTypeID
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Filter, "UserTypeID", 0);
            }
            set
            {
                Parameters.SetParameter<int>(eParameterType.Filter, "UserTypeID", value);
            }
        }
        #endregion

        #region C'tor
        public SearchMediaLoader()
        {
        }

        public SearchMediaLoader(string TVMUser, string TVMPass)
        {
            TvmUser = TVMUser;
            TvmPass = TVMPass;
        }

        public SearchMediaLoader(Dictionary<string, string> Tags)
        {
            if (Tags != null)
                m_dictTags = Tags;
        }

        public SearchMediaLoader(string TVMUser, string TVMPass, Dictionary<string, string> Tags)
        {
            if (Tags != null)
                m_dictTags = Tags;

            TvmUser = TVMUser;
            TvmPass = TVMPass;
        }

        public string GetFutureStartDate
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "GetFutureStartDate", "true");
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "GetFutureStartDate", value);
            }
        }
        #endregion

        #region Overriden Methods

        public override object BCExecute(eExecuteBehaivor behaivor)
        {
            return Execute();
        }

        public override dsItemInfo Execute()
        {
            if (ApplicationConfiguration.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                m_oCatalogSearchLoader = new TVPPro.SiteManager.CatalogLoaders.SearchMediaLoader(TvmUser, SiteHelper.GetClientIP(), PageSize, PageIndex, PictureSize, Name)
                {
                    And = CutType == eCutType.And ? true : false,
                    Exact = ExactSearch,
                    Metas = CatalogHelper.GetCatalogMetasTags(dictMetas),
                    Tags = CatalogHelper.GetCatalogMetasTags(m_dictTags),
                    MediaTypes = MediaType.HasValue ? new List<int>() { MediaType.Value } : null,
                    Description = Name,
                    Name = Name,
                    Language = int.Parse(TechnicalManager.GetLanguageID().ToString()),
                    OnlyActiveMedia = true,
                    Platform = Platform.ToString(),
                    UseFinalDate = bool.Parse(UseFinalEndDate),
                    UseStartDate = bool.Parse(GetFutureStartDate),
                    DeviceId = DeviceUDID,
                    UserTypeID = this.UserTypeID,
                    SiteGuid = SiteGuid
                };
                if (OrderBy != Enums.eOrderBy.None)
                {
                    m_oCatalogSearchLoader.OrderBy = CatalogHelper.GetCatalogOrderBy(OrderBy);
                    m_oCatalogSearchLoader.OrderDir = CatalogHelper.GetCatalogOrderDirection(OrderDirection);
                    m_oCatalogSearchLoader.OrderMetaMame = OrderByMeta;
                }
                return m_oCatalogSearchLoader.Execute() as dsItemInfo;
            }
            else
            {
                return base.Execute();
            }
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            SearchProtocol protocol = new SearchProtocol();

            protocol.root.request.search_data.channel.start_index = (PageIndex * PageSize).ToString();
            protocol.root.request.search_data.channel.media_count = PageSize.ToString();
            protocol.root.flashvars.file_format = TechnicalConfiguration.Instance.Data.TVM.FlashVars.FileFormat;
            protocol.root.flashvars.file_quality = Tvinci.Data.TVMDataLoader.Protocols.Search.file_quality.high;
            protocol.root.flashvars.use_final_end_date = UseFinalEndDate;
            protocol.root.flashvars.use_start_date = GetFutureStartDate;


            protocol.root.flashvars.player_un = TvmUser;
            protocol.root.flashvars.player_pass = TvmPass;

            //if (string.IsNullOrEmpty(PictureSize))
            //    throw new Exception("Picture size must be given");
            protocol.root.flashvars.pic_size1 = PictureSize;

            if (IsPosterPic)
            {
                protocol.root.flashvars.pic_size1_format = "POSTER";
                protocol.root.flashvars.pic_size1_quality = "HIGH";
            }

            protocol.root.flashvars.device_udid = DeviceUDID;
            protocol.root.flashvars.platform = (int)Platform;

            //Handle response info stuct
            protocol.root.request.@params.with_info = WithInfo.ToString();
            if (WithInfo)
            {
                protocol.root.request.@params.info_struct.statistics = true;
                //protocol.root.request.@params.info_struct.personal = true;
                protocol.root.request.@params.info_struct.type.MakeSchemaCompliant();
                protocol.root.request.@params.info_struct.name.MakeSchemaCompliant();
                protocol.root.request.@params.info_struct.description.MakeSchemaCompliant();

                string[] MediaInfoStructMetaNames = MediaConfiguration.Instance.Data.TVM.GalleryMediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
                string[] MediaInfoStructTagNames = MediaConfiguration.Instance.Data.TVM.GalleryMediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });

                foreach (string meta in MediaInfoStructMetaNames)
                {
                    protocol.root.request.@params.info_struct.metaCollection.Add(new meta { name = meta });
                }

                foreach (string tagName in MediaInfoStructTagNames)
                {
                    protocol.root.request.@params.info_struct.tags.Add(new tag_type { name = tagName });
                }
            }

            //Handle request cut values
            protocol.root.request.search_data.cut_values.exact = ExactSearch;
            protocol.root.request.search_data.cut_with = CutType.ToString().ToLower();

            if (MediaType.HasValue)
                protocol.root.request.search_data.cut_values.type.value = (MediaType.Value).ToString();

            if (!SideSearch && m_dictTags == null && dictMetas == null)
            {
                protocol.root.request.search_data.cut_values.name.value = Name;

                string[] MetaNames = MediaConfiguration.Instance.Data.TVM.SearchValues.Metadata.ToString().Split(new Char[] { ';' });
                string[] TagNames = MediaConfiguration.Instance.Data.TVM.SearchValues.Tags.ToString().Split(new Char[] { ';' });

                foreach (string meta in MetaNames)
                {
                    protocol.root.request.search_data.cut_values.metaCollection.Add(new cut_valuesmeta { name = meta, value = Name });
                }

                foreach (string tagName in TagNames)
                {
                    protocol.root.request.search_data.cut_values.tags.Add(new cut_valuestagstag_type { name = tagName, value = Name });
                }
            }
            else if (m_dictTags != null && m_dictTags.Count > 0)
            {
                if (!string.IsNullOrEmpty(Name))
                    protocol.root.request.search_data.cut_values.name.value = Name;

                foreach (string key in m_dictTags.Keys)
                {
                    if (!string.IsNullOrEmpty(m_dictTags[key]))
                    {
                        protocol.root.request.search_data.cut_values.tags.Add(new cut_valuestagstag_type { name = key, value = m_dictTags[key] });
                    }
                }
            }
            else if (dictMetas != null && dictMetas.Count > 0) // Hanble with search metas with multi values
            {
                if (!string.IsNullOrEmpty(Name))
                    protocol.root.request.search_data.cut_values.name.value = Name;

                foreach (string key in dictMetas.Keys)
                {
                    if (!string.IsNullOrEmpty(dictMetas[key]))
                    {
                        string[] CutMetaValues = dictMetas[key].Split(new Char[] { ';' });
                        foreach (string MetaItem in CutMetaValues)
                        {
                            protocol.root.request.search_data.cut_values.metaCollection.Add(new cut_valuesmeta { name = key, value = MetaItem });
                        }
                    }
                }
            }

            switch (OrderBy)
            {
                case Enums.eOrderBy.ABC:
                    protocol.root.request.search_data.order_values.name.order_dir = OrderDirection.ToString().ToLower();
                    break;
                case Enums.eOrderBy.Added:
                    protocol.root.request.search_data.order_values.date.order_dir = eOrderDirection.Desc.ToString().ToLower();
                    break;
                case Enums.eOrderBy.Views:
                    protocol.root.request.search_data.order_values.views.order_dir = OrderDirection.ToString().ToLower();
                    break;
                case Enums.eOrderBy.Rating:
                    protocol.root.request.search_data.order_values.rate.order_dir = OrderDirection.ToString().ToLower();
                    break;
                case Enums.eOrderBy.None:
                    break;
                case Enums.eOrderBy.Meta:
                    protocol.root.request.search_data.order_values.meta.name = OrderByMeta.ToString();
                    protocol.root.request.search_data.order_values.meta.order_dir = eOrderDirection.Asc.ToString();
                    break;
                default:
                    throw new Exception("Unknown order by value");
            }

            return protocol;
        }

        protected override dsItemInfo PreCacheHandling(object retrievedData)
        {
            dsItemInfo ret = new dsItemInfo();

            SearchProtocol result = retrievedData as SearchProtocol;

            foreach (media resMedia in result.response.channel.mediaCollection)
            {
                dsItemInfo.ItemRow newRow = ret.Item.NewItemRow();

                newRow.ID = resMedia.id;
                newRow.Title = resMedia.title;

                if (resMedia.rating != null && resMedia.rating.avg != null) newRow.Rate = double.Parse(resMedia.rating.avg);

                newRow.ImageLink = resMedia.pic_size1;
                newRow.MediaType = resMedia.type.value;
                newRow.MediaTypeID = resMedia.type.id;
                newRow.FileFormat = resMedia.file_format;
                newRow.FileID = resMedia.file_id;
                newRow.ViewCounter = Convert.ToInt32(resMedia.views.count);
                newRow.DescriptionShort = resMedia.description.value;
                newRow.Rate = Convert.ToDouble(resMedia.rating.avg);
                newRow.Duration = resMedia.duration;
                newRow.URL = resMedia.url;
                newRow.Likes = resMedia.like_counter.ToString();

                // add sub file format info
                if (resMedia.inner_medias.Count > 0)
                {
                    newRow.SubFileID = resMedia.inner_medias[0].file_id;
                    newRow.SubFileFormat = resMedia.inner_medias[0].file_format;
                    newRow.SubDuration = resMedia.inner_medias[0].duration;
                    newRow.SubURL = resMedia.inner_medias[0].url;

                    foreach (inner_mediasmedia file in resMedia.inner_medias)
                    {
                        dsItemInfo.FilesRow rowFile = ret.Files.NewFilesRow();
                        rowFile.ID = resMedia.id;
                        rowFile.FileID = file.file_id;
                        rowFile.URL = file.url;
                        rowFile.Duration = file.duration;
                        rowFile.Format = file.orig_file_format;

                        ret.Files.AddFilesRow(rowFile);
                    }
                }


                //Add create date.
                try
                {
                    // For backward compatibility
                    if (GetFutureStartDate.ToLower().Equals("true"))
                    {
                        string[] date = resMedia.date.Split('/');
                        newRow.AddedDate = new DateTime(int.Parse(date[2]), int.Parse(date[1]), int.Parse(date[0]));
                    }
                    else
                    {
                        DateTime addedDate;
                        if (DateTime.TryParseExact(resMedia.date, new string[] { "dd/MM/yyyy HH:mm:ss", "dd/MM/yyyy" }, null, System.Globalization.DateTimeStyles.None, out addedDate))
                        {
                            newRow.AddedDate = addedDate;
                        }
                        else
                        {
                            newRow.AddedDate = DateTime.UtcNow;
                        }
                        //newRow.AddedDate = DateTime.ParseExact(resMedia.date, "dd/MM/yyyy HH:mm:ss", null);
                        //newRow.AddedDate = DateTime.ParseExact(resMedia.date, "dd/MM/yyyy", null);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("", ex);
                }

                // Add external IDs
                foreach (System.Reflection.PropertyInfo property in resMedia.external_ids.GetType().GetProperties())
                {
                    if (property.CanRead)
                    {
                        string sValue = property.GetValue(resMedia.external_ids, null).ToString();
                        if (!string.IsNullOrEmpty(sValue))
                        {
                            // add column if not exist
                            if (!ret.ExtIDs.Columns.Contains(property.Name))
                                ret.ExtIDs.Columns.Add(property.Name);

                            dsItemInfo.ExtIDsRow rowExtID = ret.ExtIDs.NewExtIDsRow();
                            rowExtID[property.Name] = sValue;
                            rowExtID["ID"] = resMedia.id;
                            ret.ExtIDs.AddExtIDsRow(rowExtID);
                        }
                    }
                }

                ret.Item.AddItemRow(newRow);

                DataHelper.CollectMetasInfo(ref ret, resMedia);

                DataHelper.CollectTagsInfo(ref ret, resMedia);
            }

            return ret;
        }

        //public override eCacheMode GetCacheMode()
        //{
        //    return eCacheMode.Application
        //}

        protected override dsItemInfo FormatResults(dsItemInfo originalObject)
        {
            dsItemInfo copyObj = (dsItemInfo)originalObject.Copy();

            if (copyObj.Item.Rows.Count > 0)
            {
                DataRow[] tmpRows = new DataRow[copyObj.Item.Rows.Count];
                copyObj.Item.Rows.CopyTo(tmpRows, 0);

                foreach (string media in ExcludeMediaIDs)
                {
                    foreach (DataRow row in tmpRows)
                    {
                        if (row["ID"].ToString() == media)
                            copyObj.Item.Rows.Remove(row);
                    }
                }
            }

            return copyObj;
        }

        public override bool ShouldExtractItemsCountInSource
        {
            get
            {
                return true;
            }
        }



        protected override bool TryGetItemsCountInSource(object retrievedData, out long count)
        {
            count = 0;

            if (retrievedData == null)
                return false;

            SearchProtocol result = retrievedData as SearchProtocol;

            if (result.response.channel.media_count == null)
                return false;

            count = long.Parse(result.response.channel.media_count);

            return true;
        }

        public override bool TryGetItemsCount(out long count)
        {
            if (ApplicationConfiguration.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                return m_oCatalogSearchLoader.TryGetItemsCount(out count);
            }
            else
            {
                count = base.GetItemsInSource();
                return true;
            }
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{092F417F-A6F2-4558-AB90-8CD519DE5F1B}"); }
        }
        #endregion
    }
}
