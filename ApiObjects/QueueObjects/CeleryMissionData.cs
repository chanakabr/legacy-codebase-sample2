using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class CeleryMissionData : BaseCeleryData
    {
        #region Consts

        public const string TASK = "distributed_tasks.process_mission";
        
        #endregion

        #region Data Members

        /// <summary>
        /// Data to serialize
        /// </summary>
        private Dictionary<string, object> dynamicData;

        /// <summary>
        /// The action that is being notified
        /// </summary>
        private eRemoteTasksMission mission;

        /// <summary>
        /// Json object that holds the data
        /// </summary>
        private JObject data;

        #endregion

        /// <summary>
        /// Initializes a new queue object for celery/remote tasks missions
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="mission"></param>
        public CeleryMissionData(int groupId, eRemoteTasksMission mission, Dictionary<string, object> dynamicData)
            : base(
                // id = guid
                Guid.NewGuid().ToString(),
                // task = const
                TASK)
        {
            // Basic member initialization
            this.GroupId = groupId;
            this.dynamicData = dynamicData;
            this.mission = mission;
            this.data = new JObject();

            JObject jsonArgument = new JObject();

            if (this.dynamicData != null)
            {
                // Run on all data and convert it to Json
                foreach (var kvpData in this.dynamicData)
                {
                    this.data.Add(kvpData.Key, JToken.FromObject(kvpData.Value));
                }
            }

            this.args.Add(this.mission.ToString());
            this.args.Add(jsonArgument);
        }
    }
}
