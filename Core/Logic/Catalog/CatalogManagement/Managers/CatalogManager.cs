using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiLogic.Api.Managers;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiLogic.Catalog.CatalogManagement.Repositories;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiLogic.Catalog.CatalogManagement.Validators;
using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.MediaMarks;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using Core.Catalog.Cache;
using Core.Catalog.Response;
using Core.GroupManagers;
using DAL;
using DAL.DTO;
using GroupsCacheManager;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;
using Newtonsoft.Json;
using ODBCWrapper;
using QueueWrapper;
using Tvinci.Core.DAL;
using TVinciShared;
using MetaType = ApiObjects.MetaType;

namespace Core.Catalog.CatalogManagement
{
    public interface ITagManager
    {
        List<TagValue> GetTagValues(int groupId, List<long> idIn, int pageIndex, int pageSize, out int totalItemsCount);
    }

    public interface ICatalogManager
    {
        bool TryGetCatalogGroupCacheFromCache(int groupId, out CatalogGroupCache catalogGroupCache);
        bool DoesGroupUsesTemplates(int groupId);
        void InvalidateCatalogGroupCache(int groupId, Status resultStatus, bool shouldCheckResultObject, object resultObject = null);
        bool InvalidateCacheAndUpdateIndexForTopicAssets(int groupId, List<long> tagTopicIds, bool shouldDeleteTag, bool shouldDeleteAssets, List<long> metaTopicIds, long assetStructId, long userId, List<long> relatedEntitiesTopicIds, bool shouldDeleteRelatedEntities);
        Dictionary<int, Media> GetGroupMedia(int groupId, long mediaId);
        void GetLinearChannelValues(List<EpgCB> lEpg, int groupID, Action<EpgCB> action);
        HashSet<BooleanLeafFieldDefinitions> GetUnifiedSearchKey(int groupId, string originalKey);
        List<AssetStruct> GetLinearMediaTypes(int groupId);
        bool IsRegionalizationEnabled(int groupId);
        BooleanLeafFieldDefinitions GetMetaByName(MetaByNameInput input);
        string GetGroupDefaultLanguage(int groupId);
    }

    public class CatalogManager : ICatalogManager, ITagManager
    {
        #region Constants and Readonly

        private static IKLogger log = new KLogger(nameof(CatalogManager));
        private readonly ILabelRepository _labelRepository;
        private readonly IGroupSettingsManager _groupSettingsManager;
        private readonly IGroupManager _groupManager;
        private readonly ILayeredCache _layeredCache;
        private readonly IAssetStructMetaRepository _assetStructMetaRepository;
        private readonly ICatalogCache _catalogCache;
        private readonly IAssetStructRepository _assetStructRepository;
        private readonly IAssetStructValidator _assetStructValidator;

        internal static readonly HashSet<string> TopicsToIgnore = CatalogLogic.GetTopicsToIgnoreOnBuildIndex();
        internal const string OPC_UI_METADATA = "metadata";
        internal const string OPC_UI_AVAILABILITY = "availability";
        internal const string OPC_UI_TEXTAREA = "textarea";
        internal const string OPC_UI_READONLY = "readonly";
        internal const string OPC_UI_MANDATORY = "mandatory";

        public const string LINEAR_ASSET_STRUCT_SYSTEM_NAME = "Linear";
        public const int CURRENT_REQUEST_DAYS_OFFSET_DEFAULT = 7;

        #endregion

        private static readonly Lazy<CatalogManager> lazy = new Lazy<CatalogManager>(() =>
                new CatalogManager(LabelRepository.Instance,
                    LayeredCache.Instance,
                    AssetStructValidator.Instance,
                    AssetStructMetaRepository.Instance,
                    AssetStructRepository.Instance,
                    GroupSettingsManager.Instance,
                    GroupManager.Instance,
                    CatalogCache.Instance()),
            LazyThreadSafetyMode.PublicationOnly);

        public static CatalogManager Instance => lazy.Value;

        public CatalogManager(
            ILabelRepository labelRepository,
            ILayeredCache layeredCache,
            IAssetStructValidator assetStructValidator,
            IAssetStructMetaRepository assetStructMetaRepository,
            IAssetStructRepository assetStructRepository,
            IGroupSettingsManager groupSettingsManager,
            IGroupManager groupManager,
            ICatalogCache catalogCache)
        {
            _labelRepository = labelRepository ?? throw new ArgumentNullException(nameof(labelRepository));
            _layeredCache = layeredCache ?? throw new ArgumentNullException(nameof(layeredCache));
            _assetStructValidator = assetStructValidator ?? throw new ArgumentNullException(nameof(assetStructValidator));
            _assetStructRepository = assetStructRepository ?? throw new ArgumentNullException(nameof(assetStructRepository));
            _assetStructMetaRepository = assetStructMetaRepository ?? throw new ArgumentNullException(nameof(assetStructMetaRepository));
            _groupSettingsManager = groupSettingsManager ?? throw new ArgumentNullException(nameof(groupSettingsManager));
            _groupManager = groupManager ?? throw new ArgumentNullException(nameof(groupManager));
            _catalogCache = catalogCache ?? throw new ArgumentNullException(nameof(catalogCache));
        }

        public CatalogManager(
            ILabelRepository labelRepository,
            ILayeredCache layeredCache,
            IAssetStructValidator assetStructValidator,
            IAssetStructMetaRepository assetStructMetaRepository,
            IAssetStructRepository assetStructRepository,
            IGroupSettingsManager groupSettingsManager,
            IGroupManager groupManager,
            ICatalogCache catalogCache,
            IKLogger logger) : this(labelRepository, layeredCache, assetStructValidator, assetStructMetaRepository, assetStructRepository, groupSettingsManager, groupManager, catalogCache)
        {
            log = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Private Methods

        private Tuple<CatalogGroupCache, bool> GetCatalogGroupCache(Dictionary<string, object> funcParams)
        {
            bool res = false;
            CatalogGroupCache catalogGroupCache = null;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue && groupId.Value > 0)
                    {
                        if (!GroupSettingsManager.Instance.IsOpc(groupId.Value))
                        {
                            return new Tuple<CatalogGroupCache, bool>(null, false);
                        }

                        List<LanguageObj> languages = CatalogDAL.GetGroupLanguages(groupId.Value);
                        // return false if no languages were found or not only 1 default language found
                        if (languages == null || (languages.Count > 0 && languages.Where(x => x.IsDefault).Count() != 1))
                        {
                            return new Tuple<CatalogGroupCache, bool>(null, false);
                        }

                        DataSet topicsDs = CatalogDAL.GetTopicsByGroupId(groupId.Value);
                        List<Topic> topics = CreateTopicListFromDataSet(topicsDs);
                        // return false if topics is null
                        if (topics == null)
                        {
                            return new Tuple<CatalogGroupCache, bool>(null, false);
                        }

                        // TODO uncomment when regression tests will be fixed
                        // // non-opc accounts don't have topics and don't have CatalogGroupCache at all
                        // // check together with topics, in order not to check IsOpc(call to DB) when no need
                        // if (topics.Count == 0 && !GroupSettingsManager.Instance.IsOpc(groupId.Value))
                        // {
                        //     return new Tuple<CatalogGroupCache, bool>(null, false);
                        // }

                        var assetStructs = _assetStructRepository.GetAssetStructsByGroupId(groupId.Value);
                        // return false if asset is null
                        if (assetStructs == null)
                        {
                            return new Tuple<CatalogGroupCache, bool>(null, false);
                        }

                        catalogGroupCache = new CatalogGroupCache(groupId.Value, languages, assetStructs, topics);

                        res = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetCatalogGroupCache failed params : {0}", funcParams != null ? string.Join(";",
                         funcParams.Select(x => string.Format("key:{0}, value: {1}", x.Key, x.Value.ToString())).ToList()) : string.Empty), ex);
            }

            return new Tuple<CatalogGroupCache, bool>(catalogGroupCache, res);
        }

        public void InvalidateCatalogGroupCache(int groupId, Status resultStatus, bool shouldCheckResultObject, object resultObject = null)
        {
            if (resultStatus != null && resultStatus.Code == (int)eResponseStatus.OK && (!shouldCheckResultObject || resultObject != null))
            {
                string invalidationKey = LayeredCacheKeys.GetCatalogGroupCacheInvalidationKey(groupId);
                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key for catalogGroupCache with invalidationKey: {0}", invalidationKey);
                }
            }
        }

        private static List<Topic> CreateTopicListFromDataSet(DataSet ds)
        {
            List<Topic> response = null;
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable topicsDt = ds.Tables[0];
                EnumerableRowCollection<DataRow> translations = ds.Tables.Count == 2 ? ds.Tables[1].AsEnumerable() : new DataTable().AsEnumerable();
                if (topicsDt != null && topicsDt.Rows != null)
                {
                    response = new List<Topic>();
                    foreach (DataRow dr in topicsDt.Rows)
                    {
                        long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                        if (id > 0)
                        {
                            List<DataRow> topicTranslations = (from row in translations
                                                               where (Int64)row["TOPIC_ID"] == id
                                                               select row).ToList();
                            Topic topic = CatalogDAL.Instance.CreateTopic(id, dr, topicTranslations);
                            if (topic != null)
                            {
                                response.Add(topic);
                            }
                        }
                    }
                }
            }

            return response;
        }

        private static Type GetMetaType(Dictionary<string, Topic> topics)
        {
            Type res = typeof(string);
            MetaType metaType = MetaType.All;
            metaType = GetTopicMetaType(topics);

            switch (metaType)
            {
                case MetaType.String:
                case MetaType.MultilingualString:
                case MetaType.Tag:
                    res = typeof(string);
                    break;
                case MetaType.Number:
                    res = typeof(double);
                    break;
                case MetaType.Bool:
                    res = typeof(int);
                    break;
                case MetaType.DateTime:
                    res = typeof(DateTime);
                    break;
                case MetaType.All:
                default:
                    break;
            }

            return res;
        }

        public static MetaType GetTopicMetaType(Dictionary<string, Topic> topics)
        {
            MetaType metaType;

            // if there is no duplication of topics
            if (topics.Count == 1)
            {
                metaType = topics.Values.First().Type;
            }
            // if there is duplication but on of them is tag take the meta (were getting meta type...)
            else if (topics.Count(x => x.Key != MetaType.Tag.ToString()) == 1)
            {
                metaType = topics.First(x => x.Key != MetaType.Tag.ToString()).Value.Type;
            }
            // if there is duplication but one of them is string/translated string
            // take the first that is not a string/translated string (number, bool, datetime)
            else if (topics.Count(x => x.Key != MetaType.String.ToString() && x.Key != MetaType.MultilingualString.ToString()) == 1)
            {
                metaType = topics.First(x => x.Key != MetaType.String.ToString() && x.Key != MetaType.MultilingualString.ToString()).Value.Type;
            }
            // if all the topics are string/translated strings - just take it as a string
            else if (topics.All(x => x.Key == MetaType.String.ToString() || x.Key == MetaType.MultilingualString.ToString()))
            {
                metaType = topics.Values.First().Type;
            }
            // if there is duplication and more than one is not string/tag then take the first one (backward compatible with todays behavior)
            else
            {
                log.ErrorFormat("This is a duplication of topic, a wrong configuration was done on the account and there for we had to randomly chose one of the types");
                metaType = topics.Values.First().Type;
            }

            return metaType;
        }

        private static GenericResponse<TagValue> CreateTagValueFromDataSet(DataSet ds)
        {
            var response = new GenericResponse<TagValue>();
            response.SetStatus(eResponseStatus.TagDoesNotExist);
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
                {
                    long id = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "id", 0);
                    if (id > 0)
                    {
                        EnumerableRowCollection<DataRow> translations = ds.Tables.Count == 2 ? ds.Tables[1].AsEnumerable() : new DataTable().AsEnumerable();
                        List<DataRow> tagTranslations = (from row in translations
                                                         select row).ToList();
                        response.Object = CreateTag(id, dt.Rows[0], tagTranslations);
                        if (response.Object != null)
                        {
                            response.SetStatus(eResponseStatus.OK);
                        }
                    }
                    else
                    {
                        response.SetStatus(CreateTagResponseStatusFromResult(id));
                        return response;
                    }
                }
            }

            return response;
        }

        private static TagValue CreateTag(long id, DataRow dr, List<DataRow> tagTranslations)
        {
            TagValue result = null;
            if (id > 0)
            {
                string name = ODBCWrapper.Utils.GetSafeStr(dr, "value");
                int topicId = ODBCWrapper.Utils.GetIntSafeVal(dr, "topic_id", 0);
                DateTime? createDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "CREATE_DATE");
                DateTime? updateDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "UPDATE_DATE");
                List<LanguageContainer> tagsInOtherLanguages = new List<LanguageContainer>();
                if (tagTranslations != null && tagTranslations.Count > 0)
                {
                    foreach (DataRow translationDr in tagTranslations)
                    {
                        string languageCode = ODBCWrapper.Utils.GetSafeStr(translationDr, "CODE3");
                        string translation = ODBCWrapper.Utils.GetSafeStr(translationDr, "TRANSLATION");
                        if (!string.IsNullOrEmpty(languageCode) && !string.IsNullOrEmpty(translation))
                        {
                            tagsInOtherLanguages.Add(new LanguageContainer(languageCode, translation));
                        }
                    }
                }

                result = new TagValue()
                {
                    value = name,
                    tagId = id,
                    topicId = topicId,
                    TagsInOtherLanguages = tagsInOtherLanguages,
                    createDate = createDate.HasValue ? DateUtils.DateTimeToUtcUnixTimestampSeconds(createDate.Value) : 0,
                    updateDate = updateDate.HasValue ? DateUtils.DateTimeToUtcUnixTimestampSeconds(updateDate.Value) : 0
                };
            }

            return result;
        }

        private static Status CreateTagResponseStatusFromResult(long result)
        {
            Status responseStatus = null;
            switch (result)
            {
                case -222:
                    responseStatus = new Status((int)eResponseStatus.TagAlreadyInUse, eResponseStatus.TagAlreadyInUse.ToString());
                    break;
                case -333:
                    responseStatus = new Status((int)eResponseStatus.TagDoesNotExist, eResponseStatus.TagDoesNotExist.ToString());
                    break;
                default:
                    responseStatus = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                    break;
            }

            return responseStatus;
        }

        private static bool InvalidateCacheForTagAssets(int groupId, long tagId, bool shouldUpdateRowsStatus, long userId,
            out List<int> mediaIds, out List<int> epgIds)
        {
            // preparing media list and epg
            mediaIds = new List<int>();
            epgIds = new List<int>();

            bool result = true;
            try
            {
                DataSet ds;
                if (shouldUpdateRowsStatus)
                {
                    // update and get all assets with tag
                    ds = CatalogDAL.UpdateTagAssets(groupId, tagId, userId);
                }
                else
                {
                    // get all assets with tag
                    ds = CatalogDAL.GetTagAssets(groupId, tagId);
                }

                CreateAssetsListForUpdateIndexFromDataSet(ds, out mediaIds, out epgIds);

                result = InvalidateCacheAssets(groupId, mediaIds, epgIds);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed InvalidateCacheAndUpdateIndexForTagAssets for groupId: {0}, tagId: {1}", groupId, tagId), ex);
                result = false;
            }

            return result;
        }

        private static bool InvalidateCacheAssets(int groupId, List<int> mediaIds, List<int> epgIds)
        {
            bool result = true;

            if (mediaIds != null && mediaIds.Count > 0)
            {
                // invalidate medias
                foreach (int mediaId in mediaIds)
                {
                    result = AssetManager.Instance.InvalidateAsset(eAssetTypes.MEDIA, groupId, mediaId) && result;
                }
            }

            if (epgIds != null && epgIds.Count > 0)
            {
                // invalidate epgs
                foreach (int epgId in epgIds)
                {
                    result = AssetManager.Instance.InvalidateAsset(eAssetTypes.EPG, groupId, epgId) && result;
                }
            }

            return result;
        }

        public bool InvalidateCacheAndUpdateIndexForTopicAssets(int groupId, List<long> tagTopicIds, bool shouldDeleteTag, bool shouldDeleteAssets, List<long> metaTopicIds,
                                                                        long assetStructId, long userId, List<long> relatedEntitiesTopicIds, bool shouldDeleteRelatedEntities)
        {
            bool res = true;
            try
            {
                DataSet ds;
                // update and get all assets
                ds = CatalogDAL.UpdateTopicAssets(groupId, tagTopicIds, shouldDeleteTag, shouldDeleteAssets, metaTopicIds, assetStructId, userId, relatedEntitiesTopicIds, shouldDeleteRelatedEntities);

                // preparing media list and epg
                List<int> mediaIds = null;
                List<int> epgIds = null;

                CreateAssetsListForUpdateIndexFromDataSet(ds, out mediaIds, out epgIds);

                res = InvalidateCacheAndUpdateIndexForAssets(groupId, shouldDeleteAssets, mediaIds, epgIds);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed InvalidateCacheAndUpdateIndexForTopicAssets for groupId: {0}, assetStructId: {1}, tagTopicIds: {2}, metaTopicIds: {3}",
                            groupId, assetStructId, tagTopicIds != null ? string.Join(",", tagTopicIds) : string.Empty, metaTopicIds != null ? string.Join(",", metaTopicIds) : string.Empty), ex);
                res = false;
            }

            return res;
        }

        internal static void CreateAssetsListForUpdateIndexFromDataSet(DataSet ds, out List<int> mediaIds, out List<int> epgIds)
        {
            mediaIds = new List<int>();
            epgIds = new List<int>();

            int assetId = 0;
            int assetType = 0;

            if (ds != null && ds.Tables != null && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    assetId = ODBCWrapper.Utils.GetIntSafeVal(dr, "ASSET_ID");
                    assetType = ODBCWrapper.Utils.GetIntSafeVal(dr, "ASSET_TYPE");

                    if (assetType == (int)eObjectType.Media)
                    {
                        mediaIds.Add(assetId);
                    }

                    if (assetType == (int)eObjectType.EPG)
                    {
                        epgIds.Add(assetId);
                    }
                }
            }
        }

        private static string GetConnectingMetaValue(Topic topic, Asset mediaAsset)
        {
            string connectingMetaValue = string.Empty;

            // take only asset that need to be updated
            if (topic.Type == MetaType.Tag)
            {
                var tag = mediaAsset.Tags.FirstOrDefault(x => x.m_oTagMeta.m_sName == topic.SystemName);
                if (tag != null && tag.m_lValues != null && tag.m_lValues.Count > 0)
                {
                    connectingMetaValue = tag.m_lValues[0];
                }
            }
            else
            {
                Metas meta = mediaAsset.Metas.FirstOrDefault(x => x.m_oTagMeta.m_sName == topic.SystemName);
                if (meta != null)
                {
                    connectingMetaValue = meta.m_sValue;
                }
            }

            return connectingMetaValue;
        }

        private static void UpdateInheritedChildAsset(int groupId, long userId, Topic inhertiedTopic, Metas inheratedMetaObj, Tags inheratedTagsObj, List<Asset> childAssets)
        {
            foreach (Asset childAsset in childAssets)
            {
                bool update = false;
                // take only asset that need to be updated
                if (inhertiedTopic.Type == MetaType.Tag)
                {
                    int tagIndex = -1;
                    for (int i = 0; i < childAsset.Tags.Count; i++)
                    {
                        if (childAsset.Tags[i].m_oTagMeta.m_sName == inhertiedTopic.SystemName)
                        {
                            tagIndex = i;
                            break;
                        }
                    }

                    if (inheratedTagsObj == null)
                    {
                        if (tagIndex > -1)
                        {
                            AssetManager.RemoveTopicsFromAsset(groupId, childAsset.Id, eAssetTypes.MEDIA, new HashSet<long>() { inhertiedTopic.Id }, userId);
                        }
                    }
                    else
                    {
                        if (tagIndex == -1)
                        {
                            childAsset.Tags.Add(inheratedTagsObj);
                            update = true;
                        }
                        else if (!inheratedTagsObj.Equals(childAsset.Tags[tagIndex]))
                        {
                            childAsset.Tags[tagIndex] = inheratedTagsObj;
                            update = true;
                        }
                    }
                }
                else
                {
                    int metaIndex = -1;
                    for (int i = 0; i < childAsset.Metas.Count; i++)
                    {
                        if (childAsset.Metas[i].m_oTagMeta.m_sName == inhertiedTopic.SystemName)
                        {
                            metaIndex = i;
                            break;
                        }
                    }

                    if (inheratedMetaObj == null)
                    {
                        if (metaIndex > -1)
                        {
                            AssetManager.RemoveTopicsFromAsset(groupId, childAsset.Id, eAssetTypes.MEDIA, new HashSet<long>() { inhertiedTopic.Id }, userId);
                        }
                    }
                    else
                    {
                        if (metaIndex == -1)
                        {
                            childAsset.Metas.Add(inheratedMetaObj);
                            update = true;
                        }
                        else if (!inheratedMetaObj.Equals(childAsset.Metas[metaIndex]))
                        {
                            childAsset.Metas[metaIndex] = inheratedMetaObj;
                            update = true;
                        }
                    }
                }

                if (update)
                {
                    AssetManager.Instance.UpdateAsset(groupId, childAsset.Id, childAsset, userId, false, false);
                }
            }
        }

        private static bool GetHeritageTopics(int groupId, CatalogGroupCache catalogGroupCache, long assetStructId, long metaId, AssetStruct currentAssetStruct,
            out Topic inhertiedMeta, out Topic parentConnectingTopic, out Topic childConnectingTopic)
        {
            inhertiedMeta = null;
            parentConnectingTopic = null;
            childConnectingTopic = null;

            //get meta system_name
            if (catalogGroupCache.TopicsMapById.ContainsKey(metaId))
            {
                inhertiedMeta = catalogGroupCache.TopicsMapById[metaId];
            }
            else
            {
                log.ErrorFormat("Error. Meta is missing {0}, for group {1} not exist.", metaId, groupId);
                return false;
            }

            // get connected parent connectedParentMetaSystemName
            if (currentAssetStruct.ConnectedParentMetaId.HasValue && catalogGroupCache.TopicsMapById.ContainsKey(currentAssetStruct.ConnectedParentMetaId.Value))
            {
                parentConnectingTopic = catalogGroupCache.TopicsMapById[currentAssetStruct.ConnectedParentMetaId.Value];
            }
            else
            {
                log.ErrorFormat("Error. ConnectedParentMetaId is missing {0}, for assetStruct {1} at group {2} not exist.", metaId, assetStructId, groupId);
                return false;
            }

            // get connected
            if (currentAssetStruct.ConnectingMetaId.HasValue && catalogGroupCache.TopicsMapById.ContainsKey(currentAssetStruct.ConnectingMetaId.Value))
            {
                childConnectingTopic = catalogGroupCache.TopicsMapById[currentAssetStruct.ConnectingMetaId.Value];
            }
            else
            {
                log.ErrorFormat("Error. ConnectingMetaId is missing {0}, for assetStruct {1} at group {2} not exist.", metaId, assetStructId, groupId);
                return false;
            }

            return true;
        }

        private bool ValidateHeritage(int groupId, long assetStructId, long metaId, out CatalogGroupCache catalogGroupCache,
            out AssetStruct currentAssetStruct, out AssetStruct parentAssetStruct)
        {
            currentAssetStruct = null;
            parentAssetStruct = null;

            if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling UpdateAssetStructMeta", groupId);
                return false;
            }

            currentAssetStruct = catalogGroupCache.AssetStructsMapById[assetStructId];
            if (currentAssetStruct == null)
            {
                log.ErrorFormat("Error. AssetStruct {0}, for group {1} not exist.", assetStructId, groupId);
                return false;
            }

            if (!currentAssetStruct.ParentId.HasValue)
            {
                log.ErrorFormat("Error. AssetStruct {0} without ParentId , for group {1} not exist.", assetStructId, groupId);
                return false;
            }

            parentAssetStruct = catalogGroupCache.AssetStructsMapById[currentAssetStruct.ParentId.Value];
            if (parentAssetStruct == null)
            {
                log.ErrorFormat("Error. Parent AssetStruct {0}, for group {1} not exist.", currentAssetStruct.ParentId, groupId);
                return false;
            }

            if (!currentAssetStruct.AssetStructMetas.ContainsKey(metaId))
            {
                log.ErrorFormat("Error. metaId:{0} for AssetStruct {1}, group {2} not exist.", metaId, assetStructId, groupId);
                return false;
            }

            return true;
        }

        #endregion

        #region Internal Methods

        internal static bool InvalidateCacheAndUpdateIndexForAssets(int groupId, bool shouldDeleteAssets, List<int> mediaIds, List<int> epgIds)
        {
            bool result = true;
            if (mediaIds != null && mediaIds.Count > 0)
            {
                eAction action = shouldDeleteAssets ? eAction.Delete : eAction.Update;
                // update medias index
                if (!Module.Instance.UpdateIndex(mediaIds, groupId, action))
                {
                    result = false;
                    log.ErrorFormat("Error while update Media index. groupId:{0}, mediaIds:{1}", groupId, string.Join(",", mediaIds));
                }

                // invalidate medias
                foreach (int mediaId in mediaIds)
                {
                    result = AssetManager.Instance.InvalidateAsset(eAssetTypes.MEDIA, groupId, mediaId) && result;
                }
            }

            if (epgIds != null && epgIds.Count > 0)
            {
                // update epgs index
                if (!Module.UpdateEpgIndex(epgIds.Cast<ulong>().ToList(), groupId, eAction.Update, null, false))
                {
                    result = false;
                    log.ErrorFormat("Error while update Epg index. groupId:{0}, epgIds:{1}", groupId, string.Join(",", epgIds));
                }

                // invalidate epgs
                foreach (int epgId in epgIds)
                {
                    result = AssetManager.Instance.InvalidateAsset(eAssetTypes.EPG, groupId, epgId) && result;
                }
            }

            return result;
        }

        internal static bool UpdateIndexForAssets(int groupId, bool shouldDeleteAssets, List<int> mediaIds, List<int> epgIds)
        {
            bool result = true;
            if (mediaIds != null && mediaIds.Count > 0)
            {
                eAction action = shouldDeleteAssets ? eAction.Delete : eAction.Update;
                // update medias index
                if (!Module.Instance.UpdateIndex(mediaIds, groupId, action))
                {
                    result = false;
                    log.ErrorFormat("Error while update Media index. groupId:{0}, mediaIds:{1}", groupId, string.Join(",", mediaIds));
                }
            }

            if (epgIds != null && epgIds.Count > 0)
            {
                // update epgs index
                if (!Module.UpdateEpgIndex(epgIds.Cast<ulong>().ToList(), groupId, eAction.Update, null, false))
                {
                    result = false;
                    log.ErrorFormat("Error while update Epg index. groupId:{0}, epgIds:{1}", groupId, string.Join(",", epgIds));
                }
            }

            return result;
        }

        internal static bool IsGroupIdExcludedFromTemplatesImplementation(long groupId)
        {
            bool res = false;
            string rawStrFromConfig = ApplicationConfiguration.Current.ExcludeTemplatesImplementation.Value;
            if (rawStrFromConfig.Length > 0)
            {
                string[] strArrOfIDs = rawStrFromConfig.Split(';');
                if (strArrOfIDs != null && strArrOfIDs.Length > 0)
                {
                    List<long> listOfIDs = strArrOfIDs.Select(s =>
                    {
                        long l = 0;
                        if (Int64.TryParse(s, out l))
                            return l;
                        return 0;
                    }).ToList();

                    res = listOfIDs.Contains(groupId);
                }
            }

            return res;
        }

        internal static void UpdateChildAssetsMetaInherited(int groupId, CatalogGroupCache catalogGroupCache, long userId, AssetStruct currentAssetStruct, Asset newAsset, Asset currentAsset)
        {
            List<AssetStruct> connectedAssetStructs = catalogGroupCache.AssetStructsMapById.Values.Where(x => x.ParentId.HasValue && x.ParentId.Value == currentAssetStruct.Id).ToList();
            // Get connected AssetStructs (children)
            if (connectedAssetStructs != null && connectedAssetStructs.Count > 0)
            {
                // for-each connectedAssetStruct get connectedParentMetaSystemName
                foreach (AssetStruct connectedAssetStruct in connectedAssetStructs)
                {
                    List<AssetStructMeta> inheritedTopics = connectedAssetStruct.AssetStructMetas.Values.Where(x => x.IsInherited.HasValue && x.IsInherited.Value).ToList();
                    if (inheritedTopics != null && inheritedTopics.Count > 0)
                    {
                        List<Topic> topicsForAssetUpdate = new List<Topic>();

                        foreach (AssetStructMeta inheritedTopic in inheritedTopics)
                        {
                            if (catalogGroupCache.TopicsMapById.ContainsKey(inheritedTopic.MetaId))
                            {
                                Topic topic = catalogGroupCache.TopicsMapById[inheritedTopic.MetaId];

                                if (newAsset != null && currentAsset == null)
                                {
                                    topicsForAssetUpdate.Add(topic);
                                }
                                else
                                {
                                    if (topic.Type == MetaType.Tag)
                                    {
                                        Tags currentInheratedTagsObj = currentAsset.Tags.FirstOrDefault(x => x.m_oTagMeta.m_sName == topic.SystemName);
                                        Tags assetToUpdateInheratedTagsObj = newAsset.Tags.FirstOrDefault(x => x.m_oTagMeta.m_sName == topic.SystemName);
                                        if (currentInheratedTagsObj != null && !currentInheratedTagsObj.Equals(assetToUpdateInheratedTagsObj))
                                        {
                                            topicsForAssetUpdate.Add(topic);
                                        }
                                    }
                                    else
                                    {
                                        Metas currentInheratedMetaObj = currentAsset.Metas.FirstOrDefault(x => x.m_oTagMeta.m_sName == topic.SystemName);
                                        Metas assetToUpdateInheratedMetaObj = newAsset.Metas.FirstOrDefault(x => x.m_oTagMeta.m_sName == topic.SystemName);
                                        if (currentInheratedMetaObj != null && !currentInheratedMetaObj.Equals(assetToUpdateInheratedMetaObj))
                                        {
                                            topicsForAssetUpdate.Add(topic);
                                        }
                                    }
                                }
                            }
                        }

                        if (topicsForAssetUpdate.Count > 0)
                        {
                            AssetManager.Instance.InvalidateAsset(eAssetTypes.MEDIA, groupId, (int)newAsset.Id);

                            var data = new InheritanceParentUpdate()
                            {
                                AssetId = newAsset.Id,
                                TopicsIds = topicsForAssetUpdate.Select(x => x.Id).ToList()
                            };

                            try
                            {
                                var queue = new GenericCeleryQueue();
                                var inheritanceData = new InheritanceData(groupId, InheritanceType.ParentUpdate, JsonConvert.SerializeObject(data), userId);
                                bool enqueueSuccessful = queue.Enqueue(inheritanceData, string.Format("PROCESS_ASSET_INHERITANCE\\{0}", groupId));
                            }
                            catch
                            {
                                log.ErrorFormat("Failed enqueue of inheritance {0}", data);
                            }
                        }
                    }
                }
            }
        }

        public bool HandleParentUpdate(int groupId, long userId, long assetId, List<long> TopicsIds)
        {
            CatalogGroupCache catalogGroupCache = null;
            if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling HandleParentUpdate", groupId);
                return true;
            }

            GenericResponse<Asset> asset = AssetManager.Instance.GetAsset(groupId, assetId, eAssetTypes.MEDIA, true);

            if (asset.Status.Code != (int)eResponseStatus.OK || asset.Object == null)
            {
                log.ErrorFormat("failed to get Asset {0} for groupId: {1} when calling HandleParentUpdate", assetId, groupId);
                return true;
            }

            MediaAsset mediaAsset = asset.Object as MediaAsset;
            if (mediaAsset == null)
            {
                log.ErrorFormat("failed to get Asset {0}. Not MediaAsset for groupId: {1} when calling HandleParentUpdate", assetId, groupId);
                return true;
            }

            AssetStruct assetStruct = catalogGroupCache.AssetStructsMapById[mediaAsset.MediaType.m_nTypeID];
            if (assetStruct == null)
            {
                log.ErrorFormat("failed to get assetStruct {0} for groupId: {1} when calling HandleParentUpdate", mediaAsset.MediaType.m_nTypeID, groupId);
                return true;
            }

            AssetStruct childAssetStruct = GetChildAssetStruct(catalogGroupCache, assetStruct.Id);
            if (childAssetStruct == null)
            {
                log.ErrorFormat("failed to get childAssetStruct. ParentId: {0} for groupId: {1} when calling HandleParentUpdate", mediaAsset.MediaType.m_nTypeID, groupId);
                return true;
            }

            Topic parentConectingTopic = catalogGroupCache.TopicsMapById[childAssetStruct.ConnectedParentMetaId.Value];
            Topic childConectingTopic = catalogGroupCache.TopicsMapById[childAssetStruct.ConnectingMetaId.Value];

            string connectingMetaValue = GetConnectingMetaValue(parentConectingTopic, asset.Object);
            if (!string.IsNullOrEmpty(connectingMetaValue))
            {
                string filter = string.Format("(and asset_type='{0}' {1}='{2}' inheritance_policy='0')", childAssetStruct.Id, childConectingTopic.SystemName, connectingMetaValue);
                HashSet<long> childAssetsIds = GetAssetsIdsWithPaging(groupId, filter);
                if (childAssetsIds == null || childAssetsIds.Count == 0)
                {
                    return true;
                }

                List<KeyValuePair<eAssetTypes, long>> assetList = childAssetsIds.Select(x => new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, x)).ToList();

                List<Asset> childAssets = AssetManager.GetAssets(groupId, assetList, true);
                if (childAssets == null || childAssets.Count == 0)
                {
                    return true;
                }

                foreach (long topicId in TopicsIds)
                {
                    Topic topic = catalogGroupCache.TopicsMapById[topicId];

                    Metas inheratedMetaObj = null;
                    Tags inheratedTagsObj = null;

                    if (topic.Type == MetaType.Tag)
                    {
                        inheratedTagsObj = asset.Object.Tags.FirstOrDefault(x => x.m_oTagMeta.m_sName == topic.SystemName);
                    }
                    else
                    {
                        inheratedMetaObj = asset.Object.Metas.FirstOrDefault(x => x.m_oTagMeta.m_sName == topic.SystemName);
                    }

                    UpdateInheritedChildAsset(groupId, userId, topic, inheratedMetaObj, inheratedTagsObj, childAssets);
                }
            }


            return true;
        }

        private static AssetStruct GetChildAssetStruct(CatalogGroupCache catalogGroupCache, long parentAssetStructId)
        {
            AssetStruct assetStruct = null;

            List<AssetStruct> connectedAssetStructs = catalogGroupCache.AssetStructsMapById.Values.Where(x => x.ParentId.HasValue && x.ParentId.Value == parentAssetStructId).ToList();
            // Get connected AssetStructs (children)
            if (connectedAssetStructs != null && connectedAssetStructs.Count > 0)
            {
                assetStruct = connectedAssetStructs[0];
            }

            return assetStruct;
        }

        internal static void RemoveInheritedValue(int groupId, CatalogGroupCache catalogGroupCache, MediaAsset asset, List<long> metaIds, List<long> tagIds)
        {
            // Remove child asset.Metas, asset.Tags according to parent
            AssetStruct currentAssetStruct = null;
            if (asset.MediaType.m_nTypeID > 0 && catalogGroupCache.AssetStructsMapById.ContainsKey(asset.MediaType.m_nTypeID))
            {
                currentAssetStruct = catalogGroupCache.AssetStructsMapById[asset.MediaType.m_nTypeID];
            }

            if (currentAssetStruct == null)
            {
                return;
            }

            List<AssetStruct> connectedAssetStructs = catalogGroupCache.AssetStructsMapById.Values.Where(x => x.ParentId.HasValue && x.ParentId.Value == currentAssetStruct.Id).ToList();
            // Get connected AssetStructs (children)
            if (connectedAssetStructs != null && connectedAssetStructs.Count > 0)
            {
                // for-each connectedAssetStruct get connectedParentMetaSystemName
                foreach (AssetStruct connectedAssetStruct in connectedAssetStructs)
                {
                    // Get only inheritedTopics and filtered with ProtectFromIngest false
                    List<AssetStructMeta> inheritedTopics = connectedAssetStruct.AssetStructMetas.Values.Where(x => (x.IsInherited.HasValue && x.IsInherited.Value) &&
                                                                                                                        (!x.ProtectFromIngest.HasValue || x.ProtectFromIngest.Value == false)).ToList();
                    if (inheritedTopics != null && inheritedTopics.Count > 0)
                    {
                        List<long> inheritedTopicsToRemove = inheritedTopics.Select(x => x.MetaId).Where(x => metaIds.Contains(x) || tagIds.Contains(x)).ToList();

                        List<Topic> topicsForAssetUpdate = new List<Topic>();
                        foreach (var metaId in inheritedTopicsToRemove)
                        {
                            topicsForAssetUpdate.Add(catalogGroupCache.TopicsMapById[metaId]);
                        }

                        if (topicsForAssetUpdate.Count > 0)
                        {
                            Topic parentConectingTopic = catalogGroupCache.TopicsMapById[connectedAssetStruct.ConnectedParentMetaId.Value];
                            Topic childConectingTopic = catalogGroupCache.TopicsMapById[connectedAssetStruct.ConnectingMetaId.Value];

                            string connectingMetaValue = GetConnectingMetaValue(parentConectingTopic, asset);
                            if (!string.IsNullOrEmpty(connectingMetaValue))
                            {
                                string filter = string.Format("(and asset_type='{0}' {1}='{2}' inheritance_policy='0')", connectedAssetStruct.Id, childConectingTopic.SystemName, connectingMetaValue);
                                UnifiedSearchResult[] childAssetIds = Utils.SearchAssets(groupId, filter, 0, 0, false, true);

                                if (childAssetIds == null || childAssetIds.Length == 0)
                                {
                                    continue;
                                }

                                List<KeyValuePair<eAssetTypes, long>> assetList = childAssetIds.Select(x => new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, long.Parse(x.AssetId))).ToList();

                                List<Asset> childAssets = AssetManager.GetAssets(groupId, assetList, true);
                                if (childAssets == null || childAssets.Count == 0)
                                {
                                    continue;
                                }

                                foreach (var childAsset in childAssets)
                                {
                                    //remove topic from asset
                                    AssetManager.RemoveTopicsFromAsset(groupId, childAsset.Id, eAssetTypes.MEDIA, new HashSet<long>(inheritedTopicsToRemove), 999);
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method is here for backward compatability, redirecting all calls to the main method in GroupSettingsManager.Instance.
        /// This was done to avoid solution wide chanages
        /// </summary>
        public bool DoesGroupUsesTemplates(int groupId)
        {
            return Core.GroupManagers.GroupSettingsManager.Instance.DoesGroupUsesTemplates(groupId);
        }

        public string GetGroupDefaultLanguage(int groupId)
        {
            string lang = "";
            if (DoesGroupUsesTemplates(groupId))
            {
                CatalogGroupCache catalogGroupCache;
                if (TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    lang = catalogGroupCache.DefaultLanguage?.Code;
                }
            }
            else
            {
                
                lang = _groupManager.GetGroup(groupId)?.GetGroupDefaultLanguage()?.Code;
            }

            return lang;
        }

        public bool TryGetCatalogGroupCacheFromCache(int groupId, out CatalogGroupCache catalogGroupCache)
        {
            bool result = true;
            catalogGroupCache = null;
            try
            {
                string key = LayeredCacheKeys.GetCatalogGroupCacheKey(groupId);
                if (!LayeredCache.Instance.Get<CatalogGroupCache>(key, ref catalogGroupCache, GetCatalogGroupCache, new Dictionary<string, object>() { { "groupId", groupId } }, groupId,
                    LayeredCacheConfigNames.GET_CATALOG_GROUP_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GetCatalogGroupCacheInvalidationKey(groupId) }))
                {
                    log.ErrorFormat("Failed getting CatalogGroupCache from LayeredCache, groupId: {0}", groupId);
                    result = false;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetCatalogGroupCache with groupId: {0}", groupId), ex);
                result = false;
            }

            return result;
        }

        public GenericListResponse<AssetStruct> GetAssetStructsByIds(int groupId, List<long> ids, bool? isProtected)
        {
            GenericListResponse<AssetStruct> response = new GenericListResponse<AssetStruct>();
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetAssetStructsByIds", groupId);
                    return response;
                }

                if (ids != null && ids.Count > 0)
                {
                    response.Objects.AddRange(ids.Where(x => catalogGroupCache.AssetStructsMapById.ContainsKey(x)).Select(x => new AssetStruct(catalogGroupCache.AssetStructsMapById[x])));
                }
                else
                {
                    response.Objects.AddRange(catalogGroupCache.AssetStructsMapById.Values.Select(x => new AssetStruct(x)));
                }

                var programAssetStructIndex = -1;
                // if need to add ProgramAssetStruct
                if (ids == null || ids.Count == 0 || ids.Contains(0))
                {
                    programAssetStructIndex = response.Objects.FindIndex(x => x.IsProgramAssetStruct);
                    if (programAssetStructIndex == -1)
                    {
                        bool isProgramStruct;
                        var programAssetStructId = catalogGroupCache.GetRealAssetStructId(0, out isProgramStruct);
                        if (isProgramStruct)
                        {
                            response.Objects.Add(new AssetStruct(catalogGroupCache.AssetStructsMapById[programAssetStructId]));
                            programAssetStructIndex = response.Objects.Count - 1;
                        }
                    }
                }

                if (programAssetStructIndex > -1)
                {
                    response.Objects[programAssetStructIndex].Id = 0;
                }

                if (isProtected.HasValue)
                {
                    response.Objects = response.Objects.Where(x => x.IsPredefined.HasValue && x.IsPredefined == isProtected.Value).ToList();
                }

                response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAssetStructsByIds with groupId: {0} and ids: {1}", groupId, ids != null ? string.Join(",", ids) : string.Empty), ex);
            }

            return response;
        }

        public GenericListResponse<AssetStruct> GetAssetStructsByTopicId(int groupId, long topicId, bool? isProtected)
        {
            GenericListResponse<AssetStruct> response = new GenericListResponse<AssetStruct>();
            try
            {
                if (topicId > 0)
                {
                    CatalogGroupCache catalogGroupCache;
                    if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetAssetStructsByTopicId", groupId);
                        return response;
                    }

                    response.Objects.AddRange(catalogGroupCache.AssetStructsMapById.Values.Where(x => x.MetaIds.Contains(topicId)).Select(x => new AssetStruct(x)));

                    int programAssetStructIndex = response.Objects.FindIndex(x => x.IsProgramAssetStruct);
                    if (programAssetStructIndex > -1)
                    {
                        response.Objects[programAssetStructIndex].Id = 0;
                    }

                    if (isProtected.HasValue)
                    {
                        response.Objects = response.Objects.Where(x => x.IsPredefined.HasValue && x.IsPredefined == isProtected.Value).ToList();
                    }

                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAssetStructsByTopicIds with groupId: {0} and topicId: {1}", groupId, topicId), ex);
            }

            return response;
        }

        public GenericListResponse<AssetStruct> GetAssetStructByVirtualEntityType(int groupId, ObjectVirtualAssetInfoType virtualEntityType)
        {
            GenericListResponse<AssetStruct> response = new GenericListResponse<AssetStruct>();

            try
            {
                var objectVirtualAssetInfo = VirtualAssetPartnerConfigManager.Instance.GetObjectVirtualAssetInfo(groupId, virtualEntityType);

                if (objectVirtualAssetInfo != null)
                {
                    response = GetAssetStructsByIds(groupId, new List<long>() { objectVirtualAssetInfo.AssetStructId }, null);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAssetStructByVirtualEntity with groupId: {0} and ObjectVirtualAssetInfoType: {1}", groupId, virtualEntityType), ex);
            }

            return response;
        }

        public GenericListResponse<AssetStruct> GetLinearAssetStructs(int groupId)
        {
            var response = new GenericListResponse<AssetStruct>();
            if (!TryGetCatalogGroupCacheFromCache(groupId, out var catalogGroupCache))
            {
                return response;
            }

            response.Objects = GetLinearMediaTypes(catalogGroupCache);
            response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());

            return response;
        }

        public GenericResponse<AssetStruct> AddAssetStruct(int groupId, AssetStruct assetStructToadd, long userId, bool isProgramStruct = false)
        {
            GenericResponse<AssetStruct> result = new GenericResponse<AssetStruct>();
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling AddAssetStruct", groupId);
                    return result;
                }

                if (catalogGroupCache.AssetStructsMapBySystemName.ContainsKey(assetStructToadd.SystemName))
                {
                    result.SetStatus(eResponseStatus.AssetStructSystemNameAlreadyInUse, eResponseStatus.AssetStructSystemNameAlreadyInUse.ToString());
                    return result;
                }

                if (assetStructToadd.ParentId.HasValue && assetStructToadd.ParentId.Value > 0 && !catalogGroupCache.AssetStructsMapById.ContainsKey(assetStructToadd.ParentId.Value))
                {
                    result.SetStatus(eResponseStatus.AssetStructDoesNotExist, "Parent AssetStruct does not exist");
                    return result;
                }

                if (!isProgramStruct && assetStructToadd.Features?.Count > 0 && assetStructToadd.Features.Contains("isProgramStruct"))
                {
                    isProgramStruct = true;
                }

                // validate basic metas
                if (!isProgramStruct)
                {
                    Status validateBasicMetasResult = _assetStructValidator.ValidateBasicMetaIds(catalogGroupCache, assetStructToadd, isProgramStruct);
                    if (validateBasicMetasResult.Code != (int)eResponseStatus.OK)
                    {
                        result.SetStatus(validateBasicMetasResult);
                        return result;
                    }
                }

                // validate meta ids exist
                List<long> noneExistingMetaIds = assetStructToadd.MetaIds.Except(catalogGroupCache.TopicsMapById.Keys).ToList();
                if (noneExistingMetaIds != null && noneExistingMetaIds.Count > 0)
                {
                    result.SetStatus(eResponseStatus.MetaIdsDoesNotExist, string.Format("{0} for the following Meta Ids: {1}",
                                                                                        eResponseStatus.MetaIdsDoesNotExist.ToString(), string.Join(",", noneExistingMetaIds)));
                    return result;
                }

                // validate meta ids duplications
                var duplicateMetaExist = assetStructToadd.MetaIds.GroupBy(x => x).Any(g => g.Count() > 1);
                if (duplicateMetaExist)
                {
                    result.SetStatus(eResponseStatus.MetaIdsDuplication, "Meta ids are duplicated");
                    return result;
                }

                // validate metas with the same system name don't exist
                Status validateNoSystemNameDuplication = _assetStructValidator.ValidateNoSystemNameDuplicationOnMetaIds(catalogGroupCache, assetStructToadd);
                if (validateNoSystemNameDuplication.Code != (int)eResponseStatus.OK)
                {
                    result.SetStatus(validateNoSystemNameDuplication);
                    return result;
                }

                Dictionary<FieldTypes, Dictionary<string, int>> mappingFields = null;
                List<KeyValuePair<long, string>> epgMetaIdsToValue = null;
                List<KeyValuePair<long, string>> epgTagIdsToValue = null;

                if (isProgramStruct)
                {
                    mappingFields = EpgAssetManager.GetMappingFields(groupId);
                    epgMetaIdsToValue = new List<KeyValuePair<long, string>>();
                    epgTagIdsToValue = new List<KeyValuePair<long, string>>();
                }

                List<KeyValuePair<long, int>> metaIdsToPriority = new List<KeyValuePair<long, int>>();
                if (assetStructToadd.MetaIds != null && assetStructToadd.MetaIds.Count > 0)
                {
                    int priority = 1;
                    foreach (long metaId in assetStructToadd.MetaIds)
                    {
                        metaIdsToPriority.Add(new KeyValuePair<long, int>(metaId, priority));
                        priority++;

                        if (isProgramStruct)
                        {
                            var topic = catalogGroupCache.TopicsMapById[metaId];
                            if (!EpgAssetManager.TopicsInBasicProgramTable.Contains(topic.SystemName, StringComparer.OrdinalIgnoreCase))
                            {
                                long topicId = 0;

                                if (topic.Type == MetaType.Tag)
                                {
                                    if (mappingFields.ContainsKey(FieldTypes.Tag) && mappingFields[FieldTypes.Tag].ContainsKey(topic.SystemName.ToLower()))
                                    {
                                        topicId = mappingFields[FieldTypes.Tag][topic.SystemName.ToLower()];
                                    }
                                    epgTagIdsToValue.Add(new KeyValuePair<long, string>(topicId, topic.SystemName));
                                }
                                else
                                {
                                    if (mappingFields.ContainsKey(FieldTypes.Meta) && mappingFields[FieldTypes.Meta].ContainsKey(topic.SystemName.ToLower()))
                                    {
                                        topicId = mappingFields[FieldTypes.Meta][topic.SystemName.ToLower()];
                                    }
                                    epgMetaIdsToValue.Add(new KeyValuePair<long, string>(topicId, topic.SystemName));
                                }
                            }
                        }
                    }
                }

                List<KeyValuePair<string, string>> languageCodeToName = new List<KeyValuePair<string, string>>();
                if (assetStructToadd.NamesInOtherLanguages != null && assetStructToadd.NamesInOtherLanguages.Count > 0)
                {
                    foreach (LanguageContainer language in assetStructToadd.NamesInOtherLanguages)
                    {
                        languageCodeToName.Add(new KeyValuePair<string, string>(language.m_sLanguageCode3, language.m_sValue));
                    }
                }

                assetStructToadd.IsProgramAssetStruct = isProgramStruct;
                result = _assetStructRepository.InsertAssetStruct(groupId, userId, assetStructToadd, languageCodeToName, metaIdsToPriority);
                // For backward compatibility
                if (isProgramStruct)
                {
                    if (epgMetaIdsToValue.Count > 0 && !_assetStructMetaRepository.UpdateEpgAssetStructMetas(groupId, epgMetaIdsToValue, userId))
                    {
                        log.ErrorFormat("UpdateEpgAssetStructMetas faild for groupId: {0}, epgMetaIdsToValue: {1}.", groupId, string.Join(",", epgMetaIdsToValue));
                    }

                    if (epgTagIdsToValue.Count > 0 && !CatalogDAL.UpdateEpgAssetStructTags(groupId, epgTagIdsToValue, userId))
                    {
                        log.ErrorFormat("UpdateEpgAssetStructTags faild for groupId: {0}, epgTagIdsToValue: {1}.", groupId, string.Join(",", epgTagIdsToValue));
                    }
                }

                InvalidateCatalogGroupCache(groupId, result.Status, true, result.Object);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddAssetStruct for groupId: {0} and assetStruct: {1}", groupId, assetStructToadd.ToString()), ex);
            }

            return result;
        }

        public GenericResponse<AssetStruct> UpdateAssetStruct(int groupId, long id, AssetStruct assetStructToUpdate, bool shouldUpdateMetaIds, long userId, bool shouldCheckRegularFlowValidations = true)
        {
            GenericResponse<AssetStruct> result = new GenericResponse<AssetStruct>();
            try
            {
                bool shouldUpdateOtherNames = false;
                List<KeyValuePair<string, string>> languageCodeToName = null;
                List<KeyValuePair<long, int>> metaIdsToPriority = null;
                List<long> removedTopicIds = new List<long>();
                bool shouldUpdateAssetStructAssets = shouldUpdateMetaIds && shouldCheckRegularFlowValidations;
                AssetStruct assetStruct = null;
                CatalogGroupCache catalogGroupCache = null;
                bool isProgramStruct = false;
                Dictionary<FieldTypes, Dictionary<string, int>> mappingFields = null;
                List<KeyValuePair<long, string>> epgMetaIdsToValue = null;
                List<KeyValuePair<long, string>> epgTagIdsToValue = null;

                if (shouldCheckRegularFlowValidations)
                {
                    if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling UpdateAssetStruct", groupId);
                        return result;
                    }

                    id = catalogGroupCache.GetRealAssetStructId(id, out isProgramStruct);

                    if (isProgramStruct)
                    {
                        mappingFields = EpgAssetManager.GetMappingFields(groupId);
                        epgMetaIdsToValue = new List<KeyValuePair<long, string>>();
                        epgTagIdsToValue = new List<KeyValuePair<long, string>>();
                    }

                    if (!catalogGroupCache.AssetStructsMapById.ContainsKey(id))
                    {
                        result.SetStatus(eResponseStatus.AssetStructDoesNotExist, eResponseStatus.AssetStructDoesNotExist.ToString());
                        return result;
                    }

                    if (!isProgramStruct && assetStructToUpdate.ParentId.HasValue && assetStructToUpdate.ParentId.Value > 0)
                    {
                        if (!catalogGroupCache.AssetStructsMapById.ContainsKey(assetStructToUpdate.ParentId.Value))
                        {
                            result.SetStatus(eResponseStatus.AssetStructDoesNotExist, "Parent AssetStruct does not exist");
                            return result;
                        }

                        if (assetStructToUpdate.ParentId.Value == id)
                        {
                            result.SetStatus(eResponseStatus.ParentIdShouldNotPointToItself, "Parent id should not point to itself");
                            return result;
                        }
                    }

                    assetStruct = new AssetStruct(catalogGroupCache.AssetStructsMapById[id]);
                    if (assetStruct.IsPredefined.HasValue && assetStruct.IsPredefined.Value &&
                        assetStructToUpdate.SystemName != null && assetStruct.SystemName != assetStructToUpdate.SystemName)
                    {
                        result.SetStatus(eResponseStatus.CanNotChangePredefinedAssetStructSystemName, eResponseStatus.CanNotChangePredefinedAssetStructSystemName.ToString());
                        return result;
                    }

                    if (shouldUpdateMetaIds)
                    {
                        // validate basic metas
                        Status validateBasicMetasResult = _assetStructValidator.ValidateBasicMetaIds(catalogGroupCache, assetStructToUpdate, isProgramStruct);
                        if (validateBasicMetasResult.Code != (int)eResponseStatus.OK)
                        {
                            result.SetStatus(validateBasicMetasResult);
                            return result;
                        }

                        // validate meta ids exist
                        List<long> noneExistingMetaIds = assetStructToUpdate.MetaIds.Except(catalogGroupCache.TopicsMapById.Keys).ToList();
                        if (noneExistingMetaIds != null && noneExistingMetaIds.Count > 0)
                        {
                            result.SetStatus(eResponseStatus.MetaIdsDoesNotExist,
                                             string.Format("{0} for the following Meta Ids: {1}", eResponseStatus.MetaIdsDoesNotExist.ToString(), string.Join(",", noneExistingMetaIds)));
                            return result;
                        }

                        // validate meta ids duplications
                        var duplicateMetaExist = assetStructToUpdate.MetaIds.GroupBy(x => x).Any(g => g.Count() > 1);
                        if (duplicateMetaExist)
                        {
                            result.SetStatus(eResponseStatus.MetaIdsDuplication, "Meta ids are duplicated");
                            return result;
                        }

                        // validate metas with the same system name don't exist
                        Status validateNoSystemNameDuplication = _assetStructValidator.ValidateNoSystemNameDuplicationOnMetaIds(catalogGroupCache, assetStructToUpdate);
                        if (validateNoSystemNameDuplication.Code != (int)eResponseStatus.OK)
                        {
                            result.SetStatus(validateNoSystemNameDuplication);
                            return result;
                        }

                        if (assetStruct.SystemName.Equals(LiveToVodService.LIVE_TO_VOD_ASSET_STRUCT_SYSTEM_NAME, StringComparison.OrdinalIgnoreCase)
                            && assetStruct.MetaIds?.Except(assetStructToUpdate.MetaIds).Any() == true)
                        {
                            result.SetStatus(eResponseStatus.CanNotRemoveMetaIdsForLiveToVod, "It's not allowed to remove metas for L2V asset struct.");
                            return result;
                        }
                    }
                }

                if (assetStructToUpdate.MetaIds != null && shouldUpdateMetaIds)
                {
                    metaIdsToPriority = new List<KeyValuePair<long, int>>();
                    int priority = 1;
                    foreach (long metaId in assetStructToUpdate.MetaIds)
                    {
                        metaIdsToPriority.Add(new KeyValuePair<long, int>(metaId, priority));
                        priority++;

                        if (isProgramStruct)
                        {
                            var topic = catalogGroupCache.TopicsMapById[metaId];
                            if (!EpgAssetManager.TopicsInBasicProgramTable.Contains(topic.SystemName))
                            {
                                if (topic.Type == MetaType.Tag)
                                {
                                    long epgTagId = 0;
                                    if (mappingFields[FieldTypes.Tag].ContainsKey(topic.SystemName.ToLower()))
                                    {
                                        epgTagId = mappingFields[FieldTypes.Tag][topic.SystemName.ToLower()];
                                    }
                                    epgTagIdsToValue.Add(new KeyValuePair<long, string>(epgTagId, topic.SystemName));
                                }
                                else
                                {
                                    long epgMetaId = 0;
                                    if (mappingFields[FieldTypes.Meta].ContainsKey(topic.SystemName.ToLower()))
                                    {
                                        epgMetaId = mappingFields[FieldTypes.Meta][topic.SystemName.ToLower()];
                                    }
                                    epgMetaIdsToValue.Add(new KeyValuePair<long, string>(epgMetaId, topic.SystemName));
                                }
                            }
                        }
                    }

                    // no need to update DB if lists are equal
                    if (assetStruct != null && assetStruct.MetaIds != null && assetStructToUpdate.MetaIds.SequenceEqual(assetStruct.MetaIds))
                    {
                        shouldUpdateMetaIds = false;
                    }
                }

                // check if assets and index should be updated now
                if (shouldUpdateAssetStructAssets && assetStruct != null && assetStruct.MetaIds != null && assetStructToUpdate.MetaIds != null)
                {
                    removedTopicIds = assetStruct.MetaIds.Except(assetStructToUpdate.MetaIds).ToList();
                    // if its just the oreder of the metas being changed on the asset struct we don't need to update the assets
                    if (removedTopicIds == null || removedTopicIds.Count == 0)
                    {
                        shouldUpdateAssetStructAssets = false;
                    }
                }

                if (assetStructToUpdate.NamesInOtherLanguages != null)
                {
                    shouldUpdateOtherNames = true;
                    languageCodeToName = new List<KeyValuePair<string, string>>();
                    foreach (LanguageContainer language in assetStructToUpdate.NamesInOtherLanguages)
                    {
                        languageCodeToName.Add(new KeyValuePair<string, string>(language.m_sLanguageCode3, language.m_sValue));
                    }
                }

                if (assetStructToUpdate.DynamicData == null)
                {
                    assetStructToUpdate.DynamicData = catalogGroupCache.AssetStructsMapById[id].DynamicData;
                }

                result = _assetStructRepository.UpdateAssetStruct(
                    groupId,
                    userId,
                    assetStruct.Id,
                    assetStructToUpdate,
                    shouldUpdateOtherNames,
                    languageCodeToName,
                    shouldUpdateMetaIds,
                    metaIdsToPriority);
                // For backward compatibility
                if (isProgramStruct)
                {
                    if (epgMetaIdsToValue.Count > 0 && !_assetStructMetaRepository.UpdateEpgAssetStructMetas(groupId, epgMetaIdsToValue, userId))
                    {
                        log.ErrorFormat("UpdateEpgAssetStructMetas faild for groupId: {0}, epgMetaIdsToValue: {1}.", groupId, string.Join(",", epgMetaIdsToValue));
                    }

                    if (epgTagIdsToValue.Count > 0 && !CatalogDAL.UpdateEpgAssetStructTags(groupId, epgTagIdsToValue, userId))
                    {
                        log.ErrorFormat("UpdateEpgAssetStructTags faild for groupId: {0}, epgTagIdsToValue: {1}.", groupId, string.Join(",", epgTagIdsToValue));
                    }
                }

                if (shouldCheckRegularFlowValidations)
                {
                    if (result.Status != null && result.Status.Code == (int)eResponseStatus.OK && shouldUpdateAssetStructAssets && removedTopicIds != null && removedTopicIds.Count > 0)
                    {
                        List<long> tagTopicIds = new List<long>();
                        List<long> metaTopicIds = new List<long>();
                        List<long> relatedEntitiesTopicIds = new List<long>();

                        foreach (long topicId in removedTopicIds)
                        {
                            if (catalogGroupCache.TopicsMapById.ContainsKey(topicId))
                            {
                                Topic topicToRemoveFromAssetStruct = catalogGroupCache.TopicsMapById[topicId];
                                if (topicToRemoveFromAssetStruct.Type == MetaType.Tag)
                                {
                                    tagTopicIds.Add(topicId);
                                }
                                else if (topicToRemoveFromAssetStruct.Type == MetaType.ReleatedEntity)
                                {
                                    relatedEntitiesTopicIds.Add(topicId);
                                }
                                else
                                {
                                    metaTopicIds.Add(topicId);
                                }
                            }
                        }

                        if (!InvalidateCacheAndUpdateIndexForTopicAssets(groupId, tagTopicIds, false, false, metaTopicIds, id, userId, relatedEntitiesTopicIds, false))
                        {
                            log.ErrorFormat("Failed InvalidateCacheAndUpdateIndexForTopicAssets for groupId: {0}, assetStructId: {1}, tagTopicIds: {2}, metaTopicIds: {3}, relatedEntitiesTopicIds: {4}",
                                            groupId, id, string.Join(",", tagTopicIds), string.Join(",", metaTopicIds), string.Join(",", relatedEntitiesTopicIds));
                        }
                    }

                    InvalidateCatalogGroupCache(groupId, result.Status, true, result.Object);
                }

                if (isProgramStruct)
                {
                    result.Object.Id = 0;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateAssetStruct for groupId: {0}, id: {1} and assetStruct: {2}", groupId, id, assetStructToUpdate.ToString()), ex);
            }

            return result;
        }

        public Status DeleteAssetStruct(int groupId, long id, long userId)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling DeleteAssetStruct", groupId);
                    return result;
                }

                id = catalogGroupCache.GetRealAssetStructId(id, out bool isProgramAssetStruct);
                if (!catalogGroupCache.AssetStructsMapById.ContainsKey(id))
                {
                    result = new Status((int)eResponseStatus.AssetStructDoesNotExist, eResponseStatus.AssetStructDoesNotExist.ToString());
                    return result;
                }

                AssetStruct assetStruct = new AssetStruct(catalogGroupCache.AssetStructsMapById[id]);
                if (assetStruct.IsPredefined.HasValue && assetStruct.IsPredefined.Value)
                {
                    result = new Status((int)eResponseStatus.CanNotDeletePredefinedAssetStruct, eResponseStatus.CanNotDeletePredefinedAssetStruct.ToString());
                    return result;
                }

                bool haveChildren = catalogGroupCache.AssetStructsMapById.Any(x => x.Value.ParentId.HasValue && x.Value.ParentId.Value == id);
                if (haveChildren)
                {
                    result = new Status((int)eResponseStatus.CanNotDeleteParentAssetStruct, "Can not delete AssetStruct with children");
                    return result;
                }

                // check AssetStruct is not part of category ExtendedTypes
                GenericListResponse<ObjectVirtualAssetPartnerConfig> objectVirtualAssetPartnerConfig =
                    VirtualAssetPartnerConfigManager.Instance.GetObjectVirtualAssetPartnerConfiguration(groupId);
                if (objectVirtualAssetPartnerConfig.IsOkStatusCode() && objectVirtualAssetPartnerConfig.HasObjects())
                {
                    foreach (ObjectVirtualAssetInfo objectVirtualAssetInfo in objectVirtualAssetPartnerConfig.Objects[0].ObjectVirtualAssets)
                    {
                        if (objectVirtualAssetInfo?.ExtendedTypes?.Count > 0)
                        {
                            var assetStructExist = objectVirtualAssetInfo.ExtendedTypes.Values.Contains(id);
                            if (assetStructExist)
                            {
                                result = new Status((int)eResponseStatus.CannotDeleteAssetStruct, $"Can not delete mapped AssetStruct {id}");
                                return result;
                            }
                        }
                    }
                }

                if (CatalogDAL.DeleteAssetStruct(groupId, id, userId))
                {
                    List<long> tagTopicIds = new List<long>();
                    List<long> metaTopicIds = new List<long>();
                    List<long> relatedEntitiesTopicIds = new List<long>();
                    foreach (long topicId in assetStruct.MetaIds)
                    {
                        if (catalogGroupCache.TopicsMapById.ContainsKey(topicId))
                        {
                            Topic topicToRemoveFromAssetStruct = catalogGroupCache.TopicsMapById[topicId];
                            if (topicToRemoveFromAssetStruct.Type == MetaType.Tag)
                            {
                                tagTopicIds.Add(topicId);
                            }
                            else if (topicToRemoveFromAssetStruct.Type == MetaType.ReleatedEntity)
                            {
                                relatedEntitiesTopicIds.Add(topicId);
                            }
                            else
                            {
                                metaTopicIds.Add(topicId);
                            }
                        }
                    }

                    if (!InvalidateCacheAndUpdateIndexForTopicAssets(groupId, tagTopicIds, false, true, metaTopicIds, id, userId, relatedEntitiesTopicIds, false))
                    {
                        log.ErrorFormat("Failed InvalidateCacheAndUpdateIndexForTopicAssets for groupId: {0}, assetStructId: {1}, tagTopicIds: {2}, metaTopicIds: {3}, relatedEntitiesTopicIds: {4}",
                                        groupId, id, string.Join(",", tagTopicIds), string.Join(",", metaTopicIds), string.Join(",", relatedEntitiesTopicIds));
                    }

                    result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    InvalidateCatalogGroupCache(groupId, result, false);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed DeleteAssetStruct for groupId: {0} and assetStructId: {1}", groupId, id), ex);
            }

            return result;
        }

        public GenericResponse<AssetStruct> GetAssetStruct(int groupId, long assetStructId)
        {
            GenericResponse<AssetStruct> response = new GenericResponse<AssetStruct>();

            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetAssetStruct", groupId);
                    return response;
                }

                bool isProgramStruct;
                assetStructId = catalogGroupCache.GetRealAssetStructId(assetStructId, out isProgramStruct);

                if (!catalogGroupCache.AssetStructsMapById.ContainsKey(assetStructId))
                {
                    response.SetStatus(eResponseStatus.AssetStructDoesNotExist);
                }
                else
                {
                    response.Object = new AssetStruct(catalogGroupCache.AssetStructsMapById[assetStructId]);
                    response.SetStatus(eResponseStatus.OK);
                }

                if (isProgramStruct)
                {
                    response.Object.Id = 0;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("An Exception was occurred in GetAssetStruct. groupId:{0}, assetStructId:{1}.",
                                        groupId, assetStructId), ex);
                response.SetStatus(eResponseStatus.Error);
            }

            return response;
        }

        public BooleanLeafFieldDefinitions GetMetaByName(MetaByNameInput input)
        {
            if (!CheckMetaExists(input))
            {
                var errorMessage = $"meta not exists for group -  unified search definitions. groupId = {input.GroupId}, meta name = {input.MetaName}";
                //return error - meta not exists
                log.Error(errorMessage);

                throw new Exception(errorMessage);
            }

            var lowercasedMetaName = input.MetaName.ToLower();
            if (DoesGroupUsesTemplates(input.GroupId))
            {
                return GetUnifiedSearchKey(input.GroupId, lowercasedMetaName).FirstOrDefault();
            }

            var parentGroupId = _catalogCache.GetParentGroup(input.GroupId);
            var parentGroup = _groupManager.GetGroup(parentGroupId);

            return CatalogLogic.GetUnifiedSearchKey(input.MetaName, parentGroup).FirstOrDefault();
        }

        private bool CheckMetaExists(MetaByNameInput input)
        {
            var lowercasedMetaName = input.MetaName.ToLower();
            if (DoesGroupUsesTemplates(input.GroupId))
            {
                return CheckMetaExists(input.GroupId, lowercasedMetaName);
            }

            var parentGroupId = _catalogCache.GetParentGroup(input.GroupId);
            var parentGroup = _groupManager.GetGroup(parentGroupId);

            return Utils.CheckMetaExsits(
                input.ShouldSearchEpg,
                input.ShouldSearchMedia,
                input.ShouldSearchRecordings,
                parentGroup,
                lowercasedMetaName);
        }

        public HashSet<BooleanLeafFieldDefinitions> GetUnifiedSearchKey(int groupId, string originalKey)
        {
            Type valueType = typeof(string);
            var searchKeys = new HashSet<BooleanLeafFieldDefinitions>();
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetUnifiedSearchKey", groupId);
                    return searchKeys;
                }

                List<string> tags = catalogGroupCache.TopicsMapBySystemNameAndByType.Where(x => !TopicsToIgnore.Contains(x.Key) && x.Value.ContainsKey(MetaType.Tag.ToString()))
                                                                                    .Select(x => x.Key).ToList();

                List<string> metas = catalogGroupCache.TopicsMapBySystemNameAndByType.Where(x => !TopicsToIgnore.Contains(x.Key)
                                                                                                && (x.Value.ContainsKey(MetaType.String.ToString())
                                                                                                    || x.Value.ContainsKey(MetaType.MultilingualString.ToString())
                                                                                                    || x.Value.ContainsKey(MetaType.Number.ToString())
                                                                                                    || x.Value.ContainsKey(MetaType.Bool.ToString())
                                                                                                    || x.Value.ContainsKey(MetaType.DateTime.ToString())))
                                                                                     .Select(x => x.Key).ToList();
                if (originalKey.StartsWith("tags."))
                {
                    foreach (string tag in tags)
                    {
                        if (tag.Equals(originalKey.Substring(5), StringComparison.OrdinalIgnoreCase))
                        {
                            searchKeys.Add(new BooleanLeafFieldDefinitions()
                            {
                                Field = tag.ToLower(),
                                ValueType = valueType,
                                FieldType = eFieldType.Tag
                            });
                            break;
                        }
                    }
                }
                else if (originalKey.StartsWith("metas."))
                {
                    foreach (string meta in metas)
                    {
                        if (meta.Equals(originalKey.Substring(6), StringComparison.OrdinalIgnoreCase))
                        {
                            valueType = catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(meta) ? GetMetaType(catalogGroupCache.TopicsMapBySystemNameAndByType[meta]) : typeof(string);

                            searchKeys.Add(new BooleanLeafFieldDefinitions()
                            {
                                Field = meta.ToLower(),
                                ValueType = valueType,
                                FieldType = valueType == typeof(string) ? eFieldType.StringMeta : eFieldType.NonStringMeta
                            });
                            break;
                        }
                    }
                }
                else
                {
                    foreach (string tag in tags)
                    {
                        if (tag.Equals(originalKey, StringComparison.OrdinalIgnoreCase))
                        {
                            searchKeys.Add(new BooleanLeafFieldDefinitions()
                            {
                                Field = tag.ToLower(),
                                ValueType = valueType,
                                FieldType = eFieldType.Tag
                            });
                            break;
                        }
                    }

                    foreach (string meta in metas)
                    {
                        if (meta.Equals(originalKey, StringComparison.OrdinalIgnoreCase))
                        {
                            valueType = catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(meta) ? GetMetaType(catalogGroupCache.TopicsMapBySystemNameAndByType[meta]) : typeof(string);

                            searchKeys.Add(new BooleanLeafFieldDefinitions()
                            {
                                Field = meta.ToLower(),
                                ValueType = valueType,
                                FieldType = valueType == typeof(string) ? eFieldType.StringMeta : eFieldType.NonStringMeta
                            });
                            break;
                        }
                    }
                }

                if (searchKeys.Count == 0)
                {
                    searchKeys.Add(new BooleanLeafFieldDefinitions()
                    {
                        Field = originalKey.ToLower(),
                        ValueType = valueType,
                        FieldType = eFieldType.Default
                    });
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetUnifiedSearchKey for groupId: {0}, originalKey: {1}", groupId, originalKey), ex);
            }

            return searchKeys;
        }

        public bool CheckMetaExists(int groupId, string metaName)
        {
            bool result = false;
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling CheckMetaExsits", groupId);
                    return result;
                }

                result = catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(metaName)
                            && catalogGroupCache.TopicsMapBySystemNameAndByType[metaName].Any(x => x.Key != MetaType.Tag.ToString());
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed CheckMetaExsits for groupId: {0}, metaName: {1}", groupId, metaName), ex);
            }

            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public List<TagValue> GetAllTagValues(int groupId)
        {
            List<TagValue> result = new List<TagValue>();

            CatalogGroupCache catalogGroupCache;
            if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetGroupAssets", groupId);
                return null;
            }

            int defaultLanguage = catalogGroupCache.GetDefaultLanguage().ID;
            DataSet dataSet = null;
            try
            {
                dataSet = CatalogDAL.GetGroupTagValues(groupId);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error when getting group tag values for groupId {0}. ex = {1}", groupId, ex);
                dataSet = null;
            }

            if (dataSet == null)
            {
                return null;
            }

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0 &&
                dataSet.Tables[0] != null && dataSet.Tables[0].Rows != null && dataSet.Tables[0].Rows.Count > 0)
            {
                // First table - default language, set default for other languages as well
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    int tagId = ODBCWrapper.Utils.ExtractInteger(row, "tag_id");
                    int topicId = ODBCWrapper.Utils.ExtractInteger(row, "topic_id");
                    string tagValue = ODBCWrapper.Utils.ExtractString(row, "tag_value");
                    DateTime createDate = ODBCWrapper.Utils.ExtractDateTime(row, "create_date");
                    DateTime updateDate = ODBCWrapper.Utils.ExtractDateTime(row, "update_date");

                    if (catalogGroupCache.TopicsMapById.ContainsKey(topicId))
                    {
                        Topic topic = catalogGroupCache.TopicsMapById[topicId];

                        result.Add(new TagValue()
                        {
                            createDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(createDate),
                            languageId = defaultLanguage,
                            tagId = tagId,
                            topicId = topicId,
                            updateDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(updateDate),
                            value = tagValue
                        });
                    }
                }
            }

            // Second table - translations
            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 1 &&
                dataSet.Tables[1] != null && dataSet.Tables[1].Rows != null && dataSet.Tables[1].Rows.Count > 0)
            {
                foreach (DataRow row in dataSet.Tables[1].Rows)
                {
                    int tagId = ODBCWrapper.Utils.ExtractInteger(row, "tag_id");
                    int topicId = ODBCWrapper.Utils.ExtractInteger(row, "topic_id");
                    int languageId = ODBCWrapper.Utils.ExtractInteger(row, "language_id");
                    string tagValue = ODBCWrapper.Utils.ExtractString(row, "tag_value");
                    DateTime createDate = ODBCWrapper.Utils.ExtractDateTime(row, "create_date");
                    DateTime updateDate = ODBCWrapper.Utils.ExtractDateTime(row, "update_date");

                    if (catalogGroupCache.TopicsMapById.ContainsKey(topicId))
                    {
                        Topic topic = catalogGroupCache.TopicsMapById[topicId];

                        result.Add(new TagValue()
                        {
                            createDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(createDate),
                            languageId = languageId,
                            tagId = tagId,
                            topicId = topicId,
                            updateDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(updateDate),
                            value = tagValue
                        });
                    }
                }
            }

            return result;
        }

        public GenericResponse<TagValue> AddTag(int groupId, TagValue tag, long userId, bool isFromIngest = false)
        {
            GenericResponse<TagValue> result = new GenericResponse<TagValue>();
            try
            {
                List<KeyValuePair<string, string>> languageCodeToName = new List<KeyValuePair<string, string>>();
                if (tag.TagsInOtherLanguages != null && tag.TagsInOtherLanguages.Count > 0)
                {
                    foreach (LanguageContainer language in tag.TagsInOtherLanguages)
                    {
                        languageCodeToName.Add(new KeyValuePair<string, string>(language.m_sLanguageCode3, language.m_sValue));
                    }
                }

                CatalogGroupCache catalogGroupCache;
                if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling AddTag", groupId);
                    return result;
                }

                if (catalogGroupCache.TopicsMapById != null && catalogGroupCache.TopicsMapById.Count > 0)
                {
                    bool topic = catalogGroupCache.TopicsMapById.ContainsKey(tag.topicId);
                    if (!topic)
                    {
                        result.Status.Code = (int)eResponseStatus.TopicNotFound;
                        result.Status.Message = eResponseStatus.TopicNotFound.ToString();
                        log.ErrorFormat("Error at AddTag. TopicId not found. GroupId: {0}", groupId);
                        return result;
                    }
                }

                DataSet ds = CatalogDAL.InsertTag(groupId, tag.value, languageCodeToName, tag.topicId, userId);
                result = CreateTagValueFromDataSet(ds);

                if (!result.HasObject())
                {
                    return result;
                }

                if (!isFromIngest)
                {
                    var indexManager = IndexManagerFactory.Instance.GetIndexManager(groupId);
                    result.SetStatus(indexManager.UpdateTag(result.Object));
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddTag for groupId: {0} and tag: {1}", groupId, tag.ToString()), ex);
            }

            return result;
        }

        public static GenericResponse<TagValue> UpdateTag(int groupId, TagValue tagToUpdate, long userId, bool isFromIngest = false)
        {
            var result = new GenericResponse<TagValue>();

            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling UpdateTag", groupId);
                    return result;
                }

                result = GetTagById(groupId, tagToUpdate.tagId);
                if (!result.HasObject())
                {
                    return result;
                }

                tagToUpdate.TagsInOtherLanguages = tagToUpdate.TagsInOtherLanguages.Union(result.Object.TagsInOtherLanguages, new LanguageContainerComparer()).ToList();
                if (!result.Object.IsNeedToUpdate(tagToUpdate))
                {
                    result.SetStatus(eResponseStatus.NoValuesToUpdate);
                    return result;
                }

                List<KeyValuePair<string, string>> languageCodeToName = null;
                bool shouldUpdateOtherNames = false;

                if (tagToUpdate.TagsInOtherLanguages != null && tagToUpdate.TagsInOtherLanguages.Count > 0)
                {
                    shouldUpdateOtherNames = true;
                    languageCodeToName = new List<KeyValuePair<string, string>>();
                    foreach (LanguageContainer language in tagToUpdate.TagsInOtherLanguages)
                    {
                        languageCodeToName.Add(new KeyValuePair<string, string>(language.m_sLanguageCode3, language.m_sValue));
                    }
                }

                Topic topic = null;

                if (catalogGroupCache.TopicsMapById != null && catalogGroupCache.TopicsMapById.Count > 0)
                {
                    bool topicFound = catalogGroupCache.TopicsMapById.ContainsKey(tagToUpdate.topicId);

                    if (!topicFound)
                    {
                        result.SetStatus(eResponseStatus.TopicNotFound);
                        log.ErrorFormat("Error at UpdateTag. TopicId not found. GroupId: {0}", groupId);
                        return result;
                    }
                    else
                    {
                        topic = catalogGroupCache.TopicsMapById[tagToUpdate.topicId];
                    }
                }

                DataSet ds = CatalogDAL.UpdateTag(groupId, tagToUpdate.tagId, tagToUpdate.value, shouldUpdateOtherNames, languageCodeToName, tagToUpdate.topicId, userId);
                result = CreateTagValueFromDataSet(ds);
                if (!result.HasObject())
                {
                    return result;
                }

                if (!isFromIngest)
                {
                    List<int> mediaIds, epgIds;
                    if (!InvalidateCacheForTagAssets(groupId, tagToUpdate.tagId, false, userId, out mediaIds, out epgIds))
                    {
                        log.ErrorFormat("Failed to InvalidateCacheForTagAssets after UpdateTag for groupId: {0}, tagId: {1}", groupId, tagToUpdate.tagId);
                    }

                    //
                    // TODO: REMOVE THIS ONCE DONE WITH REST OF PARTIAL UPDATE
                    //
                    InvalidateCacheAndUpdateIndexForAssets(groupId, false, mediaIds, epgIds);

                    if (shouldUpdateOtherNames)
                    {
                        foreach (var pair in languageCodeToName)
                        {
                            string originalValue = string.Empty;
                            var tagInOtherLanguage = result.Object.TagsInOtherLanguages.FirstOrDefault(t => t.m_sLanguageCode3 == pair.Key);
                            if (tagInOtherLanguage != null)
                            {
                                originalValue = tagInOtherLanguage.m_sValue;
                            }
                        }
                    }

                    var indexManager = IndexManagerFactory.Instance.GetIndexManager(groupId);
                    result.SetStatus(indexManager.UpdateTag(result.Object));
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateTag for groupId: {0}, id: {1} and tagToUpdate: {2}", groupId, tagToUpdate.tagId, tagToUpdate.ToString()), ex);
            }

            return result;
        }

        public static Status DeleteTag(int groupId, long tagId, long userId)
        {
            var tagResponse = new GenericResponse<TagValue>();

            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling DeleteTag", groupId);
                    return tagResponse.Status;
                }

                tagResponse = GetTagById(groupId, tagId);
                if (!tagResponse.HasObject())
                {
                    return tagResponse.Status;
                }

                if (CatalogDAL.DeleteTag(groupId, tagId, userId))
                {
                    var wrapper = IndexManagerFactory.Instance.GetIndexManager(groupId);
                    tagResponse.SetStatus(wrapper.DeleteTag(tagId));
                    if (!tagResponse.HasObject())
                    {
                        return tagResponse.Status;
                    }

                    List<int> mediaIds;
                    List<int> epgIds;

                    if (!InvalidateCacheForTagAssets(groupId, tagId, true, userId, out mediaIds, out epgIds))
                    {
                        log.ErrorFormat("Failed to InvalidateCacheForTagAssets after DeleteTag for groupId: {0}, tagId: {1}", groupId, tagId);
                    }

                    if (!UpdateIndexForAssets(groupId, false, mediaIds, epgIds))
                    {
                        log.ErrorFormat("Failed to UpdateIndexForAssets after DeleteTag for groupId: {0}, tagId: {1}", groupId, tagId);
                    }

                    tagResponse.SetStatus(eResponseStatus.OK);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed DeleteTag for groupId: {0} and tagId: {1}", groupId, tagId), ex);
                tagResponse.SetStatus(eResponseStatus.Error);
            }

            return tagResponse.Status;
        }

        /// <summary>
        /// Returns all tag values for all languages by a given tag ID
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="tagId"></param>
        /// <returns></returns>
        public static GenericResponse<TagValue> GetTagById(int groupId, long tagId)
        {
            var result = new GenericResponse<TagValue>();

            try
            {
                DataSet ds = CatalogDAL.GetTag(groupId, tagId);
                result = CreateTagValueFromDataSet(ds);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetTagById for groupId: {0} and tagId: {1}", groupId, tagId), ex);
            }

            return result;
        }

        public static GenericResponse<TagValue> GetTagByValue(int groupId, string value, long topicId)
        {
            var result = new GenericResponse<TagValue>();

            try
            {
                DataSet ds = CatalogDAL.GetTagByValue(groupId, value, topicId);
                result = CreateTagValueFromDataSet(ds);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetTagByValue for groupId: {0}, value: {1}, topic:{2}", groupId, value, topicId), ex);
            }

            return result;
        }

        public static GenericListResponse<TagValue> SearchTags(int groupId, bool isExcatValue, string searchValue, int topicId, int searchLanguageId, int pageIndex, int pageSize)
        {
            GenericListResponse<TagValue> result = new GenericListResponse<TagValue>();
            CatalogGroupCache catalogGroupCache;
            if (!Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling SearchTags", groupId);
                return result;
            }

            LanguageObj searchLanguage = null;
            if (searchLanguageId != 0)
            {
                catalogGroupCache.LanguageMapById.TryGetValue(searchLanguageId, out searchLanguage);

                if (searchLanguage == null)
                {
                    log.ErrorFormat("Invalid language id {0}", searchLanguageId);
                    return result;
                }
            }

            TagSearchDefinitions definitions = new TagSearchDefinitions()
            {
                GroupId = groupId,
                Language = searchLanguage,
                PageIndex = pageIndex,
                PageSize = pageSize,
                AutocompleteSearchValue = !isExcatValue && !string.IsNullOrEmpty(searchValue) ? searchValue.Trim() : string.Empty,
                ExactSearchValue = isExcatValue && !string.IsNullOrEmpty(searchValue) ? searchValue : string.Empty,
                TopicId = topicId
            };

            int totalItemsCount = 0;
            var indexManager = IndexManagerFactory.Instance.GetIndexManager(groupId);
            List<TagValue> tagValues = indexManager.SearchTags(definitions, out totalItemsCount);
            HashSet<long> tagIds = new HashSet<long>();

            foreach (TagValue tagValue in tagValues)
            {
                if (!tagIds.Contains(tagValue.tagId))
                {
                    tagIds.Add(tagValue.tagId);
                    var tagResponse = GetTagById(groupId, tagValue.tagId);
                    if (tagResponse.HasObject())
                    {
                        result.Objects.Add(tagResponse.Object);
                    }
                }
            }

            result.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            result.TotalItems = totalItemsCount;

            return result;
        }

        public GenericListResponse<TagValue> GetTags(int groupId, List<long> idIn, int pageIndex, int pageSize)
        {
            GenericListResponse<TagValue> result = new GenericListResponse<TagValue>();

            var tagValues = GetTagValues(groupId, idIn, pageIndex, pageSize, out int totalItemsCount);

            foreach (TagValue tagValue in tagValues)
            {
                var tagResponse = GetTagById(groupId, tagValue.tagId);
                if (tagResponse.HasObject())
                {
                    result.Objects.Add(tagResponse.Object);
                }
            }

            result.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            result.TotalItems = totalItemsCount;

            return result;
        }

        public List<TagValue> GetTagValues(int groupId, List<long> idIn, int pageIndex, int pageSize, out int totalItemsCount)
        {
            var result = new List<TagValue>();
            totalItemsCount = 0;

            try
            {
                if (!TryGetCatalogGroupCacheFromCache(groupId, out CatalogGroupCache catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling SearchTags", groupId);
                    return result;
                }

                var indexManager = IndexManagerFactory.Instance.GetIndexManager(groupId);
                var tagDefinitions = new TagSearchDefinitions()
                {
                    GroupId = groupId,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TagIds = idIn
                };

                result = indexManager.SearchTags(tagDefinitions, out totalItemsCount);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed SearchTags for groupId: {0}", groupId), ex);
            }

            return result;
        }

        public GenericResponse<AssetStructMeta> UpdateAssetStructMeta(long assetStructId, long metaId, AssetStructMeta assetStructMeta, int groupId, long userId)
        {
            GenericResponse<AssetStructMeta> response = new GenericResponse<AssetStructMeta>();

            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling UpdateAssetStructMeta", groupId);
                    return response;
                }

                //BEO-9803
                if (assetStructId == 0)
                {
                    assetStructId = catalogGroupCache.GetRealAssetStructId(assetStructId, out bool isProgramStruct);
                }

                if (!catalogGroupCache.AssetStructsMapById.ContainsKey(assetStructId))
                {
                    response.SetStatus(eResponseStatus.AssetStructDoesNotExist, eResponseStatus.AssetStructDoesNotExist.ToString());
                    return response;
                }

                var assetStruct = catalogGroupCache.AssetStructsMapById[assetStructId];

                if (!assetStruct.MetaIds.Contains(metaId))
                {
                    response.SetStatus(eResponseStatus.MetaDoesNotExist, eResponseStatus.MetaDoesNotExist.ToString());
                    return response;
                }

                // Validate Metadata Inheritance
                Status metaDataInheritanceStatus = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                if (assetStructMeta.IsLocationTag.HasValue && assetStructMeta.IsLocationTag.Value)
                {
                    if (catalogGroupCache.TopicsMapById[metaId].Type != MetaType.Number)
                    {
                        response.SetStatus(eResponseStatus.InvalidMetaType, "Location tag can be set only to numeric metas.");
                        return response;
                    }

                    var locationTagMeta = assetStruct.AssetStructMetas.Values.FirstOrDefault(x => x.IsLocationTag.HasValue && x.IsLocationTag.Value);
                    if (locationTagMeta != null)
                    {
                        response.SetStatus(eResponseStatus.TagAlreadyInUse, $"Location tag is already in use for AssetStruct {assetStructId}.");
                        return response;
                    }
                }

                bool needToHandleHeritage = false;
                if (assetStructMeta.IsInherited.HasValue && assetStructMeta.IsInherited.Value)
                {
                    metaDataInheritanceStatus = ValidateMetadataInheritance(catalogGroupCache, assetStructId, metaId, assetStructMeta, out needToHandleHeritage);
                    if (metaDataInheritanceStatus.Code != (int)eResponseStatus.OK)
                    {
                        response.SetStatus(metaDataInheritanceStatus);
                        return response;
                    }
                }

                //Always allow updating with empty string alias to use system name
                if (!string.IsNullOrWhiteSpace(assetStructMeta.Alias))
                {
                    MetaType metaType = catalogGroupCache.TopicsMapById[metaId].Type;
                    var aliasUnique = ValidateAliasContentAndUniqueness(assetStruct, metaId, assetStructMeta.Alias, metaType,  catalogGroupCache.TopicsMapById);
                    if (!aliasUnique.IsOkStatusCode())
                    {
                        response.SetStatus(aliasUnique);
                        return response;
                    }
                }

                var dtoObject = ConvertToDto(assetStructMeta, metaId, assetStructId);
                var updatedMeta  = _assetStructMetaRepository.UpdateAssetStructMeta(groupId, userId, dtoObject, out bool success);

                if (success && !string.IsNullOrEmpty(updatedMeta.Alias))
                {
                    _layeredCache.SetInvalidationKey(LayeredCacheKeys.GetGroupUsingAliasNamesInvalidationKey(groupId));
                }
                
                response.Object = updatedMeta;

                response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                InvalidateCatalogGroupCache(groupId, response.Status, true, response.Object);

                if (needToHandleHeritage)
                {
                    var data = new InheritanceAssetStructMeta()
                    {
                        AssetStructId = assetStructId,
                        MetaId = metaId
                    };

                    var queue = new GenericCeleryQueue();
                    InheritanceData inheritanceData = new InheritanceData(groupId, InheritanceType.AssetStructMeta, JsonConvert.SerializeObject(data), userId);
                    bool enqueueSuccessful = queue.Enqueue(inheritanceData, string.Format("PROCESS_ASSET_INHERITANCE\\{0}", groupId));
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateAssetStructMeta for groupId: {0}, assetStructId: {1}, MetaId: {2} and assetStructMeta: {3}",
                                        groupId, assetStructId, metaId, assetStructMeta.ToString()), ex);
            }

            return response;
        }

        public bool IsGroupUsingAliases(int groupId)
        {
            var response = false;
            var key = LayeredCacheKeys.GetGroupUsingAliasNamesKey(groupId);

            if (!_layeredCache.Get(key,
                                  ref response,
                                  arg => Tuple.Create(_assetStructMetaRepository.GetGroupUsingAliases(groupId), true),
                                  null,
                                  groupId,
                                  LayeredCacheConfigNames.GET_GROUP_USING_ALIAS_NAMES,
                                  new List<string> { LayeredCacheKeys.GetGroupUsingAliasNamesInvalidationKey(groupId) },
                                  true))
            {
                log.Error($"Failed getting IsGroupUsingAliases from LayeredCache, groupId: {groupId}, key: {key}");
            }

            return response;
        }

        private AssetStructMetaDTO ConvertToDto(AssetStructMeta assetStructMeta, long metaId = 0, long assetStructId = 0)
        {
            var response = new AssetStructMetaDTO
            {
                DefaultIngestValue = assetStructMeta.DefaultIngestValue,
                IngestReferencePath = assetStructMeta.IngestReferencePath,
                IsInherited = assetStructMeta.IsInherited,
                IsLocationTag = assetStructMeta.IsLocationTag,
                ProtectFromIngest = assetStructMeta.ProtectFromIngest,
                AssetStructId = assetStructId > 0 ? assetStructId : assetStructMeta.AssetStructId,
                MetaId = metaId > 0 ? metaId : assetStructMeta.MetaId,
                SuppressedOrder = assetStructMeta.SuppressedOrder,
                CreateDate = assetStructMeta.CreateDate,
                UpdateDate = assetStructMeta.UpdateDate,
                Alias = assetStructMeta.Alias
            };
            return response;
        }

        /// <summary>
        /// Validate asset wasn't used for system name or in other aliases
        /// </summary>
        private Status ValidateAliasContentAndUniqueness(AssetStruct assetStruct, long metaId, string alias, MetaType metaType, Dictionary<long, Topic> topics)
        {
            var status = Status.Ok;

            //Validate SystemName != Alias value
            foreach (var _metaId in assetStruct.MetaIds)
            {
                if (metaId != _metaId && topics.ContainsKey(_metaId) &&
                    topics[_metaId].SystemName.Equals(alias))
                {
                    status.Set(eResponseStatus.AliasMustBeUnique,
                   $"Alias: {alias} is already in use as system name for metaId: {_metaId}");
                    return status;
                }
            }

            //Validate unique value by other meta alias and types
            var assetStructMetasWithAlias = assetStruct.AssetStructMetas.Where(asm => asm.Key != metaId && asm.Value.Alias.Equals(alias))?
                .FirstOrDefault(asm => topics.ContainsKey(asm.Key) && topics[asm.Key].Type.Equals(metaType));

            if (assetStructMetasWithAlias.HasValue && !assetStructMetasWithAlias.Value.IsDefault())
            {
                status.Set(eResponseStatus.AliasMustBeUnique,
                       $"Alias: {alias} is already in use for meta {assetStructMetasWithAlias.Value.Key} as assetStructMeta alias with type: {metaType}");
                return status;
            }

            return status;
        }

        private static Status ValidateMetadataInheritance(CatalogGroupCache catalogGroupCache, long assetStructId, long metaId, AssetStructMeta assetStructMeta,
            out bool needToHandleHeritage)
        {
            needToHandleHeritage = false;
            Status status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            // Get AssetStructMeta from cache, compare IsInherited with current assetStructMeta
            AssetStructMeta currentAssetStructMeta = catalogGroupCache.AssetStructsMapById[assetStructId].AssetStructMetas[metaId];
            if (currentAssetStructMeta.IsInherited == assetStructMeta.IsInherited)
            {
                return status;
            }

            AssetStruct currentAssetStruct = catalogGroupCache.AssetStructsMapById[assetStructId];
            if (!currentAssetStruct.ParentId.HasValue || currentAssetStruct.ParentId.Value == 0)
            {
                return new Status((int)eResponseStatus.NoParentAssociatedToTopic, "No parent associated to topic");
            }

            // check meta existed at parent
            AssetStruct parentAssetStruct = catalogGroupCache.AssetStructsMapById[currentAssetStruct.ParentId.Value];
            if (!parentAssetStruct.MetaIds.Contains(metaId))
            {
                return new Status((int)eResponseStatus.MetaDoesNotBelongToParentAssetStruct, "Meta does not belong to ParentAssetStruct");
            }

            needToHandleHeritage = true;
            return status;
        }

        public GenericListResponse<AssetStructMeta> GetAssetStructMetaList(int groupId, long? assetStructId, long? metaId)
        {
            GenericListResponse<AssetStructMeta> response = new GenericListResponse<AssetStructMeta>();

            try
            {
                if (assetStructId.HasValue)
                {
                    CatalogGroupCache catalogGroupCache;
                    if (!TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetAssetStructMetaList", groupId);
                        return response;
                    }

                    assetStructId = catalogGroupCache.GetRealAssetStructId(assetStructId.Value, out bool isProgramStruct);

                    if (catalogGroupCache.AssetStructsMapById.ContainsKey(assetStructId.Value))
                    {
                        if (metaId.HasValue)
                        {
                            if (catalogGroupCache.AssetStructsMapById[assetStructId.Value].AssetStructMetas.ContainsKey(metaId.Value))
                            {
                                response.Objects.Add(catalogGroupCache.AssetStructsMapById[assetStructId.Value].AssetStructMetas[metaId.Value]);
                            }
                        }
                        else
                        {
                            response.Objects.AddRange(catalogGroupCache.AssetStructsMapById[assetStructId.Value].AssetStructMetas.Values);
                        }
                    }

                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                else
                {
                    response.Objects = _assetStructMetaRepository.GetAssetStructMetaList(groupId, metaId.Value);
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed GetAssetStructMetaList with groupId: {groupId}, assetStructId: {assetStructId} and metaId: {metaId}", ex);
            }

            return response;
        }

        public bool HandleHeritage(int groupId, long assetStructId, long metaId, long userId)
        {
            bool result = true;
            CatalogGroupCache catalogGroupCache = null;
            AssetStruct currentAssetStruct = null;
            AssetStruct parentAssetStruct = null;

            result = ValidateHeritage(groupId, assetStructId, metaId, out catalogGroupCache, out currentAssetStruct, out parentAssetStruct);
            if (!result)
            {
                return result;
            }

            Topic inhertiedMeta = null;
            Topic parentConnectingTopic = null;
            Topic childConnectingTopic = null;

            result = GetHeritageTopics(groupId, catalogGroupCache, assetStructId, metaId, currentAssetStruct, out inhertiedMeta, out parentConnectingTopic, out childConnectingTopic);
            if (!result)
            {
                return result;
            }

            // Getting all parent assets that contains value for connected meta
            string filter = string.Format("(and asset_type='{0}' {1}+'')", parentAssetStruct.Id, parentConnectingTopic.SystemName);

            HashSet<long> parentAssetsIds = GetAssetsIdsWithPaging(groupId, filter);

            foreach (long assetId in parentAssetsIds)
            {
                GenericResponse<Asset> mediaAsset = AssetManager.Instance.GetAsset(groupId, assetId, eAssetTypes.MEDIA, true);

                if (mediaAsset.Status.Code != (int)eResponseStatus.OK || mediaAsset.Object == null)
                {
                    log.ErrorFormat("Error while getting Asset {0}. groupId {1}", assetId, groupId);
                    continue;
                }

                string connectingMetaValue = GetConnectingMetaValue(parentConnectingTopic, mediaAsset.Object);

                if (string.IsNullOrEmpty(connectingMetaValue))
                    continue;

                // Getting all child assets
                filter = string.Format("(and asset_type='{0}' {1}='{2}' inheritance_policy='0')", currentAssetStruct.Id, childConnectingTopic.SystemName, connectingMetaValue);
                HashSet<long> childAssetsIds = GetAssetsIdsWithPaging(groupId, filter);

                if (childAssetsIds == null || childAssetsIds.Count == 0)
                {
                    continue;
                }

                UpdateInheritedChildAssetWithPaging(groupId, userId, inhertiedMeta, mediaAsset.Object, childAssetsIds.Select(x => new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, x)).ToList());
            }

            return result;
        }

        private static void UpdateInheritedChildAssetWithPaging(int groupId, long userId, Topic inhertiedMeta, Asset mediaAsset, List<KeyValuePair<eAssetTypes, long>> assetList)
        {
            Metas inheratedMetaObj = null;
            Tags inheratedTagsObj = null;
            int pageSize = 1000;
            int pageIndex = 0;

            if (inhertiedMeta.Type == MetaType.Tag)
            {
                inheratedTagsObj = mediaAsset.Tags.FirstOrDefault(x => x.m_oTagMeta.m_sName == inhertiedMeta.SystemName);
            }
            else
            {
                inheratedMetaObj = mediaAsset.Metas.FirstOrDefault(x => x.m_oTagMeta.m_sName == inhertiedMeta.SystemName);
            }

            while (true)
            {
                List<Asset> childAssets = AssetManager.GetAssets(groupId, assetList.Skip(pageSize * pageIndex).Take(pageSize).ToList(), true);
                if (childAssets == null || childAssets.Count == 0)
                {
                    break;
                }

                // Update Inherited Child Asset
                UpdateInheritedChildAsset(groupId, userId, inhertiedMeta, inheratedMetaObj, inheratedTagsObj, childAssets);

                pageIndex++;
            }
        }

        private static HashSet<long> GetAssetsIdsWithPaging(int groupId, string filter)
        {
            int pageSize = 1000;
            int pageIndex = 0;
            HashSet<long> parentAssetsIds = new HashSet<long>();

            while (true)
            {
                UnifiedSearchResult[] parentAssets = Utils.SearchAssets(groupId, filter, pageIndex, pageSize, false, true);
                if (parentAssets == null || parentAssets.Length == 0)
                {
                    break;
                }

                foreach (UnifiedSearchResult asset in parentAssets)
                {
                    long assetId = long.Parse(asset.AssetId);
                    if (!parentAssetsIds.Contains(assetId))
                    {
                        parentAssetsIds.Add(assetId);
                    }
                    else
                    {
                        break;
                    }
                }

                if (parentAssets.Length < pageSize)
                    break;

                pageIndex++;
            }

            return parentAssetsIds;
        }

        private static Tuple<Dictionary<long, List<int>>, bool> GetLinearMediaRegionsFromDB(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<long, List<int>> result = new Dictionary<long, List<int>>();

            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue && groupId.Value > 0)
                    {
                        var dt = ApiDAL.GetMediaRegions(groupId.Value);
                        if (dt != null && dt.Rows != null)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                long linearChannelId = ODBCWrapper.Utils.GetLongSafeVal(row, "MEDIA_ID");
                                int regionId = ODBCWrapper.Utils.GetIntSafeVal(row, "REGION_ID");

                                if (!result.ContainsKey(linearChannelId))
                                {
                                    result.Add(linearChannelId, new List<int>());
                                }

                                result[linearChannelId].Add(regionId);
                            }
                        }
                    }
                }

                res = result != null;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetLinearMediaRegionsFromDB failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<long, List<int>>, bool>(result, res);
        }

        public bool IsRegionalizationEnabled(int groupId)
        {
            if (!_groupSettingsManager.IsOpc(groupId))
            {
                return _groupManager.GetGroup(groupId)?.isRegionalizationEnabled ?? false;
            }

            return TryGetCatalogGroupCacheFromCache(groupId, out var catalogGroupCache) && catalogGroupCache.IsRegionalizationEnabled;
        }

        internal static List<int> GetRegions(int groupId)
        {
            List<int> result = null;

            try
            {
                string key = LayeredCacheKeys.GetRegionsKey(groupId);
                if (!LayeredCache.Instance.Get<List<int>>(key,
                                                          ref result,
                                                          GetRegionsFromDB,
                                                          new Dictionary<string, object>() { { "groupId", groupId } },
                                                          groupId,
                                                          LayeredCacheKeys.GetRegionsInvalidationKey(groupId)))
                {
                    log.ErrorFormat("Failed getting GetRegions from LayeredCache, groupId: {0}, key: {1}", groupId, key);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetRegions for groupId: {0}", groupId), ex);
            }

            return result;
        }

        private static Tuple<List<int>, bool> GetRegionsFromDB(Dictionary<string, object> funcParams)
        {
            bool res = false;
            List<int> result = null;

            try
            {
                if (funcParams != null && funcParams.Count == 1 && funcParams.ContainsKey("groupId"))
                {
                    int? groupId;
                    groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue)
                    {
                        var ds = ApiDAL.Get_Regions(groupId.Value, null);

                        if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                            {
                                res = true;

                                if (result == null)
                                {
                                    result = new List<int>();
                                }

                                foreach (DataRow dr in ds.Tables[0].Rows)
                                {
                                    int id = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                                    result.Add(id);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetRegionsFromDB failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<int>, bool>(result, res);
        }

        public List<AssetStruct> GetLinearMediaTypes(int groupId)
        {
            if (!TryGetCatalogGroupCacheFromCache(groupId, out var catalogGroupCache))
            {
                log.Error($"failed to get catalogGroupCache for groupId: {groupId} when calling GetLinearMediaTypeIds");

                return new List<AssetStruct>();
            }

            return GetLinearMediaTypes(catalogGroupCache);
        }

        private static List<AssetStruct> GetLinearMediaTypes(CatalogGroupCache catalogGroupCache)
        {
            var result = new List<AssetStruct>();
            if (catalogGroupCache.AssetStructsMapBySystemName.TryGetValue(LINEAR_ASSET_STRUCT_SYSTEM_NAME, out var linearMediaType))
            {
                result.Add(linearMediaType);
            }

            var otherLinearMediaTypeIds = catalogGroupCache.AssetStructsMapById.Values
                .Where(x => x.IsLinearAssetStruct && x.Id != linearMediaType?.Id)
                .Select(x => x.Id)
                .Distinct();

            result.AddRange(otherLinearMediaTypeIds.Select(x => catalogGroupCache.AssetStructsMapById[x]));

            return result;
        }

        internal void SetHistoryValues(int groupId, UserMediaMark userMediaMark)
        {
            if (DoesGroupUsesTemplates(groupId) && TryGetCatalogGroupCacheFromCache(groupId, out CatalogGroupCache catalogGroupCache))
            {
                EpgAsset asset = null;

                if (catalogGroupCache.AssetStructsMapById.ContainsKey(userMediaMark.AssetTypeId))
                {
                    var locationAssetStractMeta = catalogGroupCache.AssetStructsMapById[userMediaMark.AssetTypeId].AssetStructMetas.Values.FirstOrDefault(x => x.IsLocationTag.HasValue && x.IsLocationTag.Value);
                    if (locationAssetStractMeta != null)
                    {
                        if (catalogGroupCache.TopicsMapById.ContainsKey(locationAssetStractMeta.MetaId))
                        {
                            var locationTagName = catalogGroupCache.TopicsMapById[locationAssetStractMeta.MetaId].SystemName;

                            if (!string.IsNullOrEmpty(locationTagName))
                            {
                                var assetResponse = AssetManager.Instance.GetAsset(groupId, userMediaMark.AssetID, userMediaMark.AssetType, true);
                                if (assetResponse.HasObject() && assetResponse.Object.Metas != null)
                                {
                                    if (userMediaMark.AssetType == eAssetTypes.EPG)
                                    {
                                        asset = assetResponse.Object as EpgAsset;
                                    }

                                    var locationTagMeta = assetResponse.Object.Metas.FirstOrDefault(x => x.m_oTagMeta.m_sName == locationTagName);
                                    if (locationTagMeta != null && int.TryParse(locationTagMeta.m_sValue, out int locationTagValue))
                                    {
                                        userMediaMark.LocationTagValue = locationTagValue;
                                    }
                                }
                            }
                        }
                    }
                }

                if (userMediaMark.AssetType == eAssetTypes.EPG)
                {
                    if (asset == null)
                    {
                        var assetResponse = AssetManager.Instance.GetAsset(groupId, userMediaMark.AssetID, userMediaMark.AssetType, true);
                        if (assetResponse.HasObject() && assetResponse.Object.Metas != null)
                        {
                            asset = assetResponse.Object as EpgAsset;
                        }
                    }

                    if (asset != null)
                    {
                        if (asset.SearchEndDate == DateTime.MinValue)
                        {
                            asset.SearchEndDate = GetProgramSearchEndDate(groupId, asset.EpgChannelId.Value.ToString(), asset.EndDate.Value);
                        }

                        userMediaMark.ExpiredAt = DateUtils.ToUtcUnixTimestampSeconds(asset.SearchEndDate);
                    }
                }


            }
        }

        #endregion

        public Dictionary<int, Media> GetGroupMedia(int groupId, long mediaId)
        {
            var mediaTranslations = new Dictionary<int, Media>();

            //temporary media dictionary
            Dictionary<int, Media> medias = new Dictionary<int, Media>();

            try
            {
                if (Instance.DoesGroupUsesTemplates(groupId))
                {
                    var dictionary = AssetManager.GetMediaForElasticSearchIndex(groupId, mediaId);
                    return dictionary.ContainsKey((int)mediaId) ? dictionary[(int)mediaId] : null;
                }

                GroupManager groupManager = new GroupManager();
                Group group = groupManager.GetGroup(groupId);

                if (group == null)
                {
                    log.Error("Error - Could not load group from cache in GetGroupMedias");
                    return mediaTranslations;
                }

                LanguageObj defaultLangauge = group.GetGroupDefaultLanguage();
                if (defaultLangauge == null)
                {
                    log.Error("Error - Could not get group default language from cache in GetGroupMedias");
                    return mediaTranslations;
                }

                StoredProcedure storedProcedure = new StoredProcedure("Get_GroupMedias_ml");
                storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
                storedProcedure.AddParameter("@GroupID", groupId);
                storedProcedure.AddParameter("@MediaID", mediaId);

                DataSet dataSet = storedProcedure.ExecuteDataSet();

                Dictionary<int, Dictionary<int, Media>> refDictionary = new Dictionary<int, Dictionary<int, Media>>();
                Utils.BuildMediaFromDataSet(ref refDictionary, ref medias, group, dataSet, (int)mediaId);
                mediaTranslations = refDictionary.ContainsKey((int)mediaId) ? refDictionary[(int)mediaId] : null;

                // get media update dates
                DataTable updateDates = CatalogDAL.Get_MediaUpdateDate(new List<int>() { (int)mediaId });
            }
            catch (Exception ex)
            {
                log.Error("Media Exception", ex);
            }

            return mediaTranslations;
        }

        public void GetLinearChannelValues(List<EpgCB> lEpg, int groupID, Action<EpgCB> action)
        {
            try
            {
                List<string> epgChannelIds = lEpg.Distinct().Select(item => item.ChannelID.ToString()).ToList<string>();
                Dictionary<string, LinearChannelSettings> linearChannelSettings = CatalogCache.Instance().GetLinearChannelSettings(groupID, epgChannelIds);

                Parallel.ForEach(lEpg.Cast<EpgCB>(), currentElement =>
                {
                    currentElement.SearchEndDate = GetProgramSearchEndDate(groupID, currentElement.ChannelID.ToString(), currentElement.EndDate, linearChannelSettings);
                    action?.Invoke(currentElement);
                });
            }
            catch (Exception ex)
            {
                log.Error($"Error - Update EPGs threw an exception. (in GetLinearChannelValues). Exception={ex.Message};Stack={ex.StackTrace}", ex);
                throw ex;
            }
        }

        public DateTime GetProgramSearchEndDate(int groupId, string channelId, DateTime endDate,
            Dictionary<string, LinearChannelSettings> linearChannelSettings = null)
        {
            DateTime searchEndDate = DateTime.MinValue;
            try
            {
                int days = ApplicationConfiguration.Current.CatalogLogicConfiguration.CurrentRequestDaysOffset.Value;

                if (days == 0)
                {
                    days = CURRENT_REQUEST_DAYS_OFFSET_DEFAULT;
                }

                if (linearChannelSettings == null)
                {
                    linearChannelSettings = CatalogCache.Instance().GetLinearChannelSettings(groupId, new List<string>() { channelId });
                }

                if (!linearChannelSettings.ContainsKey(channelId))
                {
                    searchEndDate = endDate.AddDays(days);
                }
                else if (linearChannelSettings[channelId].EnableCatchUp)
                {
                    searchEndDate =
                        endDate.AddMinutes(linearChannelSettings[channelId].CatchUpBuffer);
                }
                else
                {
                    searchEndDate = endDate;
                }
            }
            catch (Exception ex)
            {
                log.Error("error while Getting program search end date", ex);
            }

            return searchEndDate;
        }

        #region Label

        public GenericResponse<LabelValue> AddLabel(int groupId, LabelValue requestLabel, long userId)
        {
            var result = new GenericResponse<LabelValue>();

            try
            {
                result = _labelRepository.Add(groupId, requestLabel, userId);
            }
            catch (Exception e)
            {
                log.Error($"Failed {nameof(AddLabel)}, {nameof(groupId)}:{groupId}, {nameof(requestLabel)}:{requestLabel}, {nameof(userId)}:{userId}.", e, null);
            }

            return result;
        }

        public GenericResponse<LabelValue> UpdateLabel(int groupId, LabelValue requestLabel, long userId)
        {
            var result = new GenericResponse<LabelValue>();

            try
            {
                result = _labelRepository.Update(groupId, requestLabel, userId);
            }
            catch (Exception e)
            {
                log.Error($"Failed {nameof(UpdateLabel)}, {nameof(groupId)}:{groupId}, {nameof(requestLabel)}:{requestLabel}, {nameof(userId)}:{userId}.", e, null);
            }

            return result;
        }

        public Status DeleteLabel(int groupId, long labelId, long userId)
        {
            var result = Status.Error;

            try
            {
                result = _labelRepository.Delete(groupId, labelId, userId);
            }
            catch (Exception e)
            {
                log.Error($"Failed {nameof(DeleteLabel)}, {nameof(groupId)}:{groupId}, {nameof(labelId)}:{labelId}, {nameof(userId)}:{userId}.", e, null);
            }

            return result;
        }

        public GenericListResponse<LabelValue> SearchLabels(int groupId, IReadOnlyCollection<long> idIn, string labelEqual, string labelStartWith, EntityAttribute entityAttribute, int pageIndex, int pageSize)
        {
            var result = new GenericListResponse<LabelValue>();

            try
            {
                idIn = idIn ?? new List<long>();
                var predicate = GetFilterLabelsPredicate(idIn, labelEqual, labelStartWith, entityAttribute);
                result = FilterLabels(groupId, predicate, pageIndex, pageSize);
            }
            catch (Exception e)
            {
                log.Error($"Failed {nameof(SearchLabels)}, {nameof(groupId)}:{groupId}, {nameof(idIn)}:[{string.Join(",", idIn)}], {nameof(labelEqual)}:{labelEqual}, {nameof(labelStartWith)}:{labelStartWith}, {nameof(entityAttribute)}:{entityAttribute}, {nameof(pageIndex)}:{pageIndex}, {nameof(pageSize)}:{pageSize}.", e, null);
            }

            return result;
        }

        private static Func<LabelValue, bool> GetFilterLabelsPredicate(IReadOnlyCollection<long> idIn, string labelEqual, string labelStartWith, EntityAttribute entityAttribute)
        {
            Func<LabelValue, bool> predicate;
            if (idIn.Any())
            {
                predicate = x => idIn.Contains(x.Id) && x.EntityAttribute == entityAttribute;
            }
            else if (!string.IsNullOrEmpty(labelEqual))
            {
                predicate = x => labelEqual.Equals(x.Value, StringComparison.InvariantCultureIgnoreCase) && x.EntityAttribute == entityAttribute;
            }
            else if (!string.IsNullOrEmpty(labelStartWith))
            {
                predicate = x => x.Value.StartsWith(labelStartWith, StringComparison.InvariantCultureIgnoreCase) && x.EntityAttribute == entityAttribute;
            }
            else
            {
                predicate = x => x.EntityAttribute == entityAttribute;
            }

            return predicate;
        }

        private GenericListResponse<LabelValue> FilterLabels(int groupId, Func<LabelValue, bool> predicate, int pageIndex, int pageSize)
        {
            var result = new GenericListResponse<LabelValue>();

            var listResponse = _labelRepository.List(groupId);
            if (listResponse.IsOkStatusCode())
            {
                var filteredLabels = listResponse.Objects
                    .Where(predicate)
                    .ToArray();
                var pagedResult = filteredLabels
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToList();

                result = new GenericListResponse<LabelValue>(Status.Ok, pagedResult)
                {
                    TotalItems = filteredLabels.Length
                };
            }
            else
            {
                result.SetStatus(listResponse.Status);
            }

            return result;
        }

        #endregion
    }
}
