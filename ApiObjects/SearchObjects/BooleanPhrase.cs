using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects.SearchObjects
{
    /// <summary>
    /// Represents a boolean phrase with child nodes - each of them can be either another boolean phrase or a leaf
    /// </summary>
    [DataContract]
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BooleanPhrase : BooleanPhraseNode
    {
        #region Data Members

        /// <summary>
        /// List of child nodes
        /// </summary>
        [DataMember]
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto, ItemTypeNameHandling = TypeNameHandling.Auto,
            ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<BooleanPhraseNode> nodes;

        /// <summary>
        /// Boolean operation to perform between nodes: And/Or
        /// </summary>
        [DataMember]
        [JsonProperty()]
        public eCutType operand;

        #endregion

        #region Properties

        [JsonIgnore()]
        public override BooleanNodeType type
        {
            get
            {
                return BooleanNodeType.Parent;
            }
        }

        #endregion

        #region Ctor

        /// <summary>
        /// Basic initialization with default values for quick usage
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="operand"></param>
        [JsonConstructor]
        public BooleanPhrase(List<BooleanPhraseNode> nodes = null, eCutType operand = eCutType.Or)
        {
            if (nodes == null)
            {
                this.nodes = new List<BooleanPhraseNode>();
            }
            else
            {
                this.nodes = nodes;
            }

            this.operand = operand;
        }

        #endregion

    }
}
