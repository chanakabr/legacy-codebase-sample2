using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class CeleryRoutingDistributedTasksConfiguration : BaseConfig<CeleryRoutingDistributedTasksConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.DistributedTasks;
        public override string[] TcmPath => new string[] { TcmObjectKeys.CeleryRoutingConfiguration, TcmKey };


        public BaseValue<string> resize_image = new BaseValue<string>("resize_image", "ImageResizeHandler");
        public BaseValue<string> upload_image = new BaseValue<string>("upload_image", "FileUploadHandler");
        public BaseValue<string> update_index = new BaseValue<string>("update_index", "ElasticSearchHandler");
        public BaseValue<string> build_index = new BaseValue<string>("build_index", "ElasticSearchHandler");
        public BaseValue<string> merge_social_feed = new BaseValue<string>("merge_social_feed", "SocialMergeHandler");
        public BaseValue<string> update_social_feed = new BaseValue<string>("update_social_feed", "SocialFeedHandler");
        public BaseValue<string> index_snapshot_restore = new BaseValue<string>("index_snapshot_restore", "IndexSnapshotRestoreHandler");
        public BaseValue<string> transform_epg_xml_to_xtvd = new BaseValue<string>("transform_epg_xml_to_xtvd", "EPG_XDTVTransform");
        public BaseValue<string> cdr_notification = new BaseValue<string>("cdr_notification", "CdrNotificationHandler");
        public BaseValue<string> setup_task = new BaseValue<string>("setup_task", "SetupTaskHandler");
        public BaseValue<string> update_cache = new BaseValue<string>("update_cache", "UpdateCacheHandler");
        public BaseValue<string> renew_subscription = new BaseValue<string>("renew_subscription", "SubscriptionRenewHandler");
        public BaseValue<string> export = new BaseValue<string>("export", "ExportHandler");
        public BaseValue<string> image_upload = new BaseValue<string>("image_upload", "ImageUploadHandler");
        public BaseValue<string> message_announcements = new BaseValue<string>("message_announcements", "MessageAnnouncementHandler");
        public BaseValue<string> initiate_notification_action = new BaseValue<string>("initiate_notification_action", "InitiateNotificationActionHandler");
        public BaseValue<string> free_item_update = new BaseValue<string>("free_item_update", "FreeAssetUpdateHandler");
        public BaseValue<string> recording_task = new BaseValue<string>("recording_task", "RecordingTaskHandler");
        public BaseValue<string> check_pending_transaction = new BaseValue<string>("check_pending_transaction", "PendingTransactionHandler");
        public BaseValue<string> modified_recording = new BaseValue<string>("modified_recording", "ModifiedRecordingsHandler");
        public BaseValue<string> series_recording_task = new BaseValue<string>("series_recording_task", "SeriesRecordingTaskHandler");
        public BaseValue<string> user_task = new BaseValue<string>("user_task", "UserTaskHandler");
        public BaseValue<string> message_reminders = new BaseValue<string>("message_reminders", "MessageReminderHandler");
        public BaseValue<string> action_rule = new BaseValue<string>("action_rule", "ActionRuleHandler");
        public BaseValue<string> engagements = new BaseValue<string>("engagements", "EngagementHandler");
        public BaseValue<string> message_interests = new BaseValue<string>("message_interests", "MessageInterestHandler");
        public BaseValue<string> unified_renew_subscription = new BaseValue<string>("unified_renew_subscription", "SubscriptionRenewHandler");
        public BaseValue<string> ps_events = new BaseValue<string>("ps_events", "ProfessionalServicesHandler");
        public BaseValue<string> asset_inheritance = new BaseValue<string>("asset_inheritance", "AssetInheritanceHandler");
        public BaseValue<string> geo_rule_update = new BaseValue<string>("geo_rule_update", "GeoRuleUpdateHandler");


    }
}
