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
                DefaultValue = "ImageResizeHandler",
                ShouldAllowEmpty = true
            };
            upload_image = new ConfigurationManager.StringConfigurationValue("upload_image", this)
            {
                DefaultValue = "FileUploadHandler",
                ShouldAllowEmpty = true
            };
            update_index = new ConfigurationManager.StringConfigurationValue("update_index", this)
            {
                DefaultValue = "ElasticSearchHandler",
                ShouldAllowEmpty = true
            };
            build_index = new ConfigurationManager.StringConfigurationValue("build_index", this)
            {
                DefaultValue = "ElasticSearchHandler",
                ShouldAllowEmpty = true
            };
            merge_social_feed = new ConfigurationManager.StringConfigurationValue("merge_social_feed", this)
            {
                DefaultValue = "SocialMergeHandler",
                ShouldAllowEmpty = true
            };
            update_social_feed = new ConfigurationManager.StringConfigurationValue("update_social_feed", this)
            {
                DefaultValue = "SocialFeedHandler",
                ShouldAllowEmpty = true
            };
            index_snapshot_restore = new ConfigurationManager.StringConfigurationValue("index_snapshot_restore", this)
            {
                DefaultValue = "IndexSnapshotRestoreHandler",
                ShouldAllowEmpty = true
            };
            transform_epg_xml_to_xtvd = new ConfigurationManager.StringConfigurationValue("transform_epg_xml_to_xtvd", this)
            {
                DefaultValue = "EPG_XDTVTransform",
                ShouldAllowEmpty = true
            };
            cdr_notification = new ConfigurationManager.StringConfigurationValue("cdr_notification", this)
            {
                DefaultValue = "CdrNotificationHandler",
                ShouldAllowEmpty = true
            };
            setup_task = new ConfigurationManager.StringConfigurationValue("setup_task", this)
            {
                DefaultValue = "SetupTaskHandler",
                ShouldAllowEmpty = true
            };
            update_cache = new ConfigurationManager.StringConfigurationValue("update_cache", this)
            {
                DefaultValue = "UpdateCacheHandler",
                ShouldAllowEmpty = true
            };
            renew_subscription = new ConfigurationManager.StringConfigurationValue("renew_subscription", this)
            {
                DefaultValue = "SubscriptionRenewHandler",
                ShouldAllowEmpty = true
            };
            export = new ConfigurationManager.StringConfigurationValue("export", this)
            {
                DefaultValue = "ExportHandler",
                ShouldAllowEmpty = true
            };
            image_upload = new ConfigurationManager.StringConfigurationValue("image_upload", this)
            {
                DefaultValue = "ImageUploadHandler",
                ShouldAllowEmpty = true
            };
            message_announcements = new ConfigurationManager.StringConfigurationValue("message_announcements", this)
            {
                DefaultValue = "MessageAnnouncementHandler",
                ShouldAllowEmpty = true
            };
            initiate_notification_action = new ConfigurationManager.StringConfigurationValue("initiate_notification_action", this)
            {
                DefaultValue = "InitiateNotificationActionHandler",
                ShouldAllowEmpty = true
            };
            free_item_update = new ConfigurationManager.StringConfigurationValue("free_item_update", this)
            {
                DefaultValue = "FreeAssetUpdateHandler",
                ShouldAllowEmpty = true
            };
            recording_task = new ConfigurationManager.StringConfigurationValue("recording_task", this)
            {
                DefaultValue = "RecordingTaskHandler",
                ShouldAllowEmpty = true
            };
            check_pending_transaction = new ConfigurationManager.StringConfigurationValue("check_pending_transaction", this)
            {
                DefaultValue = "PendingTransactionHandler",
                ShouldAllowEmpty = true
            };
            modified_recording = new ConfigurationManager.StringConfigurationValue("modified_recording", this)
            {
                DefaultValue = "ModifiedRecordingsHandler",
                ShouldAllowEmpty = true
            };
            series_recording_task = new ConfigurationManager.StringConfigurationValue("series_recording_task", this)
            {
                DefaultValue = "SeriesRecordingTaskHandler",
                ShouldAllowEmpty = true
            };
            user_task = new ConfigurationManager.StringConfigurationValue("user_task", this)
            {
                DefaultValue = "UserTaskHandler",
                ShouldAllowEmpty = true
            };
            message_reminders = new ConfigurationManager.StringConfigurationValue("message_reminders", this)
            {
                DefaultValue = "MessageReminderHandler",
                ShouldAllowEmpty = true
            };
            action_rule = new ConfigurationManager.StringConfigurationValue("action_rule", this)
            {
                DefaultValue = "ActionRuleHandler",
                ShouldAllowEmpty = true
            };
            engagements = new ConfigurationManager.StringConfigurationValue("engagements", this)
            {
                DefaultValue = "EngagementHandler",
                ShouldAllowEmpty = true
            };
            message_interests = new ConfigurationManager.StringConfigurationValue("message_interests", this)
            {
                DefaultValue = "MessageInterestHandler",
                ShouldAllowEmpty = true
            };
            unified_renew_subscription = new ConfigurationManager.StringConfigurationValue("unified_renew_subscription", this)
            {
                DefaultValue = "SubscriptionRenewHandler",
                ShouldAllowEmpty = true
            };
            ps_events = new ConfigurationManager.StringConfigurationValue("ps_events", this)
            {
                DefaultValue = "ProfessionalServicesHandler",
                ShouldAllowEmpty = true
            };
        }
    }
}
