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
    public class BooleanPhrase : BooleanPhraseNode
    {
        #region Data Members

        /// <summary>
        /// List of child nodes
        /// </summary>
        [DataMember]
        public List<BooleanPhraseNode> nodes;

        /// <summary>
        /// Boolean operation to perform between nodes: And/Or
        /// </summary>
        [DataMember]
        public eCutType operand;

        #endregion

        #region Ctor

        /// <summary>
        /// Basic initialization with default values for quick usage
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="operand"></param>
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

        #region Methods

        public override string ToQuery()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
