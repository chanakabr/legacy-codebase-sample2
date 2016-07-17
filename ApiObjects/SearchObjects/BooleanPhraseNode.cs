using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects.SearchObjects
{
    /// <summary>
    /// Representing the abstract class of a node in a tree-boolean phrase
    /// </summary>
    [DataContract]
    public abstract class BooleanPhraseNode
    {
        public abstract BooleanNodeType type
        {
            get;
        }

        public BooleanPhraseNode()
        {
        }

        #region Statis Methods

        public static void ReplaceLeafWithPhrase(ref BooleanPhraseNode filterTree,
            Dictionary<BooleanPhraseNode, BooleanPhrase> parentMapping, BooleanLeaf leaf, BooleanPhraseNode newPhrase)
        {
            // If there is a parent to this leaf - remove the old leaf and add the new phrase instead of it
            if (parentMapping.ContainsKey(leaf))
            {
                parentMapping[leaf].nodes.Remove(leaf);
                parentMapping[leaf].nodes.Add(newPhrase);
            }
            else
            // If it doesn't exist in the mapping, it's probably the root
            {
                filterTree = newPhrase;
            }
        }

        #endregion
    }

    public enum BooleanNodeType
    {
        Leaf,
        Parent
    }
}
