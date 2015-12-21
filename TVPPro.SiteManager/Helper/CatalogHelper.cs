using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.Configuration.Media;
using TVPPro.SiteManager.DataEntities;
using TVPPro.Configuration.Technical;
using System.Data;
using Tvinci.Data.TVMDataLoader.Protocols.FlashSingleMedia;
using Tvinci.Localization;

namespace TVPPro.SiteManager.Helper
{
    public class CatalogHelper
    {
        public static List<BaseObject> MergeObjListsByOrder(List<int> order, List<BaseObject> list1, List<BaseObject> list2)
        {
            List<BaseObject> retVal = new List<BaseObject>(order.Count);
            if (list2 == null)
                list2 = new List<BaseObject>();
            IEnumerable<BaseObject> lMedias = list1.Union(list2);
            for (int i = 0; i < order.Count; i++)
            {
                retVal.Add(lMedias.Where(media => media != null && media.AssetId == order[i].ToString()).FirstOrDefault());
            }
            return retVal.Where(o => o != null).ToList();
        }

        public static dsItemInfo MediaObjToDsItemInfo(List<BaseObject> medias, string picSize, string fileFormat, string subFileFormat)
        {
            dsItemInfo retVal = new dsItemInfo();

            if (medias != null && medias.Count > 0)
            {
                picSize = picSize == null ? string.Empty : picSize;

                string picUrl = string.Empty;
                foreach (MediaObj media in medias)
                {
                    if (media != null)
                    {
                        dsItemInfo.ItemRow oRow = retVal.Item.NewItemRow();

                        oRow.ID = media.AssetId;
                        oRow.Title = media.m_sName;
                        oRow.Name = media.m_sName;
                        if (!string.IsNullOrEmpty(picSize))
                        {
                            {
                                picUrl = (from pic in media.m_lPicture where pic != null && pic.m_sSize.ToLower() == picSize.ToLower() select pic.m_sURL).FirstOrDefault();
                                if (picUrl != null)
                                    oRow.ImageLink = picUrl;
                            }
                        }
                        oRow.MediaType = media.m_oMediaType.m_sTypeName;
                        oRow.MediaTypeID = media.m_oMediaType.m_nTypeID.ToString();
                        oRow.ItemType = media.m_oMediaType.m_sTypeName;
                        oRow.DescriptionShort = media.m_sDescription;
                        oRow.Brief = !string.IsNullOrEmpty(media.m_sDescription) ? System.Web.HttpUtility.HtmlDecode(media.m_sDescription).Replace(@"\", "/") : string.Empty;
                        oRow.CreationDate = media.m_dCreationDate;
                        oRow.AddedDate = media.m_dStartDate;
                        oRow.StartDate = media.m_dStartDate;
                        oRow.EndPurchaseDate = media.m_dEndDate;

                        oRow.ViewCounter = media.m_oRatingMedia.m_nViwes;
                        oRow.Rate = media.m_oRatingMedia.m_nRatingAvg;
                        oRow.Likes = media.m_nLikeCounter.ToString();

                        oRow.CatalogStartDate = media.m_dCatalogStartDate;

                        //EntryId
                        oRow.EntryId = media.EntryId;

                        //Personal 
                        if (media.m_dLastWatchedDate != null)
                        {
                            oRow.LastWatchedDate = (DateTime)media.m_dLastWatchedDate;
                        }
                        oRow.LastWatchedDeviceName = !string.IsNullOrEmpty(media.m_sLastWatchedDevice) ? media.m_sLastWatchedDevice : string.Empty;

                        //Add files data
                        oRow.FileID = "0";
                        if (media.m_lFiles != null && media.m_lFiles.Count > 0)
                        {
                            foreach (FileMedia file in media.m_lFiles)
                            {
                                dsItemInfo.FilesRow rowFile = retVal.Files.NewFilesRow();
                                rowFile.ID = media.AssetId;
                                rowFile.Duration = file.m_nDuration.ToString();
                                rowFile.FileID = file.m_nFileId.ToString();
                                rowFile.Format = file.m_sFileFormat;
                                rowFile.URL = file.m_sUrl;
                                rowFile.CoGuid = file.m_sCoGUID;
                                rowFile.Language = file.m_sLanguage;
                                rowFile.IsDefaultLang = file.m_nIsDefaultLanguage.ToString();
                                retVal.Files.AddFilesRow(rowFile);
                                if (file.m_sFileFormat.ToLower() == fileFormat.ToLower())
                                {
                                    oRow.Duration = file.m_nDuration.ToString();
                                    oRow.FileFormat = file.m_sFileFormat;
                                    oRow.FileID = file.m_nFileId.ToString();
                                    oRow.URL = file.m_sUrl;
                                }
                                if (file.m_sFileFormat.ToLower() == subFileFormat.ToLower())
                                {
                                    oRow.SubDuration = file.m_nDuration.ToString();
                                    oRow.SubFileFormat = file.m_sFileFormat;
                                    oRow.SubFileID = file.m_nFileId.ToString();
                                    oRow.SubURL = file.m_sUrl;
                                }

                                if (file.m_oPreProvider != null)
                                {
                                    rowFile.PreProviderID = file.m_oPreProvider.ProviderID;
                                    rowFile.PreProviderName = file.m_oPreProvider.ProviderName;
                                }

                                if (file.m_oPostProvider != null)
                                {
                                    rowFile.PostProviderID = file.m_oPostProvider.ProviderID;
                                    rowFile.PostProviderName = file.m_oPostProvider.ProviderName;
                                }

                                if (file.m_oBreakProvider != null)
                                {
                                    rowFile.BreakProviderID = file.m_oBreakProvider.ProviderID;
                                    rowFile.BreakProviderName = file.m_oBreakProvider.ProviderName;
                                    rowFile.BreakPoints = file.m_sBreakpoints;
                                }

                                if (file.m_oOverlayProvider != null)
                                {
                                    rowFile.OverlayProviderID = file.m_oOverlayProvider.ProviderID;
                                    rowFile.OverlayProviderName = file.m_oOverlayProvider.ProviderName;
                                    rowFile.OverlayPoints = file.m_sOverlaypoints;
                                }
                            }
                        }

                        if (media.m_lBranding != null && media.m_lBranding.Count > 0)
                        {
                            foreach (Tvinci.Data.Loaders.TvinciPlatform.Catalog.Branding branding in media.m_lBranding)
                            {
                                dsItemInfo.FilesRow rowFile = retVal.Files.NewFilesRow();
                                rowFile.Duration = branding.m_nDuration.ToString();
                                rowFile.FileID = branding.m_nFileId.ToString();
                                rowFile.Format = branding.m_sFileFormat;
                                rowFile.URL = branding.m_sUrl;
                                retVal.Files.AddFilesRow(rowFile);
                                //TODO: Check how to do it right...
                                oRow.BrandingRecurring = branding.m_nRecurringTypeId.ToString();
                                oRow.BrandingBodyImage = branding.m_sUrl;
                                oRow.BrandingSpaceHight = branding.m_nBrandHeight.ToString();
                                oRow.BrandingSmallImage = branding.m_sUrl;
                            }

                        }
                        retVal.Item.AddItemRow(oRow);

                        //Copy metas
                        dsItemInfo.MetasRow rowMeta = retVal.Metas.NewMetasRow();
                        foreach (Metas meta in media.m_lMetas)
                        {
                            DataColumn colMetaName = (retVal.Metas.Columns.Contains(meta.m_oTagMeta.m_sName)) ? retVal.Metas.Columns[meta.m_oTagMeta.m_sName] : retVal.Metas.Columns.Add(meta.m_oTagMeta.m_sName, typeof(String));
                            rowMeta[colMetaName] = meta.m_sValue;
                        }
                        rowMeta["ID"] = media.AssetId;
                        retVal.Metas.AddMetasRow(rowMeta);

                        //Copy Tags
                        dsItemInfo.TagsRow rowTag = retVal.Tags.NewTagsRow();
                        foreach (Tags tag in media.m_lTags)
                        {
                            string sTagName = tag.m_oTagMeta.m_sName;
                            foreach (string tagValue in tag.m_lValues)
                            {
                                if (!retVal.Tags.Columns.Contains(sTagName))
                                {
                                    DataColumn colTagName = retVal.Tags.Columns.Add(sTagName, typeof(string));
                                    rowTag[colTagName] = tagValue;
                                }
                                else
                                {
                                    rowTag[sTagName] = (!String.IsNullOrEmpty(rowTag[sTagName].ToString())) ? string.Concat(rowTag[sTagName].ToString(), "|", tagValue) : tagValue;
                                }
                            }
                        }
                        rowTag["ID"] = media.AssetId;
                        retVal.Tags.AddTagsRow(rowTag);

                        //ExternalIDs
                        if (!string.IsNullOrEmpty(media.m_ExternalIDs))
                        {
                            // add column if not exist
                            if (!retVal.ExtIDs.Columns.Contains("epg_id"))
                                retVal.ExtIDs.Columns.Add("epg_id");
                            dsItemInfo.ExtIDsRow rowExtID = retVal.ExtIDs.NewExtIDsRow();
                            rowExtID["epg_id"] = media.m_ExternalIDs;
                            rowExtID["ID"] = media.AssetId;
                            retVal.ExtIDs.AddExtIDsRow(rowExtID);
                        }

                        //Pictures
                        if (media.m_lPicture != null)
                        {
                            foreach (Picture pic in media.m_lPicture)
                            {
                                dsItemInfo.PicturesRow rowPic = retVal.Pictures.NewPicturesRow();
                                rowPic.ID = media.AssetId;
                                rowPic.PicSize = pic.m_sSize;
                                rowPic.URL = pic.m_sURL;
                                rowPic.ImageId = pic.id;
                                rowPic.Version = pic.version.ToString();
                                rowPic.Ratio = pic.ratio;


                                retVal.Pictures.AddPicturesRow(rowPic);
                            }
                        }

                    }
                }
            }
            return retVal;
        }

        public static OrderDir GetCatalogOrderDirection(TVPPro.SiteManager.DataLoaders.SearchMediaLoader.eOrderDirection orderDir)
        {
            OrderDir retVal;
            switch (orderDir)
            {
                case TVPPro.SiteManager.DataLoaders.SearchMediaLoader.eOrderDirection.Asc:
                    retVal = OrderDir.ASC;
                    break;
                case TVPPro.SiteManager.DataLoaders.SearchMediaLoader.eOrderDirection.Desc:
                    retVal = OrderDir.DESC;
                    break;
                default:
                    retVal = OrderDir.ASC;
                    break;
            }
            return retVal;
        }

        public static OrderDir GetCatalogOrderDirection(TVPPro.SiteManager.DataLoaders.ShowsEpisodeLoader.eOrderDirection orderDir)
        {
            OrderDir retVal;
            switch (orderDir)
            {
                case TVPPro.SiteManager.DataLoaders.ShowsEpisodeLoader.eOrderDirection.Asc:
                    retVal = OrderDir.ASC;
                    break;
                case TVPPro.SiteManager.DataLoaders.ShowsEpisodeLoader.eOrderDirection.Desc:
                    retVal = OrderDir.DESC;
                    break;
                default:
                    retVal = OrderDir.ASC;
                    break;
            }
            return retVal;
        }

        public static OrderDir GetCatalogOrderDirection(TVPPro.SiteManager.DataLoaders.TVMSubscriptionMediaLoader.eOrderDirection orderDir)
        {
            OrderDir retVal;
            switch (orderDir)
            {
                case TVPPro.SiteManager.DataLoaders.TVMSubscriptionMediaLoader.eOrderDirection.Asc:
                    retVal = OrderDir.ASC;
                    break;
                case TVPPro.SiteManager.DataLoaders.TVMSubscriptionMediaLoader.eOrderDirection.Desc:
                    retVal = OrderDir.DESC;
                    break;
                default:
                    retVal = OrderDir.ASC;
                    break;
            }
            return retVal;
        }
        public static OrderBy GetCatalogOrderBy(TVPPro.SiteManager.Context.Enums.eOrderBy orderBy)
        {
            OrderBy retVal;
            switch (orderBy)
            {
                case TVPPro.SiteManager.Context.Enums.eOrderBy.None:
                    retVal = OrderBy.NONE;
                    break;
                case TVPPro.SiteManager.Context.Enums.eOrderBy.Added:
                    retVal = OrderBy.START_DATE;
                    break;
                case TVPPro.SiteManager.Context.Enums.eOrderBy.Views:
                    retVal = OrderBy.VIEWS;
                    break;
                case TVPPro.SiteManager.Context.Enums.eOrderBy.Rating:
                    retVal = OrderBy.RATING;
                    break;
                case TVPPro.SiteManager.Context.Enums.eOrderBy.ABC:
                    retVal = OrderBy.NAME;
                    break;
                case TVPPro.SiteManager.Context.Enums.eOrderBy.Meta:
                    retVal = OrderBy.META;
                    break;
                default:
                    retVal = OrderBy.CREATE_DATE;
                    break;
            }
            return retVal;
        }

        public static List<KeyValue> GetCatalogMetasTags(Dictionary<string, string> dictMetasTags)
        {
            List<KeyValue> retVal = new List<KeyValue>();
            if (dictMetasTags != null && dictMetasTags.Count > 0)
            {
                foreach (var item in dictMetasTags)
                {
                    KeyValue pair = new KeyValue()
                    {
                        m_sKey = item.Key,
                        m_sValue = item.Value
                    };
                    retVal.Add(pair);
                }
            }
            return retVal;
        }

        public static List<KeyValue> GetMetasTagsFromConfiguration(string type, string value)
        {
            List<KeyValue> retVal = new List<KeyValue>();
            string[] mediaInfoStructNames;
            switch (type)
            {
                case "meta":
                    mediaInfoStructNames = MediaConfiguration.Instance.Data.TVM.SearchValues.Metadata.ToString().Split(new Char[] { ';' });
                    break;
                case "tag":
                    mediaInfoStructNames = MediaConfiguration.Instance.Data.TVM.SearchValues.Tags.ToString().Split(new Char[] { ';' });
                    break;
                default:
                    mediaInfoStructNames = new string[0];
                    break;
            }

            foreach (string name in mediaInfoStructNames)
            {
                retVal.Add(new KeyValue() { m_sKey = name, m_sValue = value });
            }
            return retVal;
        }

        public static string IDsToString(List<int> ids, string type)
        {
            StringBuilder retVal = new StringBuilder();
            if (ids != null && ids.Count > 0)
            {
                retVal.AppendFormat("{0}:", type);

                foreach (var id in ids)
                {
                    retVal.AppendLine();
                    retVal.AppendFormat("{0}, ", id);
                }
                retVal.Remove(retVal.Length - 1, 1);
            }
            return retVal.ToString();
        }

    }
}

