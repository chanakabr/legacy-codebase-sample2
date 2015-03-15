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

        public BooleanPhraseNode()
        {
        }

        public abstract string ToQuery();
    }
}
