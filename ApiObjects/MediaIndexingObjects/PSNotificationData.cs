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

        /// <summary>
        /// Data to serialize
        /// </summary>
        private Dictionary<string, object> m_dicData;

        /// <summary>
        /// The action that is being notified
        /// </summary>
        [DataMember]
        public string Action;

        /// <summary>
        /// Json object that will be written to queue
        /// </summary>
        [DataMember]
        public JObject Data;

        #endregion

        #region Ctors

        /// <summary>
        /// Default ctor for simple initialzation
        /// </summary>
        public PSNotificationData()
        {
            this.m_dicData = null;
            this.Data = new JObject();
        }

        /// <summary>
        /// Initialize notification with given data
        /// </summary>
        /// <param name="p_nGroupID"></param>
        /// <param name="p_dicData"></param>
        /// <param name="p_sAction"></param>
        public PSNotificationData(int p_nGroupID, Dictionary<string, object> p_dicData, string p_sAction)
            : this()
        {
            this.GroupId = p_nGroupID;
            this.m_dicData = p_dicData;
            this.Action = p_sAction;

            if (this.m_dicData != null)
            {
                // Run on all data and convert it to Json
                foreach (var kvpData in this.m_dicData)
                {
                    this.Data.Add(kvpData.Key, JToken.FromObject(kvpData.Value));
                }
            }
        } 

        #endregion
    }
}
