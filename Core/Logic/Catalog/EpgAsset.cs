using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Epg;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TVinciShared;
using ApiObjects.Notification;
using Nest;
using Newtonsoft.Json;

namespace Core.Catalog
{
    public class EpgAsset : Asset
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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
        public DateTime SearchEndDate { get; set; }
        public bool IsIngestV2 { get; set; }

        [JsonProperty(PropertyName = "ExternalOfferIds",
            TypeNameHandling = TypeNameHandling.Auto,
            ItemTypeNameHandling = TypeNameHandling.Auto,
            ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<string> ExternalOfferIds { get; set; }

        public EpgAsset()
            : base()
        {
            AssetType = eAssetTypes.EPG;
        }

        public EpgAsset(List<EpgCB> epgCBList, string defaultLanguageCode, Dictionary<string, List<EpgPicture>> groupEpgPicturesSizes, int groupId)
            : this()
        {
            if (epgCBList != null && epgCBList.Count > 0)
            {
                var defaultEpgCB = epgCBList.FirstOrDefault(x => x.Language.Equals(defaultLanguageCode) || string.IsNullOrEmpty(x.Language));
                if (defaultEpgCB != null)
                {
                    this.Id = (long)defaultEpgCB.EpgID;
                    this.GroupId = groupId;
                    this.EpgIdentifier = defaultEpgCB.EpgIdentifier;
                    this.IsActive = defaultEpgCB.IsActive;
                    this.Status = defaultEpgCB.Status;
                    this.CreateDate = defaultEpgCB.CreateDate;
                    this.UpdateDate = defaultEpgCB.UpdateDate;
                    this.GroupId = defaultEpgCB.GroupID;
                    EpgCB epgChannel = epgCBList.FirstOrDefault(x => x.ChannelID > 0);
                    this.EpgChannelId = epgChannel != null ? (long?)epgChannel.ChannelID : null;
                    this.StartDate = defaultEpgCB.StartDate;
                    this.EndDate = defaultEpgCB.EndDate;
                    this.CoGuid = defaultEpgCB.CoGuid;
                    this.PicUrl = defaultEpgCB.PicUrl;
                    this.PicId = defaultEpgCB.PicID;
                    this.Crid = defaultEpgCB.Crid;
                    this.LikeCounter = defaultEpgCB.Statistics != null ? defaultEpgCB.Statistics.Likes : 0;
                    this.RelatedMediaId = defaultEpgCB.ExtraData != null ? defaultEpgCB.ExtraData.MediaID : 0;
                    this.FaceBookObjectId = defaultEpgCB.ExtraData != null ? defaultEpgCB.ExtraData.FBObjectID : string.Empty;
                    this.CoGuid = defaultEpgCB.EpgIdentifier;
                    this.SearchEndDate = defaultEpgCB.SearchEndDate;
                    this.IsIngestV2 = defaultEpgCB.IsIngestV2;
                    this.ExternalOfferIds = new List<string>(defaultEpgCB.ExternalOfferIds ?? new List<string>());

                    var linearChannelSettings = EpgManager.GetLinearChannelSettings(groupId, this.EpgChannelId);
                    if (linearChannelSettings != null)
                    {
                        this.LinearAssetId = linearChannelSettings.LinearMediaId;
                        this.CdvrEnabled = GetEnableData(defaultEpgCB.EnableCDVR, linearChannelSettings.EnableCDVR);
                        this.StartOverEnabled = GetEnableData(defaultEpgCB.EnableStartOver, linearChannelSettings.EnableStartOver);
                        this.CatchUpEnabled = GetEnableData(defaultEpgCB.EnableCatchUp, linearChannelSettings.EnableCatchUp);
                        this.TrickPlayEnabled = GetEnableData(defaultEpgCB.EnableTrickPlay, linearChannelSettings.EnableTrickPlay);
                    }
                    else
                    {
                        this.LinearAssetId = defaultEpgCB.LinearMediaId;
                    }

                    SetImages(defaultEpgCB.pictures, groupEpgPicturesSizes, groupId);
                }

                var tagsToSet = BuildTagsForDefaultLanguageDocument(defaultEpgCB);

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
                        SetTagValuesTranslations(tagsToSet, epgCb);
                    }

                    SetMetas(epgCb, IsDefaultLanguage);
                }


                Tags = tagsToSet.Select(t =>
                {
                    var newTagInfo = new TagMeta(t.Key, MetaType.Tag.ToString());
                    var defaultValues = defaultEpgCB.Tags[t.Key];
                    var translationValues = t.Value.Select(v => v.ToArray());
                    var newTag = new Tags(newTagInfo, defaultValues, translationValues);
                    return newTag;
                }).ToList();

            }
            else
            {
                this.UpdateDate = DateTime.UtcNow;
            }
        }

        private void SetTagValuesTranslations(Dictionary<string, List<List<LanguageContainer>>> tagsToSet, EpgCB epgCb)
        {
            if (epgCb.Tags?.Count > 0)
            {
                var onlyTagsWithTranslationValues = epgCb.Tags.Where(t => t.Value?.Any() == true);
                foreach (var translationTag in onlyTagsWithTranslationValues)
                {
                    var tagValuesDefaultLanaguage = translationTag.Value.Select(tagTranslationValue => new LanguageContainer(epgCb.Language, tagTranslationValue)).ToList();
                    if (tagsToSet.ContainsKey(translationTag.Key))
                    {
                        for (int i = 0; i < tagValuesDefaultLanaguage.Count; i++)
                        {
                            if (tagsToSet[translationTag.Key].Count > i)
                            {
                                tagsToSet[translationTag.Key][i].Add(tagValuesDefaultLanaguage[i]);
                            }
                            else
                            {
                                _Logger.Warn($"Missing default language value of tag:[{translationTag.Key}], translation value:[{translationTag.Value}], asset:[{Id}]");
                            }
                        }
                    }
                    else
                    {
                        _Logger.Warn($"Missing default language tag:[{translationTag.Key}], asset:[{Id}]");
                    }
                }
            }
        }

        private static Dictionary<string, List<List<LanguageContainer>>> BuildTagsForDefaultLanguageDocument(EpgCB defaultEpgCB)
        {
            var tagsToSet = new Dictionary<string, List<List<LanguageContainer>>>();
            if (defaultEpgCB?.Tags?.Count > 0)
            {
                var onlyTagsWithValues = defaultEpgCB.Tags.Where(t => t.Value?.Any() == true);
                foreach (var tag in onlyTagsWithValues)
                {
                    var tagValuesDefaultLanaguage = tag.Value
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Select(tagValue => new LanguageContainer(defaultEpgCB.Language, tagValue, true))
                        .ToList();
                    if (tagValuesDefaultLanaguage?.Count > 0)
                    {
                        tagsToSet[tag.Key] = new List<List<LanguageContainer>>();
                        foreach (var tagValueDefaultLang in tagValuesDefaultLanaguage)
                        {
                            tagsToSet[tag.Key].Add(new List<LanguageContainer>() { tagValueDefaultLang });
                        }
                    }
                }
            }

            return tagsToSet;
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

        private void SetImages(List<EpgPicture> epgPictures, Dictionary<string, List<EpgPicture>> groupEpgPicturesSizes, int groupId)
        {
            if (this.Images == null)
            {
                this.Images = new List<Image>();
            }

            if (WS_Utils.IsGroupIDContainedInConfig(groupId, ApplicationConfiguration.Current.UseOldImageServer.Value, ';'))
            {
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

                        if (groupEpgPicturesSizes == null || groupEpgPicturesSizes.Count == 0 || !groupEpgPicturesSizes.ContainsKey(epgPicture.Ratio))
                        {
                            this.Images.Add(new Image()
                            {
                                Url = ImageUtils.BuildImageUrl(groupId, picBaseName, epgPicture.Version, 0, 0, 0, true),
                                ContentId = picBaseName,
                                RatioName = epgPicture.Ratio,
                                Version = epgPicture.Version,
                                ImageObjectType = epgPicture.IsProgramImage ? eAssetImageType.Program : eAssetImageType.ProgramGroup,
                                ImageObjectId = this.Id,
                                ReferenceId = epgPicture.PicID,
                                ImageTypeId = epgPicture.ImageTypeId,
                                SourceUrl = epgPicture.SourceUrl
                            });
                        }
                        else
                        {
                            var filteredPicturesSizes = groupEpgPicturesSizes[epgPicture.Ratio];
                            foreach (var size in filteredPicturesSizes)
                            {
                                this.Images.Add(new Image()
                                {
                                    Url = ImageUtils.BuildImageUrl(groupId, picBaseName, epgPicture.Version, size.PicWidth, size.PicHeight, 100),
                                    ContentId = picBaseName,
                                    RatioName = epgPicture.Ratio,
                                    Version = epgPicture.Version,
                                    Height = size.PicHeight,
                                    Width = size.PicWidth,
                                    ImageObjectType = epgPicture.IsProgramImage ? eAssetImageType.Program : eAssetImageType.ProgramGroup,
                                    ImageObjectId = this.Id,
                                    ReferenceId = epgPicture.PicID,
                                    ImageTypeId = epgPicture.ImageTypeId,
                                    SourceUrl = epgPicture.SourceUrl
                                });
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
                this.ExternalOfferIds = this.ExternalOfferIds.GetUpdatedValue(epgAsset.ExternalOfferIds, ref needToUpdateBasicData);

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

        internal override AssetEvent ToAssetEvent(int groupId, long userId)
        {
            var epgEvent = new EpgAssetEvent()
            {
                GroupId = groupId,
                AssetId = this.Id,
                ExternalId = this.CoGuid,
                UserId = userId,
                Type = 0,
                LiveAssetId = LinearAssetId ?? 0
            };

            return epgEvent;
        }
    }

    public class RecordingAsset : EpgAsset
    {
        public string RecordingId { get; set; }
        public RecordingType? RecordingType { get; set; }
        public long ViewableUntilDate { get; set; }
        public bool IsMulti { get; set; }
        public RecordingAsset()
            : base()
        {
            AssetType = eAssetTypes.NPVR;
        }

        public RecordingAsset(EpgAsset item)
            : base()
        {
            AssetType = eAssetTypes.NPVR;

            EpgIdentifier = item.EpgIdentifier;
            EpgChannelId = item.EpgChannelId;
            RelatedMediaId = item.RelatedMediaId;
            Crid = item.Crid;
            LinearAssetId = item.LinearAssetId;
            CdvrEnabled = item.CdvrEnabled;
            CatchUpEnabled = item.CatchUpEnabled;
            StartOverEnabled = item.StartOverEnabled;
            TrickPlayEnabled = item.TrickPlayEnabled;
            Status = item.Status;
            IsActive = item.IsActive;
            GroupId = item.GroupId;
            LikeCounter = item.LikeCounter;
            PicId = item.PicId;
            PicUrl = item.PicUrl;
            FaceBookObjectId = item.FaceBookObjectId;
            CreateDate = item.CreateDate;
            Description = item.Description;
            DescriptionsWithLanguages = item.DescriptionsWithLanguages;
            Id = item.Id;
            Images = item.Images;
            Metas = item.Metas;
            Name = item.Name;
            RelatedEntities = item.RelatedEntities;
            Tags = item.Tags;
            UpdateDate = item.UpdateDate;
            RelatedMediaId = item.RelatedMediaId;
            StartDate = item.StartDate;
            EndDate = item.EndDate;
        }
    }
}