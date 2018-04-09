using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class CeleryRoutingDistributedTasksConfiguration : ConfigurationValue
    {
        public StringConfigurationValue resize_image;
        public StringConfigurationValue upload_image;
        public StringConfigurationValue update_index;
        public StringConfigurationValue build_index;
        public StringConfigurationValue merge_social_feed;
        public StringConfigurationValue update_social_feed;
        public StringConfigurationValue index_snapshot_restore;
        public StringConfigurationValue transform_epg_xml_to_xtvd;
        public StringConfigurationValue cdr_notification;
        public StringConfigurationValue setup_task;
        public StringConfigurationValue update_cache;
        public StringConfigurationValue renew_subscription;
        public StringConfigurationValue export;
        public StringConfigurationValue image_upload;
        public StringConfigurationValue message_announcements;
        public StringConfigurationValue initiate_notification_action;
        public StringConfigurationValue free_item_update;
        public StringConfigurationValue recording_task;
        public StringConfigurationValue check_pending_transaction;
        public StringConfigurationValue modified_recording;
        public StringConfigurationValue series_recording_task;
        public StringConfigurationValue user_task;
        public StringConfigurationValue message_reminders;
        public StringConfigurationValue action_rule;
        public StringConfigurationValue engagements;
        public StringConfigurationValue message_interests;
        public StringConfigurationValue unified_renew_subscription;
        public StringConfigurationValue ps_events;

        public CeleryRoutingDistributedTasksConfiguration(string key, ConfigurationValue parent) : base(key, parent)
        {
            resize_image = new ConfigurationManager.StringConfigurationValue("resize_image", this)
            {
                DefaultValue = "ImageResizeHandler"
            };
            upload_image = new ConfigurationManager.StringConfigurationValue("upload_image", this)
            {
                DefaultValue = "FileUploadHandler"
            };
            update_index = new ConfigurationManager.StringConfigurationValue("update_index", this)
            {
                DefaultValue = "ElasticSearchHandler"
            };
            build_index = new ConfigurationManager.StringConfigurationValue("build_index", this)
            {
                DefaultValue = "ElasticSearchHandler"
            };
            merge_social_feed = new ConfigurationManager.StringConfigurationValue("merge_social_feed", this)
            {
                DefaultValue = "SocialMergeHandler"
            };
            update_social_feed = new ConfigurationManager.StringConfigurationValue("update_social_feed", this)
            {
                DefaultValue = "SocialFeedHandler"
            };
            index_snapshot_restore = new ConfigurationManager.StringConfigurationValue("index_snapshot_restore", this)
            {
                DefaultValue = "IndexSnapshotRestoreHandler"
            };
            transform_epg_xml_to_xtvd = new ConfigurationManager.StringConfigurationValue("transform_epg_xml_to_xtvd", this)
            {
                DefaultValue = "EPG_XDTVTransform"
            };
            cdr_notification = new ConfigurationManager.StringConfigurationValue("cdr_notification", this)
            {
                DefaultValue = "CdrNotificationHandler"
            };
            setup_task = new ConfigurationManager.StringConfigurationValue("setup_task", this)
            {
                DefaultValue = "SetupTaskHandler"
            };
            update_cache = new ConfigurationManager.StringConfigurationValue("update_cache", this)
            {
                DefaultValue = "UpdateCacheHandler"
            };
            renew_subscription = new ConfigurationManager.StringConfigurationValue("renew_subscription", this)
            {
                DefaultValue = "SubscriptionRenewHandler"
            };
            export = new ConfigurationManager.StringConfigurationValue("export", this)
            {
                DefaultValue = "ExportHandler"
            };
            image_upload = new ConfigurationManager.StringConfigurationValue("image_upload", this)
            {
                DefaultValue = "ImageUploadHandler"
            };
            message_announcements = new ConfigurationManager.StringConfigurationValue("message_announcements", this)
            {
                DefaultValue = "MessageAnnouncementHandler"
            };
            initiate_notification_action = new ConfigurationManager.StringConfigurationValue("initiate_notification_action", this)
            {
                DefaultValue = "InitiateNotificationActionHandler"
            };
            free_item_update = new ConfigurationManager.StringConfigurationValue("free_item_update", this)
            {
                DefaultValue = "FreeAssetUpdateHandler"
            };
            recording_task = new ConfigurationManager.StringConfigurationValue("recording_task", this)
            {
                DefaultValue = "RecordingTaskHandler"
            };
            check_pending_transaction = new ConfigurationManager.StringConfigurationValue("check_pending_transaction", this)
            {
                DefaultValue = "PendingTransactionHandler"
            };
            modified_recording = new ConfigurationManager.StringConfigurationValue("modified_recording", this)
            {
                DefaultValue = "ModifiedRecordingsHandler"
            };
            series_recording_task = new ConfigurationManager.StringConfigurationValue("series_recording_task", this)
            {
                DefaultValue = "SeriesRecordingTaskHandler"
            };
            user_task = new ConfigurationManager.StringConfigurationValue("user_task", this)
            {
                DefaultValue = "UserTaskHandler"
            };
            message_reminders = new ConfigurationManager.StringConfigurationValue("message_reminders", this)
            {
                DefaultValue = "MessageReminderHandler"
            };
            action_rule = new ConfigurationManager.StringConfigurationValue("action_rule", this)
            {
                DefaultValue = "ActionRuleHandler"
            };
            engagements = new ConfigurationManager.StringConfigurationValue("engagements", this)
            {
                DefaultValue = "EngagementHandler"
            };
            message_interests = new ConfigurationManager.StringConfigurationValue("message_interests", this)
            {
                DefaultValue = "MessageInterestHandler"
            };
            unified_renew_subscription = new ConfigurationManager.StringConfigurationValue("unified_renew_subscription", this)
            {
                DefaultValue = "SubscriptionRenewHandler"
            };
            ps_events = new ConfigurationManager.StringConfigurationValue("ps_events", this)
            {
                DefaultValue = "ProfessionalServicesHandler"
            };
        }
    }
}
