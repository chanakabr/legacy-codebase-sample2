namespace ApiObjects.SearchPriorityGroups
{
    public class SearchPriorityGroup
    {
        public long Id { get; set; }
        public LanguageContainer[] Name { get; set; }
        public SearchPriorityCriteria Criteria { get; set; }

        public SearchPriorityGroup()
        {
        }

        public SearchPriorityGroup(long id, LanguageContainer[] name, SearchPriorityCriteriaType type, string criteriaValue)
        {
            Id = id;
            Name = name;
            Criteria = new SearchPriorityCriteria(type, criteriaValue);
        }
    }
}