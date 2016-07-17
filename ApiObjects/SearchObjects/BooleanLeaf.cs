using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects.SearchObjects
{
    /// <summary>
    /// Represents a leaf in a boolean phrase - compares a field to a value
    /// </summary>
    [DataContract]
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BooleanLeaf : BooleanPhraseNode
    {
        #region Data Members

        /// <summary>
        /// Field name that we compare the values of it to our value
        /// </summary>
        [DataMember]
        [JsonProperty()]
        public string field;

        /// <summary>
        /// The value to compare to 
        /// </summary>
        [DataMember]
        [JsonProperty()]
        public object value;

        /// <summary>
        /// The type of the value
        /// </summary>
        [DataMember]
        [JsonProperty()]
        public Type valueType;

        /// <summary>
        /// Comparison operation to perform on nodes: And/Or
        /// </summary>
        [DataMember]
        [JsonProperty()]
        public ComparisonOperator operand;

        [DataMember]
        public List<eAssetTypes> assetTypes;

        #endregion

        #region Properties

        [JsonIgnore()]
        public override BooleanNodeType type
        {
            get
            {
                return BooleanNodeType.Leaf;
            }
        }

        #endregion

        #region Ctor

        /// <summary>
        /// Basic constructor. Has default values for quick usage
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="operand"></param>
        [JsonConstructor]
        public BooleanLeaf(string field = "", object value = null, Type type = null, ComparisonOperator operand = ComparisonOperator.Equals)
        {
            this.field = field;
            this.value = value;
            this.valueType = type;
            this.operand = operand;
        }

        #endregion

    }
}
