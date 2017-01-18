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
    public class PSNotificationData : BaseCeleryData
    {
        #region Data Members

        /// <summary>
        /// Data to serialize
        /// </summary>
        private Dictionary<string, object> m_dicData;

        /// <summary>
        /// The action that is being notified
        /// </summary>
        private NotifiedAction Action;

        /// <summary>
        /// Json object that holds the data
        /// </summary>
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

            // Id = guid
            this.id = Guid.NewGuid().ToString();

            // task - from tcm
            this.task = task;

            // constant arguments - gruop id and action
            this.args.Add(this.GroupId);
            this.args.Add((int)this.Action);
            
            // dynamic data - third object in args, will be in a nested json object
            this.args.Add(this.Data);
        } 

        #endregion
    }
}
