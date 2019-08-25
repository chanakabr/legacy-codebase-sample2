using System;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.SingleMedia;
using TVPPro.Configuration.Media;
using TVPPro.Configuration.Technical;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.Context;
using TVPPro.Configuration.Site;
using System.Configuration;
using Tvinci.Data.Loaders;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.Manager;
using TVPPro.SiteManager.Services;
using KLogMonitor;
using System.Reflection;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class TVMMediaLoader : TVMAdapter<dsItemInfo>
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private FlashLoadersParams m_FlashLoadersParams;

        #region properties

        public string SiteGuid
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "SiteGuid", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "SiteGuid", value);
            }
        }

        public string MediaID
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "MediaID", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "MediaID", value);
            }
        }

        public string PicSize
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "PicSize", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "PicSize", value);
            }
        }

        public string BrandingFileFormat
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "BrandingFileFormat", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "BrandingFileFormat", value);
            }
        }

        public string BrandingPicSize
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "BrandingPicSize", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "BrandingPicSize", value);
            }
        }


        public string RepeatBrandingPicFormat
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "RepeatBrandingPicFormat", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "RepeatBrandingPicFormat", value);
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

        public string FileFormat
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "FileFormat", TechnicalConfiguration.Instance.Data.TVM.FlashVars.FileFormat);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "FileFormat", value);
            }
        }

        public string AdminToken
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "AdmineToken", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "AdmineToken", value);
            }
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

        #endregion

        protected override bool ShouldStoreInCache(LoaderAdapterItem result)
        {
            return (result != null);
        }

        public TVMMediaLoader(string mediaID)
            : this(string.Empty, string.Empty, mediaID)
        {
        }

        public TVMMediaLoader(string tvmUn, string tvmPass, string mediaID)
        {
            this.MediaID = mediaID;
            this.TvmPass = tvmPass;
            this.TvmUser = tvmUn;
        }

        public TVMMediaLoader(string tvmUn, string tvmPass, string mediaID, FlashLoadersParams FlashLoadersParams)
            : this(tvmUn, tvmPass, mediaID)
        {
            this.m_FlashLoadersParams = FlashLoadersParams;
        }
        public override object BCExecute(eExecuteBehaivor behaivor)
        {
            return Execute();
        }

        public override dsItemInfo Execute()
        {
            bool bShouldUseCache;
            if (bool.TryParse(System.Configuration.ConfigurationManager.AppSettings["ShouldUseNewCache"], out bShouldUseCache) && bShouldUseCache)
            {
                return new MediaLoader(int.Parse(MediaID), TvmUser, SiteHelper.GetClientIP(), PicSize)
                {
                    Language = int.Parse(TechnicalManager.GetLanguageID().ToString()),
                    DeviceId = DeviceUDID,
                    OnlyActiveMedia = true,
                    Platform = Platform.ToString(),
                    UseFinalDate = bool.Parse(UseFinalEndDate),
                    UseStartDate = bool.Parse(GetFutureStartDate),
                    UserTypeID = this.UserTypeID,
                    SiteGuid = SiteGuid
                }.Execute() as dsItemInfo;
            }
            else
            {
                return base.Execute();
            }
        }
        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            //if (this.m_FlashLoadersParams != null) return OverrideProtocol();

            SingleMedia result = new SingleMedia();
            result.root.request.mediaCollection.Add(new media() { id = MediaID });
            if (string.IsNullOrEmpty(BrandingFileFormat))
            {
                result.root.flashvars.file_quality = file_quality.high;
                result.root.flashvars.file_format = FileFormat;
            }
            else // for brandig media
            {
                result.root.flashvars.pic_size3_quality = "HIGH";
                result.root.flashvars.pic_size3_format = BrandingFileFormat;
                result.root.flashvars.pic_size3 = BrandingPicSize;
                result.root.flashvars.pic_size2 = "full";
                result.root.flashvars.pic_size2_quality = "HIGH";
                result.root.flashvars.pic_size2_format = RepeatBrandingPicFormat;
            }

            result.root.flashvars.use_start_date = GetFutureStartDate;
            result.root.flashvars.player_un = TvmUser;
            result.root.flashvars.player_pass = TvmPass;
            result.root.flashvars.use_final_end_date = UseFinalEndDate;
            result.root.request.@params.with_info = "true";
            result.root.flashvars.file_quality = file_quality.high;
            result.root.flashvars.file_format = FileFormat;
            result.root.__flashvars.admin_token = AdminToken;
            result.root.__flashvars.user_ip = SiteManager.Helper.SiteHelper.GetClientIP();
            //result.root.flashvars.pic_size1_format = TechnicalConfiguration.Instance.Data.TVM.FlashVars.FileFormat;
            //result.root.flashvars.pic_size1_quality = "HIGH";
            result.root.flashvars.pic_size1 = PicSize;

            result.root.flashvars.device_udid = DeviceUDID;
            result.root.flashvars.platform = (int)Platform;

            //Set the response info_struct
            result.root.request.@params.info_struct.statistics = true;
            result.root.request.@params.info_struct.name.MakeSchemaCompliant();
            result.root.request.@params.info_struct.description.MakeSchemaCompliant();
            result.root.request.@params.info_struct.type.MakeSchemaCompliant();

            string[] MetaNames = MediaConfiguration.Instance.Data.TVM.MediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
            string[] TagNames = MediaConfiguration.Instance.Data.TVM.MediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });

            foreach (string meta in MetaNames)
            {
                result.root.request.@params.info_struct.metaCollection.Add(new meta { name = meta });
            }

            foreach (string tagName in TagNames)
            {
                result.root.request.@params.info_struct.tags.Add(new tag_type { name = tagName });
            }

            return result;
        }

        protected override dsItemInfo PreCacheHandling(object retrievedData)
        {
            SingleMedia data = retrievedData as SingleMedia;

            if (data == null)
            {
                throw new Exception("");
            }

            dsItemInfo result = new dsItemInfo();
            //result.Reset();

            if (data.response.mediaCollection.Count == 1)
            {
                responsemedia mediaInfo = data.response.mediaCollection[0];

                if (!string.IsNullOrEmpty(mediaInfo.id))
                {
                    // Metas DateTable
                    DataHelper.CollectMetasInfo(ref result, mediaInfo);

                    // Tags DataTable
                    DataHelper.CollectTagsInfo(ref result, ref mediaInfo);

                    // Info DataTable
                    dsItemInfo.ItemRow row = result.Item.NewItemRow();
                    row.ID = mediaInfo.id;
                    row.ItemType = mediaInfo.type.value;
                    row.MediaTypeID = mediaInfo.type.id;
                    row.MediaType = mediaInfo.type.value;
                    row.FileFormat = mediaInfo.file_format;
                    row.ViewCounter = Convert.ToInt32(mediaInfo.views.count);
                    row.Name = mediaInfo.title;
                    row.Title = mediaInfo.title;
                    row.Brief = !string.IsNullOrEmpty(mediaInfo.description.value) ? System.Web.HttpUtility.HtmlDecode(mediaInfo.description.value).Replace(@"\", "/") : string.Empty;
                    row.DescriptionShort = !string.IsNullOrEmpty(mediaInfo.description.value) ? mediaInfo.description.value : string.Empty;
                    row.Rate = Convert.ToDouble(mediaInfo.rating.avg);
                    row.FileID = mediaInfo.file_id;
                    row.ImageLink = mediaInfo.pic_size1;
                    row.BrandingSmallImage = mediaInfo.pic_size3;
                    row.Duration = mediaInfo.duration;
                    row.BrandingSpaceHight = mediaInfo.pic_size2_bh;
                    row.BrandingRecurring = mediaInfo.pic_size2_br;
                    row.BrandingBodyImage = mediaInfo.pic_size2;
                    row.URL = mediaInfo.url;
                    row.LastWatchedDeviceName = mediaInfo.last_watched_device_name;
                    row.GeoBlock = mediaInfo.block;
                    row.Likes = mediaInfo.like_counter.ToString();

                    //Add create date.
                    try
                    {
                        // For backward compatability
                        if (GetFutureStartDate.ToLower().Equals("true"))
                        {
                            string[] date = mediaInfo.date.Split('/');
                            row.AddedDate = new DateTime(int.Parse(date[2]), int.Parse(date[1]), int.Parse(date[0]));
                        }
                        else
                        {
                            row.AddedDate = DateTime.ParseExact(mediaInfo.date, "dd/MM/yyyy HH:mm:ss", null);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("", ex);
                    }

                    // add sub file format info
                    if (mediaInfo.inner_medias.Count > 0)
                    {
                        row.SubFileID = mediaInfo.inner_medias[0].file_id;
                        row.SubFileFormat = mediaInfo.inner_medias[0].file_format;
                        row.SubDuration = mediaInfo.inner_medias[0].duration;
                        row.SubURL = mediaInfo.inner_medias[0].url;

                        foreach (inner_mediasmedia file in mediaInfo.inner_medias)
                        {
                            dsItemInfo.FilesRow rowFile = result.Files.NewFilesRow();
                            rowFile.ID = mediaInfo.id;
                            rowFile.FileID = file.file_id;
                            rowFile.URL = file.url;
                            rowFile.Duration = file.duration;
                            rowFile.Format = file.orig_file_format;

                            result.Files.AddFilesRow(rowFile);
                        }
                    }

                    //// Add external IDs
                    foreach (System.Reflection.PropertyInfo property in mediaInfo.external_ids.GetType().GetProperties())
                    {
                        if (property.CanRead)
                        {
                            string sValue = property.GetValue(mediaInfo.external_ids, null).ToString();
                            if (!string.IsNullOrEmpty(sValue))
                            {
                                // add column if not exist
                                if (!result.ExtIDs.Columns.Contains(property.Name))
                                    result.ExtIDs.Columns.Add(property.Name);

                                dsItemInfo.ExtIDsRow rowExtID = result.ExtIDs.NewExtIDsRow();
                                rowExtID[property.Name] = sValue;
                                rowExtID["ID"] = mediaInfo.id;
                                result.ExtIDs.AddExtIDsRow(rowExtID);
                            }
                        }
                    }

                    result.Item.AddItemRow(row);
                }
            }

            return result;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{3B5420B3-16A4-43e2-A873-56282B63CE82}"); }
        }
    }
}
