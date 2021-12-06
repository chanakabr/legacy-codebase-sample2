namespace ApiObjects.SearchPriorityGroups
{
    public class SearchPriorityCriteria
    {
        public SearchPriorityCriteriaType Type { get; set; }
        public string Value { get; set; }

        public SearchPriorityCriteria(SearchPriorityCriteriaType type, string value)
        {
            Type = type;
            Value = value;
        }
    }
}