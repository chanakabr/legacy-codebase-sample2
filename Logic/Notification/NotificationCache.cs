using ApiObjects;
using ApiObjects.Notification;
using CachingProvider;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
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
        MessageTemplate
    }

    public class NotificationCache
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly double DEFAULT_TIME_IN_CACHE_MINUTES = 60d; // 1 hours
        private static readonly double SHORT_IN_CACHE_MINUTES = 3d; // 3 minutes
        private static readonly string DEFAULT_CACHE_NAME = "NotificationCache";
        private static readonly string TCM_CACHE_CONFIG_NAME = "CACHE_NAME";
        private static readonly string TCM_CACHE_TIME_IN_MINUTES = "CACHE_TIME_IN_MINUTES";
        protected const string CACHE_KEY = "NOTIFICATION";

        private static object locker = new object();
        private ICachingService CacheService = null;
        private readonly double dCacheTT;
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
                default:
                    break;
            }
            return null;
        }

        private static NotificationCache instance = null;

        private string GetCacheName()
        {
            string res = TVinciShared.WS_Utils.GetTcmConfigValue(TCM_CACHE_CONFIG_NAME);
            if (res.Length > 0)
                return res;
            return DEFAULT_CACHE_NAME;
        }

        private double GetDefaultCacheTimeInMinutes()
        {
            double res = 0d;
            string timeStr = TVinciShared.WS_Utils.GetTcmConfigValue(TCM_CACHE_TIME_IN_MINUTES);
            if (timeStr.Length > 0 && Double.TryParse(timeStr, out res) && res > 0)
                return res;
            return DEFAULT_TIME_IN_CACHE_MINUTES;
        }

        private void InitializeCachingService(string cacheName, double cachingTimeMinutes)
        {
            this.CacheService = new SingleInMemoryCache(cacheName, cachingTimeMinutes);
        }

        private NotificationCache()
        {
            dCacheTT = GetDefaultCacheTimeInMinutes();
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

        public List<DbAnnouncement> GetAnnouncements(int groupId)
        {
            List<DbAnnouncement> announcements = null;
            try
            {
                string sKey = GetKey(eNotificationCacheTypes.Announcements, groupId);
                announcements = Get<List<DbAnnouncement>>(sKey);

                if (announcements == null || announcements.Count == 0)
                {
                    // get from DB
                    announcements = NotificationDal.GetAnnouncements(groupId);

                    if (announcements != null && announcements.Count > 0)
                        Set(sKey, announcements, SHORT_IN_CACHE_MINUTES);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting cache partner announcements. GID {0}, ex: {1}", groupId, ex);
            }
            return announcements;
        }

        //public List<DbReminder> GetReminders(int groupId)
        //{
        //    List<DbReminder> reminders = null;
        //    try
        //    {
        //        string sKey = GetKey(eNotificationCacheTypes.Reminders, groupId);

        //        // search reminders in cache
        //        reminders = Get<List<DbReminder>>(sKey);
        //        if (reminders == null || reminders.Count == 0)
        //        {
        //            // get reminders DB
        //            reminders = NotificationDal.GetReminders(groupId);
        //            if (reminders != null && reminders.Count > 0)
        //            {
        //                // update cache
        //                Set(sKey, reminders, SHORT_IN_CACHE_MINUTES);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.ErrorFormat("Error while getting cache partner reminders. GID {0}, ex: {1}", groupId, ex);
        //    }
        //    return reminders;
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
                    messageTemplates = NotificationDal.GetMessageTemplate(groupId, eOTTAssetTypes.None);
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

        public NotificationPartnerSettingsResponse GetPartnerNotificationSettings(int groupId)
        {
            NotificationPartnerSettingsResponse settings = null;
            try
            {
                string sKey = GetKey(eNotificationCacheTypes.PartnerNotificationConfiguration, groupId);
                settings = Get<NotificationPartnerSettingsResponse>(sKey);

                if (settings == null || settings.settings == null)
                {
                    // get from DB
                    settings = NotificationSettings.GetPartnerNotificationSettings(groupId);

                    if (settings != null)
                        Set(sKey, settings, SHORT_IN_CACHE_MINUTES);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting cache partner notification settings. GID {0}, ex: {1}", groupId, ex);
            }
            return settings;
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
                        Set(tagNameKey, associationTagName, DEFAULT_TIME_IN_CACHE_MINUTES);

                    if (mediaTypeId != 0)
                        Set(mediaTypeIdKey, mediaTypeId.ToString(), DEFAULT_TIME_IN_CACHE_MINUTES);
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
                        Set(tagNameKey, associationTagName, DEFAULT_TIME_IN_CACHE_MINUTES);

                    if (mediaTypeId != 0)
                        Set(mediaTypeIdKey, mediaTypeId.ToString(), DEFAULT_TIME_IN_CACHE_MINUTES);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting cached episode association tag name. GID {0}, ex: {1}", groupId, ex);
            }
            return associationTagName;
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
                string sKey = GetKey(eNotificationCacheTypes.Announcements, groupId);
                this.Remove(sKey);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while removing cached announcements. GID {0}, ex: {1}", groupId, ex);
            }
        }

        //public void RemoveRemindersFromCache(int groupId)
        //{
        //    try
        //    {
        //        string sKey = GetKey(eNotificationCacheTypes.Reminders, groupId);
        //        this.Remove(sKey);
        //    }
        //    catch (Exception ex)
        //    {
        //        log.ErrorFormat("Error while removing cached reminders. GID {0}, ex: {1}", groupId, ex);
        //    }
        //}

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
    }
}
