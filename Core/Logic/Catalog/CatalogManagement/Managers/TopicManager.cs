using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Response;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Tvinci.Core.DAL;
using MetaType = ApiObjects.MetaType;
using GroupsCache = GroupsCacheManager.GroupsCache;
using System.Threading;

namespace Core.Catalog.CatalogManagement
{
    public class TopicManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<TopicManager> lazy = new Lazy<TopicManager>(() => 
        new TopicManager(CatalogManager.Instance, CatalogDAL.Instance, VirtualAssetPartnerConfigManager.Instance,
            new ElasticsearchWrapper(), GroupsCache.Instance(), ConditionalAccess.Utils.Instance, Core.Notification.NotificationCache.Instance())
        , LazyThreadSafetyMode.PublicationOnly);
        public static TopicManager Instance { get { return lazy.Value; } }

        private readonly ICatalogManager _catalogManager;
        private readonly ITopicRepository _repository;
        private readonly IVirtualAssetPartnerConfigManager _virtualAssetPartnerConfigManager;
        private readonly IElasticsearchWrapper _wrapper;
        private readonly GroupsCacheManager.IGroupsCache _groupsCache;
        private readonly ConditionalAccess.IConditionalAccessUtils _conditionalAccessUtils;
        private readonly Notification.INotificationCache _notificationCache;

        public TopicManager(ICatalogManager catalogManager, ITopicRepository topicRepository, IVirtualAssetPartnerConfigManager virtualAssetPartnerConfigManager, 
            IElasticsearchWrapper elasticsearchWrapper, GroupsCacheManager.IGroupsCache groupsCache, ConditionalAccess.IConditionalAccessUtils conditionalAccessUtils,
            Notification.INotificationCache notificationCache)
        {
            _catalogManager = catalogManager;
            _repository = topicRepository;
            _virtualAssetPartnerConfigManager = virtualAssetPartnerConfigManager;
            _wrapper = elasticsearchWrapper;
            _groupsCache = groupsCache;
            _conditionalAccessUtils = conditionalAccessUtils;
            _notificationCache = notificationCache;
        }

        public GenericResponse<Topic> AddTopic(int groupId, Topic topicToAdd, long userId, bool shouldCheckRegularFlowValidations = true)
        {
            var result = new GenericResponse<Topic>();
            try
            {
                if (shouldCheckRegularFlowValidations)
                {
                    if (!_catalogManager.DoesGroupUsesTemplates(groupId))
                    {
                        result.SetStatus(eResponseStatus.AccountIsNotOpcSupported, eResponseStatus.AccountIsNotOpcSupported.ToString());
                        return result;
                    }

                    if (!_catalogManager.TryGetCatalogGroupCacheFromCache(groupId, out CatalogGroupCache _catalogGroupCache))
                    {
                        log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling AddTopic", groupId);
                        return result;
                    }

                    if (_catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(topicToAdd.SystemName))
                    {
                        result.SetStatus(eResponseStatus.MetaSystemNameAlreadyInUse, eResponseStatus.MetaSystemNameAlreadyInUse.ToString());
                        return result;
                    }

                    if (topicToAdd.ParentId.HasValue && topicToAdd.ParentId.Value.Equals(topicToAdd.Id))
                    {
                        result.SetStatus(eResponseStatus.ParentIdShouldNotPointToItself, eResponseStatus.ParentIdShouldNotPointToItself.ToString());
                        return result;
                    }

                    if (topicToAdd.ParentId.HasValue && !_catalogGroupCache.TopicsMapById.ContainsKey(topicToAdd.ParentId.Value))
                    {
                        result.SetStatus(eResponseStatus.ParentIdNotExist, eResponseStatus.ParentIdNotExist.ToString());
                        return result;
                    }
                }

                List<KeyValuePair<string, string>> languageCodeToName = new List<KeyValuePair<string, string>>();
                if (topicToAdd.NamesInOtherLanguages != null && topicToAdd.NamesInOtherLanguages.Count > 0)
                {
                    foreach (LanguageContainer language in topicToAdd.NamesInOtherLanguages)
                    {
                        languageCodeToName.Add(new KeyValuePair<string, string>(language.m_sLanguageCode3, language.m_sValue));
                    }
                }

                result.Object = _repository.InsertTopic(groupId, topicToAdd.Name, languageCodeToName, topicToAdd.SystemName, topicToAdd.Type, topicToAdd.GetCommaSeparatedFeatures(),
                                                      topicToAdd.IsPredefined, topicToAdd.ParentId, topicToAdd.HelpText, userId, shouldCheckRegularFlowValidations, topicToAdd.DynamicData, out long _id);

                if (_id < 0)
                {
                    result = new GenericResponse<Topic>(new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()));
                    switch (_id)
                    {
                        case -222:
                            result = new GenericResponse<Topic>(new Status((int)eResponseStatus.MetaSystemNameAlreadyInUse, eResponseStatus.MetaSystemNameAlreadyInUse.ToString()));
                            break;
                        case -333:
                            result = new GenericResponse<Topic>(new Status((int)eResponseStatus.MetaDoesNotExist, eResponseStatus.MetaDoesNotExist.ToString()));
                            break;
                        default:
                            break;
                    }

                    return result;
                }

                if (result?.Object != null)
                {
                    result.SetStatus(eResponseStatus.OK);
                }

                if (shouldCheckRegularFlowValidations)
                {
                    _catalogManager.InvalidateCatalogGroupCache(groupId, result.Status, true, result.Object);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddTopic for groupId: {0} and topic: {1}", groupId, topicToAdd.ToString()), ex);
            }

            return result;
        }

        public GenericResponse<Topic> UpdateTopic(int groupId, long id, Topic topicToUpdate, long userId)
        {
            var result = new GenericResponse<Topic>();
            try
            {
                if (!_catalogManager.DoesGroupUsesTemplates(groupId))
                {
                    result.SetStatus(eResponseStatus.AccountIsNotOpcSupported, eResponseStatus.AccountIsNotOpcSupported.ToString());
                    return result;
                }

                if (!_catalogManager.TryGetCatalogGroupCacheFromCache(groupId, out CatalogGroupCache _catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling UpdateTopic", groupId);
                    return result;
                }

                if (!_catalogGroupCache.TopicsMapById.ContainsKey(id))
                {
                    result.SetStatus(eResponseStatus.MetaDoesNotExist, eResponseStatus.MetaDoesNotExist.ToString());
                    return result;
                }

                if (topicToUpdate.ParentId.HasValue && topicToUpdate.ParentId.Value.Equals(topicToUpdate.Id))
                {
                    result.SetStatus(eResponseStatus.ParentIdShouldNotPointToItself, eResponseStatus.ParentIdShouldNotPointToItself.ToString());
                    return result;
                }

                if (topicToUpdate.ParentId.HasValue && !_catalogGroupCache.TopicsMapById.ContainsKey(topicToUpdate.ParentId.Value))
                {
                    result.SetStatus(eResponseStatus.ParentIdNotExist, eResponseStatus.ParentIdNotExist.ToString());
                    return result;
                }

                /************** According to new TVM UI system name can not be modified so no need for this validation *************************************************/
                //if (topic.IsPredefined.HasValue && topic.IsPredefined.Value && topicToUpdate.SystemName != null && topicToUpdate.SystemName != topic.SystemName)
                //{
                //    result.Status = new Status((int)eResponseStatus.CanNotChangePredefinedMetaSystemName, eResponseStatus.CanNotChangePredefinedMetaSystemName.ToString());
                //    return result;
                //}

                List<KeyValuePair<string, string>> languageCodeToName = null;
                bool shouldUpdateOtherNames = false;
                if (topicToUpdate.NamesInOtherLanguages != null)
                {
                    shouldUpdateOtherNames = true;
                    languageCodeToName = new List<KeyValuePair<string, string>>();
                    foreach (LanguageContainer language in topicToUpdate.NamesInOtherLanguages)
                    {
                        languageCodeToName.Add(new KeyValuePair<string, string>(language.m_sLanguageCode3, language.m_sValue));
                    }
                }

                var topic = new Topic(_catalogGroupCache.TopicsMapById[id]);
                result.Object = _repository.UpdateTopic(groupId, id, topicToUpdate.Name, shouldUpdateOtherNames, languageCodeToName, topicToUpdate.GetCommaSeparatedFeatures(topic.Features),
                                                    topicToUpdate.ParentId, topicToUpdate.HelpText, userId, topicToUpdate.DynamicData);

                if (result?.Object != null)
                {
                    result.SetStatus(eResponseStatus.OK);
                }

                _catalogManager.InvalidateCatalogGroupCache(groupId, result.Status, true, result.Object);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateTopic for groupId: {0}, id: {1} and topic: {2}", groupId, id, topicToUpdate.ToString()), ex);
            }

            return result;
        }

        public Status DeleteTopic(int groupId, long id, long userId)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                if (!_catalogManager.DoesGroupUsesTemplates(groupId))
                {
                    result.Set((int)eResponseStatus.AccountIsNotOpcSupported, eResponseStatus.AccountIsNotOpcSupported.ToString());
                    return result;
                }

                if (!_catalogManager.TryGetCatalogGroupCacheFromCache(groupId, out CatalogGroupCache _catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling DeleteTopic", groupId);
                    return result;
                }

                if (!_catalogGroupCache.TopicsMapById.ContainsKey(id))
                {
                    result = new Status((int)eResponseStatus.MetaDoesNotExist, eResponseStatus.MetaDoesNotExist.ToString());
                    return result;
                }

                var topic = new Topic(_catalogGroupCache.TopicsMapById[id]);
                if (topic.IsPredefined.HasValue && topic.IsPredefined.Value)
                {
                    result = new Status((int)eResponseStatus.CanNotDeletePredefinedMeta, eResponseStatus.CanNotDeletePredefinedMeta.ToString());
                    return result;
                }

                var objectVirtualAssetPartnerConfig = _virtualAssetPartnerConfigManager.GetObjectVirtualAssetPartnerConfiguration(groupId);
                if (objectVirtualAssetPartnerConfig.HasObjects())
                {
                    if (objectVirtualAssetPartnerConfig.Objects.Any(x => x.ObjectVirtualAssets.Any(y => y.MetaId == id)))
                    {
                        result = new Status((int)eResponseStatus.CanNotDeleteObjectVirtualAssetMeta, eResponseStatus.CanNotDeleteObjectVirtualAssetMeta.ToString());
                        return result;
                    }
                }

                bool haveConnection = _catalogGroupCache.AssetStructsMapById.Any
                    (x => (x.Value.ConnectedParentMetaId.HasValue && x.Value.ConnectedParentMetaId.Value == id) ||
                          (x.Value.ConnectingMetaId.HasValue && x.Value.ConnectingMetaId.Value == id));
                if (haveConnection)
                {
                    result = new Status((int)eResponseStatus.CanNotDeleteConnectingAssetStructMeta, "Can not delete Connecting/connected AssetStruct Meta");
                    return result;
                }

                if (_repository.DeleteTopic(groupId, id, userId))
                {
                    var tagTopicIds = new List<long>();
                    var metaTopicIds = new List<long>();
                    var relatedEntitiesTopicIds = new List<long>();

                    if (topic.Type == MetaType.Tag)
                    {
                        Status deleteTopicFromEsResult = _wrapper.DeleteTagsByTopic(groupId, _catalogGroupCache, id);
                        if (deleteTopicFromEsResult == null || deleteTopicFromEsResult.Code != (int)eResponseStatus.OK)
                        {
                            log.ErrorFormat("Failed deleting topic from ElasticSearch, for groupId: {0} and topicId: {1}", groupId, id);
                        }

                        tagTopicIds.Add(id);
                    }
                    else if (topic.Type == MetaType.ReleatedEntity)
                    {
                        relatedEntitiesTopicIds.Add(id);
                    }
                    else
                    {
                        metaTopicIds.Add(id);
                    }

                    // shouldDelete = isTag on purpose, since we are in DeleteTopic, if its a tag then delete it, on UpdateAssetStruct we don't delete the tag itself
                    if (!_catalogManager.InvalidateCacheAndUpdateIndexForTopicAssets(groupId, tagTopicIds, topic.Type == MetaType.Tag, false, metaTopicIds, 0, userId, relatedEntitiesTopicIds, topic.Type == MetaType.ReleatedEntity))
                    {
                        log.ErrorFormat("Failed InvalidateCacheAndUpdateIndexForTopicAssets for groupId: {0} and topicType: {1}, isTag: {2}", groupId, id, topic.Type.ToString());
                    }

                    result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    _catalogManager.InvalidateCatalogGroupCache(groupId, result, false);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed DeleteTopic for groupId: {0} and assetStructId: {1}", groupId, id), ex);
            }

            return result;
        }

        public GenericListResponse<Topic> GetTopicsByAssetStructId(int groupId, long assetStructId, MetaType type)
        {
            GenericListResponse<Topic> response = new GenericListResponse<Topic>();
            try
            {
                if (!_catalogManager.DoesGroupUsesTemplates(groupId))
                {
                    response.SetStatus(eResponseStatus.AccountIsNotOpcSupported, eResponseStatus.AccountIsNotOpcSupported.ToString());
                    return response;
                }

                if (!_catalogManager.TryGetCatalogGroupCacheFromCache(groupId, out CatalogGroupCache _catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetTopicsByAssetStructId", groupId);
                    return response;
                }

                assetStructId = _catalogGroupCache.GetRealAssetStructId(assetStructId, out bool isProgramStruct);

                if (_catalogGroupCache.AssetStructsMapById.ContainsKey(assetStructId))
                {
                    List<long> topicIds = _catalogGroupCache.AssetStructsMapById[assetStructId].MetaIds;
                    if (topicIds != null && topicIds.Count > 0)
                    {
                        response.Objects = topicIds.Where(x => _catalogGroupCache.TopicsMapById.ContainsKey(x) 
                        && (type == MetaType.All || _catalogGroupCache.TopicsMapById[x].Type == type))
                        .Select(x => _catalogGroupCache.TopicsMapById[x]).ToList();
                    }
                }

                response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetTopicsByAssetStructId with groupId: {0} and assetStructId: {1}", groupId, assetStructId), ex);
            }

            return response;
        }

        public GenericListResponse<Topic> GetTopicsByIds(int groupId, List<long> ids, MetaType type)
        {
            var response = new GenericListResponse<Topic>();
            try
            {
                if (!_catalogManager.DoesGroupUsesTemplates(groupId))
                {
                    response.SetStatus(eResponseStatus.AccountIsNotOpcSupported, eResponseStatus.AccountIsNotOpcSupported.ToString());
                    return response;
                }

                if (!_catalogManager.TryGetCatalogGroupCacheFromCache(groupId, out CatalogGroupCache _catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetTopicsByIds", groupId);
                    return response;
                }

                if (ids != null && ids.Count > 0)
                {
                    response.Objects = ids.Where(x => _catalogGroupCache.TopicsMapById.ContainsKey(x) && (type == MetaType.All || _catalogGroupCache.TopicsMapById[x].Type == type))
                                                .Select(x => _catalogGroupCache.TopicsMapById[x]).ToList();
                }
                else
                {
                    response.Objects = _catalogGroupCache.TopicsMapById.Values.Where(x => type == MetaType.All || x.Type == type).ToList();
                }

                response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetTopicsByIds with groupId: {0} and ids: {1}", groupId, ids != null ? string.Join(",", ids) : string.Empty), ex);
            }

            return response;
        }

        public MetaResponse GetGroupMetaList(int groupId, eAssetTypes assetType, ApiObjects.MetaType metaType, MetaFieldName fieldNameEqual, MetaFieldName fieldNameNotEqual, List<MetaFeatureType> metaFeatureTypeList)
        {
            var response = new MetaResponse()
            {
                Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            try
            {
                var groupManager = new GroupsCacheManager.GroupManager(_groupsCache);
                GroupsCacheManager.Group group = _groupsCache.GetGroup(groupId);

                if (group == null)
                {
                    log.ErrorFormat("Error. Group {0} data is empty.", groupId);
                    return response;
                }

                response.MetaList = new List<Meta>();

                if (assetType != eAssetTypes.MEDIA && group.m_oEpgGroupSettings != null)
                {
                    if (group.m_oEpgGroupSettings.metas != null && (metaType == ApiObjects.MetaType.String || metaType == ApiObjects.MetaType.All))
                    {
                        foreach (long key in group.m_oEpgGroupSettings.metas.Keys)
                        {
                            string val = group.m_oEpgGroupSettings.metas[key];

                            var meta = new Meta()
                            {
                                AssetType = eAssetTypes.EPG,
                                FieldName = MetaFieldName.None,
                                Name = GetTagName(val, group.m_oEpgGroupSettings.MetasDisplayName),
                                Type = ApiObjects.MetaType.String,
                                MultipleValue = false,
                                PartnerId = group.m_oEpgGroupSettings.GroupId
                            };

                            meta.Id = BuildMetaId(meta, key.ToString());
                            response.MetaList.Add(meta);
                        }
                    }

                    if (group.m_oEpgGroupSettings.tags != null && (metaType == ApiObjects.MetaType.Tag || metaType == ApiObjects.MetaType.All))
                    {
                        foreach (long key in group.m_oEpgGroupSettings.tags.Keys)
                        {
                            string val = group.m_oEpgGroupSettings.tags[key];

                            Meta meta = new Meta()
                            {
                                AssetType = eAssetTypes.EPG,
                                FieldName = MetaFieldName.None,
                                Name = GetTagName(val, group.m_oEpgGroupSettings.TagsDisplayName),
                                Type = ApiObjects.MetaType.Tag,
                                MultipleValue = true,
                                PartnerId = group.m_oEpgGroupSettings.GroupId
                            };

                            meta.Id = BuildMetaId(meta, key.ToString());
                            response.MetaList.Add(meta);
                        }
                    }

                    var epgAliasMappings = _conditionalAccessUtils.GetAliasMappingFields(groupId);
                    if (epgAliasMappings != null)
                    {
                        var metasMapping = response.MetaList.Where(x => epgAliasMappings.Select(y => y.Name).ToList().Contains(x.Name)).ToList();

                        foreach (var metaMapping in metasMapping)
                        {
                            ApiObjects.Epg.FieldTypeEntity epgFieldTypeEntity = epgAliasMappings.FirstOrDefault(x => x.Name == metaMapping.Name);
                            if (epgFieldTypeEntity != null)
                            {
                                metaMapping.FieldName = GetFieldNameByAlias(epgFieldTypeEntity.Alias);
                            }
                        }
                    }

                    // filter result according to fieldNameEqual, fieldNameNotEqual
                    if (response.MetaList != null && response.MetaList.Count > 0)
                        response.MetaList = FilterMetaList(response.MetaList, fieldNameEqual, fieldNameNotEqual);
                }

                if (assetType == eAssetTypes.MEDIA || assetType == eAssetTypes.UNKNOWN)
                {
                    Meta meta;

                    if (metaType != ApiObjects.MetaType.Tag && group.m_oMetasValuesByGroupId != null)
                    {
                        List<int> partnerIds = group.m_oMetasValuesByGroupId.Keys.ToList();
                        int keyIndex = 0;
                        int partnerId = 0;
                        foreach (Dictionary<string, string> groupMetas in group.m_oMetasValuesByGroupId.Values)
                        {
                            partnerId = partnerIds[keyIndex];
                            keyIndex++;
                            foreach (var metaVal in groupMetas)
                            {
                                meta = new Meta()
                                {
                                    AssetType = eAssetTypes.MEDIA,
                                    FieldName = MetaFieldName.None,
                                    Name = metaVal.Value,
                                    Type = APILogic.Utils.GetMetaTypeByDbName(metaVal.Key),
                                    PartnerId = partnerId,
                                    MultipleValue = false
                                };

                                meta.Id = BuildMetaId(meta, metaVal.Key);

                                if (long.TryParse(meta.Id, out long topicId))
                                    meta.DynamicData = GetTopicDynamicData(topicId);

                                if (meta.Type == metaType || metaType == ApiObjects.MetaType.All)
                                {
                                    response.MetaList.Add(meta);
                                }
                            }
                        }
                    }

                    if ((metaType == ApiObjects.MetaType.Tag || metaType == ApiObjects.MetaType.All) && group.m_oGroupTags != null)
                    {
                        foreach (var tagVal in group.m_oGroupTags)
                        {
                            meta = new Meta()
                            {
                                AssetType = eAssetTypes.MEDIA,
                                FieldName = MetaFieldName.None,
                                Name = tagVal.Value,
                                Type = ApiObjects.MetaType.Tag,
                                MultipleValue = true
                            };

                            meta.PartnerId = GetPartnerIdforTag(tagVal, group);
                            meta.Id = BuildMetaId(meta, tagVal.Key.ToString());

                            if (long.TryParse(meta.Id, out long topicId))
                                meta.DynamicData = GetTopicDynamicData(topicId);

                            response.MetaList.Add(meta);
                        }
                    }
                }

                // Update Meta with topic_interest
                if (response.MetaList != null && response.MetaList.Count > 0)
                {
                    List<Meta> topicInterestList = _notificationCache.GetPartnerTopicInterests(groupId);
                    Meta topicInterestMeta;

                    if (topicInterestList != null && topicInterestList.Count > 0)
                    {
                        var topicInerestMetaMap = topicInterestList.ToDictionary(x => x.Id, m => m);

                        foreach (var meta in response.MetaList)
                        {
                            topicInterestMeta = topicInerestMetaMap.ContainsKey(meta.Id) ? topicInerestMetaMap[meta.Id] : null;
                            if (topicInterestMeta != null)
                            {
                                meta.Features = topicInterestMeta.Features;
                                meta.ParentId = topicInterestMeta.ParentId;
                                meta.Id = topicInterestMeta.Id;

                                if (long.TryParse(meta.Id, out long topicId))
                                    meta.DynamicData = GetTopicDynamicData(topicId);
                            }
                        }

                        // filer metaFeatureTypeList if requested
                        if (metaFeatureTypeList?.Count > 0)
                        {
                            var metaListFilterd = response.MetaList.Where(x => metaFeatureTypeList.All(d => x.Features != null && x.Features.Contains(d)));
                            if (metaListFilterd != null)
                                response.MetaList = metaListFilterd.ToList();
                        }
                    }
                }

                response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to get meta for group = {0}", groupId), ex);
            }
            return response;
        }

        public Dictionary<string, string> GetTopicDynamicData(long topicId)
        {
            return _repository.GetTopicDynamicData(topicId);
        }

        private string GetTagName(string val, List<string> list)
        {
            if (list == null)
                return val;

            if (string.IsNullOrEmpty(val))
                return val;

            return list.FirstOrDefault(x => x.ToLower() == val);
        }

        private string BuildMetaId(Meta meta, string metaDBId)
        {
            string prefix = meta.AssetType == eAssetTypes.MEDIA && meta.Type == ApiObjects.MetaType.String ? "_NAME" : "";
            return ApiObjectsUtils.Base64Encode(string.Format("{0}_{1}_{2}_{3}{4}", meta.PartnerId, (int)meta.AssetType, meta.MultipleValue ? 1 : 0, metaDBId, prefix));
        }

        private MetaFieldName GetFieldNameByAlias(string alias)
        {
            switch (alias)
            {
                case "episode_number":
                    return MetaFieldName.EpisodeNumber;
                case "season_number":
                    return MetaFieldName.SeasonNumber;
                case "series_id":
                    return MetaFieldName.SeriesId;
                default:
                    log.ErrorFormat("The is no EPG FieldName mapping. alias: {0}", alias);
                    throw new Exception(string.Format("The is no EPG FieldName mapping. alias: {0}", alias));
            }
        }

        private List<Meta> FilterMetaList(List<Meta> list, MetaFieldName fieldNameEqual, MetaFieldName fieldNameNotEqual)
        {
            List<Meta> filteredMetaList = null;
            if (fieldNameEqual == MetaFieldName.All && fieldNameNotEqual == MetaFieldName.All)
                return list;

            if (fieldNameEqual != MetaFieldName.All)
            {
                switch (fieldNameEqual)
                {
                    case MetaFieldName.None:
                        return list.Where(x => x.FieldName != MetaFieldName.EpisodeNumber && x.FieldName != MetaFieldName.SeasonNumber && x.FieldName != MetaFieldName.SeriesId).ToList();
                    case MetaFieldName.SeriesId:
                    case MetaFieldName.SeasonNumber:
                    case MetaFieldName.EpisodeNumber:
                        return list.Where(x => x.FieldName == fieldNameEqual).ToList();
                    case MetaFieldName.All:
                    default:
                        break;
                }
            }
            else if (fieldNameNotEqual != MetaFieldName.All)
            {
                switch (fieldNameNotEqual)
                {
                    case MetaFieldName.None:
                        return list.Where(x => x.FieldName == MetaFieldName.EpisodeNumber || x.FieldName == MetaFieldName.SeasonNumber || x.FieldName == MetaFieldName.SeriesId).ToList();
                    case MetaFieldName.SeriesId:
                    case MetaFieldName.SeasonNumber:
                    case MetaFieldName.EpisodeNumber:
                        return list.Where(x => x.FieldName != fieldNameNotEqual).ToList();
                    case MetaFieldName.All:
                    default:
                        break;
                }
            }

            return filteredMetaList;
        }

        private int GetPartnerIdforTag(KeyValuePair<int, string> tagVal, GroupsCacheManager.Group group)
        {
            if (group.TagToGroup != null && group.TagToGroup.ContainsKey(tagVal.Key))
            {
                return group.TagToGroup[tagVal.Key];
            }

            log.ErrorFormat("Failed to get groupId from group.TagToGroup. tagVal.key:{0}", tagVal.Key);
            return 0;
        }
    }
}
