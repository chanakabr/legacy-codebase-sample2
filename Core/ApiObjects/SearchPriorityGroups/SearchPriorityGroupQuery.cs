namespace ApiObjects.SearchPriorityGroups
{
    public class SearchPriorityGroupQuery
    {
        public long? IdEqual { get; set; }
        public bool ActiveOnly { get; set; }
        public SearchPriorityGroupOrderBy OrderBy { get; set; }
        public string Language { get; set; }
        public string DefaultLanguage { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public SearchPriorityGroupQuery() { }

        public SearchPriorityGroupQuery(long? idEqual, bool activeOnly, SearchPriorityGroupOrderBy orderBy, string language, string defaultLanguage, int pageIndex, int pageSize)
        {
            IdEqual = idEqual;
            ActiveOnly = activeOnly;
            OrderBy = orderBy;
            Language = language;
            DefaultLanguage = defaultLanguage;
            PageIndex = pageIndex;
            PageSize = pageSize;
        }
    }
}
