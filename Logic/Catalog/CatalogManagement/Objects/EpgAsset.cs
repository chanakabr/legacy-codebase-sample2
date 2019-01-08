using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.Epg;
using ConfigurationManager;
using System;
using System.Collections.Generic;
using System.IO;
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

        public EpgAsset(List<EpgCB> epgCBList, string defaultLanguageCode, List<Ratio> ratios, int groupId)
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
                EpgCB epgChannel = epgCBList.FirstOrDefault(x => x.ChannelID > 0);
                this.EpgChannelId = epgChannel != null ? (long?)epgChannel.ChannelID : null;
                this.StartDate = epgCBList[0].StartDate;
                this.EndDate = epgCBList[0].EndDate;
                this.CoGuid = epgCBList[0].CoGuid;
                this.PicUrl = epgCBList[0].PicUrl;
                this.PicId = epgCBList[0].PicID;
                this.Crid = epgCBList[0].Crid;
                this.LikeCounter = epgCBList[0].Statistics != null ? epgCBList[0].Statistics.Likes : 0;
                this.RelatedMediaId = epgCBList[0].ExtraData != null ? epgCBList[0].ExtraData.MediaID : 0;
                this.FaceBookObjectId = epgCBList[0].ExtraData != null ? epgCBList[0].ExtraData.FBObjectID : string.Empty;
                this.CoGuid = epgCBList[0].EpgIdentifier;

                var linearChannelSettings = EpgManager.GetLinearChannelSettings(groupId, this.EpgChannelId);
                if (linearChannelSettings != null)
                {
                    this.LinearAssetId = linearChannelSettings.linearMediaId;
                    this.CdvrEnabled = GetEnableData(epgCBList[0].EnableCDVR, linearChannelSettings.EnableCDVR);
                    this.StartOverEnabled = GetEnableData(epgCBList[0].EnableStartOver, linearChannelSettings.EnableStartOver);
                    this.CatchUpEnabled = GetEnableData(epgCBList[0].EnableCatchUp, linearChannelSettings.EnableCatchUp);
                    this.TrickPlayEnabled = GetEnableData(epgCBList[0].EnableTrickPlay, linearChannelSettings.EnableTrickPlay);
                }

                SetImages(epgCBList[0].pictures, ratios, groupId);

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
                }
            }
            else
            {
                this.UpdateDate = DateTime.UtcNow;
            }
        }

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
                    // check if Genre exist
                    int index = this.Tags.FindIndex(x => x.m_oTagMeta.m_sName.Equals(tag.Key));
                    if (index == -1)
                    {
                        // not exist 
                        List<string> mainValues = IsDefaultLanguage ? tag.Value : null;
                        List<LanguageContainer[]> languageContainers = new List<LanguageContainer[]>
                            (tag.Value.Select(v => SetTagLanguageContainer(null, v, epgCb.Language, IsDefaultLanguage)));

                        this.Tags.Add(new Tags(new TagMeta(tag.Key, MetaType.Tag.ToString()), mainValues, languageContainers));
                    }
                    else
                    {
                        // exist 
                        if (IsDefaultLanguage)
                        {
                            this.Tags[index].m_lValues = tag.Value;
                        }

                        for (int i = 0; i < this.Tags[index].Values.Count; i++)
                        {
                            if (i < tag.Value.Count)
                            {
                                this.Tags[index].Values[i] = SetTagLanguageContainer(this.Tags[index].Values[i], tag.Value[i], epgCb.Language, IsDefaultLanguage);
                            }
                        }
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
                                                 SetMetaLanguageContainer(null, meta.Value, epgCb.Language, IsDefaultLanguage)));
                    }
                    else
                    {
                        if (IsDefaultLanguage)
                        {
                            this.Metas[index].m_sValue = meta.Value[0];
                        }

                        this.Metas[index].Value = SetMetaLanguageContainer(this.Metas[index].Value, meta.Value, epgCb.Language, IsDefaultLanguage);
                    }
                }
            }
        }

        private LanguageContainer[] SetMetaLanguageContainer(LanguageContainer[] sourceLanguageValues, List<string> newLanguageValues, string languageCode, bool isDefault)
        {
            List<LanguageContainer> langContainers = new List<LanguageContainer>();

            if (sourceLanguageValues != null && sourceLanguageValues.Length > 0)
            {
                langContainers.AddRange(sourceLanguageValues);
            }

            if (newLanguageValues != null && newLanguageValues.Count > 0)
            {
                langContainers.AddRange(newLanguageValues.Select(x => new LanguageContainer(languageCode, x, isDefault)));
            }

            return langContainers.ToArray();
        }

        private LanguageContainer[] SetTagLanguageContainer(LanguageContainer[] sourceLanguageValues, string newLanguageValue, string languageCode, bool isDefault)
        {
            List<LanguageContainer> langContainers = new List<LanguageContainer>();

            if (sourceLanguageValues != null && sourceLanguageValues.Length > 0)
            {
                langContainers.AddRange(sourceLanguageValues);
            }

            langContainers.Add(new LanguageContainer(languageCode, newLanguageValue, isDefault));

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

        private void SetImages(List<EpgPicture> epgPictures, List<Ratio> ratios, int groupId)
        {
            if (this.Images == null)
            {
                this.Images = new List<Image>();
            }

            if (WS_Utils.IsGroupIDContainedInConfig(groupId, ApplicationConfiguration.UseOldImageServer.Value, ';'))
            {
                // TODO SHIR - ask ira about this
                // use old image server flow
                //MutateFullEpgPicURLOldImageServerFlow(epgList, pictures);
            }
            else
            {
                if (epgPictures != null && epgPictures.Count > 0)
                {
                    foreach (EpgPicture epgPicture in epgPictures)
                    {
                        // get picture base URL
                        string picBaseName = Path.GetFileNameWithoutExtension(epgPicture.Url);

                        if (ratios == null || ratios.Count == 0)
                        {
                            this.Images.Add(new Image()
                            {
                                Url = ImageUtils.BuildImageUrl(groupId, picBaseName, epgPicture.Version, 0, 0, 0, true),
                                ContentId = picBaseName,
                                RatioName = epgPicture.Ratio,
                                Version = epgPicture.Version
                            });
                        }
                        else
                        {
                            var filteredRatios = ratios.Where(x => x.Name.Equals(epgPicture.Ratio)).ToList();
                            if (filteredRatios != null)
                            {
                                foreach (Ratio ratio in filteredRatios)
                                {
                                    this.Images.Add(new Image()
                                    {
                                        Url = ImageUtils.BuildImageUrl(groupId, picBaseName, epgPicture.Version, ratio.Width, ratio.Height, 100),
                                        ContentId = picBaseName,
                                        RatioName = epgPicture.Ratio,
                                        Height = ratio.Height,
                                        Width = ratio.Width,
                                        Version = epgPicture.Version
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }

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

        /// <summary>
        /// if epgEnable 2 return false else get value from LinearChannelSettings
        /// </summary>
        /// <param name="epgEnable"></param>
        /// <param name="linearSettingsEnable"></param>
        /// <returns></returns>
        private bool GetEnableData(int epgEnable, bool linearSettingsEnable)
        {
            if (epgEnable != 2)
            {
                return linearSettingsEnable;
            }

            return false;
        }
    }
}