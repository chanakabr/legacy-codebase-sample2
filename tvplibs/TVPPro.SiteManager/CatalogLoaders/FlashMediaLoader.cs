using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.Manager;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using System.Xml;
using TVPPro.Configuration.Technical;
using Tvinci.Data.TVMDataLoader.Protocols.FlashSingleMedia;
using TVPPro.SiteManager.Helper;
using System.Xml.Serialization;
using System.IO;
using System.Configuration;
using TVPPro.Configuration.Media;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class FlashMediaLoader : MediaLoader
    {
        #region Constructors
        public FlashMediaLoader(int mediaID, int groupID, string userIP, string picSize) :
            base(mediaID, groupID, userIP, picSize)
        {
        }
        public FlashMediaLoader(int mediaID, string userName, string userIP, string picSize) :
            base(mediaID, userName, userIP, picSize)
        {
        }
        #endregion

        protected override object ExecuteMultiMediaAdapter(List<BaseObject> medias)
        {
            XmlDocument retVal = new XmlDocument();

            string fileFormat = TechnicalConfiguration.Instance.Data.Player.MainFileFormat;
            string subFileFormat = TechnicalConfiguration.Instance.Data.Player.TrailerFileFormat;

            if (medias.Count == 1)
            {
                FlashSingleMedia data = MediaObjToFlashSingleMedia(medias[0] as MediaObj, PicSize, fileFormat, subFileFormat);
                if (data != null)
                {
                    XmlSerializer xs = new XmlSerializer(data.GetType());

                    using (StringWriter sw = new StringWriter())
                    {
                        XmlDocument xdoc = new XmlDocument();

                        xs.Serialize(sw, data);
                        xdoc.LoadXml(sw.ToString());

                        XmlNode xn = xdoc.SelectSingleNode("FlashSingleMedia/response");

                        if (xn != null)
                            retVal.LoadXml(xn.OuterXml);
                    }

                }
            }
            return retVal;
        }

        private FlashSingleMedia MediaObjToFlashSingleMedia(MediaObj media, string picSize, string fileFormat, string subFileFormat)
        {
            FlashSingleMedia retVal = null;
            if (media != null)
            {
                retVal = new FlashSingleMedia();
                retVal.response = new response()
                {
                    mediaCollection = new responsemediaCollection()
                };
                responsemedia rMedia = new responsemedia();

                rMedia.id = media.AssetId;
                rMedia.title = media.m_sName;
                rMedia.name = new responsemedianame() { value = media.m_sName };
                rMedia.pic_size2 = (from pic in media.m_lPicture where pic.m_sSize.ToLower() == picSize.ToLower() select pic.m_sURL).FirstOrDefault();
                rMedia.type = new responsemediatype() { id = media.m_oMediaType.m_nTypeID.ToString(), value = media.m_oMediaType.m_sTypeName };
                rMedia.description = new responsemediadescription() { value = media.m_sDescription };


                // Trailer
                inner_mediasmedia innerMedia = new inner_mediasmedia();
                innerMedia.id = media.AssetId;
                innerMedia.title = media.m_sName;
                innerMedia.pic_size2 = (from pic in media.m_lPicture where pic.m_sSize.ToLower() == picSize.ToLower() select pic.m_sURL).FirstOrDefault();

                // files 
                if (media.m_lFiles != null && media.m_lFiles.Count > 0)
                {
                    rMedia.parameterCollection = getAdvertisingParameters(media.m_lTags, media.m_lMetas);
                    foreach (FileMedia file in media.m_lFiles)
                    {
                        if (file.m_sFileFormat.ToLower() == fileFormat.ToLower())
                        {
                            rMedia.duration = file.m_nDuration.ToString();
                            rMedia.file_format = file.m_sFileFormat;
                            rMedia.file_id = file.m_nFileId.ToString();
                            rMedia.url = file.m_sUrl;
                            rMedia.billing = file.m_sBillingType;
                            rMedia.cdn_id = file.m_nCdnID.ToString();
                            rMedia.orig_file_format = string.Empty;
                            if (file.m_oPreProvider != null)
                            {
                                rMedia.pre_provider.ad_provider_id = file.m_oPreProvider.ProviderID;
                                rMedia.pre_provider.ad_provider_name = file.m_oPreProvider.ProviderName;
                            }
                            else
                                rMedia.pre_provider = null;
                            if (file.m_oBreakProvider != null)
                            {
                                rMedia.break_provider.ad_provider_id = file.m_oBreakProvider.ProviderID;
                                rMedia.break_provider.ad_provider_name = file.m_oBreakProvider.ProviderName;
                                rMedia.breakpoints = file.m_sBreakpoints;
                            }
                            else
                            {
                                rMedia.break_provider = null;
                                rMedia.breakpoints = null;
                            }
                            if (file.m_oOverlayProvider != null)
                            {
                                rMedia.overlay_provider.ad_provider_id = file.m_oOverlayProvider.ProviderID;
                                rMedia.overlay_provider.ad_provider_name = file.m_oOverlayProvider.ProviderName;
                                rMedia.overlaypoints = file.m_sOverlaypoints;
                            }
                            else
                            {
                                rMedia.overlay_provider = null;
                                rMedia.overlaypoints = null;
                            }
                            if (file.m_oPostProvider != null)
                            {
                                rMedia.post_provider.ad_provider_id = file.m_oPostProvider.ProviderID;
                                rMedia.post_provider.ad_provider_name = file.m_oPostProvider.ProviderName;
                            }
                            else
                                rMedia.post_provider = null;
                        }

                        // Treiler
                        if (file.m_sFileFormat.ToLower() == subFileFormat.ToLower())
                        {
                            innerMedia.duration = file.m_nDuration.ToString();
                            innerMedia.file_format = file.m_sFileFormat;
                            innerMedia.file_id = file.m_nFileId.ToString();
                            innerMedia.url = file.m_sUrl;
                            innerMedia.billing = file.m_sBillingType;
                            innerMedia.cdn_id = file.m_nCdnID.ToString();
                            innerMedia.orig_file_format = string.Empty;
                            if (file.m_oPreProvider != null)
                            {
                                innerMedia.pre_provider.ad_provider_id = file.m_oPreProvider.ProviderID;
                                innerMedia.pre_provider.ad_provider_name = file.m_oPreProvider.ProviderName;
                            }
                            else
                                innerMedia.pre_provider = null;
                            if (file.m_oBreakProvider != null)
                            {
                                innerMedia.break_provider.ad_provider_id = file.m_oBreakProvider.ProviderID;
                                innerMedia.break_provider.ad_provider_name = file.m_oBreakProvider.ProviderName;
                                innerMedia.breakpoints = file.m_sBreakpoints;
                            }
                            else
                            {
                                innerMedia.break_provider = null;
                                innerMedia.breakpoints = string.Empty;
                            }
                            if (file.m_oOverlayProvider != null)
                            {
                                innerMedia.overlay_provider.ad_provider_id = file.m_oOverlayProvider.ProviderID;
                                innerMedia.overlay_provider.ad_provider_name = file.m_oOverlayProvider.ProviderName;
                                innerMedia.overlaypoints = file.m_sOverlaypoints;
                            }
                            else
                            {
                                innerMedia.overlay_provider = null;
                                innerMedia.overlaypoints = string.Empty;
                            }
                            if (file.m_oPostProvider != null)
                            {
                                innerMedia.post_provider.ad_provider_id = file.m_oPostProvider.ProviderID;
                                innerMedia.post_provider.ad_provider_name = file.m_oPostProvider.ProviderName;
                            }
                            else
                                innerMedia.post_provider = null;
                        }
                    }
                }

                rMedia.inner_medias = new inner_medias();

                if (!string.IsNullOrEmpty(innerMedia.url))
                {
                    rMedia.inner_medias.Add(innerMedia);
                }

                retVal.response.mediaCollection.Add(rMedia);
            }

            return retVal;
        }

        private parameterCollection getAdvertisingParameters(List<Tags> tagsCollection, List<Metas> metasCollection)
        {
            parameterCollection res = new parameterCollection();
            string[] metasToExtract, tagsToExtract;
            getTagsAndMetasInLowerCaseToExtract(out metasToExtract, out tagsToExtract);
            if (tagsToExtract != null)
            {
                foreach (Tags t in tagsCollection)
                {
                    string tagNameLowerCase = t.m_oTagMeta.m_sName.ToLower();
                    if (tagsToExtract.Contains(tagNameLowerCase))
                    {
                        StringBuilder tagValues = new StringBuilder(string.Empty);
                        int numOfVals = t.m_lValues.Count;
                        for (int i = 0; i < numOfVals; i++)
                        {
                            if (i == 0)
                                tagValues.Append(t.m_lValues[0]);
                            else
                                tagValues.Append(String.Concat(";", t.m_lValues[i]));
                        }
                        res.Add(new parameter() { parameter_key = t.m_oTagMeta.m_sName, parameter_value = tagValues.ToString() });
                    }
                }
            }
            if (metasToExtract != null)
            {
                foreach (Metas m in metasCollection)
                {
                    string metaNameLowerCase = m.m_oTagMeta.m_sName.ToLower();
                    if (metasToExtract.Contains(metaNameLowerCase))
                        res.Add(new parameter() { parameter_key = m.m_oTagMeta.m_sName, parameter_value = m.m_sValue });
                }
            }
            return res;
        }

        private void getTagsAndMetasInLowerCaseToExtract(out string[] metasToExtract, out string[] tagsToExtract)
        {
            string tagsFromConfig = MediaConfiguration.Instance.Data.TVM.AdvertisingValues.Tags;
            string metasFromConfig = MediaConfiguration.Instance.Data.TVM.AdvertisingValues.Metas;
            if (string.IsNullOrEmpty(tagsFromConfig))
                tagsToExtract = null;
            else
            {
                tagsToExtract = tagsFromConfig.Split(';');
                tagsToExtract = tagsToExtract.Select(s => s.ToLowerInvariant()).ToArray();
            }
            if (string.IsNullOrEmpty(metasFromConfig))
                metasToExtract = null;
            else
            {
                metasToExtract = metasFromConfig.Split(';');
                metasToExtract = metasToExtract.Select(s => s.ToLowerInvariant()).ToArray();
            }

        }
    }
}
