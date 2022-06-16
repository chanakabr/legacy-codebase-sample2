using ApiObjects;
using ApiObjects.Notification;
using CachingProvider;
using CachingProvider.LayeredCache;
using Phx.Lib.Appconfig;
using DAL;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Core.Notification
{
    public enum eNotificationCacheTypes
    {
        PartnerNotificationConfiguration,
        Announcements,
        OTTAssetType,
        EpisodeAssociationTagName,
        EpisodeMediaTypeId,
        Reminders,
        MessageTemplate,
        TopicInterests,
        EpisodeNumberMeta,
        SeasonNumberMeta
    }

    public interface INotificationCache
    {
        NotificationPartnerSettingsResponse GetPartnerNotificationSettings(int groupId);
        List<ApiObjects.Meta> GetPartnerTopicInterests(int groupId);
        bool TryGetAnnouncements(int groupId, ref List<DbAnnouncement> announcements);
    }

    public class NotificationCache : INotificationCache
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly uint DEFAULT_TIME_IN_CACHE_SECONDS = 216000; // 1 hours
        private static readonly double SHORT_IN_CACHE_MINUTES = 3d; // 3 minutes
        private static readonly string DEFAULT_CACHE_NAME = "NotificationCache";
        protected const string CACHE_KEY = "NOTIFICATION";

        private static object locker = new object();
        private ICachingService CacheService = null;
        private readonly uint dCacheTT;
        private string sKeyCache = string.Empty;

        private string GetKey(eNotificationCacheTypes type, int groupId, int mediaTypeId = 0)
        {
            switch (type)
            {
                case eNotificationCacheTypes.PartnerNotificationConfiguration:
                    return string.Format("PartnerNotificationSettings_{0}", groupId);
                case eNotificationCacheTypes.Announcements:
                    return string.Format("GroupAnnouncements_{0}", groupId);
                case eNotificationCacheTypes.OTTAssetType:
                    return string.Format("GroupOTTAssetTypes_{0}_{1}", groupId, mediaTypeId);
                case eNotificationCacheTypes.EpisodeAssociationTagName:
                    return string.Format("EpisodeAssociationTagName_{0}", groupId);
                case eNotificationCacheTypes.EpisodeMediaTypeId:
                    return string.Format("EpisodeMediaTypeId_{0}", groupId);
                case eNotificationCacheTypes.Reminders:
                    return string.Format("Reminders_{0}", groupId);
                case eNotificationCacheTypes.MessageTemplate:
                    return string.Format("MessageTemplates_{0}", groupId);
                case eNotificationCacheTypes.TopicInterests:
                    return string.Format("TopicInterests_{0}", groupId);
                case eNotificationCacheTypes.EpisodeNumberMeta:
                case eNotificationCacheTypes.SeasonNumberMeta:
                    return type.ToString() + "_" + groupId;
                default:
                    break;
            }
            return null;
        }

        private static NotificationCache instance = null;

        private string GetCacheName()
        {
            string res = ApplicationConfiguration.Current.NotificationCacheConfiguration.Name.Value;
            if (res.Length > 0)
                return res;
            return DEFAULT_CACHE_NAME;
        }

        private uint GetDefaultCacheTimeInSeconds()
        {
            uint result = (uint)ApplicationConfiguration.Current.NotificationCacheConfiguration.TTLSeconds.Value;

            if (result <= 0)
            {
                result = DEFAULT_TIME_IN_CACHE_SECONDS;
            }

            return result;
        }

        private void InitializeCachingService(string cacheName, uint expirationInSeconds)
        {
            this.CacheService = SingleInMemoryCache.GetInstance(InMemoryCacheType.General, expirationInSeconds);
        }

        public NotificationCache()
        {
            dCacheTT = GetDefaultCacheTimeInSeconds();
            InitializeCachingService(GetCacheName(), dCacheTT);
            sKeyCache = CACHE_KEY; // the key for cache in the inner memory start with CACHE_KEY prefix
        }

        public static NotificationCache Instance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new NotificationCache();
                    }
                }
            }
            return instance;
        }

        public object Get(string sKey)
        {
            sKey = string.Format("{0}{1}", sKeyCache, sKey);
            BaseModuleCache bModule = CacheService.Get(sKey);
            if (bModule != null)
                return bModule.result;

            return null;
        }

        public T Get<T>(string sKey) where T : class
        {
            sKey = string.Format("{0}{1}", sKeyCache, sKey);
            return CacheService.Get<T>(sKey);
        }

        private void Remove(string sKey)
        {
            sKey = string.Format("{0}{1}", sKeyCache, sKey);
            CacheService.Remove(sKey);
        }

        public bool Set(string sKey, object oValue)
        {
            return Set(sKey, oValue, dCacheTT);
        }

        public bool Set(string sKey, object oValue, double dCacheTime)
        {
            sKey = string.Format("{0}{1}", sKeyCache, sKey);
            BaseModuleCache bModule = new BaseModuleCache(oValue);
            return CacheService.Set(sKey, bModule, dCacheTime);
        }

        //public List<DbAnnouncement> GetAnnouncements(int groupId)
        //{
        //    List<DbAnnouncement> announcements = null;
        //    try
        //    {
        //        string sKey = GetKey(eNotificationCacheTypes.Announcements, groupId);
        //        announcements = Get<List<DbAnnouncement>>(sKey);

        //        if (announcements == null || announcements.Count == 0)
        //        {
        //            // get from DB
        //            announcements = NotificationDal.GetAnnouncements(groupId);

        //            if (announcements != null && announcements.Count > 0)
        //                Set(sKey, announcements, SHORT_IN_CACHE_MINUTES);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.ErrorFormat("Error while getting cache partner announcements. GID {0}, ex: {1}", groupId, ex);
        //    }
        //    return announcements;
        //}

        public List<MessageTemplate> GetMessageTemplates(int groupId)
        {
            List<MessageTemplate> messageTemplates = null;
            try
            {
                string sKey = GetKey(eNotificationCacheTypes.MessageTemplate, groupId);

                // search reminders in cache
                messageTemplates = Get<List<MessageTemplate>>(sKey);
                if (messageTemplates == null || messageTemplates.Count == 0)
                {
                    // get reminders DB
                    messageTemplates = NotificationDal.GetMessageTemplate(groupId, MessageTemplateType.None);
                    if (messageTemplates != null && messageTemplates.Count > 0)
                    {
                        // update cache
                        Set(sKey, messageTemplates, SHORT_IN_CACHE_MINUTES);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting cache message templates. GID {0}, ex: {1}", groupId, ex);
            }
            return messageTemplates;
        }

        public List<ApiObjects.Meta> GetPartnerTopicInterests(int groupId)
        {
            List<ApiObjects.Meta> topicInterests = null;
            try
            {
                string sKey = GetKey(eNotificationCacheTypes.TopicInterests, groupId);

                // search reminders in cache
                topicInterests = Get<List<ApiObjects.Meta>>(sKey);
                if (topicInterests == null || topicInterests.Count == 0)
                {
                    // get reminders DB
                    topicInterests = Tvinci.Core.DAL.CatalogDAL.GetTopicInterests(groupId);

                    if (topicInterests != null && topicInterests.Count > 0)
                    {
                        // update cache
                        Set(sKey, topicInterests, SHORT_IN_CACHE_MINUTES);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting cache topic interests. GID {0}, ex: {1}", groupId, ex);
            }
            return topicInterests;
        }

        public void RemoveTopicInterestsFromCache(int groupId)
        {
            try
            {
                string sKey = GetKey(eNotificationCacheTypes.TopicInterests, groupId);
                this.Remove(sKey);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while removing cached topic interest. GID {0}, ex: {1}", groupId, ex);
            }
        }

        public NotificationPartnerSettingsResponse GetPartnerNotificationSettings(int groupId)
        {
            NotificationPartnerSettingsResponse response = null;
            try
            {
                var cacheKey = GetKey(eNotificationCacheTypes.PartnerNotificationConfiguration, groupId);
                response = Get<NotificationPartnerSettingsResponse>(cacheKey);

                if (response?.settings == null)
                {
                    response = NotificationSettings.GetPartnerNotificationSettings(groupId);
                    if (response != null) Set(cacheKey, response, SHORT_IN_CACHE_MINUTES);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting cache partner notification settings. GID {0}, ex: {1}", groupId, ex);
            }
            return response;
        }

        public int GetEpisodeMediaTypeId(int groupId)
        {
            string associationTagName = null;
            int mediaTypeId = 0;
            try
            {
                string tagNameKey = GetKey(eNotificationCacheTypes.EpisodeAssociationTagName, groupId);
                string mediaTypeIdKey = GetKey(eNotificationCacheTypes.EpisodeMediaTypeId, groupId);
                var mediaTypeIdstring = Get<string>(mediaTypeIdKey);

                if (!int.TryParse(mediaTypeIdstring, out mediaTypeId) || mediaTypeId == 0)
                {
                    // get from DB
                    NotificationDal.GetEpisodeAssociationTag(groupId, out associationTagName, out mediaTypeId);

                    if (!string.IsNullOrEmpty(associationTagName))
                        Set(tagNameKey, associationTagName, DEFAULT_TIME_IN_CACHE_SECONDS);

                    if (mediaTypeId != 0)
                        Set(mediaTypeIdKey, mediaTypeId.ToString(), DEFAULT_TIME_IN_CACHE_SECONDS);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting media type ID. GID {0}, ex: {1}", groupId, ex);
            }
            return mediaTypeId;
        }

        public string GetEpisodeAssociationTagName(int groupId)
        {
            string associationTagName = null;
            int mediaTypeId = 0;
            try
            {
                string tagNameKey = GetKey(eNotificationCacheTypes.EpisodeAssociationTagName, groupId);
                string mediaTypeIdKey = GetKey(eNotificationCacheTypes.EpisodeMediaTypeId, groupId);

                associationTagName = Get<string>(tagNameKey);

                if (string.IsNullOrEmpty(associationTagName))
                {
                    // get from DB
                    NotificationDal.GetEpisodeAssociationTag(groupId, out associationTagName, out mediaTypeId);

                    if (!string.IsNullOrEmpty(associationTagName))
                        Set(tagNameKey, associationTagName, DEFAULT_TIME_IN_CACHE_SECONDS);

                    if (mediaTypeId != 0)
                        Set(mediaTypeIdKey, mediaTypeId.ToString(), DEFAULT_TIME_IN_CACHE_SECONDS);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting cached episode association tag name. GID {0}, ex: {1}", groupId, ex);
            }
            return associationTagName;
        }
        
        public string GetEpisodeNumberMeta(int groupId)
        {
            string episodeNumberMetaKey = GetKey(eNotificationCacheTypes.EpisodeNumberMeta, groupId);

            var episodeNumberMetaCached = Get<string>(episodeNumberMetaKey);

            if (!string.IsNullOrEmpty(episodeNumberMetaCached))
            {
                return episodeNumberMetaCached;
            }
            
            var (episodeNumberMeta, _) = GetEpisodeAndSeasonNumberMetasAndSetCache(groupId);

            return episodeNumberMeta;
        }

        public string GetSeasonNumberMeta(int groupId)
        {
            var seasonNumberMetaKey = GetKey(eNotificationCacheTypes.SeasonNumberMeta, groupId);

            var seasonNumberMetaCached = Get<string>(seasonNumberMetaKey);

            if (!string.IsNullOrEmpty(seasonNumberMetaCached))
            {
                return seasonNumberMetaCached;
            }
            
            var (_, seasonNumberMeta) = GetEpisodeAndSeasonNumberMetasAndSetCache(groupId);

            return seasonNumberMeta;
        }
        
        private (string episodeNumberMeta, string seasonNumberMeta) GetEpisodeAndSeasonNumberMetasAndSetCache(int groupId)
        {
            var episodeNumberMetaKey = GetKey(eNotificationCacheTypes.EpisodeNumberMeta, groupId);
            var seasonNumberMetaKey = GetKey(eNotificationCacheTypes.SeasonNumberMeta, groupId);

            var (episodeNumberMeta, seasonNumberMeta) = NotificationDal.GetEpisodeAndSeasonNumberMetas(groupId);

            if (!string.IsNullOrEmpty(episodeNumberMeta))
            {
                Set(episodeNumberMetaKey, episodeNumberMeta, DEFAULT_TIME_IN_CACHE_SECONDS);
            }

            if (!string.IsNullOrEmpty(seasonNumberMeta))
            {
                Set(seasonNumberMetaKey, seasonNumberMeta, DEFAULT_TIME_IN_CACHE_SECONDS);
            }

            return (episodeNumberMeta, seasonNumberMeta);
        }

        public void RemovePartnerNotificationSettingsFromCache(int groupId)
        {
            try
            {
                string sKey = GetKey(eNotificationCacheTypes.PartnerNotificationConfiguration, groupId);
                this.Remove(sKey);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while removing cache partner notification settings. GID {0}, ex: {1}", groupId, ex);
            }
        }

        public void RemoveAnnouncementsFromCache(int groupId)
        {
            try
            {
                if (!LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAnnouncementsInvalidationKey(groupId)))
                    log.ErrorFormat("Error while trying to update invalidation key: {0}", LayeredCacheKeys.GetAnnouncementsInvalidationKey(groupId));
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while removing cached announcements. GID {0}, ex: {1}", groupId, ex);
            }
        }

        public void RemoveMessageTemplateFromCache(int groupId)
        {
            try
            {
                string sKey = GetKey(eNotificationCacheTypes.MessageTemplate, groupId);
                this.Remove(sKey);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while removing cached message templates. GID {0}, ex: {1}", groupId, ex);
            }
        }

        public eOTTAssetTypes GetOTTAssetTypeByMediaTypeId(int groupId, int mediaTypeId)
        {
            eOTTAssetTypes assetType = eOTTAssetTypes.None;
            try
            {
                string sKey = GetKey(eNotificationCacheTypes.OTTAssetType, groupId, mediaTypeId);
                var ottAssetType = Get(sKey);

                if (ottAssetType == null)
                {
                    // get from DB
                    assetType = NotificationDal.GetOttAssetTypByMediaType(mediaTypeId);

                    Set(sKey, assetType, SHORT_IN_CACHE_MINUTES);
                }
                else
                {
                    assetType = (eOTTAssetTypes)Enum.Parse(typeof(eOTTAssetTypes), ottAssetType.ToString());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting cache OTTAssetType . GID {0}, ex: {1}", groupId, ex);
            }
            return assetType;
        }

        public bool TryGetAnnouncements(int groupId, ref List<DbAnnouncement> announcements)
        {
            bool res = false;
            try
            {
                string key = LayeredCacheKeys.GetAnnouncementsKey(groupId);

                Dictionary<string, object> funcParams = new Dictionary<string, object>() { { "groupId", groupId } };
                res = LayeredCache.Instance.Get<List<DbAnnouncement>>(key, ref announcements, InitializeAnnouncements, funcParams,
                                                                    groupId, LayeredCacheConfigNames.GET_ANNOUNCEMENTS_LAYERED_CACHE_CONFIG_NAME, new List<string> { LayeredCacheKeys.GetAnnouncementsInvalidationKey(groupId) });
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetAnnouncements, groupId: {0}", groupId), ex);
            }

            return res && announcements != null && announcements.Count > 0;
        }

        private static Tuple<List<DbAnnouncement>, bool> InitializeAnnouncements(Dictionary<string, object> funcParams)
        {
            List<DbAnnouncement> announcements = new List<DbAnnouncement>();
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue)
                        announcements = NotificationDal.Instance.GetAnnouncements(groupId.Value);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("InitializeDomainEntitlements failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            bool res = (announcements != null && announcements.Count > 0);

            return new Tuple<List<DbAnnouncement>, bool>(announcements, res);
        }

        internal static bool TryGetTopicNotifications(int groupId, SubscribeReference subscribeReference, ref List<TopicNotification> topics)
        {
            bool res = false;
            try
            {
                List<long> topicsIds = new List<long>();

                string key = LayeredCacheKeys.GetTopicNotificationsKey(groupId, (int)subscribeReference.Type);

                Dictionary<string, object> funcParams = new Dictionary<string, object>() { { "groupId", groupId }, { "SubscribeReferenceType", (int)subscribeReference.Type } };
                res = LayeredCache.Instance.Get<List<long>>(key, ref topicsIds, InitializeTopics, funcParams, groupId,
                                                            LayeredCacheConfigNames.GET_TOPIC_NOTIFICATIONS_LAYERED_CACHE_CONFIG_NAME,
                                                            new List<string> { LayeredCacheKeys.GetTopicNotificationsInvalidationKey(groupId, (int)subscribeReference.Type) });

                if (res && topicsIds.Count > 0)
                {
                    topics = NotificationDal.GetTopicsNotificationsCB(topicsIds);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed TryGetTopicNotifications, groupId: {0}", groupId, ex);
            }

            return res && topics != null && topics.Count > 0;
        }

        private static Tuple<List<long>, bool> InitializeTopics(Dictionary<string, object> funcParams)
        {
            List<long> topics = null;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("SubscribeReferenceType"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    int? type = funcParams["SubscribeReferenceType"] as int?;
                    if (groupId.HasValue && type.HasValue)
                    {

                        var dt = NotificationDal.GetTopicNotifications(groupId.Value, type.Value);
                        if (dt != null && dt.Rows?.Count > 0)
                        {
                            topics = new List<long>();
                            foreach (DataRow row in dt.Rows)
                            {
                                topics.Add(ODBCWrapper.Utils.GetLongSafeVal(row, "id"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("InitializeDomainEntitlements failed, parameters : {0}", string.Join(";", funcParams.Keys), ex);
            }

            bool res = topics?.Count > 0;

            return new Tuple<List<long>, bool>(topics, res);
        }
    }
}
