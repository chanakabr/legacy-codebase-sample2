using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ConfigurationManager
{
    public class CeleryRoutingConfiguration : BaseConfig<CeleryRoutingConfiguration>
    {
        private static readonly char[] dot = new char[] { '.' };
        public override string TcmKey => TcmObjectKeys.CeleryRoutingConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };


        private static readonly Dictionary<string, string> distributedTaskshandlerDefaultMapping = new Dictionary<string, string>()
        {
            {"resize_image"                , "ImageResizeHandler"},
            {"upload_image"                , "FileUploadHandler"},
            {"update_index"                , "ElasticSearchHandler"},
            {"build_index"                 , "ElasticSearchHandler"},
            {"merge_social_feed"           , "SocialMergeHandler"},
            {"update_social_feed"          , "SocialFeedHandler"},
            {"index_snapshot_restore"      , "IndexSnapshotRestoreHandler"},
            {"transform_epg_xml_to_xtvd"   , "EPG_XDTVTransform"},
            {"cdr_notification"            , "CdrNotificationHandler"},
            {"setup_task"                  , "SetupTaskHandler"},
            {"update_cache"                , "UpdateCacheHandler"},
            {"renew_subscription"          , "SubscriptionRenewHandler"},
            {"export"                      , "ExportHandler"},
            {"image_upload"                , "ImageUploadHandler"},
            {"message_announcements"       , "MessageAnnouncementHandler"},
            {"initiate_notification_action", "InitiateNotificationActionHandler"},
            {"free_item_update"            , "FreeAssetUpdateHandler"},
            {"recording_task"              , "RecordingTaskHandler"},
            {"check_pending_transaction"   , "PendingTransactionHandler"},
            {"modified_recording"          , "ModifiedRecordingsHandler"},
            {"series_recording_task"       , "SeriesRecordingTaskHandler"},
            {"user_task"                   , "UserTaskHandler"},
            {"message_reminders"           , "MessageReminderHandler"},
            {"action_rule"                 , "ActionRuleHandler"},
            {"engagements"                 , "EngagementHandler"},
            {"message_interests"           , "MessageInterestHandler"},
            {"unified_renew_subscription"  , "SubscriptionRenewHandler"},
            {"ps_events"                   , "ProfessionalServicesHandler"},
            {"asset_inheritance"           , "AssetInheritanceHandler"},
            {"geo_rule_update"             , "GeoRuleUpdateHandler"},
            {"bulk_upload_live_asset"     , "LiveAssetBulkUploadHandler" }
        };

        public BaseValue<Dictionary<string, string>> distributedTasks = new BaseValue<Dictionary<string, string>>(TcmObjectKeys.DistributedTasks, distributedTaskshandlerDefaultMapping);

        public string GetHandler(string key)
        {
            string result = string.Empty, location = string.Empty, keyToFind = null;
            string[] splitedHandler = key.Split(dot, System.StringSplitOptions.RemoveEmptyEntries);
            if (splitedHandler.Length > 1)
            {
                location = splitedHandler[0];
                keyToFind = splitedHandler[1];
            }
            else if (splitedHandler.Length > 0)
            {
                keyToFind = splitedHandler[0];
            }

            if (!string.IsNullOrEmpty(keyToFind))
            {
                switch (location)
                {
                    case TcmObjectKeys.DistributedTasks:
                    default:
                        distributedTasks.ActualValue.TryGetValue(keyToFind, out result);
                        break;
                }
            }

            return result;
        }

    }
}
