using ApiObjects.SearchObjects;

namespace ApiLogic.IndexManager.QueryBuilders.ESV2QueryBuilders.SearchPriority.Models
{
    public class KSqlEsPriorityGroup : BaseEsPriorityGroup
    {
        public KSqlEsPriorityGroup(BooleanPhraseNode tree)
        {
            Tree = tree;
        }

        public BooleanPhraseNode Tree { get; }
    }
}
