using ApiLogic.Api.Managers;
using ApiLogic.Notification.Managers;
using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Api.Managers;
using Core.Catalog.Response;
using CouchbaseManager;
using DAL;
using Force.DeepCloner;
using Phx.Lib.Log;
using Newtonsoft.Json;
using Synchronizer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using ApiLogic.Api.Managers.Rule;
using ApiLogic.Catalog;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiObjects.SearchPriorityGroups;
using FeatureFlag;
using Tvinci.Core.DAL;
using TVinciShared;
using ApiObjects.Rules;
using Core.GroupManagers;
using ApiObjects.Base;

namespace Core.Catalog.CatalogManagement
{
    public class AssetManager : IAssetManager
    {
        #region Constants and Read-only

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string IS_NEW_TAG_COLUMN_NAME = "tag_id";
        private const string TITLE_META_NAME = "Title";
        private const string DESCRIPTION_META_NAME = "Description";
        private const string EXTERNAL_ID_META_NAME = "External ID";
        private const string MEDIAPREP_ID_META_NAME = "MediaPrep ID";
        private const string STATUS_META_NAME = "Status";
        private const string PLAYBACK_START_DATE_META_NAME = "Playback Start Date";
        private const string CATALOG_END_DATE_META_NAME = "Catalog End Date";
        private const string PLAYBACK_END_DATE_META_NAME = "Playback End Date";
        private const string CATALOG_START_DATE_META_NAME = "Catalog Start Date";
        private const string CREATE_DATE_META_NAME = "Creation Date";
        public const string ENTRY_ID_META_SYSTEM_NAME = "EntryID";
        private const string DEVICE_RULE_ID = "DeviceRuleId";
        private const string GEO_BLOCK_RULE_ID = "GeoBlockRuleId";
        private const string ACTION_IS_NOT_ALLOWED = "Action is not allowed";

        public const string EXTERNAL_ID_META_SYSTEM_NAME = "ExternalID";
        public const string NAME_META_SYSTEM_NAME = "Name";
        public const string DESCRIPTION_META_SYSTEM_NAME = "Description";
        public const string STATUS_META_SYSTEM_NAME = "Status";
        public const string CATALOG_START_DATE_TIME_META_SYSTEM_NAME = "CatalogStartDateTime";
        public const string CATALOG_END_DATE_TIME_META_SYSTEM_NAME = "CatalogEndDateTime";
        public const string PLAYBACK_START_DATE_TIME_META_SYSTEM_NAME = "PlaybackStartDateTime";
        public const string PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME = "PlaybackEndDateTime";
        public const string CREATE_DATE_TIME_META_SYSTEM_NAME = "CreateDate";

        internal const string CHANNEL_ID_META_SYSTEM_NAME = "CollectionEntityId";  //BEO-11199
        internal const string MANUAL_ASSET_STRUCT_NAME = "Manual";
        internal const string DYNAMIC_ASSET_STRUCT_NAME = "Dynamic";
        internal const string EXTERNAL_ASSET_STRUCT_NAME = "External";
        private const string TOPIC_ID_COLUMN_NAME = "TOPIC_ID";
        private const string TABLE_NAME_BASIC = "BASIC";
        private const string TABLE_NAME_METAS = "METAS";
        private const string TABLE_NAME_TAGS = "TAGS";
        private const string TABLE_NAME_FILES = "FILES";
        private const string TABLE_NAME_FILES_LABELS = "FILES_LABELS";
        private const string TABLE_NAME_FILES_DYNAMIC_DATA = "DYNAMIC_DATA";
        private const string TABLE_NAME_IMAGES = "IMAGES";
        private const string TABLE_NAME_NEW_TAGS = "NEW_TAGS";
        private const string TABLE_NAME_UPDATE_DATE = "UPDATE_DATE";
        private const string TABLE_NAME_RELATED_ENTITIES = "RELATED_ENTITIES";
        private const string TABLE_NAME_LINEAR = "LINEAR";
        private const string TABLE_NAME_GEO_AVAILABILITY = "GEO_AVAILABILITY";
        private const string TABLE_NAME_LIVE_TO_VOD = "LIVE_TO_VOD";

        public static readonly Dictionary<string, string> BasicMediaAssetMetasSystemNameToName = new Dictionary<string, string>()
        {
            { NAME_META_SYSTEM_NAME, TITLE_META_NAME },
            { DESCRIPTION_META_SYSTEM_NAME, DESCRIPTION_META_NAME },
            { EXTERNAL_ID_META_SYSTEM_NAME, EXTERNAL_ID_META_NAME },
            { ENTRY_ID_META_SYSTEM_NAME, MEDIAPREP_ID_META_NAME },
            { STATUS_META_SYSTEM_NAME, STATUS_META_NAME },
            { PLAYBACK_START_DATE_TIME_META_SYSTEM_NAME, PLAYBACK_START_DATE_META_NAME },
            { PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME, PLAYBACK_END_DATE_META_NAME },
            { CATALOG_START_DATE_TIME_META_SYSTEM_NAME, CATALOG_START_DATE_META_NAME },
            { CATALOG_END_DATE_TIME_META_SYSTEM_NAME, CATALOG_END_DATE_META_NAME },
            { CREATE_DATE_TIME_META_SYSTEM_NAME, CREATE_DATE_META_NAME }
        };

        internal static readonly Dictionary<string, string> BasicMetasSystemNamesToType = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { NAME_META_SYSTEM_NAME, MetaType.MultilingualString.ToString() },
            { DESCRIPTION_META_SYSTEM_NAME, MetaType.MultilingualString.ToString() },
            { EXTERNAL_ID_META_SYSTEM_NAME, MetaType.String.ToString() },
            { ENTRY_ID_META_SYSTEM_NAME, MetaType.String.ToString() },
            { STATUS_META_SYSTEM_NAME, MetaType.Bool.ToString() },
            { PLAYBACK_START_DATE_TIME_META_SYSTEM_NAME, MetaType.DateTime.ToString() },
            { PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME, MetaType.DateTime.ToString() },
            { CATALOG_START_DATE_TIME_META_SYSTEM_NAME, MetaType.DateTime.ToString() },
            { CATALOG_END_DATE_TIME_META_SYSTEM_NAME, MetaType.DateTime.ToString() },
            { CREATE_DATE_TIME_META_SYSTEM_NAME, MetaType.DateTime.ToString() }
        };

        #endregion
        
        private static readonly Lazy<MediaAssetService> MediaAssetServiceLazy = new Lazy<MediaAssetService>(() => MediaAssetService.Instance, LazyThreadSafetyMode.PublicationOnly);
        private static readonly Lazy<AssetManager> AssetManagerLazy = new Lazy<AssetManager>(() => new AssetManager(), LazyThreadSafetyMode.PublicationOnly);

        public static AssetManager Instance => AssetManagerLazy.Value;

        #region Internal Methods

        public bool InvalidateAsset(eAssetTypes assetType, int groupId, long assetId, [System.Runtime.CompilerServices.CallerMemberName] string callingMethod = "")
        {
            bool result = true;
            string invalidationKey = LayeredCacheKeys.GetAssetInvalidationKey(groupId, assetType.ToString(), assetId);
            if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
            {
                result = false;
                log.ErrorFormat("Failed to invalidate asset with id: {0}, assetType: {1}, invalidationKey: {2} after {3}", assetId, assetType.ToString(), invalidationKey, callingMethod);
            }

            return result;
        }

        internal static void InvalidateGroupLinearAssets(int groupId)
        {
            DataTable dt = CatalogDAL.GetGroupLinearMediaIds(groupId);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    Instance.InvalidateAsset(eAssetTypes.MEDIA, groupId, ODBCWrapper.Utils.GetLongSafeVal(dr, "MEDIA_ID"));
                }
            }
        }

        public VirtualAssetInfoResponse AddVirtualAsset(int groupId, VirtualAssetInfo virtualAssetInfo, string type = null)
        {
            VirtualAssetInfoResponse virtualAssetInfoResponse = new VirtualAssetInfoResponse()
            {
                Status = VirtualAssetInfoStatus.NotRelevant
            };

            try
            {
                ObjectVirtualAssetInfo objectVirtualAssetInfo = GetObjectVirtualAssetInfo(groupId, virtualAssetInfo.Type);

                if (objectVirtualAssetInfo == null)
                {
                    log.Debug($"objectVirtualAssetInfo  is null for groupId {groupId}, type {virtualAssetInfo.Type} ");
                    return virtualAssetInfoResponse;
                }

                MediaAsset virtualAsset = null;

                if (!virtualAssetInfo.DuplicateAssetId.HasValue)
                {
                    virtualAsset = new MediaAsset()
                    {
                        AssetType = eAssetTypes.MEDIA,
                        IsActive = virtualAssetInfo.IsActive,
                        CoGuid = Guid.NewGuid().ToString(),
                        Name = virtualAssetInfo.Name,
                        Description = virtualAssetInfo.Description,
                        CreateDate = DateTime.UtcNow,
                        StartDate = virtualAssetInfo.StartDate,
                        EndDate = virtualAssetInfo.EndDate,
                        MediaType = new MediaType()
                        {
                            m_nTypeID = objectVirtualAssetInfo.AssetStructId
                        }
                    };
                }
                else
                {
                    DuplicateAsset(groupId, virtualAssetInfo.DuplicateAssetId.Value, out virtualAsset);
                }

                log.Debug($"objectVirtualAssetInfo for groupId {groupId}, type {virtualAssetInfo.Type} AssetStructId {objectVirtualAssetInfo.AssetStructId} meta: {objectVirtualAssetInfo.MetaId}");

                if (!string.IsNullOrEmpty(type) && objectVirtualAssetInfo.ExtendedTypes?.Count > 0 && objectVirtualAssetInfo.ExtendedTypes.ContainsKey(type))
                {
                    virtualAsset.MediaType.m_nTypeID = (int)objectVirtualAssetInfo.ExtendedTypes[type];
                }

                if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out CatalogGroupCache catalogGroupCache))
                {
                    log.Error($"failed to get catalogGroupCache for groupId: {groupId} when calling AddVirtualAsset");
                    virtualAssetInfoResponse.Status = VirtualAssetInfoStatus.Error;
                    return virtualAssetInfoResponse;
                }

                if (catalogGroupCache.TopicsMapById.ContainsKey(objectVirtualAssetInfo.MetaId))
                {
                    var meta = catalogGroupCache.TopicsMapById[objectVirtualAssetInfo.MetaId];

                    if (!virtualAssetInfo.DuplicateAssetId.HasValue)
                    {
                        virtualAsset.Metas.Add(new Metas()
                        {
                            m_oTagMeta = new TagMeta()
                            {
                                m_sName = meta.SystemName,
                                m_sType = meta.Type.ToString()
                            },
                            m_sValue = virtualAssetInfo.Id.ToString()
                        });

                        if (virtualAssetInfo.AssetUserRuleId.HasValue)
                        {
                            HandleAssetUserRuleForVirtualAsset(groupId, virtualAssetInfo.AssetUserRuleId.Value, catalogGroupCache, ref virtualAsset); //BEO-12840
                        }
                    }
                    else
                    {
                        //Replace default meta
                        var replacementMetaIndex = virtualAsset.Metas.FindIndex(tm => tm.m_oTagMeta.m_sName == meta.SystemName);
                        if (replacementMetaIndex != -1)
                        {
                            var replacementMeta = virtualAsset.Metas[replacementMetaIndex];
                            replacementMeta.m_sValue = virtualAssetInfo.Id.ToString();
                            virtualAsset.Metas[replacementMetaIndex] = replacementMeta;
                        }
                    }

                    var response = AddAsset(groupId, virtualAsset, virtualAssetInfo.UserId);

                    if (response?.Object != null && virtualAssetInfo.DuplicateAssetId.HasValue)
                    {
                        DuplicateAssetImages(groupId, virtualAssetInfo.UserId, response.Object.Id, virtualAsset.Images);
                    }

                    if (!response.IsOkStatusCode())
                    {
                        virtualAssetInfoResponse.Status = VirtualAssetInfoStatus.Error;
                        log.Debug($"failed to AddVirtualAsset. groupId {groupId}, virtualAssetInfo {virtualAssetInfo.ToString()}, status:{response.ToStringStatus()}");
                    }
                    else
                    {
                        virtualAssetInfoResponse.Status = VirtualAssetInfoStatus.OK;
                        virtualAssetInfoResponse.AssetId = response.Object.Id;
                    }
                }
            }
            catch (Exception exc)
            {
                log.Debug($"failed to AddVirtualAsset. groupId {groupId}, exc {exc}");
            }

            log.Debug($"end to AddVirtualAsset. groupId {groupId} AssetId {virtualAssetInfoResponse.AssetId} Status {virtualAssetInfoResponse.Status}");

            return virtualAssetInfoResponse;
        }

        public void HandleAssetUserRuleForVirtualAsset(int groupId, long assetUserRuleId, CatalogGroupCache catalogGroupCache, ref MediaAsset virtualAsset)
        {
            try
            {
                var assetUserRuleResponse = Api.Managers.AssetUserRuleManager.Instance.GetAssetUserRuleByRuleId(groupId, assetUserRuleId);
                if (assetUserRuleResponse.HasObject())
                {
                    var condition = assetUserRuleResponse.Object.Conditions.FirstOrDefault(x => x is AssetShopCondition);
                    if (condition != null)
                    {
                        var shopMetaResponse = ShopMarkerService.Instance.GetShopMarkerTopic(groupId);

                        if (shopMetaResponse.IsOkStatusCode())
                        {
                            var topic = shopMetaResponse.Object;

                            if (!catalogGroupCache.AssetStructsMapById[virtualAsset.MediaType.m_nTypeID].MetaIds.Contains(topic.Id))
                            {
                                return;
                            }

                            TagMeta tm = new TagMeta()
                            {
                                m_sName = topic.SystemName,
                                m_sType = topic.Type.ToString()
                            };

                            if (topic.Type == MetaType.Tag)
                            {
                                Tags tag = new Tags()
                                {
                                    m_oTagMeta = tm,
                                    m_lValues = ((AssetShopCondition)condition).Values
                                };

                                virtualAsset.Tags.Add(tag);
                            }
                            else
                            {
                                string filter = ((AssetShopCondition)condition).Values[0];

                                virtualAsset.Metas.Add(new Metas()
                                {
                                    m_oTagMeta = tm,
                                    m_sValue = filter
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed to HandleAssetUserRuleForVirtualAsset - ", ex);
            }
        }

        private static void DuplicateAsset(int groupId, long originalAssetId, out MediaAsset newAsset)
        {
            newAsset = null;
            var originalAsset = GetMediaAssetsFromCache(groupId, new List<long> { originalAssetId }, true);
            if (originalAsset == null || originalAsset.Count == 0)
            {
                log.Warn($"Original asset: {originalAssetId} wasn't found for group Id: {groupId}");
                return;
            }

            newAsset = (MediaAsset)originalAsset.First().DeepClone();
            newAsset.Id = 0;
            newAsset.CoGuid = Guid.NewGuid().ToString();
        }

        private static void DuplicateAssetImages(int groupId, long userId, long newAssetId, List<Image> images)
        {
            if (images != null && images.Count > 0)
            {
                var imageTypeIds = new HashSet<long>();

                foreach (var image in images)
                {
                    if (imageTypeIds.Contains(image.ImageTypeId))
                    {
                        continue;
                    }

                    var newImage = new Image()
                    {
                        ImageTypeId = image.ImageTypeId,
                        ImageObjectId = newAssetId,
                        ImageObjectType = image.ImageObjectType
                    };

                    var newImageResponse = ImageManager.Instance.AddImage(groupId, newImage, userId);
                    if (newImageResponse.HasObject())
                    {
                        imageTypeIds.Add(image.ImageTypeId);

                        string imageOriginalUrl = $"{image.Url}/width/0/height/0";

                        var status = ImageManager.Instance.SetContent(groupId, userId, newImageResponse.Object.Id, imageOriginalUrl);
                        if (!status.IsOkStatusCode())
                        {
                            log.Error($"Failed to set image for asset id:{newAssetId}, url:{imageOriginalUrl}");
                        }
                    }
                }
            }
        }

        public static VirtualAssetInfoResponse DeleteVirtualAsset(int groupId, VirtualAssetInfo virtualAssetInfo)
        {
            VirtualAssetInfoResponse response = new VirtualAssetInfoResponse() { Status = VirtualAssetInfoStatus.Error };
            try
            {
                response = GetVirtualAsset(groupId, virtualAssetInfo, out bool needToCreateVirtualAsset, out Asset virtualAsset);
                if (response.Status == VirtualAssetInfoStatus.NotRelevant)
                {
                    return response;
                }

                if (virtualAsset != null)
                {
                    Status status = AssetManager.Instance.DeleteAsset(groupId, virtualAsset.Id, eAssetTypes.MEDIA, virtualAssetInfo.UserId, true);
                    if (status == null || !status.IsOkStatusCode())
                    {
                        log.Debug($"Failed delete virtual asset {virtualAsset.Id}. for virtualAssetInfo {virtualAssetInfo.ToString()}, status:{status.ToString()}");
                        response.Status = VirtualAssetInfoStatus.Error;
                        return response;
                    }

                    response.Status = VirtualAssetInfoStatus.OK;
                    return response;
                }
            }
            catch (Exception exc)
            {
                log.Debug($"failed to DeleteVirtualAsset. groupId {groupId}, exc {exc}");
                response.Status = VirtualAssetInfoStatus.Error;
            }

            return response;

        }

        public VirtualAssetInfoResponse UpdateVirtualAsset(int groupId, VirtualAssetInfo virtualAssetInfo)
        {
            VirtualAssetInfoResponse response = new VirtualAssetInfoResponse()
            {
                Status = VirtualAssetInfoStatus.NotRelevant
            };

            bool needToCreateVirtualAsset = false;
            response = GetVirtualAsset(groupId, virtualAssetInfo, out needToCreateVirtualAsset, out Asset virtualAsset);

            if (response.Status == VirtualAssetInfoStatus.NotRelevant)
            {
                return response;
            }

            if (needToCreateVirtualAsset)
            {
                response = AddVirtualAsset(groupId, virtualAssetInfo);
                if (response.Status == VirtualAssetInfoStatus.Error)
                {
                    log.Error($"Failed while UpdateVirtualAsset. Error at AddVirtualAsset. groupId: {groupId}, assetName: {virtualAssetInfo.Name}");
                    return response;
                }

                return response;
            }

            if (virtualAsset == null)
            {
                log.Debug($"No virtualAsset for virtualAssetInfo {virtualAssetInfo.ToString()}");
                response.Status = VirtualAssetInfoStatus.Error;
                return response;
            }

            virtualAsset.Name = virtualAssetInfo.Name;
            virtualAsset.Description = virtualAssetInfo.Description;

            GenericResponse<Asset> assetUpdateResponse = AssetManager.Instance.UpdateAsset(groupId, virtualAsset.Id, virtualAsset, virtualAssetInfo.UserId);

            if (!assetUpdateResponse.IsOkStatusCode())
            {
                log.Debug($"Failed update virtualAssetInfo {virtualAsset.ToString()}, groupId {groupId}, status:{assetUpdateResponse.ToStringStatus()}");
                response.Status = VirtualAssetInfoStatus.Error;
                response.ResponseStatus = assetUpdateResponse.Status;
                return response;
            }

            response.Status = VirtualAssetInfoStatus.OK;
            return response;
        }

        #endregion

        #region Private Methods

        private static GenericResponse<Asset> CreateMediaAssetResponseFromDataSet(int groupId, Dictionary<string, DataTable> tables, bool isForMigration = false, bool isMinimalOutput = false)
        {
            GenericResponse<Asset> result = new GenericResponse<Asset>();

            DataRow basicDataRow = tables[TABLE_NAME_BASIC].Rows[0];
            long id = ODBCWrapper.Utils.GetLongSafeVal(basicDataRow, "ID", 0);
            if (id <= 0)
            {
                result.SetStatus(CreateAssetResponseStatusFromResult(id));
                return result;
            }

            result.Object = MediaAssetServiceLazy.Value.CreateMediaAsset(
                groupId,
                tables[TABLE_NAME_BASIC],
                tables[TABLE_NAME_METAS],
                tables[TABLE_NAME_TAGS],
                tables[TABLE_NAME_NEW_TAGS],
                tables[TABLE_NAME_FILES],
                tables[TABLE_NAME_FILES_LABELS],
                tables[TABLE_NAME_FILES_DYNAMIC_DATA],
                tables[TABLE_NAME_IMAGES],
                null,
                null,
                tables[TABLE_NAME_RELATED_ENTITIES],
                tables[TABLE_NAME_LIVE_TO_VOD],
                false,
                isForMigration,
                isMinimalOutput);

            if (result.Object != null)
            {
                result.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return result;
        }

        private static Status ValidateMediaAssetForInsert(
            int groupId,
            CatalogGroupCache catalogGroupCache,
            ref AssetStruct assetStruct,
            MediaAsset asset,
            ref XmlDocument metasXmlDoc,
            ref XmlDocument tagsXmlDoc,
            ref (XmlDocument tagsXmlDocToAddItem, DataTable tagsXmlDocToAddDataTable) tagsToAdd,
            ref DateTime? assetCatalogStartDate,
            ref DateTime? assetFinalEndDate,
            ref XmlDocument relatedEntitiesXmlDoc,
            bool isFromIngest,
            long userId,
            out DateTime startDate,
            out DateTime endDate)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            startDate = asset.StartDate ?? DateTime.UtcNow;
            endDate = asset.EndDate ?? DateTime.MaxValue;
            if (!Asset.IsStartAndEndDatesAreValid(startDate, endDate))
            {
                result = new Status(eResponseStatus.StartDateShouldBeLessThanEndDate, eResponseStatus.StartDateShouldBeLessThanEndDate.ToString());
                return result;
            }

            HashSet<long> assetStructMetaIds = new HashSet<long>(assetStruct.MetaIds);

            if (asset.InheritancePolicy == AssetInheritancePolicy.Enable)
            {
                SetInheritedValue(groupId, catalogGroupCache, assetStruct, asset);
            }

            result = ValidateMediaAssetMetasAndTagsNamesAndTypes(groupId, catalogGroupCache, asset.Metas, asset.Tags, assetStructMetaIds, ref metasXmlDoc, ref tagsToAdd, ref assetCatalogStartDate, ref assetFinalEndDate, isFromIngest, userId);
            if (result.Code != (int)eResponseStatus.OK)
            {
                return result;
            }

            // validate device rule id
            if (asset.DeviceRuleId.HasValue && (asset.DeviceRuleId.Value <= 0 || !TvmRuleManager.ValidateDeviceRuleExists(groupId, asset.DeviceRuleId.Value)))
            {
                result = new Status((int)eResponseStatus.DeviceRuleDoesNotExistForGroup, eResponseStatus.DeviceRuleDoesNotExistForGroup.ToString());
                return result;
            }

            // validate geoblock rule id
            if (asset.GeoBlockRuleId.HasValue && (asset.GeoBlockRuleId.Value <= 0 || !TvmRuleManager.ValidateGeoBlockRuleExists(groupId, asset.GeoBlockRuleId.Value)))
            {
                result = new Status((int)eResponseStatus.GeoBlockRuleDoesNotExistForGroup, eResponseStatus.GeoBlockRuleDoesNotExistForGroup.ToString());
                return result;
            }

            result = ValidateRelatedEntities(groupId, catalogGroupCache, assetStructMetaIds, asset.RelatedEntities, ref relatedEntitiesXmlDoc);

            if (result.Code != (int)eResponseStatus.OK)
            {
                return result;
            }

            return result;
        }

        private static Status ValidateMediaAssetForUpdate(
            int groupId,
            CatalogGroupCache catalogGroupCache,
            ref AssetStruct assetStruct,
            MediaAsset asset,
            HashSet<string> currentAssetMetasAndTags,
            ref XmlDocument metasXmlDocToAdd,
            ref (XmlDocument tagsXmlDocToUpdateItem, DataTable tagsXmlDocToUpdateDataTable) tagsToAddPair,
            ref XmlDocument metasXmlDocToUpdate,
            ref (XmlDocument tagsXmlDocToUpdateItem, DataTable tagsXmlDocToUpdateDataTable) tagsToUpdatePair,
            ref DateTime? assetCatalogStartDate,
            ref DateTime? assetFinalEndDate,
            ref XmlDocument relatedEntitiesXmlDocToAdd,
            ref XmlDocument relatedEntitiesXmlDocToUpdate,
            MediaAsset currentAsset,
            long userId,
            out DateTime startDate,
            out DateTime endDate,
            bool isFromIngest = false)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            HashSet<long> assetStructMetaIds = new HashSet<long>(assetStruct.MetaIds);

            endDate = asset.EndDate ?? (currentAsset.EndDate ?? DateTime.MaxValue);
            startDate = asset.StartDate ?? (currentAsset.StartDate ?? DateTime.UtcNow);

            if (!Asset.IsStartAndEndDatesAreValid(startDate, endDate))
            {
                result = new Status(eResponseStatus.StartDateShouldBeLessThanEndDate, eResponseStatus.StartDateShouldBeLessThanEndDate.ToString());
                return result;
            }

            List<Metas> metasToAdd = asset.Metas != null && currentAssetMetasAndTags != null ? asset.Metas.Where(x => !currentAssetMetasAndTags.Contains(x.m_oTagMeta.m_sName)).ToList() : new List<Metas>();
            List<Tags> tagsToAdd = asset.Tags != null && currentAssetMetasAndTags != null ? asset.Tags.Where(x => !currentAssetMetasAndTags.Contains(x.m_oTagMeta.m_sName)).ToList() : new List<Tags>();
            List<RelatedEntities> relatedEntitiesToAdd = asset.RelatedEntities != null && currentAssetMetasAndTags != null ? asset.RelatedEntities.Where(x => !currentAssetMetasAndTags.Contains(x.TagMeta.m_sName)).ToList() : new List<RelatedEntities>();

            result = ValidateMediaAssetMetasAndTagsNamesAndTypes(groupId, catalogGroupCache, metasToAdd, tagsToAdd, assetStructMetaIds, ref metasXmlDocToAdd, ref tagsToAddPair, ref assetCatalogStartDate, ref assetFinalEndDate, isFromIngest, userId);
            if (result.Code != (int)eResponseStatus.OK)
            {
                return result;
            }

            result = ValidateRelatedEntities(groupId, catalogGroupCache, assetStructMetaIds, relatedEntitiesToAdd, ref relatedEntitiesXmlDocToAdd);
            if (result.Code != (int)eResponseStatus.OK)
            {
                return result;
            }

            List<Metas> metasToUpdate = asset.Metas != null && currentAssetMetasAndTags != null ? asset.Metas.Where(x => currentAssetMetasAndTags.Contains(x.m_oTagMeta.m_sName)).ToList() : new List<Metas>();
            List<Tags> tagsToUpdate = asset.Tags != null && currentAssetMetasAndTags != null ? asset.Tags.Where(x => currentAssetMetasAndTags.Contains(x.m_oTagMeta.m_sName)).ToList() : new List<Tags>();
            List<RelatedEntities> relatedEntitiesToUpdate = asset.RelatedEntities != null && currentAssetMetasAndTags != null ? asset.RelatedEntities.Where(x => currentAssetMetasAndTags.Contains(x.TagMeta.m_sName)).ToList() : new List<RelatedEntities>();

            result = ValidateMediaAssetMetasAndTagsNamesAndTypes(groupId, catalogGroupCache, metasToUpdate, tagsToUpdate, assetStructMetaIds, ref metasXmlDocToUpdate, ref tagsToUpdatePair, ref assetCatalogStartDate, ref assetFinalEndDate, isFromIngest, userId);
            if (result.Code != (int)eResponseStatus.OK)
            {
                return result;
            }

            result = ValidateRelatedEntitiesLimitaion(relatedEntitiesToAdd, currentAsset.RelatedEntities);
            if (result.Code != (int)eResponseStatus.OK)
            {
                return result;
            }

            result = ValidateRelatedEntities(groupId, catalogGroupCache, assetStructMetaIds, relatedEntitiesToUpdate, ref relatedEntitiesXmlDocToUpdate);
            if (result.Code != (int)eResponseStatus.OK)
            {
                return result;
            }

            // validate device rule id
            if (asset.DeviceRuleId.HasValue && asset.DeviceRuleId.Value != 0)
            {
                if (asset.DeviceRuleId.Value < 0 || !TvmRuleManager.ValidateDeviceRuleExists(groupId, asset.DeviceRuleId.Value))
                {
                    return new Status((int)eResponseStatus.DeviceRuleDoesNotExistForGroup, eResponseStatus.DeviceRuleDoesNotExistForGroup.ToString());
                }
            }

            // validate geoblock rule id
            if (asset.GeoBlockRuleId.HasValue && asset.GeoBlockRuleId.Value != 0)
            {
                if (asset.GeoBlockRuleId.Value < 0 || !TvmRuleManager.ValidateGeoBlockRuleExists(groupId, asset.GeoBlockRuleId.Value))
                {
                    return new Status((int)eResponseStatus.GeoBlockRuleDoesNotExistForGroup, eResponseStatus.GeoBlockRuleDoesNotExistForGroup.ToString());
                }
            }

            return result;
        }

        private static Status ValidateMediaAssetMetasAndTagsNamesAndTypes(
            int groupId,
            CatalogGroupCache catalogGroupCache,
            List<Metas> metas,
            List<Tags> tags,
            HashSet<long> assetStructMetaIds,
            ref XmlDocument metasXmlDoc,
            ref (XmlDocument tagsXmlDocItem, DataTable tagsXmlDocDataTable) tagsPair,
            ref DateTime? assetCatalogStartDate,
            ref DateTime? assetFinalEndDate,
            bool isFromIngest,
            long userId)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            HashSet<string> tempHashSet = new HashSet<string>();
            if (metas != null && metas.Count > 0)
            {
                metasXmlDoc = new XmlDocument();
                XmlNode rootNode = metasXmlDoc.CreateElement("root");
                metasXmlDoc.AppendChild(rootNode);
                foreach (Metas meta in metas)
                {
                    var metaName = meta.m_oTagMeta.m_sName.Trim();

                    // validate duplicates do not exist
                    if (tempHashSet.Contains(metaName))
                    {
                        result.Message = string.Format("Duplicate meta sent, meta name: {0}", metaName);
                        return result;
                    }

                    tempHashSet.Add(metaName);

                    // validate meta exists on group
                    if (!catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(metaName)
                        || !catalogGroupCache.TopicsMapBySystemNameAndByType[metaName].ContainsKey(meta.m_oTagMeta.m_sType))
                    {
                        if (isFromIngest) { continue; }

                        result.Message = string.Format("meta: {0} does not exist for group", metaName);
                        return result;
                    }

                    // validate meta exists on asset struct
                    if (!assetStructMetaIds.Contains(catalogGroupCache.TopicsMapBySystemNameAndByType[metaName][meta.m_oTagMeta.m_sType].Id))
                    {
                        if (isFromIngest) { continue; }

                        result.Message = string.Format("meta: {0} is not part of assetStruct", metaName);
                        return result;
                    }

                    Topic topic = catalogGroupCache.TopicsMapBySystemNameAndByType[metaName][meta.m_oTagMeta.m_sType];
                    // validate correct type was sent
                    if (topic.Type.ToString().ToLower() != meta.m_oTagMeta.m_sType.ToLower())
                    {
                        result = new Status((int)eResponseStatus.InvalidMetaType, string.Format("{0} was sent for meta: {1}", eResponseStatus.InvalidMetaType.ToString(), metaName));
                        return result;
                    }

                    // Validate meta values are correct and add to to metaXml
                    if (!IsMetaValueValid(meta, topic.Id, catalogGroupCache.GetDefaultLanguage().ID, catalogGroupCache.LanguageMapByCode, ref result, ref metasXmlDoc, ref rootNode,
                                            ref assetCatalogStartDate, ref assetFinalEndDate))
                    {
                        return result;
                    }
                }

                tempHashSet.Clear();
            }

            if (tags != null && tags.Count > 0)
            {
                var tagsXmlDoc = new XmlDocument();
                XmlNode rootNode = tagsXmlDoc.CreateElement("root");
                tagsXmlDoc.AppendChild(rootNode);
                var tagsDataTable = GenerateTagsDataTable();
                foreach (Tags tag in tags)
                {
                    var tagName = tag.m_oTagMeta.m_sName.Trim();

                    // validate duplicates do not exist
                    if (tempHashSet.Contains(tagName))
                    {
                        result.Message = string.Format("Duplicate tag sent, tag name: {0}", tagName);
                        return result;
                    }

                    tempHashSet.Add(tagName);

                    // validate tag exists on group
                    if (!catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(tagName)
                        || !catalogGroupCache.TopicsMapBySystemNameAndByType[tagName].ContainsKey(tag.m_oTagMeta.m_sType))
                    {
                        if (isFromIngest) { continue; }

                        result.Message = string.Format("tag: {0} does not exist for group", tagName);
                        return result;
                    }

                    // validate tag exists on asset struct
                    if (!assetStructMetaIds.Contains(catalogGroupCache.TopicsMapBySystemNameAndByType[tagName][tag.m_oTagMeta.m_sType].Id))
                    {
                        if (isFromIngest) { continue; }

                        result.Message = string.Format("tag: {0} is not part of assetStruct", tagName);
                        return result;
                    }

                    Topic topic = catalogGroupCache.TopicsMapBySystemNameAndByType[tagName][tag.m_oTagMeta.m_sType];
                    // validate correct type was sent
                    if (topic.Type != MetaType.Tag || topic.Type.ToString().ToLower() != tag.m_oTagMeta.m_sType.ToLower())
                    {
                        result = new Status((int)eResponseStatus.InvalidMetaType, string.Format("{0} was sent for meta: {1}", eResponseStatus.InvalidMetaType.ToString(), tagName));
                        return result;
                    }

                    int index = 0;
                    // insert default language values into tagsXml
                    foreach (string tagValue in tag.m_lValues)
                    {
                        index++;
                        AddTopicLanguageValueToXml(tagsXmlDoc, rootNode, topic.Id, catalogGroupCache.GetDefaultLanguage().ID, tagValue, index);
                        // TODO: For some reason we skip languageId in stored procedure and assign const value equal to 2. Be aware.
                        AddTopicLanguageValueToDataTable(tagsDataTable, topic.Id, tagValue, index);
                    }
                }

                tagsPair.tagsXmlDocItem = tagsXmlDoc;
                tagsPair.tagsXmlDocDataTable = tagsDataTable;

                //Get missing tags
                DataTable missingTags = CatalogDAL.GetMissingTags(groupId, tagsXmlDoc);
                if (missingTags?.Rows?.Count > 0)
                {
                    long topicId;
                    string value;
                    string tagKeyToLock;

                    DistributedLock _locker = new DistributedLock(new LockContext(groupId, userId), PhoenixFeatureFlagInstance.Get());
                    CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);

                    // foreach missing tags insert new value using lock (BEO-9714)
                    foreach (DataRow row in missingTags.Rows)
                    {
                        topicId = ODBCWrapper.Utils.GetLongSafeVal(row, "topic_id");
                        value = ODBCWrapper.Utils.GetSafeStr(row, "value");

                        tagKeyToLock = $"AddTagLock_{groupId}_{topicId}_{value}".ToLower();

                        const string lockInitiator = "missingTagsLocker";
                        if (_locker.Lock(new string[] { tagKeyToLock }, 3, 100, 180, lockInitiator, "_missing-tags-initiator"))
                        {
                            if (!couchbaseManager.IsKeyExists(tagKeyToLock))
                            {
                                CatalogDAL.InsertTag(groupId, value, null, topicId, userId);
                                //add document to couchbase for only for flag use do to parallel proccess
                                couchbaseManager.Add(tagKeyToLock, string.Empty, 300);
                            }

                            _locker.Unlock(new[] { tagKeyToLock }, lockInitiator);
                        }
                    }
                }
            }

            result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            return result;
        }

        private static Tuple<Dictionary<string, MediaAsset>, bool> GetMediaAssets(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<string, MediaAsset> result = new Dictionary<string, MediaAsset>();
            try
            {
                if (funcParams != null && funcParams.ContainsKey("ids") && funcParams.ContainsKey("isAllowedToViewInactiveAssets") && funcParams.ContainsKey("groupId"))
                {
                    List<long> ids;
                    int? groupId = funcParams["groupId"] as int?;
                    bool? isAllowedToViewInactiveAssets = funcParams["isAllowedToViewInactiveAssets"] as bool?;

                    if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) && funcParams[LayeredCache.MISSING_KEYS] != null)
                    {
                        ids = ((List<string>)funcParams[LayeredCache.MISSING_KEYS]).Select(x => long.Parse(x)).ToList();
                    }
                    else
                    {
                        ids = funcParams["ids"] != null ? funcParams["ids"] as List<long> : null;
                    }

                    List<MediaAsset> mediaAssets = new List<MediaAsset>();
                    List<long> missingAssetIds = null;

                    if (ids != null && groupId.HasValue && isAllowedToViewInactiveAssets.HasValue)
                    {
                        CatalogGroupCache catalogGroupCache;
                        if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId.Value, out catalogGroupCache))
                        {
                            log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetMediaAssets", groupId);
                        }
                        else
                        {
                            DataSet ds = CatalogDAL.GetMediaAssets(groupId.Value, ids, catalogGroupCache.GetDefaultLanguage().ID, isAllowedToViewInactiveAssets.Value);
                            mediaAssets = MediaAssetServiceLazy.Value.CreateMediaAssets(groupId.Value, ds)?.ToList();

                            if (isAllowedToViewInactiveAssets.Value && mediaAssets?.Count != ids.Count)
                            {
                                if (mediaAssets == null)
                                {
                                    mediaAssets = new List<MediaAsset>();
                                }
                                // get missing asset Ids
                                missingAssetIds = ids.Where(i => !mediaAssets.Any(e => i == e.Id)).ToList();
                            }
                        }

                        res = true;
                    }

                    if (res)
                    {
                        result = mediaAssets.ToDictionary(x => LayeredCacheKeys.GetAssetKey(eAssetTypes.MEDIA.ToString(), x.Id), x => x);

                        if (missingAssetIds?.Count > 0)
                        {
                            foreach (var missingAssetId in missingAssetIds)
                            {
                                result.TryAdd(LayeredCacheKeys.GetAssetKey(eAssetTypes.MEDIA.ToString(), missingAssetId),
                                    new MediaAsset() { Id = missingAssetId, IndexStatus = AssetIndexStatus.Deleted, AssetType = eAssetTypes.MEDIA });

                                log.DebugFormat("Get Deleted MediaAsset {0}, groupId {1}", missingAssetId, groupId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetMediaAssets failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, MediaAsset>, bool>(result, res);
        }

        private static List<MediaAsset> GetMediaAssetsFromCache(int groupId, List<long> ids, bool isAllowedToViewInactiveAssets)
        {
            List<MediaAsset> mediaAssets = null;
            try
            {
                if (ids == null || ids.Count == 0)
                {
                    return mediaAssets;
                }

                eAssetTypes assetType = eAssetTypes.MEDIA;
                Dictionary<string, MediaAsset> mediaAssetMap = null;
                Dictionary<string, string> keyToOriginalValueMap = LayeredCacheKeys.GetAssetsKeyMap(assetType.ToString(), ids);
                Dictionary<string, List<string>> invalidationKeysMap = LayeredCacheKeys.GetAssetsInvalidationKeysMap(groupId, assetType.ToString(), ids);

                if (!LayeredCache.Instance.GetValues(
                    keyToOriginalValueMap,
                    ref mediaAssetMap,
                    GetMediaAssets,
                    new Dictionary<string, object> { { "groupId", groupId }, { "ids", ids }, { "isAllowedToViewInactiveAssets", isAllowedToViewInactiveAssets } },
                    groupId,
                    LayeredCacheConfigNames.GET_ASSETS_LIST_CACHE_CONFIG_NAME,
                    invalidationKeysMap,
                    true))
                {
                    log.ErrorFormat("Failed getting GetMediaAssetsFromCache from LayeredCache, groupId: {0}, ids: {1}", groupId, ids != null ? string.Join(",", ids) : string.Empty);
                }
                else if (mediaAssetMap != null)
                {
                    mediaAssets = mediaAssetMap.Values.ToList();
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetMediaAssetsFromCache with groupId: {0}, ids: {1}", groupId, ids != null ? string.Join(",", ids) : string.Empty), ex);
            }

            return mediaAssets;
        }

        private static GenericResponse<Asset> AddMediaAsset(int groupId, ref CatalogGroupCache catalogGroupCache, MediaAsset assetToAdd, bool isLinear, long userId,
                                                            bool isFromIngest = false)
        {
            GenericResponse<Asset> result = new GenericResponse<Asset>();
            try
            {
                // validate assetStruct Exists
                var getAssetStructResponse = GetAssetStruct(assetToAdd, catalogGroupCache);
                var assetStruct = getAssetStructResponse.Object;
                if (!getAssetStructResponse.IsOkStatusCode())
                {
                    return new GenericResponse<Asset>(getAssetStructResponse.Status);
                }

                // validate asset
                XmlDocument metasXmlDoc = null, tagsXmlDoc = null, relatedEntitiesXmlDoc = null;
                var tagsToAdd = (tagsXmlDocToAddItem: (XmlDocument)null, tagsXmlDocToAddDataTable: (DataTable)null);
                DateTime? assetCatalogStartDate = null, assetFinalEndDate = null;

                if (!assetToAdd.InheritancePolicy.HasValue)
                {
                    assetToAdd.InheritancePolicy = AssetInheritancePolicy.Enable;
                }

                Status validateAssetTopicsResult = ValidateMediaAssetForInsert(groupId, catalogGroupCache, ref assetStruct, assetToAdd, ref metasXmlDoc, ref tagsXmlDoc, ref tagsToAdd,
                                                                                ref assetCatalogStartDate, ref assetFinalEndDate, ref relatedEntitiesXmlDoc, isFromIngest,
                                                                                userId, out var startDate, out var endDate);

                if (validateAssetTopicsResult.Code != (int)eResponseStatus.OK)
                {
                    result.SetStatus(validateAssetTopicsResult);
                    return result;
                }

                // Update asset catalogStartDate and finalEndDate
                assetToAdd.CatalogStartDate = assetCatalogStartDate ?? assetToAdd.CatalogStartDate;
                assetToAdd.FinalEndDate = assetFinalEndDate ?? assetToAdd.FinalEndDate;

                // Add Name meta values (for languages that are not default)
                ExtractBasicTopicLanguageAndValuesFromMediaAsset(assetToAdd, catalogGroupCache, ref metasXmlDoc, NAME_META_SYSTEM_NAME);

                // Add Description meta values (for languages that are not default)
                ExtractBasicTopicLanguageAndValuesFromMediaAsset(assetToAdd, catalogGroupCache, ref metasXmlDoc, DESCRIPTION_META_SYSTEM_NAME);

                DateTime catalogStartDate = assetToAdd.CatalogStartDate ?? startDate;
                DataSet ds = CatalogDAL.InsertMediaAsset(groupId, catalogGroupCache.GetDefaultLanguage().ID, metasXmlDoc, tagsToAdd.tagsXmlDocToAddItem, assetToAdd.CoGuid,
                                                        assetToAdd.EntryId, assetToAdd.DeviceRuleId, assetToAdd.GeoBlockRuleId, assetToAdd.IsActive,
                                                        startDate, endDate, catalogStartDate, assetToAdd.FinalEndDate, assetStruct.Id, userId, (int)assetToAdd.InheritancePolicy,
                                                        relatedEntitiesXmlDoc, isFromIngest);

                Dictionary<string, DataTable> tables = null;
                Status status = BuildTableDicAfterInsertMediaAsset(ds, out tables, isFromIngest);
                if (status.Code != (int)eResponseStatus.OK)
                {
                    result.SetStatus(status);
                    return result;
                }
                result = CreateMediaAssetResponseFromDataSet(groupId, tables, false, isFromIngest);

                if (result.HasObject() && result.Object.Id > 0 && !isLinear)
                {
                    // UpdateIndex
                    if (!isFromIngest)
                    {
                        bool indexingResult = IndexManagerFactory.Instance.GetIndexManager(groupId).UpsertMedia((int)result.Object.Id);
                        if (!indexingResult)
                        {
                            log.ErrorFormat("Failed UpsertMedia index for assetId: {0}, groupId: {1} after AddMediaAsset", result.Object.Id, groupId);
                        }

                        if (assetToAdd.IsActive.HasValue && assetToAdd.IsActive.Value)
                        {
                            Notification.Module.AddFollowNotificationRequestForOpc(groupId, (MediaAsset)result.Object, userId, catalogGroupCache);
                        }
                    }

                    CatalogManager.UpdateChildAssetsMetaInherited(groupId, catalogGroupCache, userId, assetStruct, assetToAdd, null);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddMediaAsset for groupId: {0} and asset: {1}", groupId, assetToAdd.ToString()), ex);
            }

            return result;
        }

        public static bool GetMediaAssetMetasAndTags(int groupId, long assetId, out List<ApiObjects.Catalog.Metas> metas, out List<ApiObjects.Catalog.Tags> tags)
        {
            metas = null;
            tags = null;
            if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out var catalogGroupCache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling AddAsset", groupId);
                return false;
            }

            var defaultLanguage = catalogGroupCache.GetDefaultLanguage();
            var ds = CatalogDAL.GetMediaAssetTags(groupId, assetId, defaultLanguage.ID);
            if (ds == null || ds.Tables == null || ds.Tables.Count == 0)
            {
                log.ErrorFormat("GetMediaAssetTags ds is empty");
                return false;
            }
            var tagsTable = ds.Tables[0];

            ds = CatalogDAL.GetMediaAssetMetas(groupId, assetId, defaultLanguage.ID);
            if (ds == null || ds.Tables == null || ds.Tables.Count == 0)
            {
                log.ErrorFormat("GetMediaAssetMetas ds is empty");
                return false;
            }

            var metasTable = ds.Tables[0];

            DateTime? maxUpdateDate = null;
            MediaAssetServiceLazy.Value.TryGetMetasAndTags(groupId, new List<LanguageObj> {defaultLanguage},
                metasTable, tagsTable, ref metas, ref tags, ref maxUpdateDate);

            return true;
        }

        private static Status CreateAssetResponseStatusFromResult(long result)
        {
            Status responseStatus = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            switch (result)
            {
                case -111:
                    responseStatus = new Status((int)eResponseStatus.AssetExternalIdMustBeUnique, eResponseStatus.AssetExternalIdMustBeUnique.ToString());
                    break;
                default:
                    break;
            }

            return responseStatus;
        }

        private static bool IsMetaValueValid(Metas meta, long topicId, int defaultLanguageId, Dictionary<string, LanguageObj> LanguageMapByCode, ref Status resultStatus,
                                             ref XmlDocument metasXmlDoc, ref XmlNode rootNode, ref DateTime? assetCatalogStartDate, ref DateTime? assetFinalEndDate)
        {
            // Validate meta values are correct
            MetaType metaType;
            bool isValidMeta = false;
            bool isValidMetaValue = false;
            bool isMultilingualStringValue = false;
            if (!Enum.TryParse<MetaType>(meta.m_oTagMeta.m_sType, out metaType))
            {
                resultStatus = new Status((int)eResponseStatus.InvalidMetaType, string.Format("{0} was sent for meta: {1}", eResponseStatus.InvalidMetaType.ToString(), meta.m_oTagMeta.m_sName));
                return false;
            }

            switch (metaType)
            {
                case MetaType.String:
                    isValidMeta = true;
                    isValidMetaValue = true;
                    break;
                case MetaType.MultilingualString:
                    isValidMeta = true;
                    isValidMetaValue = true;
                    isMultilingualStringValue = true;
                    break;
                case MetaType.Number:
                    isValidMeta = true;
                    double doubleVal;
                    isValidMetaValue = double.TryParse(meta.m_sValue, out doubleVal);
                    break;
                case MetaType.Bool:
                    isValidMeta = true;
                    bool boolVal;
                    isValidMetaValue = BoolUtils.TryConvert(meta.m_sValue, out boolVal);
                    break;
                case MetaType.DateTime:
                    isValidMeta = true;
                    DateTime dateTimeVal;
                    isValidMetaValue = DateTime.TryParseExact(meta.m_sValue, DateUtils.MAIN_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out dateTimeVal);
                    break;
                default:
                case MetaType.All:
                case MetaType.Tag:
                case MetaType.ReleatedEntity:
                    break;
            }

            if (!isValidMeta)
            {
                resultStatus = new Status((int)eResponseStatus.InvalidMetaType, string.Format("{0} was sent for meta: {1}", eResponseStatus.InvalidMetaType.ToString(), meta.m_oTagMeta.m_sName));
                return false;
            }

            if (!isValidMetaValue)
            {
                log.ErrorFormat("IsMetaValueValid-InvalidValueSentForMeta. metaName: {0}, metaType:{1}, metaValue:{2}, topicId:{3}.",
                                meta.m_oTagMeta.m_sName, metaType, meta.m_sValue, topicId);
                resultStatus = new Status((int)eResponseStatus.InvalidValueSentForMeta,
                                          string.Format("{0} metaName: {1}", eResponseStatus.InvalidValueSentForMeta.ToString(), meta.m_oTagMeta.m_sName));
                return false;
            }

            if (BasicMetasSystemNamesToType.ContainsKey(meta.m_oTagMeta.m_sName))
            {
                switch (meta.m_oTagMeta.m_sName)
                {
                    case CATALOG_START_DATE_TIME_META_SYSTEM_NAME:
                        DateTime catalogStartDate;
                        if (DateTime.TryParseExact(meta.m_sValue, DateUtils.MAIN_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out catalogStartDate))
                        {
                            assetCatalogStartDate = catalogStartDate;
                        }
                        else
                        {
                            log.ErrorFormat("IsMetaValueValid failed to parse {0} meta, value: {1}", CATALOG_START_DATE_TIME_META_SYSTEM_NAME, meta.m_sValue);
                        }
                        break;
                    case PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME:
                        DateTime finalEndDate;
                        if (DateTime.TryParseExact(meta.m_sValue, DateUtils.MAIN_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out finalEndDate))
                        {
                            assetFinalEndDate = finalEndDate;
                        }
                        else
                        {
                            log.ErrorFormat("IsMetaValueValid failed to parse {0} meta, value: {1}", CATALOG_END_DATE_TIME_META_SYSTEM_NAME, meta.m_sValue);
                        }
                        break;
                    default:
                        log.WarnFormat("IsMetaValueValid found basic meta that isn't on switch case, meta name: {0}", meta.m_oTagMeta.m_sName);
                        break;
                }
            }
            else
            {
                AddTopicLanguageValueToXml(metasXmlDoc, rootNode, topicId, defaultLanguageId, meta.m_sValue);
            }

            if (isMultilingualStringValue)
            {
                foreach (LanguageContainer language in meta.Value)
                {
                    if (LanguageMapByCode.ContainsKey(language.m_sLanguageCode3))
                    {
                        AddTopicLanguageValueToXml(metasXmlDoc, rootNode, topicId, LanguageMapByCode[language.m_sLanguageCode3].ID, language.m_sValue);
                    }
                }
            }

            return true;
        }

        private static DataTable GenerateTagsDataTable()
        {
            var tagsDataTable = new DataTable();
            tagsDataTable.Columns.Add("topic_id", typeof(long));
            tagsDataTable.Columns.Add("value", typeof(string));
            tagsDataTable.Columns.Add("order_num", typeof(int));
            return tagsDataTable;
        }

        private static void AddTopicLanguageValueToDataTable(DataTable tagsDataTable, long topicId, string tagValue, int index)
        {
            if (tagValue != null)
            {
                var row = tagsDataTable.NewRow();
                row[0] = topicId;
                row[1] = tagValue;
                row[2] = index;
                tagsDataTable.Rows.Add(row);
            }
        }

        private static void AddTopicLanguageValueToXml(XmlDocument metasXmlDoc, XmlNode rootNode, long topicId, int languageId, string value, int order = 0)
        {
            if (value != null)
            {
                XmlNode rowNode;
                XmlNode topicIdNode;
                XmlNode languageIdNode;
                XmlNode valueNode;
                XmlNode orderNode;

                rowNode = metasXmlDoc.CreateElement("row");
                topicIdNode = metasXmlDoc.CreateElement("topic_id");
                topicIdNode.InnerText = topicId.ToString();
                rowNode.AppendChild(topicIdNode);
                languageIdNode = metasXmlDoc.CreateElement("language_id");
                languageIdNode.InnerText = languageId.ToString();
                rowNode.AppendChild(languageIdNode);
                valueNode = metasXmlDoc.CreateElement("value");
                valueNode.InnerText = value;
                rowNode.AppendChild(valueNode);
                orderNode = metasXmlDoc.CreateElement("order");
                orderNode.InnerText = order.ToString();
                rowNode.AppendChild(orderNode);
                rootNode.AppendChild(rowNode);
            }
        }

        private static void ExtractBasicTopicLanguageAndValuesFromMediaAsset(MediaAsset asset, CatalogGroupCache catalogGroupCache, ref XmlDocument xmlDoc, string basicTopicSystemName)
        {
            // Add Name meta values (for languages that are not default)
            if (catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(basicTopicSystemName) && BasicMetasSystemNamesToType.ContainsKey(basicTopicSystemName))
            {
                Topic topic = catalogGroupCache.TopicsMapBySystemNameAndByType[basicTopicSystemName][BasicMetasSystemNamesToType[basicTopicSystemName]];
                XmlNode rootNode;
                if (xmlDoc == null)
                {
                    xmlDoc = new XmlDocument();
                    rootNode = xmlDoc.CreateElement("root");
                    xmlDoc.AppendChild(rootNode);
                }
                else
                {
                    rootNode = xmlDoc.FirstChild;
                }

                if (basicTopicSystemName == NAME_META_SYSTEM_NAME && !string.IsNullOrEmpty(asset.Name))
                {
                    AddTopicLanguageValueToXml(xmlDoc, rootNode, topic.Id, catalogGroupCache.GetDefaultLanguage().ID, asset.Name);
                    if (asset.NamesWithLanguages != null && asset.NamesWithLanguages.Count > 0)
                    {
                        foreach (LanguageContainer language in asset.NamesWithLanguages)
                        {
                            if (catalogGroupCache.LanguageMapByCode.ContainsKey(language.m_sLanguageCode3))
                            {
                                AddTopicLanguageValueToXml(xmlDoc, rootNode, topic.Id, catalogGroupCache.LanguageMapByCode[language.m_sLanguageCode3].ID, language.m_sValue);
                            }
                        }
                    }
                }
                else if (basicTopicSystemName == DESCRIPTION_META_SYSTEM_NAME && asset.Description != null)
                {
                    AddTopicLanguageValueToXml(xmlDoc, rootNode, topic.Id, catalogGroupCache.GetDefaultLanguage().ID, asset.Description);
                    if (asset.DescriptionsWithLanguages != null && asset.DescriptionsWithLanguages.Count > 0)
                    {
                        foreach (LanguageContainer language in asset.DescriptionsWithLanguages)
                        {
                            if (catalogGroupCache.LanguageMapByCode.ContainsKey(language.m_sLanguageCode3))
                            {
                                AddTopicLanguageValueToXml(xmlDoc, rootNode, topic.Id, catalogGroupCache.LanguageMapByCode[language.m_sLanguageCode3].ID, language.m_sValue);
                            }
                        }
                    }
                }
            }
        }

        private static List<Asset> GetNpvrAssetsFromCache(int groupId, List<long> ids)
        {
            log.ErrorFormat("Opc account doesn't support recordings at the moment");
            throw new NotImplementedException();
        }

        private static List<Asset> GetAssetsFromCache(int groupId, List<KeyValuePair<eAssetTypes, long>> assets, bool isAllowedToViewInactiveAssets, Dictionary<string, string> epgIdToDocumentId = null)
        {
            List<Asset> result = null;
            try
            {
                if (assets != null && assets.Count > 0)
                {
                    result = new List<Asset>();
                    List<long> mediaIds = assets.Where(x => x.Key == eAssetTypes.MEDIA).Select(x => x.Value).Distinct().ToList();
                    List<long> epgIds = assets.Where(x => x.Key == eAssetTypes.EPG).Select(x => x.Value).Distinct().ToList();
                    List<long> npvrIds = assets.Where(x => x.Key == eAssetTypes.NPVR).Select(x => x.Value).Distinct().ToList();
                    if (mediaIds != null && mediaIds.Count > 0)
                    {
                        List<MediaAsset> mediaAssets = GetMediaAssetsFromCache(groupId, mediaIds, isAllowedToViewInactiveAssets);
                        if (mediaAssets == null || mediaAssets.Count != mediaIds.Count)
                        {
                            List<long> missingMediaIds = mediaAssets == null ? mediaIds : mediaIds.Except(mediaAssets.Select(x => x.Id)).ToList();
                            log.WarnFormat("GetMediaAssetsFromCache didn't find the following mediaIds: {0}", string.Join(",", missingMediaIds));
                        }
                        else if (mediaAssets != null)
                        {
                            result.AddRange(mediaAssets);
                        }
                    }

                    if (epgIds != null && epgIds.Count > 0)
                    {
                        var epgAssetsFromCache = EpgAssetManager.GetEpgAssetsFromCache(epgIds, groupId, new List<string>() { "*" }, epgIdToDocumentId);
                        if (epgAssetsFromCache != null && epgAssetsFromCache.Count > 0)
                        {
                            result.AddRange(epgAssetsFromCache);
                        }
                    }

                    if (npvrIds != null && npvrIds.Count > 0)
                    {
                        var npvrAssetsFromCache = GetNpvrAssetsFromCache(groupId, npvrIds);
                        if (npvrAssetsFromCache != null)
                        {
                            result.AddRange(npvrAssetsFromCache);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAssetsFromCache with groupId: {0}, assets: {1}", groupId,
                                        assets != null ? string.Join(",", assets.Select(x => string.Format("{0}_{1}", x.Key, x.Value)).ToList()) : string.Empty), ex);
            }

            return result;
        }

        private static GenericResponse<Asset> UpdateMediaAsset(int groupId, ref CatalogGroupCache catalogGroupCache, MediaAsset currentAsset, MediaAsset assetToUpdate, bool isLinear,
                                                                long userId, bool isFromIngest = false, bool isForMigration = false, bool isFromChannel = false)
        {
            GenericResponse<Asset> result = new GenericResponse<Asset>();
            Status status = null;
            try
            {
                if (!isForMigration && !isFromChannel)
                {
                    status = AssetUserRuleManager.CheckAssetUserRuleList(groupId, userId, currentAsset);
                    if (status == null || status.Code == (int)eResponseStatus.ActionIsNotAllowed)
                    {
                        result.SetStatus(eResponseStatus.ActionIsNotAllowed, ACTION_IS_NOT_ALLOWED);
                        return result;
                    }
                }

                var validateRespose = ValidateEditingAssetOfCategoryVersion(groupId, currentAsset, catalogGroupCache);
                if (!validateRespose.IsOkStatusCode())
                {
                    result.SetStatus(validateRespose);
                    return result;
                }

                // validate asset
                XmlDocument metasXmlDocToAdd = null, metasXmlDocToUpdate = null;
                var tagsToUpdate = (tagsXmlDocToUpdateItem: (XmlDocument)null, tagsXmlDocToUpdateDataTable: (DataTable)null);
                var tagsToAdd = (tagsXmlDocToAddItem: (XmlDocument)null, tagsXmlDocToAddDataTable: (DataTable)null);
                XmlDocument relatedEntitiesXmlDocToAdd = null, relatedEntitiesXmlDocToUpdate = null;

                AssetStruct assetStruct = null;
                DateTime? assetCatalogStartDate = null, assetFinalEndDate = null;
                if (currentAsset.MediaType.m_nTypeID > 0 && catalogGroupCache.AssetStructsMapById.ContainsKey(currentAsset.MediaType.m_nTypeID))
                {
                    assetStruct = catalogGroupCache.AssetStructsMapById[currentAsset.MediaType.m_nTypeID];
                }

                HashSet<string> currentAssetMetasAndTags = new HashSet<string>(currentAsset.Metas.Select(x => x.m_oTagMeta.m_sName), StringComparer.OrdinalIgnoreCase);
                currentAssetMetasAndTags.UnionWith(currentAsset.Tags.Select(x => x.m_oTagMeta.m_sName));
                if (currentAsset.RelatedEntities != null)
                {
                    currentAssetMetasAndTags.UnionWith(currentAsset.RelatedEntities.Select(x => x.TagMeta.m_sName));
                }

                // in case isFromIngest = true. need to filter out the protected metas and tags. Should not update their values.
                if (isFromIngest)
                {
                    MediaIngestProtectProcessor.Instance.ProcessIngestProtect(currentAsset, assetToUpdate, catalogGroupCache);
                }

                Status validateAssetTopicsResult = ValidateMediaAssetForUpdate(groupId,
                    catalogGroupCache,
                    ref assetStruct,
                    assetToUpdate,
                    currentAssetMetasAndTags,
                    ref metasXmlDocToAdd,
                    ref tagsToAdd,
                    ref metasXmlDocToUpdate,
                    ref tagsToUpdate,
                    ref assetCatalogStartDate,
                    ref assetFinalEndDate,
                    ref relatedEntitiesXmlDocToAdd,
                    ref relatedEntitiesXmlDocToUpdate,
                    currentAsset,
                    userId,
                    out var startDate,
                    out var endDate,
                    isFromIngest);
                if (validateAssetTopicsResult.Code != (int)eResponseStatus.OK)
                {
                    result.SetStatus(validateAssetTopicsResult);
                    return result;
                }

                // Update asset catalogStartDate and finalEndDate
                assetToUpdate.CatalogStartDate = assetCatalogStartDate ?? assetToUpdate.CatalogStartDate;
                assetToUpdate.FinalEndDate = assetFinalEndDate ?? assetToUpdate.FinalEndDate;

                // Add Name meta values (for languages that are not default), Name can only be updated
                if (string.IsNullOrEmpty(currentAsset.Name))
                {
                    ExtractBasicTopicLanguageAndValuesFromMediaAsset(assetToUpdate, catalogGroupCache, ref metasXmlDocToAdd, NAME_META_SYSTEM_NAME);
                }
                else
                {
                    ExtractBasicTopicLanguageAndValuesFromMediaAsset(assetToUpdate, catalogGroupCache, ref metasXmlDocToUpdate, NAME_META_SYSTEM_NAME);
                }

                // Add Description meta values (for languages that are not default), Description can be updated or added
                if (currentAsset.Description == null && !string.IsNullOrEmpty(assetToUpdate.Description))
                {
                    ExtractBasicTopicLanguageAndValuesFromMediaAsset(assetToUpdate, catalogGroupCache, ref metasXmlDocToAdd, DESCRIPTION_META_SYSTEM_NAME);
                }
                else if (currentAsset.Description != null)
                {
                    ExtractBasicTopicLanguageAndValuesFromMediaAsset(assetToUpdate, catalogGroupCache, ref metasXmlDocToUpdate, DESCRIPTION_META_SYSTEM_NAME);
                }

                DateTime catalogStartDate = assetToUpdate.CatalogStartDate ?? (currentAsset.CatalogStartDate ?? DateTime.UtcNow);
                AssetInheritancePolicy inheritancePolicy = assetToUpdate.InheritancePolicy ?? (currentAsset.InheritancePolicy ?? AssetInheritancePolicy.Enable);

                // TODO - Lior. Need to extract all values from tags that are part of the mediaObj properties (Basic metas)

                DataSet ds = CatalogDAL.UpdateMediaAsset(groupId,
                    assetToUpdate.Id,
                    catalogGroupCache.GetDefaultLanguage().ID,
                    metasXmlDocToAdd,
                    tagsToAdd.tagsXmlDocToAddItem,
                    metasXmlDocToUpdate,
                    tagsToUpdate,
                    assetToUpdate.CoGuid,
                    assetToUpdate.EntryId,
                    assetToUpdate.DeviceRuleId,
                    assetToUpdate.GeoBlockRuleId,
                    assetToUpdate.IsActive,
                    startDate,
                    endDate,
                    catalogStartDate,
                    assetToUpdate.FinalEndDate,
                    userId,
                    (int) inheritancePolicy,
                    relatedEntitiesXmlDocToAdd,
                    relatedEntitiesXmlDocToUpdate,
                    isFromIngest);

                Dictionary<string, DataTable> tables = null;
                status = BuildTableDicAfterUpdateMediaAsset(ds, assetToUpdate.Id, out tables, isFromIngest);
                if (status.Code != (int)eResponseStatus.OK)
                {
                    result.SetStatus(status);
                    return result;
                }

                result = CreateMediaAssetResponseFromDataSet(groupId, tables, isForMigration, isFromIngest);
                if (!isForMigration && result != null && result.HasObject() && result.Object.Id > 0 && !isLinear)
                {
                    if (assetStruct.ParentId.HasValue && assetStruct.ParentId.Value > 0)
                    {
                        DataSet updateDS = UpdateAssetInheritancePolicy(groupId, userId, catalogGroupCache, assetStruct, inheritancePolicy, result.Object, isFromIngest);

                        if (updateDS != null)
                        {
                            status = BuildTableDicAfterUpdateMediaAsset(updateDS, assetToUpdate.Id, out tables, isFromIngest);
                            if (!status.IsOkStatusCode())
                            {
                                result.SetStatus(status);
                                return result;
                            }
                            result = CreateMediaAssetResponseFromDataSet(groupId, tables, isFromIngest);
                        }
                    }

                    // UpdateIndex
                    if (!isFromIngest)
                    {
                        if (!isFromChannel && (assetStruct.SystemName == MANUAL_ASSET_STRUCT_NAME || assetStruct.SystemName == DYNAMIC_ASSET_STRUCT_NAME || assetStruct.SystemName == EXTERNAL_ASSET_STRUCT_NAME))
                        {
                            if (assetStruct.TopicsMapBySystemName == null || assetStruct.TopicsMapBySystemName.Count == 0)
                            {
                                if (CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                                {
                                    assetStruct.TopicsMapBySystemName = catalogGroupCache.TopicsMapById.Where(x => assetStruct.MetaIds.Contains(x.Key))
                                                                      .OrderBy(x => assetStruct.MetaIds.IndexOf(x.Key))
                                                                      .ToDictionary(x => x.Value.SystemName, y => y.Value);
                                }
                            }

                            if (assetStruct.SystemName == EXTERNAL_ASSET_STRUCT_NAME)
                            {
                                UpdateExternalChannel(groupId, userId, result.Object as MediaAsset, assetStruct);
                            }
                            else
                            {
                                UpdateChannel(groupId, userId, result.Object as MediaAsset, assetStruct);
                            }
                        }

                        bool indexingResult = IndexManagerFactory.Instance.GetIndexManager(groupId).UpsertMedia((int)result.Object.Id);
                        if (!indexingResult)
                        {
                            log.ErrorFormat("Failed UpsertMedia index for assetId: {0}, groupId: {1} after UpdateMediaAsset", result.Object.Id, groupId);
                        }
                        else if (Core.Api.Managers.AssetRuleManager.IsGeoAssetRulesEnabled(groupId))
                        {
                            Catalog.Module.Instance.UpdateIndex(new List<int>() { (int)result.Object.Id }, groupId, eAction.GeoUpdate);
                        }

                        if (assetToUpdate.IsActive.HasValue && assetToUpdate.IsActive.Value)
                        {
                            Notification.Module.AddFollowNotificationRequestForOpc(groupId, (MediaAsset)result.Object, userId, catalogGroupCache);
                        }
                    }

                    // update meta inherited
                    CatalogManager.UpdateChildAssetsMetaInherited(groupId, catalogGroupCache, userId, assetStruct, result.Object, currentAsset);

                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetMediaAssetUserRulesInvalidationKey(groupId, result.Object.Id));
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateAsset for groupId: {0} and asset: {1}", groupId, assetToUpdate.ToString()), ex);
            }

            return result;
        }

        private static void UpdateAssetInheritancePolicy(int groupId, long userId, CatalogGroupCache catalogGroupCache, MediaAsset mediaAsset)
        {
            AssetStruct assetStruct = catalogGroupCache.AssetStructsMapById[mediaAsset.MediaType.m_nTypeID];
            if (assetStruct == null)
            {
                log.ErrorFormat("failed to get assetStruct {0} for groupId: {1} when calling UpdateAssetInheritancePolicy", mediaAsset.MediaType.m_nTypeID, groupId);
                return;
            }

            if (assetStruct.ParentId.HasValue && assetStruct.ParentId.Value > 0)
            {
                AssetInheritancePolicy assetInheritancePolicy = mediaAsset.InheritancePolicy ?? AssetInheritancePolicy.Enable;
                UpdateAssetInheritancePolicy(groupId, userId, catalogGroupCache, assetStruct, assetInheritancePolicy, mediaAsset, false);
            }
        }

        private static DataSet UpdateAssetInheritancePolicy(int groupId, long userId, CatalogGroupCache catalogGroupCache, AssetStruct assetStruct, AssetInheritancePolicy inheritancePolicy, Asset asset, bool isMinimalOutput)
        {
            DataSet ds = null;
            if (inheritancePolicy == AssetInheritancePolicy.Enable)
            {
                var inherited = assetStruct.AssetStructMetas.Where(x => x.Value.IsInherited.HasValue && x.Value.IsInherited.Value).ToList();
                if (inherited != null && inherited.Count > 0)
                {
                    Asset parentAsset = GetParentAsset(groupId, catalogGroupCache, assetStruct, asset);
                    if (parentAsset != null)
                    {
                        foreach (var kvp in inherited)
                        {
                            Topic topic = catalogGroupCache.TopicsMapById[kvp.Value.MetaId];
                            if (topic.Type == MetaType.Tag)
                            {
                                Tags tag = asset.Tags.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower().Equals(topic.SystemName.ToLower()));
                                Tags parentTag = parentAsset.Tags.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower().Equals(topic.SystemName.ToLower()));

                                if (tag == null || (tag != null && !tag.Equals(parentTag)))
                                {
                                    inheritancePolicy = AssetInheritancePolicy.Disable;
                                    break;
                                }
                            }
                            else
                            {
                                Metas meta = asset.Metas.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower().Equals(topic.SystemName.ToLower()));
                                Metas parentMeta = parentAsset.Metas.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower().Equals(topic.SystemName.ToLower()));

                                if (meta == null || (meta != null && !meta.Equals(parentMeta)))
                                {
                                    inheritancePolicy = AssetInheritancePolicy.Disable;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (inheritancePolicy == AssetInheritancePolicy.Disable)
                {
                    //update inheritancePolicy
                    var tagsToUpdate = (tagsXmlDocToAddItem: (XmlDocument)null, tagsXmlDocToAddDataTable: (DataTable)null);
                    ds = CatalogDAL.UpdateMediaAsset(groupId,
                        asset.Id,
                        catalogGroupCache.GetDefaultLanguage().ID,
                        null,
                        null,
                        null,
                        tagsToUpdate,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        userId,
                        (int) inheritancePolicy,
                        null,
                        null,
                        isMinimalOutput);
                }
            }

            return ds;
        }

        private static System.Collections.Concurrent.ConcurrentDictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>> CreateGroupMediaMapFromDataSet(int groupId, DataSet ds, CatalogGroupCache catalogGroupCache)
        {
            // <assetId, <languageId, media>>
            System.Collections.Concurrent.ConcurrentDictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>> groupAssetsMap = new System.Collections.Concurrent.ConcurrentDictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>>();

            try
            {
                if (ds == null || ds.Tables == null || ds.Tables.Count < 7)
                {
                    log.WarnFormat("CreateGroupMediaMapFromDataSet didn't receive dataset with 7 or more tables");
                    return null;
                }

                // Basic details table
                if (ds.Tables[0] == null || ds.Tables[0].Rows == null || ds.Tables[0].Rows.Count <= 0)
                {
                    log.WarnFormat("CreateGroupMediaMapFromDataSet - basic details table is not valid");
                    return null;
                }

                EnumerableRowCollection<DataRow> metas = new DataTable().AsEnumerable();
                // metas table
                if (ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                {
                    metas = ds.Tables[1].AsEnumerable();
                }

                EnumerableRowCollection<DataRow> tags = new DataTable().AsEnumerable();
                // tags table
                if (ds.Tables[2] != null && ds.Tables[2].Rows != null && ds.Tables[2].Rows.Count > 0)
                {
                    tags = ds.Tables[2].AsEnumerable();
                }

                EnumerableRowCollection<DataRow> fileTypes = new DataTable().AsEnumerable();
                // file types table
                if (ds.Tables[3] != null && ds.Tables[3].Rows != null && ds.Tables[3].Rows.Count > 0)
                {
                    fileTypes = ds.Tables[3].AsEnumerable();
                }

                // images table is returned empty

                EnumerableRowCollection<DataRow> assetUpdateDate = new DataTable().AsEnumerable();
                // update dates table
                if (ds.Tables[5] != null && ds.Tables[5].Rows != null && ds.Tables[5].Rows.Count > 0)
                {
                    assetUpdateDate = ds.Tables[5].AsEnumerable();
                }

                Dictionary<int, List<DataRow>> geoAvailability = new Dictionary<int, List<DataRow>>();
                // Geo Availability
                if (ds.Tables.Count > 6 && ds.Tables[6] != null && ds.Tables[6].Rows != null && ds.Tables[6].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[6].Rows)
                    {
                        int mediaId = ODBCWrapper.Utils.GetIntSafeVal(row, "MEDIA_ID");
                        int countryId = ODBCWrapper.Utils.GetIntSafeVal(row, "COUNTRY_ID");
                        if (mediaId > 0 && countryId > 0)
                        {
                            if (!geoAvailability.ContainsKey(mediaId))
                            {
                                geoAvailability.Add(mediaId, new List<DataRow>());
                            }
                            geoAvailability[mediaId].Add(row);
                        }
                    }
                }

                var liveToVodProperties = new DataTable().AsEnumerable();
                // l2v table
                if (ds.Tables[7]?.Rows?.Count > 0)
                {
                    liveToVodProperties = ds.Tables[7].AsEnumerable();
                }

                var linearChannelsRegionsMapping = RegionManager.Instance.GetLinearMediaRegions(groupId);

                System.Threading.Tasks.Parallel.ForEach(ds.Tables[0].AsEnumerable(), (basicDataRow, state) =>
                {
                    int id = ODBCWrapper.Utils.GetIntSafeVal(basicDataRow, "ID", 0);

                    try
                    {
                        if (id > 0 && !groupAssetsMap.ContainsKey(id))
                        {
                            Dictionary<string, DataTable> tables = new Dictionary<string, DataTable>();
                            DataTable basicDataTable = ds.Tables[0].Clone();
                            basicDataTable.ImportRow(basicDataRow);
                            tables.Add(TABLE_NAME_BASIC, basicDataTable);

                            var assetMetas = metas.Where(row => (long)row["ASSET_ID"] == id);
                            tables.Add(TABLE_NAME_METAS, assetMetas.Any() ? assetMetas.CopyToDataTable() : ds.Tables[1].Clone());

                            var assetTags = tags.Where(row => (long)row["ASSET_ID"] == id);
                            tables.Add(TABLE_NAME_TAGS, assetTags.Any() ? assetTags.CopyToDataTable() : ds.Tables[2].Clone());

                            var assetUpdateDateRow = assetUpdateDate.Where(row => (long)row["ID"] == id);
                            tables.Add(TABLE_NAME_UPDATE_DATE, assetUpdateDateRow.Any() ? assetUpdateDateRow.CopyToDataTable() : ds.Tables[5].Clone());

                            var liveToVodPropertiesRow = liveToVodProperties.Where(row => (long)row["MEDIA_ID"] == id).ToList();
                            tables.Add(TABLE_NAME_LIVE_TO_VOD, liveToVodPropertiesRow.Any() ? liveToVodPropertiesRow.CopyToDataTable() : ds.Tables[7].Clone());

                            var mediaAsset = MediaAssetServiceLazy.Value.CreateMediaAsset(
                                groupId,
                                tables[TABLE_NAME_BASIC],
                                tables[TABLE_NAME_METAS],
                                tables[TABLE_NAME_TAGS],
                                null,
                                null,
                                null,
                                null,
                                null,
                                tables[TABLE_NAME_UPDATE_DATE],
                                null,
                                null,
                                tables[TABLE_NAME_LIVE_TO_VOD],
                                true);
                            if (mediaAsset != null)
                            {
                                EnumerableRowCollection<DataRow> assetFileTypes = null;
                                if (fileTypes.Any())
                                {
                                    assetFileTypes = (from row in fileTypes
                                                      where (Int64)row["MEDIA_ID"] == id
                                                      select row);
                                }

                                Dictionary<int, ApiObjects.SearchObjects.Media> assets = CreateMediasFromMediaAssetAndLanguages(groupId, mediaAsset, assetFileTypes, catalogGroupCache, linearChannelsRegionsMapping);
                                if (geoAvailability.ContainsKey(id))
                                {
                                    foreach (DataRow row in geoAvailability[id])
                                    {
                                        int countryId = ODBCWrapper.Utils.GetIntSafeVal(row, "COUNTRY_ID");
                                        int isAllowed = ODBCWrapper.Utils.GetIntSafeVal(row, "IS_ALLOWED");

                                        if (isAllowed > 0)
                                        {

                                            foreach (var asset in assets.Values)
                                            {
                                                asset.allowedCountries.Add(countryId);
                                            }
                                        }
                                        else
                                        {
                                            foreach (var asset in assets.Values)
                                            {
                                                asset.blockedCountries.Add(countryId);
                                            }
                                        }
                                    }
                                }

                                // If no allowed countries were found for this media - use 0, that indicates that the media is allowed everywhere
                                foreach (ApiObjects.SearchObjects.Media media in assets.Values)
                                {
                                    if (media.allowedCountries.Count == 0)
                                    {
                                        media.allowedCountries.Add(0);
                                    }
                                }

                                groupAssetsMap.TryAdd((int)mediaAsset.Id, assets);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error when creating group media map from data set - for media id {0} ; group {1} ; error = {2}", id, groupId, ex);
                    }
                });

            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed CreateGroupMediaMapFromDataSet for groupId: {0}", groupId), ex);
            }

            return groupAssetsMap;
        }

        private static Dictionary<int, ApiObjects.SearchObjects.Media> CreateMediasFromMediaAssetAndLanguages(int groupId, MediaAsset mediaAsset,
                        EnumerableRowCollection<DataRow> assetFileTypes, CatalogGroupCache catalogGroupCache,
                        Dictionary<long, List<int>> linearChannelsRegionsMapping)
        {
            GetFileTypes(assetFileTypes, out HashSet<int> fileTypes, out HashSet<int> freeFileTypes);
            var now = DateTime.UtcNow;
            var max = DateTime.MaxValue;

            // create separate Media for each language with language-specific versions of name, description, metas and tags
            var languageToMedia = new Dictionary<int, ApiObjects.SearchObjects.Media>();
            foreach (LanguageObj language in catalogGroupCache.LanguageMapById.Values)
            {
                string name = GetValueForLanguage(language, mediaAsset.NamesWithLanguages, mediaAsset.Name);
                string description = GetValueForLanguage(language, mediaAsset.DescriptionsWithLanguages, mediaAsset.Description);
                Dictionary<string, string> metas = GetMetasForLanguage(language, mediaAsset.Metas);
                Dictionary<string, HashSet<string>> tags = GetTagsForLanguage(language, mediaAsset.Tags);

                var mediaAssetId = (int)mediaAsset.Id;
                List<int> regions = new List<int>();
                regions = linearChannelsRegionsMapping != null && linearChannelsRegionsMapping.ContainsKey(mediaAssetId)
                    ? linearChannelsRegionsMapping[mediaAssetId]
                    : new List<int> { 0 };

                ApiObjects.SearchObjects.Media media = new ApiObjects.SearchObjects.Media()
                {
                    m_nMediaID = mediaAssetId,
                    m_sName = name,
                    m_sDescription = description,
                    m_nMediaTypeID = mediaAsset.MediaType.m_nTypeID,
                    m_nIsActive = mediaAsset.IsActive.HasValue && mediaAsset.IsActive.Value ? 1 : 0,
                    m_nGroupID = groupId,
                    m_sCreateDate = mediaAsset.CreateDate.Value.ToESDateFormat(),
                    m_sEndDate = mediaAsset.EndDate.GetValueOrDefault(max).ToESDateFormat(),
                    m_sFinalEndDate = mediaAsset.FinalEndDate.GetValueOrDefault(max).ToESDateFormat(),
                    m_sStartDate = mediaAsset.StartDate.GetValueOrDefault(now).ToESDateFormat(),
                    CatalogStartDate = mediaAsset.CatalogStartDate.GetValueOrDefault(now).ToESDateFormat(),
                    m_sUpdateDate = mediaAsset.UpdateDate.GetValueOrDefault(now).ToESDateFormat(),
                    m_sUserTypes = mediaAsset.UserTypes,
                    m_nDeviceRuleId = mediaAsset.DeviceRuleId.GetValueOrDefault(0),
                    geoBlockRule = mediaAsset.GeoBlockRuleId.GetValueOrDefault(0),
                    CoGuid = mediaAsset.CoGuid,
                    EntryId = mediaAsset.EntryId,
                    m_dMeatsValues = metas,
                    m_dTagValues = tags,
                    m_sMFTypes = fileTypes != null ? string.Join(";", fileTypes) : string.Empty,
                    freeFileTypes = freeFileTypes != null ? new List<int>(freeFileTypes) : new List<int>(),
                    isFree = freeFileTypes != null && freeFileTypes.Count > 0,
                    inheritancePolicy = (int)mediaAsset.InheritancePolicy,
                    allowedCountries = new List<int>(),
                    blockedCountries = new List<int>(),
                    epgIdentifier = mediaAsset.FallBackEpgIdentifier,
                    regions = regions
                };

                if (mediaAsset is LiveToVodAsset liveToVodAsset)
                {
                    media.L2vLinearAssetId = liveToVodAsset.LinearAssetId;
                    media.L2vEpgChannelId = liveToVodAsset.EpgChannelId;
                    media.L2vEpgId = liveToVodAsset.EpgIdentifier;
                    media.L2vCrid = liveToVodAsset.Crid;
                    media.L2vOriginalEndDate = liveToVodAsset.OriginalEndDate.ToESDateFormat();
                    media.L2vOriginalStartDate = liveToVodAsset.OriginalStartDate.ToESDateFormat();
                }

                Utils.ExtractSuppressedValue(catalogGroupCache, media);

                languageToMedia.Add(language.ID, media);
            }

            return languageToMedia;
        }

        private static void GetFileTypes(EnumerableRowCollection<DataRow> assetFileTypes, out HashSet<int> fileTypes, out HashSet<int> freeFileTypes)
        {
            fileTypes = null;
            freeFileTypes = null;
            if (assetFileTypes != null && assetFileTypes.Any())
            {
                fileTypes = new HashSet<int>();
                freeFileTypes = new HashSet<int>();
                foreach (DataRow dr in assetFileTypes)
                {
                    int fileTypeId = ODBCWrapper.Utils.GetIntSafeVal(dr, "MEDIA_TYPE_ID");
                    bool isFree = ODBCWrapper.Utils.GetIntSafeVal(dr, "IS_FREE", 0) == 1;

                    if (fileTypeId > 0)
                    {
                        fileTypes.Add(fileTypeId);
                        if (isFree) freeFileTypes.Add(fileTypeId);
                    }
                }
            }
        }

        private static string GetValueForLanguage(LanguageObj language, IEnumerable<LanguageContainer> languagesValues, string defaultLanguageValue)
        {
            if (language.IsDefault) return defaultLanguageValue;
            var nameWithLanguages = languagesValues.FirstOrDefault(x => x.m_sLanguageCode3 == language.Code);
            return nameWithLanguages == null ? defaultLanguageValue : nameWithLanguages.m_sValue;
        }

        private static IEnumerable<string> GetValuesForLanguage(LanguageObj language, IEnumerable<LanguageContainer> languagesValues)
        {
            return languagesValues.Where(z => z.m_sLanguageCode3 == language.Code).Select(z => z.m_sValue);
        }

        private static readonly string DateTimeType = MetaType.DateTime.ToString();
        private static readonly string BoolType = MetaType.Bool.ToString();
        private static readonly string NumberType = MetaType.Number.ToString();
        private static readonly string StringType = MetaType.String.ToString();
        private static Dictionary<string, string> GetMetasForLanguage(LanguageObj language, List<Metas> metas)
        {
            var languageMetas = new Dictionary<string, string>();
            if (metas == null || metas.Count == 0) return languageMetas;

            foreach (Metas meta in metas)
            {
                var metaName = meta.m_oTagMeta.m_sName;
                var metaType = meta.m_oTagMeta.m_sType;

                if (metaType == DateTimeType
                    && DateTime.TryParseExact(meta.m_sValue, DateUtils.MAIN_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var date))
                {
                    languageMetas.Add(metaName, date.ToESDateFormat());
                }
                else if (metaType == BoolType || metaType == NumberType || metaType == StringType)
                {
                    languageMetas.TryAdd(metaName, meta.m_sValue);
                }
                else
                {
                    if (language.IsDefault)
                    {
                        languageMetas.Add(metaName, meta.m_sValue);
                    }
                    else if (meta.Value != null)
                    {
                        var values = GetValuesForLanguage(language, meta.Value);
                        if (values.Count() == 1) languageMetas.Add(metaName, values.First());
                    }
                }
            }

            return languageMetas;
        }

        private static Dictionary<string, HashSet<string>> GetTagsForLanguage(LanguageObj language, List<Tags> tags)
        {
            if (tags == null || tags.Count == 0) return new Dictionary<string, HashSet<string>>();
            if (language.IsDefault)
            {
                return tags.ToDictionary(x => x.m_oTagMeta.m_sName, x => new HashSet<string>(x.m_lValues, StringComparer.OrdinalIgnoreCase));
            }

            return tags
                .Where(x => x.Values != null)
                .ToDictionary(x => x.m_oTagMeta.m_sName,
                    x => new HashSet<string>(x.Values.SelectMany(y => GetValuesForLanguage(language, y)).ToList(), StringComparer.OrdinalIgnoreCase));
        }

        private static Status ValidateBasicTopicIdsToRemove(CatalogGroupCache catalogGroupCache, HashSet<long> topicIds)
        {
            Status result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            if (topicIds != null && topicIds.Count > 0 && catalogGroupCache.TopicsMapBySystemNameAndByType != null && catalogGroupCache.TopicsMapBySystemNameAndByType.Count > 0)
            {
                List<long> basicMetaIds = catalogGroupCache.TopicsMapBySystemNameAndByType.Where(x => BasicMetasSystemNamesToType.ContainsKey(x.Key)
                                                                                                && x.Value.ContainsKey(BasicMetasSystemNamesToType[x.Key]))
                                                                                                .Select(x => x.Value[BasicMetasSystemNamesToType[x.Key]].Id).ToList();
                if (basicMetaIds != null && basicMetaIds.Count > 0)
                {
                    List<long> basicMetaIdsToRemove = basicMetaIds.Intersect(topicIds).ToList();
                    if (basicMetaIdsToRemove != null && basicMetaIdsToRemove.Count > 0)
                    {
                        result = new Status((int)eResponseStatus.CanNotRemoveBasicMetaIds, string.Format("{0} for the following Meta Ids: {1}",
                                            eResponseStatus.CanNotRemoveBasicMetaIds.ToString(), string.Join(",", basicMetaIdsToRemove)));
                    }
                }
            }

            return result;
        }

        private static GenericResponse<Asset> AddLinearMediaAsset(int groupId, MediaAsset mediaAsset, LiveAsset assetToAdd, long userId)
        {
            GenericResponse<Asset> result = new GenericResponse<Asset>();
            try
            {
                DataTable dt = CatalogDAL.InsertLinearMediaAsset(
                    groupId,
                    assetToAdd.EnableCdvrState,
                    assetToAdd.EnableCatchUpState,
                    assetToAdd.EnableRecordingPlaybackNonEntitledChannelState,
                    assetToAdd.EnableStartOverState,
                    assetToAdd.EnableTrickPlayState,
                    assetToAdd.BufferCatchUp,
                    assetToAdd.PaddingBeforeProgramStarts,
                    assetToAdd.PaddingAfterProgramEnds,
                    assetToAdd.BufferTrickPlay,
                    assetToAdd.ExternalCdvrId,
                    assetToAdd.ExternalEpgIngestId,
                    mediaAsset.Id,
                    assetToAdd.ChannelType,
                    userId);
                result.Object = MediaAssetServiceLazy.Value.CreateLinearMediaAsset(groupId, mediaAsset, dt);
                if (result.Object != null)
                {
                    result.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());

                    // UpdateIndex
                    bool indexingResult = IndexManagerFactory.Instance.GetIndexManager(groupId).UpsertMedia(result.Object.Id);
                    if (!indexingResult)
                    {
                        log.ErrorFormat("Failed UpsertMedia index for assetId: {0}, groupId: {1} after AddLinearMediaAsset", result.Object.Id, groupId);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddLinearMediaAsset for groupId: {0} and asset: {1}", groupId, assetToAdd.ToString()), ex);
            }

            return result;
        }

        private static GenericResponse<Asset> UpdateLinearMediaAsset(int groupId, MediaAsset mediaAsset, LiveAsset assetToUpdate, long userId, bool isForMigration = false)
        {
            GenericResponse<Asset> result = new GenericResponse<Asset>();
            try
            {
                DataTable dt = CatalogDAL.UpdateLinearMediaAsset(
                    groupId,
                    mediaAsset.Id,
                    assetToUpdate.EnableCdvrState,
                    assetToUpdate.EnableCatchUpState,
                    assetToUpdate.EnableRecordingPlaybackNonEntitledChannelState,
                    assetToUpdate.EnableStartOverState,
                    assetToUpdate.EnableTrickPlayState,
                    assetToUpdate.BufferCatchUp,
                    assetToUpdate.PaddingBeforeProgramStarts,
                    assetToUpdate.PaddingAfterProgramEnds,
                    assetToUpdate.BufferTrickPlay,
                    assetToUpdate.ExternalCdvrId,
                    assetToUpdate.ExternalEpgIngestId,
                    assetToUpdate.ChannelType,
                    userId);
                result.Object = MediaAssetServiceLazy.Value.CreateLinearMediaAsset(groupId, mediaAsset, dt, assetToUpdate.EpgChannelId);

                if (result.Object != null)
                {
                    result.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());

                    if (!isForMigration)
                    {
                        // UpdateIndex
                        bool indexingResult = IndexManagerFactory.Instance.GetIndexManager(groupId).UpsertMedia(result.Object.Id);
                        if (!indexingResult)
                        {
                            log.ErrorFormat("Failed UpsertMedia index for assetId: {0}, groupId: {1} after UpdateLinearMediaAsset", result.Object.Id, groupId);
                        }
                        //extracted it from upsertMedia it was called also for OPC accounts,searchDefinitions
                         //not sure it's required but better be safe
                         LayeredCache.Instance.SetInvalidationKey(
                             LayeredCacheKeys.GetMediaInvalidationKey(groupId, result.Object.Id));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateLinearMediaAsset for groupId: {0} and asset: {1}", groupId, assetToUpdate.ToString()), ex);
            }

            return result;
        }

        private static void SetInheritedValue(int groupId, CatalogGroupCache catalogGroupCache, AssetStruct assetStruct, MediaAsset asset)
        {
            // Add to asset.Metas, asset.Tags the missing values from parent
            var inherited = assetStruct.AssetStructMetas.Where(x => x.Value.IsInherited.HasValue && x.Value.IsInherited.Value).ToList();
            if (inherited != null && inherited.Count > 0)
            {
                Asset parentAsset = GetParentAsset(groupId, catalogGroupCache, assetStruct, asset);
                if (parentAsset == null)
                {
                    return;
                }

                List<Topic> topicsToInherit = new List<Topic>();
                foreach (var kvp in inherited)
                {
                    Topic topic = catalogGroupCache.TopicsMapById[kvp.Value.MetaId];
                    if (topic.Type == MetaType.Tag)
                    {
                        Tags tag = asset.Tags.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower().Equals(topic.SystemName.ToLower()));
                        Tags parentTag = parentAsset.Tags.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower().Equals(topic.SystemName.ToLower()));

                        if (tag != null)
                        {
                            if (!tag.Equals(parentTag))
                            {
                                asset.InheritancePolicy = AssetInheritancePolicy.Disable;
                                return;
                            }
                        }
                        else
                        {
                            topicsToInherit.Add(topic);
                        }
                    }
                    else
                    {
                        Metas meta = asset.Metas.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower().Equals(topic.SystemName.ToLower()));
                        Metas parentMeta = parentAsset.Metas.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower().Equals(topic.SystemName.ToLower()));

                        if (meta != null)
                        {
                            if (!meta.Equals(parentMeta))
                            {
                                asset.InheritancePolicy = AssetInheritancePolicy.Disable;
                                return;
                            }
                        }
                        else
                        {
                            topicsToInherit.Add(topic);
                        }
                    }
                }

                if (topicsToInherit.Count > 0 && assetStruct.ParentId.HasValue)
                {
                    foreach (var topic in topicsToInherit)
                    {
                        if (topic.Type == MetaType.Tag)
                        {
                            Tags tag = parentAsset.Tags.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower().Equals(topic.SystemName.ToLower()));
                            if (tag != null)
                            {
                                asset.Tags.Add(tag);
                            }
                        }
                        else
                        {
                            Metas meta = parentAsset.Metas.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower().Equals(topic.SystemName.ToLower()));
                            if (meta != null)
                            {
                                asset.Metas.Add(meta);
                            }
                        }
                    }
                }
            }
        }

        private static Asset GetParentAsset(int groupId, CatalogGroupCache catalogGroupCache, AssetStruct assetStruct, Asset asset)
        {
            Asset parentAsset = null;

            if (!assetStruct.ParentId.HasValue)
            {
                return parentAsset;
            }
            Topic parentConnectingTopic = catalogGroupCache.TopicsMapById[assetStruct.ConnectedParentMetaId.Value];
            Topic childConnectingTopic = catalogGroupCache.TopicsMapById[assetStruct.ConnectingMetaId.Value];

            string connectedValue = string.Empty;
            if (childConnectingTopic.Type == MetaType.Tag)
            {
                Tags t = asset.Tags.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower().Equals(childConnectingTopic.SystemName.ToLower()));
                if (t != null && t.m_lValues != null && t.m_lValues.Count > 0)
                {
                    connectedValue = t.m_lValues[0];
                }
            }
            else
            {
                Metas meta = asset.Metas.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower().Equals(childConnectingTopic.SystemName.ToLower()));
                if (meta != null)
                {
                    connectedValue = meta.m_sValue;
                }
            }

            if (!string.IsNullOrEmpty(connectedValue))
            {
                string filter = string.Format("(and asset_type='{0}' {1}='{2}')", assetStruct.ParentId.Value, parentConnectingTopic.SystemName, connectedValue);
                UnifiedSearchResult[] assets = Core.Catalog.Utils.SearchAssets(groupId, filter, 0, 0, false, true);
                if (assets != null && assets.Length > 0)
                {
                    GenericResponse<Asset> response = AssetManager.Instance.GetAsset(groupId, long.Parse(assets[0].AssetId), eAssetTypes.MEDIA, true);
                    if (response.Status.Code != (int)eResponseStatus.OK || response.Object == null)
                    {
                        log.ErrorFormat("Failed to get ");
                        return parentAsset;
                    }

                    parentAsset = response.Object;
                }
            }

            return parentAsset;
        }

        private static Status DeleteMediaAsset(int groupId, long mediaId, long userId, MediaAsset currentAsset, bool isFromChannel = false)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            if (!isFromChannel)
            {
                var status = AssetUserRuleManager.CheckAssetUserRuleList(groupId, userId, currentAsset);
                if (status == null || status.Code == (int)eResponseStatus.ActionIsNotAllowed)
                {
                    result.Set((int)eResponseStatus.ActionIsNotAllowed, ACTION_IS_NOT_ALLOWED);
                    return result;
                }
            }

            if (currentAsset == null)
            {
                result.Set((int)eResponseStatus.AssetDoesNotExist, eResponseStatus.AssetDoesNotExist.ToString());
                return result;
            }

            CatalogGroupCache catalogGroupCache;
            if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
            {
                log.Error($"failed to get catalogGroupCache for groupId: {groupId} when calling DeleteMediaAsset");
                return result;
            }

            var validateRespose = ValidateEditingAssetOfCategoryVersion(groupId, currentAsset, catalogGroupCache);
            if (!validateRespose.IsOkStatusCode())
            {
                result.Set(validateRespose);
                return result;
            }

            if (CatalogDAL.DeleteMediaAsset(groupId, mediaId, userId))
            {
                result.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                if (!isFromChannel)
                {
                    AssetStruct assetStruct = null;
                    if (currentAsset.MediaType.m_nTypeID > 0 && catalogGroupCache.AssetStructsMapById.ContainsKey(currentAsset.MediaType.m_nTypeID))
                    {
                        assetStruct = catalogGroupCache.AssetStructsMapById[currentAsset.MediaType.m_nTypeID];
                    }

                    if (assetStruct.SystemName == MANUAL_ASSET_STRUCT_NAME || assetStruct.SystemName == DYNAMIC_ASSET_STRUCT_NAME || assetStruct.SystemName == EXTERNAL_ASSET_STRUCT_NAME)
                    {
                        if (assetStruct.TopicsMapBySystemName == null || assetStruct.TopicsMapBySystemName.Count == 0)
                        {

                            assetStruct.TopicsMapBySystemName = catalogGroupCache.TopicsMapById.Where(x => assetStruct.MetaIds.Contains(x.Key))
                                                              .OrderBy(x => assetStruct.MetaIds.IndexOf(x.Key))
                                                              .ToDictionary(x => x.Value.SystemName, y => y.Value);

                        }

                        if (assetStruct.SystemName == EXTERNAL_ASSET_STRUCT_NAME)
                        {
                            DeleteExternalChannel(groupId, userId, currentAsset, assetStruct);
                        }
                        else
                        {
                            DeleteChannel(groupId, userId, currentAsset, assetStruct);
                        }
                    }

                    if (assetStruct.IsLinearAssetStruct)
                    {
                        DeleteChannelFromRegions(groupId, userId, currentAsset);
                    }
                }

                // Delete Index
                bool indexingResult = IndexManagerFactory.Instance.GetIndexManager(groupId).DeleteMedia((int)mediaId);
                if (!indexingResult)
                {
                    log.ErrorFormat("Failed to delete media index for assetId: {0}, groupId: {1} after DeleteAsset", mediaId, groupId);
                }
                //extracted it from upsertMedia it was called also for OPC accounts,searchDefinitions
                //not sure it's required but better be safe
                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetMediaInvalidationKey(groupId, mediaId));

            }
            else
            {
                log.ErrorFormat("Failed to delete media asset with id: {0}, groupId: {1}", mediaId, groupId);
            }

            return result;
        }

        private static void DeleteChannelFromRegions(int groupId, long userId, MediaAsset currentAsset)
        {
            var affectedRegions = CatalogDAL.DeleteChannelFromRegions(groupId, userId, currentAsset.Id);
            if (affectedRegions.Count == 0)
                return;

            log.Debug($"GroupId: {groupId}, Affected regions from linear asset: {currentAsset.Id} " +
                $"delete: {string.Join(",", affectedRegions)}");
            RegionManager.InvalidateRegions(groupId);
        }

        private static int GetTotalAmountOfDistinctAssets(List<BaseObject> assets)
        {
            int result = 0;
            try
            {
                result += assets.Where(x => x.AssetType == eAssetTypes.MEDIA).Select(x => x.AssetId).Distinct().Count();
                result += assets.Where(x => x.AssetType == eAssetTypes.EPG).Select(x => x.AssetId).Distinct().Count();
                result += assets.Where(x => x.AssetType == eAssetTypes.NPVR).Select(x => x.AssetId).Distinct().Count();
            }
            catch (Exception ex)
            {
                log.Error("Failed GetTotalAmountOfDistinctAssets", ex);
            }

            return result;
        }

        private static void UpdateChannel(int groupId, long userId, MediaAsset asset, AssetStruct assetStruct)
        {
            // Check assetStruct catalogId existence
            if (assetStruct.TopicsMapBySystemName.ContainsKey(AssetManager.CHANNEL_ID_META_SYSTEM_NAME))
            {
                if (asset != null && asset.Metas != null && asset.Metas.Count > 0)
                {
                    int channelId = 0;
                    var tagMeta = asset.Metas.Where(x => x.m_oTagMeta.m_sName == AssetManager.CHANNEL_ID_META_SYSTEM_NAME).FirstOrDefault();
                    if (tagMeta == null)
                    {
                        log.ErrorFormat("Error while update asset {0} channel. channelId is missing. groupId {1}", asset.Id, groupId);
                        return;
                    }

                    int.TryParse(tagMeta.m_sValue, out channelId);
                    if (channelId == 0)
                    {
                        log.ErrorFormat("Error while update asset {0} channel. channelId is missing. groupId {1}", asset.Id, groupId);
                        return;
                    }

                    var contextData = new ContextData(groupId) { UserId = userId };
                    GenericResponse<GroupsCacheManager.Channel> channelToUpdate = ChannelManager.Instance.GetChannelById(contextData, channelId, true);

                    if (!channelToUpdate.HasObject())
                    {
                        log.ErrorFormat("Failed UpdateChannel. channel not found. groupId {0}, channelId {1}", groupId, channelId);
                        return;
                    }

                    GroupsCacheManager.Channel channel = channelToUpdate.Object;

                    channel.m_sName = asset.Name;
                    channel.NamesInOtherLanguages = asset.NamesWithLanguages;
                    channel.m_sDescription = asset.Description;
                    channel.DescriptionInOtherLanguages = asset.DescriptionsWithLanguages;

                    GenericResponse<GroupsCacheManager.Channel> channelUpdateResponse = ChannelManager.Instance.UpdateChannel(groupId, channel.m_nChannelID, channel, UserSearchContext.GetByUserId(userId), false, true);

                    if (!channelUpdateResponse.IsOkStatusCode())
                    {
                        log.ErrorFormat("Failed update channelId {0}, groupId {1}, assetId {2}", channel.m_nChannelID, groupId, asset.Id);
                    }
                }
            }
        }

        private static void UpdateExternalChannel(int groupId, long userId, MediaAsset asset, AssetStruct assetStruct)
        {
            if (assetStruct.TopicsMapBySystemName.ContainsKey(AssetManager.CHANNEL_ID_META_SYSTEM_NAME))
            {
                if (asset != null && asset.Metas != null && asset.Metas.Count > 0)
                {
                    int channelId = 0;
                    var tagMeta = asset.Metas.Where(x => x.m_oTagMeta.m_sName == AssetManager.CHANNEL_ID_META_SYSTEM_NAME).FirstOrDefault();
                    if (tagMeta == null)
                    {
                        log.ErrorFormat("Error while update asset {0} External channel. channelId is missing. groupId {1}", asset.Id, groupId);
                        return;
                    }

                    int.TryParse(tagMeta.m_sValue, out channelId);
                    if (channelId == 0)
                    {
                        log.ErrorFormat("Error while update asset {0} External channel. channelId is missing. groupId {1}", asset.Id, groupId);
                        return;
                    }

                    //check external channel exist
                    ExternalChannel channelToUpdate = CatalogDAL.GetExternalChannelById(groupId, channelId);
                    if (channelToUpdate == null || channelToUpdate.ID <= 0)
                    {
                        log.ErrorFormat("Failed UpdateExternalChannel. External channel not found. groupId {0}, ExternalchannelId {1}", groupId, channelId);
                        return;
                    }

                    channelToUpdate.Name = asset.Name;

                    ExternalChannelResponse externalChannelResponse = Api.api.SetExternalChannel(groupId, channelToUpdate, userId, true);

                    if (externalChannelResponse == null || externalChannelResponse.ExternalChannel == null)
                    {
                        log.ErrorFormat("Failed update channelId{0}, groupId {1}, channelId {2}", channelToUpdate.ID, groupId);
                    }
                }
            }
        }

        private static void DeleteExternalChannel(int groupId, long userId, MediaAsset asset, AssetStruct assetStruct)
        {
            if (assetStruct.TopicsMapBySystemName.ContainsKey(AssetManager.CHANNEL_ID_META_SYSTEM_NAME))
            {
                if (asset != null && asset.Metas != null && asset.Metas.Count > 0)
                {
                    int channelId = 0;
                    var tagMeta = asset.Metas.Where(x => x.m_oTagMeta.m_sName == AssetManager.CHANNEL_ID_META_SYSTEM_NAME).FirstOrDefault();
                    if (tagMeta == null)
                    {
                        log.ErrorFormat("Error while delete asset {0} External channel. channelId is missing. groupId {1}", asset.Id, groupId);
                        return;
                    }

                    int.TryParse(tagMeta.m_sValue, out channelId);
                    if (channelId == 0)
                    {
                        log.ErrorFormat("Error while delete asset {0} External channel. channelId is missing. groupId {1}", asset.Id, groupId);
                        return;
                    }

                    //check external channel exist
                    ExternalChannel channelToUpdate = CatalogDAL.GetExternalChannelById(groupId, channelId);
                    if (channelToUpdate == null || channelToUpdate.ID <= 0)
                    {
                        log.ErrorFormat("Failed UpdateExternalChannel. External channel not found. groupId {0}, ExternalchannelId {1}", groupId, channelId);
                        return;
                    }

                    channelToUpdate.Name = asset.Name;

                    Status status = Api.api.DeleteExternalChannel(groupId, channelToUpdate.ID, userId, true);
                    if (status.Code != (int)eResponseStatus.OK)
                    {
                        log.ErrorFormat("Failed delete channelId {0}, groupId {1}, assetId {2}", channelId, groupId, asset.Id);

                    }
                }
            }

            return;
        }

        private static void DeleteChannel(int groupId, long userId, MediaAsset asset, AssetStruct assetStruct)
        {
            // Check assetStruct catalogId existence
            if (assetStruct.TopicsMapBySystemName.ContainsKey(CHANNEL_ID_META_SYSTEM_NAME))
            {
                if (asset != null && asset.Metas != null && asset.Metas.Count > 0)
                {
                    int channelId = 0;
                    var tagMeta = asset.Metas.Where(x => x.m_oTagMeta.m_sName == CHANNEL_ID_META_SYSTEM_NAME).FirstOrDefault();
                    if (tagMeta == null)
                    {
                        log.ErrorFormat("Error while delete asset {0} channel. channelId is missing. groupId {1}", asset.Id, groupId);
                        return;
                    }

                    int.TryParse(tagMeta.m_sValue, out channelId);
                    if (channelId == 0)
                    {
                        log.ErrorFormat("Error while delete asset {0} channel. channelId is missing. groupId {1}", asset.Id, groupId);
                        return;
                    }

                    Status status = ChannelManager.Instance.DeleteChannel(groupId, channelId, userId, true);
                    if (status.Code != (int)eResponseStatus.OK)
                    {
                        log.ErrorFormat("Failed delete  channelId {0}, groupId {1}, assetId {2}", channelId, groupId, asset.Id);

                    }
                }
            }

            return;
        }

        private static Status ValidateRelatedEntities(int groupId, CatalogGroupCache catalogGroupCache, HashSet<long> assetStructMetaIds,
            List<RelatedEntities> relatedEntitiesList, ref XmlDocument relatedEntitiesXmlDoc)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            HashSet<string> tempHashSet = new HashSet<string>();

            if (relatedEntitiesList?.Count > 0)
            {
                relatedEntitiesXmlDoc = new XmlDocument();
                XmlNode rootNode = relatedEntitiesXmlDoc.CreateElement("root");
                relatedEntitiesXmlDoc.AppendChild(rootNode);

                foreach (RelatedEntities relatedEntities in relatedEntitiesList)
                {
                    // validate duplicates do not exist
                    if (tempHashSet.Contains(relatedEntities.TagMeta.m_sName))
                    {
                        result.Message = string.Format("Duplicate relatedEntities sent, relatedEntities name: {0}", relatedEntities.TagMeta.m_sName);
                        return result;
                    }

                    tempHashSet.Add(relatedEntities.TagMeta.m_sName);

                    //validate relatedEntity exists on group
                    if (!catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(relatedEntities.TagMeta.m_sName)
                        || !catalogGroupCache.TopicsMapBySystemNameAndByType[relatedEntities.TagMeta.m_sName].ContainsKey(relatedEntities.TagMeta.m_sType))
                    {
                        result.Message = string.Format("relatedEntities: {0} does not exist for group", relatedEntities.TagMeta.m_sName);
                        return result;
                    }

                    // validate meta exists on asset struct
                    if (!assetStructMetaIds.Contains(catalogGroupCache.TopicsMapBySystemNameAndByType[relatedEntities.TagMeta.m_sName][relatedEntities.TagMeta.m_sType].Id))
                    {
                        result.Message = string.Format("relatedEntities: {0} is not part of assetStruct", relatedEntities.TagMeta.m_sName);
                        return result;
                    }

                    Topic topic = catalogGroupCache.TopicsMapBySystemNameAndByType[relatedEntities.TagMeta.m_sName][relatedEntities.TagMeta.m_sType];
                    // validate correct type was sent
                    if (topic.Type.ToString().ToLower() != relatedEntities.TagMeta.m_sType.ToLower())
                    {
                        result = new Status((int)eResponseStatus.InvalidMetaType, string.Format("{0} was sent for relatedEntities: {1}", eResponseStatus.InvalidMetaType.ToString(), relatedEntities.TagMeta.m_sName));
                        return result;
                    }

                    string value = relatedEntities.Items?.Count > 0 ? JsonConvert.SerializeObject(relatedEntities.Items) : string.Empty;

                    AddRealtedEntitiesValueToXml(ref relatedEntitiesXmlDoc, rootNode, topic.Id, value);
                }
            }

            result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            return result;
        }

        private static Status ValidateRelatedEntitiesLimitaion(List<RelatedEntities> relatedEntitiesToAdd, List<RelatedEntities> relatedEntitiesToUpdate)
        {
            var relatedEntitiesToAddCount = relatedEntitiesToAdd?.Count ?? 0;
            var relatedEntitiesToUpdateCount = relatedEntitiesToUpdate?.Count ?? 0;

            if (relatedEntitiesToAddCount + relatedEntitiesToUpdateCount > 5)
            {
                return new Status() { Code = (int)eResponseStatus.RelatedEntitiesExceedLimitation };
            }

            return Status.Ok;
        }

        private static void AddRealtedEntitiesValueToXml(ref XmlDocument relatedEntitiesXmlDoc, XmlNode rootNode, long topicId, string value)
        {
            if (value != null)
            {
                XmlNode rowNode;
                XmlNode topicIdNode;
                XmlNode valueNode;
                rowNode = relatedEntitiesXmlDoc.CreateElement("row");
                topicIdNode = relatedEntitiesXmlDoc.CreateElement("topic_id");
                topicIdNode.InnerText = topicId.ToString();
                rowNode.AppendChild(topicIdNode);
                valueNode = relatedEntitiesXmlDoc.CreateElement("value");
                valueNode.InnerText = value;
                rowNode.AppendChild(valueNode);
                rootNode.AppendChild(rowNode);
            }
        }

        private static Status BuildTableDicAfterInsertMediaAsset(DataSet ds, out Dictionary<string, DataTable> tables, bool isMinimalOutput)
        {
            var status = BuildTableDictFromDataSet(ds, out tables, isMinimalOutput);

            if (!status.IsOkStatusCode())
            {
                log.Warn($"{nameof(BuildTableDicAfterInsertMediaAsset)} failed to build tables from dataset");
                return status;
            }

            return new Status((int)eResponseStatus.OK);
        }

        private static Status BuildTableDicAfterUpdateMediaAsset(DataSet ds, long assetId, out Dictionary<string, DataTable> tables, bool isMinimalOutput)
        {
            var status = BuildTableDictFromDataSet(ds, out tables, isMinimalOutput);

            if (!status.IsOkStatusCode())
            {
                log.Warn($"{nameof(BuildTableDicAfterUpdateMediaAsset)} failed to build tables from dataset associated with assetId {assetId}");
                return status;
            }

            return new Status((int)eResponseStatus.OK);
        }

        private static Status BuildTableDictFromDataSet(DataSet ds, out Dictionary<string, DataTable> tables,
            bool isMinimalOutput)
        {
            tables = null;

            if (ds == null || ds.Tables == null)
            {
                log.ErrorFormat($"{nameof(BuildTableDictFromDataSet)} ds is empty");
                return new Status((int)eResponseStatus.Error);
            }

            // Basic details tables
            if (ds.Tables[0] == null || ds.Tables[0].Rows == null || ds.Tables[0].Rows.Count != 1)
            {
                log.WarnFormat($"{nameof(BuildTableDictFromDataSet)} - basic details table is not valid");
                return new Status((int)eResponseStatus.Error);
            }

            DataRow basicDataRow = ds.Tables[0].Rows[0];
            long id = ODBCWrapper.Utils.GetLongSafeVal(basicDataRow, "ID", 0);
            if (id <= 0)
            {
                return CreateAssetResponseStatusFromResult(id);
            }

            if (!isMinimalOutput && ds.Tables.Count < 9)
            {
                log.Warn($"{nameof(BuildTableDictFromDataSet)} received dataset with {ds.Tables.Count} but expects 9 or more.");
                return new Status((int)eResponseStatus.Error);
            }

            if (isMinimalOutput && ds.Tables.Count < 1)
            {
                log.Warn($"{nameof(BuildTableDictFromDataSet)} received dataset with {ds.Tables.Count} but expects 1 or more.");
                return new Status((int)eResponseStatus.Error);
            }

            tables = new Dictionary<string, DataTable>();
            tables.Add(TABLE_NAME_BASIC, ds.Tables[0]);
            tables.Add(TABLE_NAME_METAS, GetDataTableByIndex(ds, 1, isMinimalOutput));
            tables.Add(TABLE_NAME_TAGS, GetDataTableByIndex(ds, 2, isMinimalOutput));
            tables.Add(TABLE_NAME_FILES, GetDataTableByIndex(ds, 3, isMinimalOutput));
            tables.Add(TABLE_NAME_FILES_LABELS, GetDataTableByIndex(ds, 4, isMinimalOutput));
            if (ds.Tables.Count <= 9)
            {
                // TODO
                // This branch is used for backward compatibility and can be deleted
                // in the future when MediaFile's dynamic data is delivered.
                tables.Add(TABLE_NAME_FILES_DYNAMIC_DATA, new DataTable());
                tables.Add(TABLE_NAME_IMAGES, GetDataTableByIndex(ds, 5, isMinimalOutput));
                tables.Add(TABLE_NAME_NEW_TAGS, GetDataTableByIndex(ds, 6, isMinimalOutput));
                tables.Add(TABLE_NAME_RELATED_ENTITIES, GetDataTableByIndex(ds, 7, isMinimalOutput));
                tables.Add(TABLE_NAME_LIVE_TO_VOD, GetDataTableByIndex(ds, 8, isMinimalOutput));
            }
            else if (ds.Tables.Count == 10)
            {
                tables.Add(TABLE_NAME_FILES_DYNAMIC_DATA, GetDataTableByIndex(ds, 5, isMinimalOutput));
                tables.Add(TABLE_NAME_IMAGES, GetDataTableByIndex(ds, 6, isMinimalOutput));
                tables.Add(TABLE_NAME_NEW_TAGS, GetDataTableByIndex(ds, 7, isMinimalOutput));
                tables.Add(TABLE_NAME_RELATED_ENTITIES, GetDataTableByIndex(ds, 8, isMinimalOutput));
                tables.Add(TABLE_NAME_LIVE_TO_VOD, GetDataTableByIndex(ds, 9, isMinimalOutput));
            }

            return new Status((int)eResponseStatus.OK);
        }

        private static DataTable GetDataTableByIndex(DataSet ds, int index, bool isMinimalOutput)
        {
            return isMinimalOutput
                ? new DataTable()
                : ds.Tables[index];
        }

        private static Status BuildTableDicAfterGetMediaAssetForElasitcSearch(DataSet ds, long mediaId, out Dictionary<string, DataTable> tables)
        {
            tables = null;
            if (ds == null || ds.Tables == null)
            {
                log.WarnFormat("GetMediaForElasticSearchIndex - dataset or tables are null. MediaId :{0}", mediaId);
                return new Status((int)eResponseStatus.Error);
            }

            // Basic details tables
            if (ds.Tables[0] == null || ds.Tables[0].Rows == null || ds.Tables[0].Rows.Count != 1)
            {
                log.WarnFormat("GetMediaForElasticSearchIndex - basic details table is not valid. MediaId :{0}", mediaId);
                return new Status((int)eResponseStatus.Error);
            }

            tables = new Dictionary<string, DataTable>();
            tables.Add(TABLE_NAME_BASIC, ds.Tables[0]);
            tables.Add(TABLE_NAME_METAS, ds.Tables[1]);
            tables.Add(TABLE_NAME_TAGS, ds.Tables[2]);
            tables.Add(TABLE_NAME_FILES, ds.Tables[3]);
            tables.Add(TABLE_NAME_UPDATE_DATE, ds.Tables[5]);
            tables.Add(TABLE_NAME_GEO_AVAILABILITY, ds.Tables[6]);
            tables.Add(TABLE_NAME_LIVE_TO_VOD, ds.Tables[7]);

            return new Status((int)eResponseStatus.OK);
        }

        private static Status ValidateNoneExistingTopicIdsToRemove(MediaAsset mediaAsset, HashSet<long> topicIds, CatalogGroupCache catalogGroupCache)
        {
            var result = Status.Ok;

            List<long> existingTopicsIds = mediaAsset.Metas.Where(x => catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(x.m_oTagMeta.m_sName)
                                                                        && catalogGroupCache.TopicsMapBySystemNameAndByType[x.m_oTagMeta.m_sName].ContainsKey(x.m_oTagMeta.m_sType))
                                                                        .Select(x => catalogGroupCache.TopicsMapBySystemNameAndByType[x.m_oTagMeta.m_sName][x.m_oTagMeta.m_sType].Id).ToList();
            existingTopicsIds.AddRange(mediaAsset.Tags.Where(x => catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(x.m_oTagMeta.m_sName)
                                                            && catalogGroupCache.TopicsMapBySystemNameAndByType[x.m_oTagMeta.m_sName].ContainsKey(x.m_oTagMeta.m_sType))
                                                            .Select(x => catalogGroupCache.TopicsMapBySystemNameAndByType[x.m_oTagMeta.m_sName][x.m_oTagMeta.m_sType].Id).ToList());
            if (mediaAsset.RelatedEntities != null)
            {
                existingTopicsIds.AddRange(mediaAsset.RelatedEntities.Where(x => catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(x.TagMeta.m_sName)
                                                                && catalogGroupCache.TopicsMapBySystemNameAndByType[x.TagMeta.m_sName].ContainsKey(x.TagMeta.m_sType))
                                                                .Select(x => catalogGroupCache.TopicsMapBySystemNameAndByType[x.TagMeta.m_sName][x.TagMeta.m_sType].Id).ToList());
            }

            List<long> noneExistingMetaIds = topicIds.Except(existingTopicsIds).ToList();
            if (noneExistingMetaIds != null && noneExistingMetaIds.Count > 0)
            {
                result.Set(eResponseStatus.MetaIdsDoesNotExistOnAsset,
                           string.Format("{0} for the following Meta Ids: {1}", eResponseStatus.MetaIdsDoesNotExistOnAsset.ToString(), string.Join(",", noneExistingMetaIds)));
            }

            return result;
        }

        private static void RemoveNoneExistingTopicIds(HashSet<long> topicIds, Asset asset, CatalogGroupCache catalogGroupCache)
        {
            List<long> topicsToRemove = new List<long>();
            foreach (var topicId in topicIds)
            {
                if (!catalogGroupCache.TopicsMapById.ContainsKey(topicId))
                {
                    topicsToRemove.Add(topicId);
                    continue;
                }

                var topic = catalogGroupCache.TopicsMapById[topicId];

                if (topic.Type == MetaType.Tag)
                {
                    if (!asset.Tags.Any(x => x.m_oTagMeta.m_sName.Equals(topic.SystemName) && x.m_oTagMeta.m_sType.Equals(topic.Type.ToString())))
                    {
                        topicsToRemove.Add(topicId);
                    }
                    continue;
                }

                if (topic.Type == MetaType.ReleatedEntity)
                {
                    if (!asset.RelatedEntities.Any(x => x.TagMeta.m_sName.Equals(topic.SystemName) && x.TagMeta.m_sType.Equals(topic.Type.ToString())))
                    {
                        topicsToRemove.Add(topicId);
                    }
                    continue;
                }

                // all other metaTypes
                if (!asset.Metas.Any(x => x.m_oTagMeta.m_sName.Equals(topic.SystemName) && x.m_oTagMeta.m_sType.Equals(topic.Type.ToString())))
                {
                    topicsToRemove.Add(topicId);
                }
            }

            foreach (var topicToRemove in topicsToRemove)
            {
                topicIds.Remove(topicToRemove);
            }
        }

        public static ObjectVirtualAssetInfo GetObjectVirtualAssetInfo(int groupId, ObjectVirtualAssetInfoType objectVirtualAssetInfoType)
        {
            ObjectVirtualAssetInfo objectVirtualAssetInfo = VirtualAssetPartnerConfigManager.Instance.GetObjectVirtualAssetInfo(groupId, objectVirtualAssetInfoType);

            if (objectVirtualAssetInfo == null)
            {
                log.Debug($"No objectVirtualAssetInfo for groupId {groupId}. virtualAssetInfo.Type {objectVirtualAssetInfoType}");
                return objectVirtualAssetInfo;
            }

            return objectVirtualAssetInfo;
        }

        public static VirtualAssetInfoResponse GetVirtualAsset(int groupId, VirtualAssetInfo virtualAssetInfo, out bool needToCreateVirtualAsset, out Asset asset)
        {
            VirtualAssetInfoResponse response = new VirtualAssetInfoResponse() { Status = VirtualAssetInfoStatus.NotRelevant };
            asset = null;
            needToCreateVirtualAsset = false;

            if (virtualAssetInfo.DuplicateAssetId.HasValue)
            {
                GenericResponse<Asset> duplicatedAssetResponse = Instance.GetAsset(groupId, virtualAssetInfo.DuplicateAssetId.Value, eAssetTypes.MEDIA, true);
                if (!duplicatedAssetResponse.HasObject())
                {
                    log.Debug($"GetVirtualAsset. duplicated virtual Asset not found. groupId {groupId}, DuplicateAssetId {virtualAssetInfo.DuplicateAssetId}");
                    response.Status = VirtualAssetInfoStatus.Error;
                    return response;
                }

                response.Status = VirtualAssetInfoStatus.OK;
                response.AssetId = virtualAssetInfo.DuplicateAssetId.Value;
                asset = duplicatedAssetResponse.Object;
                return response;
            }

            ObjectVirtualAssetInfo objectVirtualAssetInfo = GetObjectVirtualAssetInfo(groupId, virtualAssetInfo.Type);
            if (objectVirtualAssetInfo == null)
            {
                return response;
            }

            if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out CatalogGroupCache catalogGroupCache))
            {
                log.Error($"failed to get catalogGroupCache for groupId: {groupId} when calling GetVirtualAsset");
                response.Status = VirtualAssetInfoStatus.Error;
                return response;
            }

            string assetTypeQuery = $"asset_type='{objectVirtualAssetInfo.AssetStructId}'";

            if (virtualAssetInfo.withExtendedTypes && objectVirtualAssetInfo.ExtendedTypes?.Count > 0)
            {
                StringBuilder assetType = new StringBuilder($"(or {assetTypeQuery}");
                foreach (var item in objectVirtualAssetInfo.ExtendedTypes.Values)
                {
                    assetType.Append($" asset_type='{item}'");
                }

                assetType.Append(")");

                assetTypeQuery = assetType.ToString();
            }

            // build ElasticSearch filter
            string filter = $"(and {catalogGroupCache.TopicsMapById[objectVirtualAssetInfo.MetaId].SystemName}='{virtualAssetInfo.Id}' {assetTypeQuery})";

            UnifiedSearchResult[] assets = Core.Catalog.Utils.SearchAssets(groupId, filter, 0, 0, false, false);

            if (assets == null || assets.Length == 0)
            {
                log.Debug($"GetVirtualAsset. Asset not found. groupId {groupId}, virtualAssetInfo {virtualAssetInfo}");
                needToCreateVirtualAsset = true;
                response.Status = VirtualAssetInfoStatus.OK;
                return response;
            }

            GenericResponse<Asset> assetResponse = AssetManager.Instance.GetAsset(groupId, long.Parse(assets[0].AssetId), eAssetTypes.MEDIA, true);

            if (!assetResponse.HasObject())
            {
                log.Debug($"GetVirtualAsset. virtual Asset not found. groupId {groupId}, virtualAssetInfo {virtualAssetInfo}");
                response.Status = VirtualAssetInfoStatus.Error;
                return response;
            }

            response.Status = VirtualAssetInfoStatus.OK;
            response.AssetId = assetResponse.Object.Id;
            asset = assetResponse.Object;
            return response;
        }

        private static Status ValidateEditingAssetOfCategoryVersion(int groupId, MediaAsset currentAsset, CatalogGroupCache catalogGroupCache)
        {
            var status = Status.Ok;

            ObjectVirtualAssetInfo objectVirtualAssetInfo = GetObjectVirtualAssetInfo(groupId, ObjectVirtualAssetInfoType.Category);
            if (objectVirtualAssetInfo != null)
            {
                bool isCategoryStruct = currentAsset.MediaType.m_nTypeID == objectVirtualAssetInfo.AssetStructId;
                if (!isCategoryStruct && objectVirtualAssetInfo.ExtendedTypes != null)
                {
                    isCategoryStruct = objectVirtualAssetInfo.ExtendedTypes.Values.Any(x => x == currentAsset.MediaType.m_nTypeID);
                }

                if (isCategoryStruct)
                {
                    var meta = catalogGroupCache.TopicsMapById[objectVirtualAssetInfo.MetaId];
                    var categoryMeta = currentAsset.Metas.FirstOrDefault(x => x.m_oTagMeta.m_sName == meta.SystemName);
                    if (categoryMeta != null)
                    {
                        if (long.TryParse(categoryMeta.m_sValue, out long categoryItemId))
                        {
                            var categoryItemResponse = CategoryCache.Instance.GetCategoryItem(groupId, categoryItemId);
                            if (categoryItemResponse.HasObject() && categoryItemResponse.Object.VersionId.HasValue)
                            {
                                var categoryVersionResponse = CategoryCache.Instance.GetCategoryVersion(groupId, categoryItemResponse.Object.VersionId.Value);
                                if (categoryVersionResponse.HasObject() && categoryVersionResponse.Object.State != CategoryVersionState.Draft)
                                {
                                    status.Set(eResponseStatus.CategoryVersionIsNotDraft,
                                               $"Cannot edit media asset {currentAsset.Id} with a categoryItem {categoryItemId} in version {categoryVersionResponse.Object.Id} state {categoryVersionResponse.Object.State}");
                                    return status;
                                }
                            }
                        }
                    }
                }
            }

            return status;
        }

        #endregion

        #region Public Methods

        public static MediaObj GetMediaObj(int groupId, long id)
        {
            MediaObj result = null;
            try
            {
                if (id > 0)
                {
                    CatalogGroupCache catalogGroupCache;
                    if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetAsset", groupId);
                        return result;
                    }

                    // isAllowedToViewInactiveAssets = false due to backward compatibility
                    List<Asset> assets = AssetManager.GetAssets(groupId, new List<KeyValuePair<eAssetTypes, long>>() { new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, id) }, false);
                    if (assets != null && assets.Count == 1)
                    {
                        result = new MediaObj(groupId, assets[0] as MediaAsset);
                        if (assets[0] is LiveAsset)
                        {
                            //BEO-8950
                            var liveAsset = assets[0] as LiveAsset;

                            result.m_ExternalIDs = liveAsset.EpgChannelId.ToString();
                            result.EnableCDVR = liveAsset.CdvrEnabled;
                            result.EnableCatchUp = liveAsset.CatchUpEnabled;
                            result.EnableStartOver = liveAsset.StartOverEnabled;
                            result.EnableTrickPlay = liveAsset.TrickPlayEnabled;
                            result.EnableRecordingPlaybackNonEntitledChannel = liveAsset.RecordingPlaybackNonEntitledChannelEnabled;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAsset for groupId: {0} and id: {1}", groupId, id), ex);
            }

            return result;
        }

        public GenericResponse<Asset> GetAsset(int groupId, long id, eAssetTypes assetType, bool isAllowedToViewInactiveAssets)
        {
            GenericResponse<Asset> result = new GenericResponse<Asset>();
            try
            {
                if (id > 0 && assetType != eAssetTypes.UNKNOWN)
                {
                    List<Asset> assets = GetAssets(groupId, new List<KeyValuePair<eAssetTypes, long>>() { new KeyValuePair<eAssetTypes, long>(assetType, id) }, isAllowedToViewInactiveAssets);
                    if (assets == null || assets.Count != 1 || assets[0] == null)
                    {
                        log.ErrorFormat("Failed getting asset from GetAssetFromCache, for groupId: {0}, id: {1}, assetType: {2}", groupId, id, assetType.ToString());
                        result.SetStatus(eResponseStatus.AssetDoesNotExist, eResponseStatus.AssetDoesNotExist.ToString());
                    }
                    else
                    {
                        result.Object = assets[0];
                        result.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAsset for groupId: {0}, id: {1}, assetType: {2}", groupId, id, assetType.ToString()), ex);
            }

            return result;
        }

        public static List<Asset> GetAssets(int groupId, List<KeyValuePair<eAssetTypes, long>> assets, bool isAllowedToViewInactiveAssets, bool isAllowedToViewDeletedAssets = true, Dictionary<string, string> epgIdToDocumentId = null)
        {
            List<Asset> result = null;
            try
            {
                if (assets != null && assets.Count > 0)
                {
                    result = GetAssetsFromCache(groupId, assets, isAllowedToViewInactiveAssets, epgIdToDocumentId);

                    if (!isAllowedToViewDeletedAssets && result?.Count > 0)
                    {
                        result = result.Where(x => x.IndexStatus != AssetIndexStatus.Deleted).ToList();
                    }

                    if (result == null || result.Count != assets.Count)
                    {
                        log.ErrorFormat("Failed getting assets from GetAssetsFromCache, for groupId: {0}, assets: {1}", groupId,
                                        assets != null ? string.Join(",", assets.Select(x => string.Format("{0}_{1}", x.Key, x.Value)).ToList()) : string.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAssets for groupId: {0}, assets: {1}", groupId,
                                        assets != null ? string.Join(",", assets.Select(x => string.Format("{0}_{1}", x.Key, x.Value)).ToList()) : string.Empty), ex);
            }

            return result;
        }

        public static GenericListResponse<AssetPriority> GetOrderedAssets(
            int groupId,
            List<BaseObject> assets,
            bool isAllowedToViewInactiveAssets,
            IReadOnlyDictionary<double, SearchPriorityGroup> priorityGroupsMapping = null)
        {
            var resultScore = new GenericListResponse<AssetPriority>();

            try
            {
                if (assets != null && assets.Count > 0)
                {
                    var assetsToRetrieve = new List<KeyValuePair<eAssetTypes, long>>();
                    var items = new HashSet<string>();

                    var recordingsMap = new Dictionary<string, RecordingSearchResult>();

                    var epgIdToDocumentId = new Dictionary<string, string>();
                    var epgFeatureVersion = GroupSettingsManager.Instance.GetEpgFeatureVersion(groupId);

                    foreach (var item in assets)
                    {
                        var assetType = item.AssetType;
                        var assetId = item.AssetId;

                        if (item.AssetType == eAssetTypes.NPVR)
                        {
                            RecordingSearchResult rsr = (RecordingSearchResult)item;
                            recordingsMap.Add(item.AssetId, rsr);

                            if (!string.IsNullOrEmpty(rsr.EpgId))
                            {
                                assetId = rsr.EpgId;
                            }
                            else
                            {
                                //fallback? Get from recordings
                                log.Debug($"GetOrderedAssets, couldn't find epgId for asset id: {item.AssetId}");
                            }

                            assetType = eAssetTypes.EPG;
                        }
                        else if (item.AssetType == eAssetTypes.EPG && epgFeatureVersion != EpgFeatureVersion.V1 && item is EpgSearchResult)
                        {
                            var epgSearchResult = item as EpgSearchResult;
                            if (!string.IsNullOrEmpty(epgSearchResult.DocumentId))
                            {
                                epgIdToDocumentId.Add(item.AssetId, epgSearchResult.DocumentId);
                            }
                        }

                        var key = string.Format("{0}_{1}", assetType.ToString(), assetId);

                        if (!items.Contains(key))
                        {
                            items.Add(key);
                            assetsToRetrieve.Add(new KeyValuePair<eAssetTypes, long>(assetType, long.Parse(assetId)));
                        }
                    }

                    int totalAmountOfDistinctAssets = assetsToRetrieve.Count;

                    var unOrderedAssets = GetAssets(groupId, assetsToRetrieve, isAllowedToViewInactiveAssets, true, epgIdToDocumentId);

                    if (!isAllowedToViewInactiveAssets && (unOrderedAssets == null || unOrderedAssets.Count == 0))
                    {
                        resultScore.SetStatus(eResponseStatus.OK);
                        return resultScore;
                    }

                    var keyFormat = "{0}_{1}"; // mapped asset key format = assetType_assetId
                    var mappedAssets = unOrderedAssets.ToDictionary(x => string.Format(keyFormat, x.AssetType.ToString(), x.Id), x => x);
                    var priorityGroupGetter = priorityGroupsMapping == null ? (Func<BaseObject, long?>)(_ => null) : GetPriorityGroupId;
                    foreach (var baseAsset in assets)
                    {
                        var isNpvr = baseAsset.AssetType == eAssetTypes.NPVR;

                        AssetPriority assetPriority = null;
                        if (!isNpvr)
                        {
                            var key = string.Format(keyFormat, baseAsset.AssetType.ToString(), baseAsset.AssetId);
                            if (!mappedAssets.Keys.Contains(key))
                            {
                                log.DebugFormat("GetOrderedAssets: Asset {0} with Key {1} not found in mapped assets", baseAsset.AssetId, key);
                                continue;
                            }

                            assetPriority = new AssetPriority(mappedAssets[key], priorityGroupGetter(baseAsset));
                        }
                        else if (recordingsMap.ContainsKey(baseAsset.AssetId))
                        {
                            var key = string.Format(keyFormat, eAssetTypes.EPG.ToString(), recordingsMap[baseAsset.AssetId].EpgId);
                            if (!mappedAssets.ContainsKey(key))
                            {
                                log.DebugFormat("GetOrderedAssets: NPVR asset {0} with Key {1} not found in mapped assets", baseAsset.AssetId, key);
                                continue;
                            }

                            assetPriority = new AssetPriority(mappedAssets[key], priorityGroupGetter(baseAsset));
                        }

                        if (assetPriority.Asset.IndexStatus == AssetIndexStatus.Deleted)
                        {
                            resultScore.Objects.Add(assetPriority);
                            continue;
                        }

                        if (!isAllowedToViewInactiveAssets
                            || isNpvr
                            || Math.Abs((baseAsset.m_dUpdateDate - assetPriority.Asset.UpdateDate.Value).TotalSeconds) <= 1)
                        {
                            if (isNpvr)
                            {
                                RecordingAsset recordingAsset = new RecordingAsset((EpgAsset)assetPriority?.Asset);
                                recordingAsset.RecordingId = baseAsset.AssetId;
                                recordingAsset.RecordingType = recordingsMap[baseAsset.AssetId].RecordingType;
                                recordingAsset.IsMulti = recordingsMap[baseAsset.AssetId].IsMulti;

                                assetPriority = new AssetPriority(recordingAsset, priorityGroupGetter(baseAsset));
                                resultScore.Objects.Add(assetPriority);
                            }
                            else
                            {
                                resultScore.Objects.Add(assetPriority);
                            }
                        }
                        else
                        {
                            assetPriority.Asset.IndexStatus = AssetIndexStatus.NotUpdated;
                            resultScore.Objects.Add(assetPriority);
                            log.DebugFormat("Get NotUpdated Asset {0}, groupId {1}", assetPriority.Asset.Id, groupId);
                        }
                    }

                    resultScore.SetStatus(eResponseStatus.OK);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetOrderedAssets for groupId: {0}, assets: {1}", groupId,
                                        assets != null ? string.Join(",", assets.Select(x => string.Format("{0}_{1}", x.AssetType.ToString(), x.AssetId)).ToList()) : string.Empty), ex);
            }

            return resultScore;

            long? GetPriorityGroupId(BaseObject baseObject)
            {
                if (!(baseObject is UnifiedSearchResult searchResult))
                {
                    return null;
                }

                if (!priorityGroupsMapping.TryGetValue(searchResult.Score, out var searchPriorityGroup))
                {
                    return null;
                }

                return searchPriorityGroup.Id;
            }
        }

        /// <summary>
        /// Returns dictionary of [assetId, [language, media]]
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static Dictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>> GetMediaForElasticSearchIndex(int groupId, long mediaId)
        {
            // <assetId, <languageId, media>>
            Dictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>> result = new Dictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>>();
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetMediaForElasticSearchIndex", groupId);
                    return result;
                }

                DataSet ds = CatalogDAL.GetMediaAssetForElasitcSearch(groupId, mediaId, catalogGroupCache.GetDefaultLanguage().ID);

                Dictionary<string, DataTable> tables = null;
                Status status = BuildTableDicAfterGetMediaAssetForElasitcSearch(ds, mediaId, out tables);
                if (status.Code != (int)eResponseStatus.OK)
                {
                    return result;
                }

                var mediaAsset = MediaAssetServiceLazy.Value.CreateMediaAsset(
                    groupId,
                    tables[TABLE_NAME_BASIC],
                    tables[TABLE_NAME_METAS],
                    tables[TABLE_NAME_TAGS],
                    null,
                    tables[TABLE_NAME_FILES],
                    null,
                    null,
                    null,
                    tables[TABLE_NAME_UPDATE_DATE],
                    null,
                    null,
                    tables[TABLE_NAME_LIVE_TO_VOD],
                    true);
                if (mediaAsset != null)
                {
                    EnumerableRowCollection<DataRow> assetFileTypes = null;
                    if (ds != null && ds.Tables != null && ds.Tables[3] != null && ds.Tables[3].Rows != null && ds.Tables[3].Rows.Count > 0)
                    {
                        assetFileTypes = ds.Tables[3].AsEnumerable();
                    }

                    var linearChannelsRegionsMapping = RegionManager.Instance.GetLinearMediaRegions(groupId);
                    log.Debug($"GetMediaForElasticSearchIndex -> Got linearChannelsRegionsMapping with {linearChannelsRegionsMapping?.Count} medias");

                    Dictionary<int, ApiObjects.SearchObjects.Media> assets = CreateMediasFromMediaAssetAndLanguages(groupId, mediaAsset, assetFileTypes, catalogGroupCache, linearChannelsRegionsMapping);

                    if (ds != null && ds.Tables != null && ds.Tables.Count > 6 && ds.Tables[6] != null && ds.Tables[6].Rows != null && ds.Tables[6].Rows.Count > 0)
                    {
                        foreach (DataRow row in ds.Tables[6].Rows)
                        {
                            int countryId = ODBCWrapper.Utils.GetIntSafeVal(row, "COUNTRY_ID");
                            if (countryId > 0)
                            {
                                int isAllowed = ODBCWrapper.Utils.GetIntSafeVal(row, "IS_ALLOWED");
                                if (isAllowed > 0)
                                {
                                    foreach (var asset in assets.Values)
                                    {
                                        asset.allowedCountries.Add(countryId);
                                    }
                                }
                                else
                                {
                                    foreach (var asset in assets.Values)
                                    {
                                        asset.blockedCountries.Add(countryId);
                                    }
                                }
                            }
                        }
                    }

                    // If no allowed countries were found for this media - use 0, that indicates that the media is allowed everywhere
                    foreach (var asset in assets.Values)
                    {
                        if (asset.allowedCountries.Count == 0)
                        {
                            asset.allowedCountries.Add(0);
                        }
                    }

                    result.Add((int)mediaAsset.Id, assets);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetMediaForElasticSearchIndex for groupId: {0}", groupId), ex);
            }

            if (result != null && result.Keys.Count > 0)
            {
                log.Debug($"GetMediaForElasticSearchIndex . groupId {groupId}, result.Keys.Count {result.Keys.Count}");
            }


            return result;
        }

        public GenericResponse<Asset> AddAsset(int groupId, Asset assetToAdd, long userId, bool isFromIngest = false)
        {
            GenericResponse<Asset> result = new GenericResponse<Asset>();
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling AddAsset", groupId);
                    return result;
                }

                var setShopMetaStatus = TrySetShopMeta(groupId, assetToAdd, catalogGroupCache, userId);
                if (!setShopMetaStatus.IsOkStatusCode())
                {
                    return new GenericResponse<Asset>(setShopMetaStatus);
                }

                switch (assetToAdd.AssetType)
                {
                    case eAssetTypes.EPG:
                        if (assetToAdd is EpgAsset epgAssetToAdd)
                        {
                            result = EpgAssetManager.AddEpgAsset(groupId, epgAssetToAdd, userId, catalogGroupCache);
                            if (!isFromIngest && result.HasObject())
                            {
                                var asset = result.Object;
                                var epgAssetEvent = asset.ToAssetEvent(groupId, userId);
                                epgAssetEvent.Insert();
                                NotifyChannelWasUpdated(groupId, userId, epgAssetToAdd);
                            }
                        }
                        break;
                    case eAssetTypes.NPVR:
                        break;
                    case eAssetTypes.MEDIA:
                        bool isLinear = assetToAdd is LiveAsset;
                        MediaAsset mediaAssetToAdd = assetToAdd as MediaAsset;
                        if (mediaAssetToAdd != null)
                        {
                            result = AddMediaAsset(groupId, ref catalogGroupCache, mediaAssetToAdd, isLinear, userId, isFromIngest);
                            if (isLinear && result.HasObject())
                            {
                                LiveAsset linearMediaAssetToAdd = assetToAdd as LiveAsset;
                                result = AddLinearMediaAsset(groupId, result.Object as MediaAsset, linearMediaAssetToAdd, userId);
                            }
                        }
                        break;
                    default:
                    case eAssetTypes.UNKNOWN:
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddAsset for groupId: {0} and asset: {1}", groupId, assetToAdd.ToString()), ex);
            }

            return result;
        }

        private static void NotifyChannelWasUpdated(int groupId, long userId, EpgAsset epgAsset, EpgAsset oldEpgAsset = null)
        {
            var startDate = epgAsset.StartDate.Value;
            var endDate = epgAsset.EndDate.Value;
            if (oldEpgAsset != null) // when program was moved to another day, should notify about both dates
            {
                startDate = startDate < oldEpgAsset.StartDate.Value ? startDate : oldEpgAsset.StartDate.Value;
                endDate = endDate > oldEpgAsset.EndDate.Value ? endDate : oldEpgAsset.EndDate.Value;
            }

            EpgNotificationManager.Instance().ChannelWasUpdated(
                KLogger.GetRequestId(),
                groupId,
                userId,
                epgAsset.LinearAssetId.Value,
                epgAsset.EpgChannelId.Value,
                startDate,
                endDate,
                false);
        }

        public GenericResponse<Asset> UpdateAsset(int groupId, long id, Asset assetToUpdate, long userId, bool isFromIngest = false,
                                                        bool isCleared = false, bool isForMigration = false, bool isFromChannel = false)
        {
            GenericResponse<Asset> result = new GenericResponse<Asset>();
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling UpdateAsset", groupId);
                    return result;
                }

                // validate that asset exist
                // isAllowedToViewInactiveAssets = true because only operator can update asset
                GenericResponse<Asset> oldAsset = null;

                if (!isForMigration && !isCleared)
                {
                    oldAsset = GetAsset(groupId, id, assetToUpdate.AssetType, true);

                    if (!oldAsset.HasObject())
                    {
                        return oldAsset;
                    }
                }

                assetToUpdate.Id = id;

                switch (assetToUpdate.AssetType)
                {
                    case eAssetTypes.EPG:
                        if (assetToUpdate is EpgAsset epgAssetToUpdate)
                        {
                            var oldEpgAsset = oldAsset?.Object as EpgAsset;
                            result = EpgAssetManager.UpdateEpgAsset(groupId, epgAssetToUpdate, userId, oldEpgAsset, catalogGroupCache);

                            if (!isFromIngest && result.HasObject())
                            {
                                var epgAssetEvent = result.Object.ToAssetEvent(groupId, userId);
                                epgAssetEvent.Update();
                                NotifyChannelWasUpdated(groupId, userId, epgAssetToUpdate, oldEpgAsset);
                            }
                        }
                        break;
                    case eAssetTypes.NPVR:
                        break;
                    case eAssetTypes.MEDIA:
                        bool isLinear = assetToUpdate is LiveAsset;
                        MediaAsset mediaAssetToUpdate = assetToUpdate as MediaAsset;
                        MediaAsset currentAsset = null;

                        if (isForMigration || (isFromIngest && isCleared))
                        {
                            currentAsset = new MediaAsset()
                            {
                                MediaType = mediaAssetToUpdate.MediaType,
                                MediaAssetType = mediaAssetToUpdate.MediaAssetType,
                                Description = null
                            };
                        }
                        else
                        {
                            currentAsset = oldAsset.Object as MediaAsset;
                        }

                        // validate that existing asset is indeed linear media
                        if (isLinear && currentAsset.MediaAssetType != MediaAssetType.Linear)
                        {
                            result.SetStatus(eResponseStatus.AssetDoesNotExist, eResponseStatus.AssetDoesNotExist.ToString());
                            return result;
                        }

                        mediaAssetToUpdate.Id = id;
                        if (currentAsset != null && mediaAssetToUpdate != null)
                        {
                            result = UpdateMediaAsset(groupId, ref catalogGroupCache, currentAsset, mediaAssetToUpdate, isLinear, userId, isFromIngest, isForMigration, isFromChannel);
                            if (isLinear && result != null && result.Status != null && result.Status.Code == (int)eResponseStatus.OK)
                            {
                                LiveAsset linearMediaAssetToUpdate = assetToUpdate as LiveAsset;
                                if (isFromIngest)
                                {
                                    var oldLiveAsset = currentAsset as LiveAsset;
                                    linearMediaAssetToUpdate.ExternalEpgIngestId = string.IsNullOrEmpty(linearMediaAssetToUpdate.ExternalEpgIngestId) ? oldLiveAsset.ExternalEpgIngestId : linearMediaAssetToUpdate.ExternalEpgIngestId;
                                    linearMediaAssetToUpdate.EnableCatchUpState = oldLiveAsset.EnableCatchUpState;
                                    linearMediaAssetToUpdate.EnableCdvrState = oldLiveAsset.EnableCdvrState;
                                    linearMediaAssetToUpdate.EnableRecordingPlaybackNonEntitledChannelState = oldLiveAsset.EnableRecordingPlaybackNonEntitledChannelState;
                                    linearMediaAssetToUpdate.EnableStartOverState = oldLiveAsset.EnableStartOverState;
                                    linearMediaAssetToUpdate.EnableTrickPlayState = oldLiveAsset.EnableTrickPlayState;
                                    linearMediaAssetToUpdate.BufferCatchUp = oldLiveAsset.BufferCatchUp;
                                    linearMediaAssetToUpdate.PaddingBeforeProgramStarts = oldLiveAsset.PaddingBeforeProgramStarts;
                                    linearMediaAssetToUpdate.PaddingAfterProgramEnds = oldLiveAsset.PaddingAfterProgramEnds;
                                    linearMediaAssetToUpdate.BufferTrickPlay = oldLiveAsset.BufferTrickPlay;
                                    linearMediaAssetToUpdate.ChannelType = oldLiveAsset.ChannelType;
                                }

                                result = UpdateLinearMediaAsset(groupId, result.Object as MediaAsset, linearMediaAssetToUpdate, userId, isForMigration);
                            }
                        }
                        break;
                    default:
                    case eAssetTypes.UNKNOWN:
                        break;
                }

                if (!isFromIngest && result.IsOkStatusCode())
                {
                    // invalidate asset
                    Instance.InvalidateAsset(assetToUpdate.AssetType, groupId, id);
                }

                //Retry if has failed recordings and start time is within 30 minutes
                if (RecordingsDAL.GetDomainRetryRecordingDoc(groupId, id).HasValue
                    && result.Object.StartDate.HasValue && result.Object.StartDate.Value.AddMinutes(-30) <= DateTime.UtcNow)
                {
                    var recordingDt = RecordingsDAL.GetRecordingByEpgId(groupId, id);
                    long recordingId = 0;
                    if (recordingDt != null && recordingDt.Rows != null && recordingDt.Rows.Count == 1)
                    {
                        var dr = recordingDt.Rows[0];
                        recordingId = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
                    }
                    var recording = Recordings.RecordingsManager.Instance.RecordRetry(groupId, recordingId);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateAsset for groupId: {0} , id: {1} , assetType: {2}", groupId, id, assetToUpdate.AssetType.ToString()), ex);
            }

            return result;
        }

        public void DeleteAssetsByTypeAndDate(long partnerId, long assetStructId, DateTime finalEndDate, long userId)
        {
            var deletedCount =
                CatalogDAL.DeleteMediaAssetsByTypeAndDate(partnerId, assetStructId, finalEndDate, userId);

            log.DebugFormat("Media assets were deleted from SQL. partnerId: {0}, assetStructId: {1}, finalEndDate: {2}, deletedCount: {3}", partnerId, assetStructId, finalEndDate, deletedCount);
        }

        public Status DeleteAsset(int groupId, long id, eAssetTypes assetType, long userId, bool isFromChannel = false)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                // validate that asset exist
                // isAllowedToViewInactiveAssets = true because only operator can delete asset
                List<Asset> assets = GetAssets(groupId, new List<KeyValuePair<eAssetTypes, long>>() { new KeyValuePair<eAssetTypes, long>(assetType, id) }, true);
                if (assets == null || assets.Count != 1 || assets[0] == null || assets[0].IndexStatus == AssetIndexStatus.Deleted)
                {
                    result.Set((int)eResponseStatus.AssetDoesNotExist, eResponseStatus.AssetDoesNotExist.ToString());
                    return result;
                }

                switch (assetType)
                {
                    case eAssetTypes.EPG:
                        if (assets[0] is EpgAsset epgAsset)
                        {
                            result = EpgAssetManager.DeleteEpgAsset(groupId, id, userId);
                            if (result.IsOkStatusCode())
                            {
                                var epgAssetEvent = assets[0].ToAssetEvent(groupId, userId);
                                epgAssetEvent.Delete();
                                NotifyChannelWasUpdated(groupId, userId, epgAsset);
                            }
                        }
                        break;
                    case eAssetTypes.NPVR:
                        break;
                    case eAssetTypes.MEDIA:
                        result = DeleteMediaAsset(groupId, id, userId, assets[0] as MediaAsset, isFromChannel);
                        break;
                    default:
                    case eAssetTypes.UNKNOWN:
                        break;
                }

                if (result.IsOkStatusCode())
                {
                    // invalidate asset
                    Instance.InvalidateAsset(assetType, groupId, id);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed DeleteTopic for groupId: {0} , id: {1} , assetType: {2}", groupId, id, assetType.ToString()), ex);
            }

            return result;
        }

        internal static bool ClearAsset(int groupId, long id, eAssetTypes assetType, long userId)
        {
            bool result = false;
            try
            {
                if (CatalogDAL.DeleteMediaAsset(groupId, id, userId, true))
                {
                    result = true;
                    Instance.InvalidateAsset(assetType, groupId, id);
                }
                else
                {
                    log.ErrorFormat("Failed to Clear media asset with id: {0}, groupId: {1}", id, groupId);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed ClearAsset for groupId: {0} , id: {1} , assetType: {2}", groupId, id, assetType.ToString()), ex);
            }

            return result;
        }

        public static Status RemoveTopicsFromAsset(int groupId, long id, eAssetTypes assetType, HashSet<long> topicIds, long userId, bool isFromIngest = false)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                // validate that asset exist
                // isAllowedToViewInactiveAssets = true because only operator can remove topics from asset
                var currentAsset = AssetManager.Instance.GetAsset(groupId, id, assetType, true);
                if (!currentAsset.HasObject())
                {
                    result = new Status((int)eResponseStatus.AssetDoesNotExist, eResponseStatus.AssetDoesNotExist.ToString());
                    return result;
                }

                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling RemoveTopicsFromAsset", groupId);
                    return result;
                }

                // validate not trying to remove basic topicIds
                var validateBasicTopicsResult = ValidateBasicTopicIdsToRemove(catalogGroupCache, topicIds);
                if (!validateBasicTopicsResult.IsOkStatusCode())
                {
                    result = validateBasicTopicsResult;
                    return result;
                }

                if (isFromIngest)
                {
                    RemoveNoneExistingTopicIds(topicIds, currentAsset.Object, catalogGroupCache);
                    if (topicIds.Count == 0)
                    {
                        result.Set(eResponseStatus.OK);
                        return result;
                    }
                }

                switch (assetType)
                {
                    case eAssetTypes.EPG:
                        result = EpgAssetManager.RemoveTopicsFromProgram(groupId, topicIds, userId, catalogGroupCache, currentAsset.Object);
                        if (result.IsOkStatusCode() && currentAsset.Object is EpgAsset epgAsset)
                        {
                            NotifyChannelWasUpdated(groupId, userId, epgAsset);
                        }
                        break;
                    case eAssetTypes.MEDIA:
                        result = RemoveTopicsFromMediaAsset(groupId, id, topicIds, userId, catalogGroupCache, currentAsset.Object as MediaAsset, isFromIngest);
                        break;
                    case eAssetTypes.NPVR:
                    case eAssetTypes.UNKNOWN:
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed RemoveTopicsFromAsset for groupId: {0} , id: {1} , assetType: {2}", groupId, id, assetType.ToString()), ex);
            }

            return result;
        }

        private static Status RemoveTopicsFromMediaAsset(int groupId, long id, HashSet<long> topicIds, long userId, CatalogGroupCache catalogGroupCache, MediaAsset mediaAsset, bool isFromIngest)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            try
            {
                int dbAssetType = 0;

                // validate topicsIds exist on asset
                if (mediaAsset != null)
                {
                    if (!isFromIngest)
                    {
                        result = ValidateNoneExistingTopicIdsToRemove(mediaAsset, topicIds, catalogGroupCache);
                        if (!result.IsOkStatusCode()) { return result; }
                    }

                    List<long> tagIds = catalogGroupCache.TopicsMapById.Where(x => topicIds.Contains(x.Key) && x.Value.Type == MetaType.Tag
                                                                              && !CatalogManager.TopicsToIgnore.Contains(x.Value.SystemName)).Select(x => x.Key).ToList();
                    List<long> metaIds = catalogGroupCache.TopicsMapById.Where(x => topicIds.Contains(x.Key) && x.Value.Type != MetaType.Tag && x.Value.Type != MetaType.ReleatedEntity
                                                                              && !CatalogManager.TopicsToIgnore.Contains(x.Value.SystemName)).Select(x => x.Key).ToList();
                    List<long> releatedEntityIds = catalogGroupCache.TopicsMapById.Where(x => topicIds.Contains(x.Key) && x.Value.Type == MetaType.ReleatedEntity
                                                                              && !CatalogManager.TopicsToIgnore.Contains(x.Value.SystemName)).Select(x => x.Key).ToList();

                    if (CatalogDAL.RemoveMetasAndTagsFromAsset(groupId, id, dbAssetType, metaIds, tagIds, userId, releatedEntityIds))
                    {
                        if (metaIds?.Count > 0 || tagIds?.Count > 0)
                        {
                            CatalogManager.RemoveInheritedValue(groupId, catalogGroupCache, mediaAsset, metaIds, tagIds);
                        }

                        // invalidate asset
                        Instance.InvalidateAsset(eAssetTypes.MEDIA, groupId, id);

                        //Get updated Asset
                        var assetResponse = AssetManager.Instance.GetAsset(groupId, mediaAsset.Id, eAssetTypes.MEDIA, true);
                        if (assetResponse != null && assetResponse.HasObject() && assetResponse.Object is MediaAsset)
                        {
                            // if need UpdateAssetInheritancePolicy
                            UpdateAssetInheritancePolicy(groupId, userId, catalogGroupCache, assetResponse.Object as MediaAsset);
                        }

                        result.Set(eResponseStatus.OK);

                        // UpdateIndex
                        if (!isFromIngest)
                        {
                            var indexingResult = IndexManagerFactory.Instance.GetIndexManager(groupId).UpsertMedia(id);
                            if (!indexingResult)
                            {
                                log.ErrorFormat("Failed UpsertMedia index for assetId: {0}, type: {1}, groupId: {2} after RemoveTopicsFromMediaAsset", id, eAssetTypes.MEDIA.ToString(), groupId);
                            }

                            //extracted it from upsertMedia it was called also for OPC accounts,searchDefinitions
                            //not sure it's required but better be safe
                            LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetMediaInvalidationKey(groupId, id));

                        }
                    }
                    else
                    {
                        log.ErrorFormat("Failed to remove topics from asset with id: {0}, type: {1}, groupId: {2}", id, eAssetTypes.MEDIA.ToString(), groupId);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed RemoveTopicsFromMediaAsset for groupId: {0} , id: {1} , assetType: {2}", groupId, id, eAssetTypes.MEDIA.ToString()), ex);
            }

            return result;
        }

        private static Status TrySetShopMeta(int groupId, Asset asset, CatalogGroupCache catalogGroupCache, long userId)
        {
            var getAssetUserRulesResponse = AssetUserRuleManager.Instance.GetAssetUserRuleList(groupId, userId, ruleConditionType: RuleConditionType.AssetShop);
            if (!getAssetUserRulesResponse.IsOkStatusCode())
            {
                return getAssetUserRulesResponse.Status;
            }

            var shopAssetRule = getAssetUserRulesResponse.Objects?.FirstOrDefault();
            if (shopAssetRule == null)
            {
                return Status.Ok;
            }

            var getAssetStructResponse = GetAssetStruct(asset, catalogGroupCache);
            if (!getAssetStructResponse.IsOkStatusCode())
            {
                return getAssetStructResponse.Status;
            }

            if (!getAssetStructResponse.HasObject())
            {
                return Status.Ok;
            }

            var response = ShopMarkerService.Instance.SetShopMarkerMeta(groupId, getAssetStructResponse.Object, asset, shopAssetRule);

            return response;
        }

        private static GenericResponse<AssetStruct> GetAssetStruct(Asset asset, CatalogGroupCache catalogGroupCache)
        {
            if (asset is MediaAsset mediaAsset)
            {
                if (mediaAsset.MediaType.m_nTypeID > 0
                    && catalogGroupCache.AssetStructsMapById.TryGetValue(mediaAsset.MediaType.m_nTypeID, out var assetStructById))
                {
                    return new GenericResponse<AssetStruct>(Status.Ok, assetStructById);
                }

                if (!string.IsNullOrEmpty(mediaAsset.MediaType.m_sTypeName)
                    && catalogGroupCache.AssetStructsMapBySystemName.TryGetValue(mediaAsset.MediaType.m_sTypeName, out var assetStructByName))
                {
                    return new GenericResponse<AssetStruct>(Status.Ok, assetStructByName);
                }

                return new GenericResponse<AssetStruct>(eResponseStatus.AssetStructDoesNotExist, eResponseStatus.AssetStructDoesNotExist.ToString());
            }

            if (asset is EpgAsset)
            {
                var epgAssetStruct = catalogGroupCache.AssetStructsMapById[catalogGroupCache.GetProgramAssetStructId()];

                return new GenericResponse<AssetStruct>(Status.Ok, epgAssetStruct);
            }

            return new GenericResponse<AssetStruct>(Status.Ok);
        }

        /// <summary>
        /// Returns dictionary of [assetId, [language, media]] - use in remote task
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static System.Collections.Concurrent.ConcurrentDictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>> GetGroupMediaAssets(int groupId, long nextId, long pageSize)
        {
            // <assetId, <languageId, media>>
            System.Collections.Concurrent.ConcurrentDictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>> groupMediaAssetsMap = new System.Collections.Concurrent.ConcurrentDictionary<int, Dictionary<int, ApiObjects.SearchObjects.Media>>();
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetGroupMediaAssets", groupId);
                    return groupMediaAssetsMap;
                }

                DataSet groupAssetsDs = CatalogDAL.GetGroupMediaAssets(groupId, catalogGroupCache.GetDefaultLanguage().ID, nextId, pageSize);
                groupMediaAssetsMap = CreateGroupMediaMapFromDataSet(groupId, groupAssetsDs, catalogGroupCache);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetGroupMediaAssets for groupId: {0}", groupId), ex);
            }

            return groupMediaAssetsMap;
        }

        public static List<Topic> GetBasicMediaAssetTopics()
        {
            List<Topic> result = new List<Topic>();
            foreach (var meta in BasicMediaAssetMetasSystemNameToName)
            {
                Topic topicToAdd = new Topic(meta.Key, true, meta.Value);
                switch (meta.Key)
                {
                    case NAME_META_SYSTEM_NAME:
                        topicToAdd.SetType(MetaType.MultilingualString);
                        topicToAdd.SearchRelated = true;
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_METADATA);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_MANDATORY);
                        break;
                    case DESCRIPTION_META_SYSTEM_NAME:
                        topicToAdd.SetType(MetaType.MultilingualString);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_METADATA);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_TEXTAREA);
                        break;
                    case EXTERNAL_ID_META_SYSTEM_NAME:
                        topicToAdd.SetType(MetaType.String);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_METADATA);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_READONLY);
                        break;
                    case ENTRY_ID_META_SYSTEM_NAME:
                        topicToAdd.SetType(MetaType.String);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_METADATA);
                        break;
                    case STATUS_META_SYSTEM_NAME:
                        topicToAdd.SetType(MetaType.Bool);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_MANDATORY);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_AVAILABILITY);
                        break;
                    case PLAYBACK_START_DATE_TIME_META_SYSTEM_NAME:
                    case PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME:
                    case CATALOG_START_DATE_TIME_META_SYSTEM_NAME:
                    case CATALOG_END_DATE_TIME_META_SYSTEM_NAME:
                        topicToAdd.SetType(MetaType.DateTime);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_MANDATORY);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_AVAILABILITY);
                        break;
                    case CREATE_DATE_TIME_META_SYSTEM_NAME:
                        topicToAdd.SetType(MetaType.DateTime);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_MANDATORY);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_AVAILABILITY);
                        break;
                    default:
                        throw new Exception(string.Format("missing mapping for metaSystemName: {0} on GetBasicMediaAssetTopics", meta.Key));
                }

                result.Add(topicToAdd);
            }

            return result;
        }

        public IEnumerable<Asset> GetAssets(long groupId, IEnumerable<KeyValuePair<eAssetTypes, long>> assetTypes, bool isAllowedToViewInactiveAssets)
        {
            return GetAssets((int)groupId, assetTypes.ToList(), isAllowedToViewInactiveAssets);
        }

        #endregion
    }
}
