using System;
using System.Data;
using System.Globalization;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.SingleMedia;
using TVPPro.Configuration.Media;
using TVPPro.Configuration.Technical;
using TVPPro.SiteManager.Context;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.Helper;
using System.Configuration;
using TVPPro.SiteManager.CatalogLoaders;
using System.Collections.Generic;
using TVPPro.SiteManager.Manager;
using TVPPro.SiteManager.Services;
using Phx.Lib.Log;
using System.Reflection;
using Phx.Lib.Appconfig;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class TVMMultiMediaLoader : TVMAdapter<dsItemInfo>
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        #region Load properties

        /// <summary>
        /// A unique token which gets Medias Ids stirng : mediaID|MediaID"
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

        public string[] MediaArrayID
        {
            get
            {
                return Parameters.GetParameter<string[]>(eParameterType.Retrieve, "MediaArrayID", null);
            }
            set
            {
                Parameters.SetParameter<string[]>(eParameterType.Retrieve, "MediaArrayID", value);
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

        public int MediaType
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Retrieve, "MediaType", 0);
            }
            set
            {
                Parameters.SetParameter<int>(eParameterType.Retrieve, "MediaType", value);
            }
        }

        public string[] TagsList
        {
            get
            {
                return Parameters.GetParameter<string[]>(eParameterType.Retrieve, "TagsList", new string[] { });
            }
            set
            {
                Parameters.SetParameter<string[]>(eParameterType.Retrieve, "TagsList", value);
            }
        }

        public string[] MetasList
        {
            get
            {
                return Parameters.GetParameter<string[]>(eParameterType.Retrieve, "MetasList", new string[] { });
            }
            set
            {
                Parameters.SetParameter<string[]>(eParameterType.Retrieve, "MetasList", value);
            }
        }

        public Enums.eOrderBy OrderBy
        {
            get
            {
                return Parameters.GetParameter<Enums.eOrderBy>(eParameterType.Filter, "OrderBy", Enums.eOrderBy.Added);
            }
            set
            {
                Parameters.SetParameter<Enums.eOrderBy>(eParameterType.Filter, "OrderBy", value);
            }
        }

        public string OrderByMeta
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Filter, "OrderByMeta", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Filter, "OrderByMeta", value);
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
        #endregion

        #region Constractor
        public TVMMultiMediaLoader(string[] MediaList, string PictureSize, int MediaTypeId)
        {
            PicSize = PictureSize;
            MediaType = MediaTypeId;
            MediaArrayID = MediaList;
        }

        public TVMMultiMediaLoader(string tvmUn, string tvmPass, string[] MediaList, string PictureSize, int MediaTypeId)
        {
            PicSize = PictureSize;
            MediaType = MediaTypeId;
            MediaArrayID = MediaList;
            TvmUser = tvmUn;
            TvmPass = tvmPass;
        }
        #endregion Constractor

        public override object BCExecute(eExecuteBehaivor behaivor)
        {
            return Execute();
        }

        public override dsItemInfo Execute()
        {
            
            if (ApplicationConfiguration.Current.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                List<int> mediaIDs = new List<int>();
                foreach (var id in MediaArrayID)
                {
                    mediaIDs.Add(int.Parse(id));
                }
                return new MediaLoader(mediaIDs, TvmUser, SiteHelper.GetClientIP(), PicSize)
                 {
                     Language = int.Parse(TechnicalManager.GetLanguageID().ToString()),
                     OnlyActiveMedia = true,
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
            SingleMedia result = new SingleMedia();
            if (MediaArrayID != null)
            {
                foreach (string media in MediaArrayID)
                {
                    result.root.request.mediaCollection.Add(new Tvinci.Data.TVMDataLoader.Protocols.SingleMedia.media() { id = media });
                }

                result.root.request.@params.with_info = "true";

                result.root.flashvars.player_un = TvmUser;
                result.root.flashvars.player_pass = TvmPass;

                // views / rating
                result.root.request.@params.info_struct.statistics = true;
                // Type
                result.root.request.@params.info_struct.type.MakeSchemaCompliant();
                result.root.flashvars.pic_size1 = PicSize;

                //Set the response info_struct
                result.root.request.@params.info_struct.statistics = true;
                result.root.request.@params.info_struct.name.MakeSchemaCompliant();
                result.root.request.@params.info_struct.description.MakeSchemaCompliant();
                result.root.request.@params.info_struct.type.MakeSchemaCompliant();

                //result.root.request.@params.info_struct.metaCollection.Add(new meta() { name = "Season" });
                //result.root.request.@params.info_struct.metaCollection.Add(new meta() { name = "Episode Number" });
                ////result.root.request.@params.info_struct.metaCollection.Add(new meta() { name = "Episode Name" });
                //// META1_STR_NAME
                //result.root.request.@params.info_struct.tags.Add(new tag_type { name = "Series Name" });

                string[] MetaNames = MediaConfiguration.Instance.Data.TVM.GalleryMediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
                string[] TagNames = MediaConfiguration.Instance.Data.TVM.GalleryMediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });

                foreach (string meta in MetaNames)
                {
                    result.root.request.@params.info_struct.metaCollection.Add(new meta { name = meta });
                }

                foreach (string tagName in TagNames)
                {
                    result.root.request.@params.info_struct.tags.Add(new tag_type { name = tagName });
                }

                if (IsPosterPic)
                {
                    result.root.flashvars.pic_size1_format = "POSTER";
                    result.root.flashvars.pic_size1_quality = "HIGH";
                }

                result.root.flashvars.file_quality = file_quality.high;
                result.root.flashvars.file_format = this.FlashVarsFileFormat;

            }
            return result;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{DBF9ED38-FDB8-40e1-B45C-F32AC1C833CC}"); }
        }

        protected override dsItemInfo PreCacheHandling(object retrievedData)
        {
            SingleMedia data = retrievedData as SingleMedia;

            if (data == null)
            {
                throw new Exception("");
            }

            dsItemInfo result = new dsItemInfo();

            foreach (responsemedia newMedia in data.response.mediaCollection)
            {
                if (newMedia.type.id != MediaType.ToString() && MediaType != 1 && MediaType != 0)
                    continue;

                dsItemInfo.ItemRow row = result.Item.NewItemRow();
                row.ID = newMedia.id;
                row.Title = newMedia.title;
                row.ImageLink = newMedia.pic_size1;
                row.ViewCounter = int.Parse(newMedia.views.count);
                row.Rate = Math.Round(Convert.ToDouble(newMedia.rating.avg));
                row.MediaTypeID = newMedia.type.id;
                row.MediaType = newMedia.type.value;
                row.FileID = newMedia.file_id;
                row.DescriptionShort = !string.IsNullOrEmpty(newMedia.description.value) ? newMedia.description.value : string.Empty;
                row.URL = newMedia.url;
                row.LastWatchedDeviceName = newMedia.last_watched_device_name;
                row.Likes = newMedia.like_counter.ToString();

                if (!string.IsNullOrEmpty(newMedia.date))
                {
                    //XXX: See why???
                    //CultureInfo culture = new CultureInfo("en-US");
                    //culture.DateTimeFormat.ShortDatePattern = @"dd/MM/yyyy";
                    //culture.DateTimeFormat.LongDatePattern = "";
                    //row.CreationDate = Convert.ToDateTime(newMedia.date, culture);

                    string[] date = newMedia.date.Split('/');
                    row.AddedDate = new DateTime(int.Parse(date[2]), int.Parse(date[1]), int.Parse(date[0]));
                }

                // add sub file format info
                if (newMedia.inner_medias.Count > 0)
                {
                    row.SubFileID = newMedia.inner_medias[0].file_id;
                    row.SubFileFormat = newMedia.inner_medias[0].file_format;
                    row.SubDuration = newMedia.inner_medias[0].duration;
                    row.SubURL = newMedia.inner_medias[0].url;

                    foreach (inner_mediasmedia file in newMedia.inner_medias)
                    {
                        dsItemInfo.FilesRow rowFile = result.Files.NewFilesRow();
                        rowFile.ID = newMedia.id;
                        rowFile.FileID = file.file_id;
                        rowFile.URL = file.url;
                        rowFile.Duration = file.duration;
                        rowFile.Format = file.orig_file_format;

                        result.Files.AddFilesRow(rowFile);
                    }
                }

                DataHelper.CollectMetasInfo(ref result, newMedia);

                DataHelper.CollectTagsInfo(ref result, newMedia);

                result.Item.AddItemRow(row);
            }

            return result;
        }

        protected override dsItemInfo FormatResults(dsItemInfo originalObject)
        {
            dsItemInfo copyObject = originalObject.Copy() as dsItemInfo;
            if (copyObject.Item.Rows.Count > 0)
            {
                copyObject.Item.DefaultView.RowFilter = "";

                if (!string.IsNullOrEmpty(OrderByMeta))
                {
                    try
                    {
                        copyObject.Metas.DefaultView.Sort = OrderByMeta;

                        DataTable dtMetasSorted = copyObject.Metas.DefaultView.ToTable();
                        copyObject.Metas.Clear();
                        copyObject.Metas.Merge(dtMetasSorted, true);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("", ex);
                    }
                }
                else
                {
                    switch (OrderBy)
                    {
                        case (Enums.eOrderBy.Added):
                            copyObject.Item.DefaultView.Sort = "CreationDate desc";
                            break;
                        case (Enums.eOrderBy.Rating):
                            copyObject.Item.DefaultView.Sort = "Rate desc";
                            break;
                        case (Enums.eOrderBy.Views):
                            copyObject.Item.DefaultView.Sort = "ViewCounter desc";
                            break;
                        default:
                            copyObject.Item.DefaultView.Sort = "Title asc";
                            break;
                    }

                    DataTable dtItemSorted = copyObject.Item.DefaultView.ToTable();
                    copyObject.Item.Clear();
                    copyObject.Item.Merge(dtItemSorted, true);
                }
            }

            return copyObject;
        }
    }
}
