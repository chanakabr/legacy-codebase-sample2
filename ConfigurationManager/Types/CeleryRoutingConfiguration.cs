using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class CeleryRoutingConfiguration : StringConfigurationValue
    {
        private JObject json;
        
        public CeleryRoutingConfiguration(string key) : base(key)
        {
            if (!string.IsNullOrEmpty(this.Value))
            {
                json = JObject.Parse(this.Value);
            }
        }

        internal override bool Validate()
        {
            bool result = base.Validate();

            if (json != null)
            {
                JToken tempToken = null;

                List<string> paths = new List<string>()
            {
                "distributed_tasks.resize_image",
                "distributed_tasks.upload_image",
                "distributed_tasks.update_index",
                "distributed_tasks.build_index",
                "distributed_tasks.merge_social_feed",
                "distributed_tasks.update_social_feed",
                "distributed_tasks.index_snapshot_restore",
                "distributed_tasks.transform_epg_xml_to_xtvd",
                "distributed_tasks.cdr_notification",
                "distributed_tasks.setup_task",
                "distributed_tasks.update_cache",
                "distributed_tasks.renew_subscription",
                "distributed_tasks.export",
                "distributed_tasks.image_upload",
                "distributed_tasks.message_announcements",
                "distributed_tasks.initiate_notification_action",
                "distributed_tasks.free_item_update",
                "distributed_tasks.recording_task",
                "distributed_tasks.recording_mission",
                "distributed_tasks.check_pending_transaction",
                "distributed_tasks.modified_recording",
                "distributed_tasks.series_recording_task",
                "distributed_tasks.user_task",
                "distributed_tasks.message_reminders",
                "distributed_tasks.action_rule",
                "distributed_tasks.engagements",
                "distributed_tasks.message_interests",
                "distributed_tasks.unified_renew_subscription",
                "distributed_tasks.ps_tasks",
            };

                foreach (var path in paths)
                {
                    tempToken = json.SelectToken(path);

                    if (tempToken == null)
                    {
                        result = false;

                        LogError(string.Format("Missing celery routing for {0}", path));
                    }
                }
            }

            return result;
        }

        public string GetHandler(string path)
        {
            string result = string.Empty;

            if (json != null)
            {
                var tempToken = json.SelectToken(path);

                if (tempToken != null)
                {
                    result = tempToken.Value<string>();
                }
            }

            return result;
        }
    }
}
