using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects
{
    public class KSQLChannel
    {
        #region Members
        
        [DataMember]
        public int ID
        {
            get;
            set;
        }
        
        [DataMember]
        public int ChannelType
        {
            get
            {
                return 4;
            }
        }
        
        [DataMember]
        public int GroupID
        {
            get;
            set;
        }
        
        [DataMember]
        public int IsActive
        {
            get;
            set;
        }
        
        [DataMember]
        public int Status
        {
            get;
            set;
        }
        
        [DataMember]
        public List<int> AssetTypes
        {
            get;
            set;
        }
        
        [DataMember]
        public string Name
        {
            get;
            set;
        }
        
        [DataMember]
        public string Description
        {
            get;
            set;
        }
        
        [DataMember]
        public ApiObjects.SearchObjects.OrderObj Order
        {
            get;
            set;
        }

        /// <summary>
        /// KSQL filter query - for KSQL channels
        /// </summary>
        [DataMember]
        public string FilterQuery
        {
            get;
            set;
        }

        /// <summary>
        /// Based on the KSQL filter query, and assuming it is valid, this is the tree object that represents the filter
        /// </summary>
        [DataMember]
        public ApiObjects.SearchObjects.BooleanPhraseNode filterTree
        {
            get;
            set;
        }

        #endregion
    }
}
