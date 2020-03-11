using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using Tvinci.Data.Loaders;

namespace TVPPro
{
    public static class Extensions
    {
        public static string ToStringEx(this MediaResponse mediaResponse)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("MediaResponse");
            if (mediaResponse != null && mediaResponse.m_lObj != null)
            {
                foreach (MediaObj media in mediaResponse.m_lObj)
                {
                    if (media != null)
                    {
                        sb.AppendLine();
                        sb.AppendFormat(media.ToStringEx());
                        sb.AppendFormat("External IDs = {0}", media.m_ExternalIDs != null ? media.m_ExternalIDs.FirstOrDefault().ToString() : "null");
                        if (media.m_lFiles != null && media.m_lFiles.Count > 0)
                        {
                            sb.AppendLine();
                            sb.Append("Files:");
                            foreach (FileMedia file in media.m_lFiles)
                            {
                                sb.AppendLine();
                                sb.AppendFormat("FileID = {0}, FileFormat = {1}, Duration = {2}, URL = {3}, BillingType = {4}, CdnID= {5}", file.m_nFileId, file.m_sFileFormat, file.m_nDuration, file.m_sUrl, file.m_sBillingType, file.m_nCdnID);
                            }
                        }
                        if (media.m_lPicture != null && media.m_lPicture.Count > 0)
                        {
                            sb.AppendLine();
                            sb.AppendFormat("Pictures:");
                            foreach (Picture pic in media.m_lPicture)
                            {
                                sb.AppendLine();
                                sb.AppendFormat("PicSize = {0}, PicURL = {1}", pic.m_sSize, pic.m_sURL);
                            }
                        }
                        if (media.m_lMetas != null && media.m_lMetas.Count > 0)
                        {
                            sb.AppendLine();
                            sb.AppendFormat("Metas: ");
                            foreach (Metas meta in media.m_lMetas)
                            {
                                sb.AppendFormat("Name: {0}, Value: {1} | ", meta.m_oTagMeta.m_sName, meta.m_sValue);
                            }
                        }
                        if (media.m_lTags != null && media.m_lTags.Count > 0)
                        {
                            sb.AppendLine();
                            sb.AppendFormat("Tags: ");
                            foreach (Tags tag in media.m_lTags)
                            {
                                sb.AppendFormat("Name: {0}, Values: ", tag.m_oTagMeta.m_sName);
                                foreach (var val in tag.m_lValues)
                                {
                                    sb.AppendFormat("{0}, ", val);
                                }
                                sb.Append(" | ");
                            }
                        }
                        if (media.m_lBranding != null && media.m_lBranding.Count > 0)
                        {
                            sb.AppendLine();
                            sb.AppendFormat("Branding: ");
                            foreach (var br in media.m_lBranding)
                            {
                                sb.AppendLine();
                                sb.AppendFormat("FileID = {0}, Duration = {1}, FileFormat = {2}, URL = {3}, BrandingHieght = {4}, RecurringTypeID = {5}, BillingType = {6}, CdnID = {7}",
                                    br.m_nFileId, br.m_nDuration, br.m_sFileFormat, br.m_sUrl, br.m_nBrandHeight, br.m_nRecurringTypeId, br.m_sBillingType, br.m_nCdnID);
                            }
                        }
                        sb.AppendLine();
                        sb.Append("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                    }
                    else
                    {
                        sb.AppendLine();
                        sb.Append("Media is Null !!!"); ;
                        sb.Append("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                    }
                }
            }
            return sb.ToString();
        }

        public static string ToStringEx(this List<ProgramObj> programs)
        {
            StringBuilder retVal = new StringBuilder();
            if (programs != null && programs.Count > 0)
            {
                retVal.Append("Programs:");
                foreach (var program in programs)
                {
                    if (program != null)
                    {
                        retVal.AppendLine();
                        retVal.AppendFormat("ProgramID = {0}, UpdateDate = {1}", program.AssetId, program.m_dUpdateDate);
                    }
                    else
                    {
                        retVal.AppendLine();
                        retVal.AppendFormat("null");
                    }
                }
                retVal.Remove(retVal.Length - 1, 1);
            }
            return retVal.ToString();
        }
        
        public static string ToStringEx(this EpgProgramResponse response)
        {
            StringBuilder retVal = new StringBuilder();
            if (response != null && response.m_lObj != null && response.m_lObj.Count > 0)
            {
                retVal.Append("Programs:");
                foreach (ProgramObj program in response.m_lObj)
                {
                    if (program != null)
                    {
                        retVal.AppendLine();
                        retVal.AppendFormat("ProgramID = {0}, UpdateDate = {1}", program.AssetId, program.m_dUpdateDate);
                    }
                    else
                    {
                        retVal.AppendLine();
                        retVal.AppendFormat("null");
                    }
                }
                retVal.Remove(retVal.Length - 1, 1);
            }
            return retVal.ToString();
        }

        public static string ToStringEx(this List<MediaObj> medias)
        {
            StringBuilder retVal = new StringBuilder();
            if (medias != null && medias.Count > 0)
            {
                retVal.Append("Medias:");
                foreach (var media in medias)
                {
                    retVal.AppendLine();
                    retVal.AppendFormat("MediaID = {0}, Type = {1} - {2}, Name = {3}, UpdateDate = {4}", media.AssetId, media.m_oMediaType.m_sTypeName, media.m_oMediaType.m_nTypeID, media.m_sName, media.m_dUpdateDate);
                }
                retVal.Remove(retVal.Length - 1, 1);
            }
            return retVal.ToString();
        }

        public static string ToStringEx(this PicResponse picResponse)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("PicResponse");
            if (picResponse != null && picResponse.m_lObj != null)
            {
                foreach (PicObj pic in picResponse.m_lObj)
                {
                    if (pic != null)
                    {
                        sb.AppendLine();
                        sb.AppendFormat("PictureID = {0}, UpdateDate = {1}", pic.AssetId, pic.m_dUpdateDate);
                        sb.AppendLine();
                        if (pic.m_Picture!= null && pic.m_Picture.Count > 0)
                        {
                            sb.AppendLine();
                            sb.AppendFormat("Pictures:");
                            foreach (Picture picUrl in pic.m_Picture)
                            {
                                sb.AppendLine();
                                sb.AppendFormat("PicSize = {0}, PicURL = {1}", picUrl.m_sSize, picUrl.m_sURL);
                            }
                        }
                        sb.Append("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                    }
                    else
                    {
                        sb.AppendLine();
                        sb.Append("Picture is Null !!!"); ;
                        sb.Append("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                    }
                }
            }
            return sb.ToString();
        }

        public static string ToStringEx(this List<PicObj> pics)
        {
            StringBuilder retVal = new StringBuilder();
            if (pics != null && pics.Count > 0)
            {
                retVal.Append("Pictures:");
                foreach (var pic in pics)
                {
                    retVal.AppendLine();
                    retVal.AppendFormat("PictureID = {0}, UpdateDate = {1}, Number Of URLs = {2}", pic.AssetId, pic.m_dUpdateDate, pic.m_Picture != null ? pic.m_Picture.Count : 0);
                }
                retVal.Remove(retVal.Length - 1, 1);
            }
            return retVal.ToString();
        }

        public static string ToStringEx(this List<SearchResult> results)
        {
            StringBuilder retVal = new StringBuilder();
            if (results != null && results.Count > 0)
            {
                retVal.Append("IDs with UpdateDate:");

                foreach (var res in results)
                {
                    retVal.AppendLine();
                    retVal.AppendFormat("{0} - {1},", res.assetID, res.UpdateDate);
                }
                retVal.Remove(retVal.Length - 1, 1);
            }
            return retVal.ToString();
        }

        public static string ToStringEx(this List<channelObj> channels)
        {
            StringBuilder retVal = new StringBuilder();
            if (channels != null && channels.Count > 0)
            {
                retVal.Append("Channels:");

                foreach (var channel in channels)
                {
                    retVal.AppendLine();
                    retVal.AppendFormat("ID = {0}, Title = {1}, Description = {2}, LinearStartTime = {3}, EditorRemarks = {4}", channel.m_nChannelID, channel.m_sTitle, channel.m_sDescription, channel.m_dLinearStartTime, channel.m_sEditorRemarks);
                }
                retVal.Remove(retVal.Length - 1, 1);
            }
            return retVal.ToString();
        }

        public static string ToStringEx(this List<Comments> comments)
        {
            StringBuilder retVal = new StringBuilder();
            if (comments != null && comments.Count > 0)
            {
                retVal.Append("Comments:");

                foreach (var comment in comments)
                {
                    retVal.AppendLine();
                    retVal.AppendFormat("ID = {0}, Writer = {1}, CreateDate = {2}, AssetID = {3}, Language = {4}, Header = {5}, SubHeader = {6}, ContantText = {7},", 
                        comment.Id, comment.m_sWriter, comment.m_dCreateDate, comment.m_nAssetID, comment.m_nLang, comment.m_sHeader, comment.m_sSubHeader, comment.m_sContentText);
                }
                retVal.Remove(retVal.Length - 1, 1);
            }
            return retVal.ToString();
        }

        public static string ToStringEx(this List<ChannelResponse> channels)
        {
            StringBuilder retVal = new StringBuilder();
            if (channels != null && channels.Count > 0)
            {
                retVal.Append("Channels:");

                foreach (var channel in channels)
                {
                    retVal.AppendLine();
                    retVal.AppendFormat("ChannelID = {0}, ChannelName = {1}, ChannelDescription = {2}, ChannelTotalItems = {3}",
                        channel.Id, channel.m_sName, channel.m_sDescription, channel.m_nTotalItems);
                    if (channel.m_nMedias != null && channel.m_nMedias.Count > 0)
                    {
                        retVal.AppendLine();
                        retVal.Append(channel.m_nMedias.ToStringEx());
                    }
                }
                retVal.Remove(retVal.Length - 1, 1);
            }
            return retVal.ToString();
        }

        public static string ToStringEx(this MediaSearchRequest searchRequest)
        {
            StringBuilder retVal = new StringBuilder();
            if (searchRequest != null)
            {
                retVal.AppendFormat("MediaSearchRequest: GroupID = {0}, PageIndex = {1}, PageSize = {2}, Name = {3}, ", searchRequest.m_nGroupID, searchRequest.m_nPageIndex, searchRequest.m_nPageSize, !string.IsNullOrEmpty(searchRequest.m_sName) ? searchRequest.m_sName : string.Empty);
                retVal.AppendFormat("And = {0}, Exact = {1}, OrderBy = {2}, OrderDir = {3}, ", searchRequest.m_bAnd, searchRequest.m_bExact, searchRequest.m_oOrderObj.m_eOrderBy, searchRequest.m_oOrderObj.m_eOrderDir);
                if (searchRequest.m_nMediaTypes != null && searchRequest.m_nMediaTypes.Count > 0)
                {
                    retVal.AppendLine();
                    retVal.Append("MediaTypes: ");
                    foreach (int type in searchRequest.m_nMediaTypes)
                    {
                        retVal.AppendFormat("{0}, ", type);
                    }
                    retVal.Remove(retVal.Length - 1, 1);
                }

                if (searchRequest.m_lMetas != null && searchRequest.m_lMetas.Count > 0)
                {
                    retVal.AppendLine();
                    retVal.Append("Metas: ");
                    foreach (KeyValue meta in searchRequest.m_lMetas)
                    {
                        retVal.AppendFormat("{0} = {1}, ", meta.m_sKey, meta.m_sValue);
                    }
                    retVal.Remove(retVal.Length - 1, 1);
                }
                if (searchRequest.m_lTags != null && searchRequest.m_lTags.Count > 0)
                {
                    retVal.AppendLine();
                    retVal.Append("Tags: ");
                    foreach (KeyValue tag in searchRequest.m_lTags)
                    {
                        retVal.AppendFormat("{0} = {1}, ", tag.m_sKey, tag.m_sValue);
                    }
                    retVal.Remove(retVal.Length - 1, 1);
                }
            }
            return retVal.ToString();
        }

        public static string ToStringEx(this MediaObj media)
        {
            if (media != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("MediaID = {0}, Name = {1}, Description = {2}, CreationDate = {3}, EndDate = {4}, FinalDate = {5}, PublishDate = {6}, StartDate = {7}, LikeCounter = {8}, UpdateDate = {9}",
                                media.AssetId, media.m_sName, media.m_sDescription, media.m_dCreationDate, media.m_dEndDate, media.m_dFinalDate, media.m_dPublishDate, media.m_dStartDate, media.m_nLikeCounter, media.m_dUpdateDate);

                sb.AppendLine();

                if (media.m_oMediaType != null)
                {
                    sb.AppendFormat("MediaType: MediaTypeName = {1}, MediaTypeID = {0} | ", media.m_oMediaType.m_nTypeID, media.m_oMediaType.m_sTypeName);
                }

                if (media.m_oRatingMedia != null)
                {
                    sb.AppendFormat("Rating : RatingAvg = {0}, RatingCount = {1}, RatingSum = {2}, Views = {3}",
                                    media.m_oRatingMedia.m_nRatingAvg, media.m_oRatingMedia.m_nRatingCount, media.m_oRatingMedia.m_nRatingSum, media.m_oRatingMedia.m_nViwes);
                }

                return sb.ToString();   
            }

            return string.Empty;
        }
    }
}
