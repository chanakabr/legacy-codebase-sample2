namespace ApiObjects.SearchPriority
{
    public class SearchPriorityGroup
    {
        public int Id { get; set; }

        public long GroupId { get; set; }

        public string Name { get; set; }

        public Criteria Criteria { get; set; }
    }
}
