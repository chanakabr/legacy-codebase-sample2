using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using TVinciShared;

namespace Core.Catalog.CatalogManagement
{
    public class EpgAsset : Asset
    {
        public string EpgIdentifier { get; set; }
        public long? EpgChannelId { get; set; }
        public long? RelatedMediaId { get; set; }
        public string Crid { get; set; }
        public long? LinearAssetId { get; set; }
        public bool? CdvrEnabled { get; set; }
        public bool? CatchUpEnabled { get; set; }
        public bool? StartOverEnabled { get; set; }
        public bool? TrickPlayEnabled { get; set; }
        public int? Status { get; set; }
        public bool? IsActive { get; set; }
        public int GroupId { get; set; }
        public long LikeCounter { get; set; }
        public int PicId { get; set; }
        public string PicUrl { get; set; }
        public string FaceBookObjectId { get; set; }

        public EpgAsset()
            : base()
        {
            this.AssetType = eAssetTypes.EPG;
        }

        public EpgAsset(List<EpgCB> epgCBList, string defaultLanguageCode)
            : base()
        {
            this.AssetType = eAssetTypes.EPG;

            if (epgCBList != null && epgCBList.Count > 0)
            {
                this.Id = (long)epgCBList[0].EpgID;
                this.EpgIdentifier = epgCBList[0].EpgIdentifier;
                this.IsActive = epgCBList[0].IsActive;
                this.Status = epgCBList[0].Status;
                this.CreateDate = epgCBList[0].CreateDate;
                this.UpdateDate = epgCBList[0].UpdateDate;
                this.GroupId = epgCBList[0].GroupID;
                this.EpgChannelId = epgCBList[0].ChannelID;
                this.StartDate = epgCBList[0].StartDate;
                this.EndDate = epgCBList[0].EndDate;
                this.CoGuid = epgCBList[0].CoGuid;
                this.PicUrl = epgCBList[0].PicUrl;
                this.PicId = epgCBList[0].PicID;
                this.Crid = epgCBList[0].Crid;
                this.LikeCounter = epgCBList[0].Statistics != null ? epgCBList[0].Statistics.Likes : 0;
                this.RelatedMediaId = epgCBList[0].ExtraData != null ? epgCBList[0].ExtraData.MediaID : 0;
                this.FaceBookObjectId = epgCBList[0].ExtraData != null ? epgCBList[0].ExtraData.FBObjectID : string.Empty;
                this.LinearAssetId = epgCBList[0].LinearMediaId;

                if (epgCBList[0].EnableCDVR != 2)
                {
                    this.CdvrEnabled = Convert.ToBoolean(epgCBList[0].EnableCDVR);
                }

                if (epgCBList[0].EnableStartOver != 2)
                {
                    this.StartOverEnabled = Convert.ToBoolean(epgCBList[0].EnableStartOver);
                }

                if (epgCBList[0].EnableCatchUp != 2)
                {
                    this.CatchUpEnabled = Convert.ToBoolean(epgCBList[0].EnableCatchUp);
                }

                if (epgCBList[0].EnableTrickPlay != 2)
                {
                    this.TrickPlayEnabled = Convert.ToBoolean(epgCBList[0].EnableTrickPlay);
                }

                foreach (var epgCb in epgCBList)
                {
                    bool IsDefaultLanguage = epgCb.Language.Equals(defaultLanguageCode);
                    if (IsDefaultLanguage)
                    {
                        this.Name = epgCb.Name;
                        this.Description = epgCb.Description;
                    }
                    else
                    {
                        SetNoneDefaultNameAndDescription(epgCb);
                    }

                    SetMetas(epgCb, IsDefaultLanguage);
                    SetTags(epgCb, IsDefaultLanguage);
                    // TODO ANAT - SetImages                   
                    //this.Images = epgCB.pictures;
                }
            }
            else
            {
                this.UpdateDate = DateTime.UtcNow;
            }
        }

        // TODO ANAT - CHECK WITH LIOR IF THIS METHOD IS CORRECT
        private void SetTags(EpgCB epgCb, bool IsDefaultLanguage)
        {
            if (this.Tags == null)
            {
                this.Tags = new List<Tags>();
            }

            if (epgCb.Tags != null && epgCb.Tags.Count > 0)
            {
                foreach (var tag in epgCb.Tags)
                {
                    int index = this.Tags.FindIndex(x => x.m_oTagMeta.m_sName.Equals(tag.Key));
                    if (index == -1)
                    {
                        List<string> mainValues = IsDefaultLanguage ? tag.Value : null;

                        this.Tags.Add(new Tags(new TagMeta(tag.Key, MetaType.Tag.ToString()),
                                               mainValues,
                                               new List<LanguageContainer[]>() { GetLanguageContainer(null, tag.Value, epgCb.Language, IsDefaultLanguage) }));
                    }
                    else
                    {
                        if (IsDefaultLanguage)
                        {
                            this.Tags[index].m_lValues = tag.Value;
                        }

                        this.Tags[index].Values.Add(GetLanguageContainer(null, tag.Value, epgCb.Language, IsDefaultLanguage));
                    }
                }
            }
        }

        private void SetMetas(EpgCB epgCb, bool IsDefaultLanguage)
        {
            if (this.Metas == null)
            {
                this.Metas = new List<Metas>();
            }

            if (epgCb.Metas != null && epgCb.Metas.Count > 0)
            {
                foreach (var meta in epgCb.Metas)
                {
                    int index = this.Metas.FindIndex(x => x.m_oTagMeta.m_sName.Equals(meta.Key));
                    if (index == -1)
                    {
                        string mainValue = IsDefaultLanguage ? meta.Value[0] : string.Empty;

                        this.Metas.Add(new Metas(new TagMeta(meta.Key, MetaType.MultilingualString.ToString()),
                                                 mainValue,
                                                 GetLanguageContainer(null, meta.Value, epgCb.Language, IsDefaultLanguage)));
                    }
                    else
                    {
                        if (IsDefaultLanguage)
                        {
                            this.Metas[index].m_sValue = meta.Value[0];
                        }

                        this.Metas[index].Value = GetLanguageContainer(this.Metas[index].Value, meta.Value, epgCb.Language, IsDefaultLanguage);
                    }
                }
            }
        }

        private LanguageContainer[] GetLanguageContainer(LanguageContainer[] sourceLanguageValues, List<string> newLanguageValues, string languageCode, bool IsDefault)
        {
            List<LanguageContainer> langContainers = new List<LanguageContainer>();

            if (sourceLanguageValues != null && sourceLanguageValues.Length > 0)
            {
                langContainers.AddRange(sourceLanguageValues);
            }

            if (newLanguageValues != null && newLanguageValues.Count > 0)
            {
                langContainers.AddRange(newLanguageValues.Select(x => new LanguageContainer(languageCode, x, IsDefault)));
            }

            return langContainers.ToArray();
        }

        private void SetNoneDefaultNameAndDescription(EpgCB epgCb)
        {
            // Names
            if (this.NamesWithLanguages == null)
            {
                this.NamesWithLanguages = new List<LanguageContainer>();
            }

            this.NamesWithLanguages.Add(new LanguageContainer(epgCb.Language, epgCb.Name));

            // Descriptions
            if (this.DescriptionsWithLanguages == null)
            {
                this.DescriptionsWithLanguages = new List<LanguageContainer>();
            }

            this.DescriptionsWithLanguages.Add(new LanguageContainer(epgCb.Language, epgCb.Description));
        }

        // TODO ANAT - CHECK IF NEED THE METHOD MutateFullEpgPicURL FOR IMAGE HANDELING?
        //private static void MutateFullEpgPicURL(List<EPGChannelProgrammeObject> epgList, Dictionary<int, List<EpgPicture>> pictures, int groupId)
        //{
        //    try
        //    {
        //        if (WS_Utils.IsGroupIDContainedInConfig(groupId, ApplicationConfiguration.UseOldImageServer.Value, ';'))
        //        {
        //            // use old image server flow
        //            MutateFullEpgPicURLOldImageServerFlow(epgList, pictures);
        //        }
        //        else
        //        {
        //            EpgPicture pictureItem;
        //            List<EpgPicture> finalEpgPicture = null;
        //            foreach (ApiObjects.EPGChannelProgrammeObject oProgram in epgList)
        //            {
        //                int progGroup = int.Parse(oProgram.GROUP_ID);

        //                finalEpgPicture = new List<EpgPicture>();
        //                if (oProgram.EPG_PICTURES != null && oProgram.EPG_PICTURES.Count > 0) // work with list of pictures --LUNA version 
        //                {
        //                    foreach (EpgPicture pict in oProgram.EPG_PICTURES)
        //                    {
        //                        // get picture base URL
        //                        string picBaseName = Path.GetFileNameWithoutExtension(pict.Url);

        //                        if (pictures == null || !pictures.ContainsKey(progGroup))
        //                        {
        //                            pictureItem = new EpgPicture();
        //                            pictureItem.Ratio = pict.Ratio;

        //                            // build image URL. 
        //                            // template: <image_server_url>/p/<partner_id>/entry_id/<image_id>/version/<image_version>
        //                            // Example:  http://localhost/ImageServer/Service.svc/GetImage/p/215/entry_id/123/version/10
        //                            pictureItem.Url = ImageUtils.BuildImageUrl(groupId, picBaseName, 0, 0, 0, 100, true);

        //                            finalEpgPicture.Add(pictureItem);
        //                        }
        //                        else
        //                        {
        //                            if (!pictures.ContainsKey(progGroup))
        //                                continue;

        //                            List<EpgPicture> ratios = pictures[progGroup].Where(x => x.Ratio == pict.Ratio).ToList();

        //                            foreach (EpgPicture ratioItem in ratios)
        //                            {
        //                                pictureItem = new EpgPicture();
        //                                pictureItem.Ratio = pict.Ratio;
        //                                pictureItem.PicHeight = ratioItem.PicHeight;
        //                                pictureItem.PicWidth = ratioItem.PicWidth;
        //                                pictureItem.Version = 0;
        //                                pictureItem.Id = picBaseName;

        //                                // build image URL. 
        //                                // template: <image_server_url>/p/<partner_id>/entry_id/<image_id>/version/<image_version>/width/<image_width>/height/<image_height>/quality/<image_quality>
        //                                // Example:  http://localhost/ImageServer/Service.svc/GetImage/p/215/entry_id/123/version/10/width/432/height/230/quality/100
        //                                pictureItem.Url = ImageUtils.BuildImageUrl(groupId, picBaseName, 0, ratioItem.PicWidth, ratioItem.PicHeight, 100);

        //                                finalEpgPicture.Add(pictureItem);
        //                            }
        //                        }
        //                    }
        //                }

        //                oProgram.EPG_PICTURES = finalEpgPicture; // Reassignment epg pictures

        //                // complete the picURL for back support                
        //                string baseEpgPicUrl = string.Empty;
        //                if (oProgram != null &&
        //                    !string.IsNullOrEmpty(oProgram.PIC_URL) &&
        //                    pictures.ContainsKey(progGroup) &&
        //                    pictures[progGroup] != null)
        //                {
        //                    EpgPicture pict = pictures[progGroup].First();
        //                    if (pict != null && !string.IsNullOrEmpty(pict.Url))
        //                    {
        //                        baseEpgPicUrl = pict.Url;
        //                        if (pict.PicHeight != 0 && pict.PicWidth != 0)
        //                        {
        //                            oProgram.PIC_URL = oProgram.PIC_URL.Replace(".", string.Format("_{0}X{1}.", pict.PicWidth, pict.PicHeight));
        //                        }
        //                        oProgram.PIC_URL = string.Format("{0}{1}", baseEpgPicUrl, oProgram.PIC_URL);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error("MutateFullEpgPicURL - " + string.Format("Failed ex={0}", ex.Message), ex);
        //    }
        //}

        /// <summary>
        /// Update all empty fields from given epgAsset
        /// </summary>
        /// <param name="epgCB"></param>
        /// <returns>return if need to update basic data (epg_channels_schedule table)</returns>
        public override bool UpdateFields(Asset asset)
        {
            bool needToUpdateBasicData = false;

            needToUpdateBasicData = base.UpdateFields(asset);

            if (asset is EpgAsset)
            {
                EpgAsset epgAsset = asset as EpgAsset;
                this.EpgIdentifier = epgAsset.EpgIdentifier;
                this.GroupId = epgAsset.GroupId;
                this.PicUrl = epgAsset.PicUrl;
                this.EpgChannelId = this.EpgChannelId.GetUpdatedValue(epgAsset.EpgChannelId, ref needToUpdateBasicData);
                this.Status = this.Status.GetUpdatedValue(epgAsset.Status, ref needToUpdateBasicData);
                this.IsActive = this.IsActive.GetUpdatedValue(epgAsset.IsActive, ref needToUpdateBasicData);
                this.RelatedMediaId = this.RelatedMediaId.GetUpdatedValue(epgAsset.RelatedMediaId, ref needToUpdateBasicData);
                this.LinearAssetId = this.LinearAssetId.GetUpdatedValue(epgAsset.LinearAssetId, ref needToUpdateBasicData);
                this.CdvrEnabled = this.CdvrEnabled.GetUpdatedValue(epgAsset.CdvrEnabled, ref needToUpdateBasicData);
                this.CatchUpEnabled = this.CatchUpEnabled.GetUpdatedValue(epgAsset.CatchUpEnabled, ref needToUpdateBasicData);
                this.StartOverEnabled = this.StartOverEnabled.GetUpdatedValue(epgAsset.StartOverEnabled, ref needToUpdateBasicData);
                this.TrickPlayEnabled = this.TrickPlayEnabled.GetUpdatedValue(epgAsset.TrickPlayEnabled, ref needToUpdateBasicData);
                this.FaceBookObjectId = this.FaceBookObjectId.GetUpdatedValue(epgAsset.FaceBookObjectId, ref needToUpdateBasicData);
                this.Crid = this.Crid.GetUpdatedValue(epgAsset.Crid, ref needToUpdateBasicData);

                if (this.PicId != epgAsset.PicId && this.PicId > 0)
                {
                    needToUpdateBasicData = true;
                }
                else
                {
                    this.PicId = epgAsset.PicId;
                }

                if (this.LikeCounter != epgAsset.LikeCounter && this.LikeCounter > 0)
                {
                    needToUpdateBasicData = true;
                }
                else
                {
                    this.LikeCounter = epgAsset.LikeCounter;
                }
            }

            return needToUpdateBasicData;
        }
    }
}