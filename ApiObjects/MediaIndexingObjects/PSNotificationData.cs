using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects.MediaIndexingObjects
{
    [Serializable]
    public class PSNotificationData : QueueObject
    {
        #region Data Members

        public string id;
        public string task;
        public List<object> args;

        /// <summary>
        /// Data to serialize
        /// </summary>
        private Dictionary<string, object> m_dicData;

        /// <summary>
        /// The action that is being notified
        /// </summary>
        [DataMember]
        private NotifiedAction Action;

        /// <summary>
        /// Json object that holds the data
        /// </summary>
        [DataMember]
        private JObject Data;

        #endregion

        #region Ctors

        /// <summary>
        /// Default ctor for simple initialzation
        /// </summary>
        public PSNotificationData()
        {
            this.m_dicData = null;
            this.Data = new JObject();
            this.args = new List<object>();
        }

        /// <summary>
        /// Initialize notification with given data
        /// </summary>
        /// <param name="p_nGroupID"></param>
        /// <param name="p_dicData"></param>
        /// <param name="p_eAction"></param>
        public PSNotificationData(string task, int p_nGroupID, Dictionary<string, object> p_dicData, NotifiedAction p_eAction)
            : this()
        {
            this.GroupId = p_nGroupID;
            this.m_dicData = p_dicData;
            this.Action = p_eAction;

            JObject jsonArgument = new JObject();

            if (this.m_dicData != null)
            {
                // Run on all data and convert it to Json
                foreach (var kvpData in this.m_dicData)
                {
                    this.Data.Add(kvpData.Key, JToken.FromObject(kvpData.Value));
                }
            }

            // Put all arguments in one json object
            jsonArgument.Add("GroupId", JToken.FromObject(this.GroupId));
            jsonArgument.Add("Action", JToken.FromObject(this.Action));
            jsonArgument.Add("Data", this.Data);
            
            // Id = guid
            this.id = Guid.NewGuid().ToString();

            // task - from tcm
            this.task = task;

            // Like we said, args will hold only one json object with everything in it
            this.args.Add(jsonArgument);
        } 

        #endregion
    }
}
