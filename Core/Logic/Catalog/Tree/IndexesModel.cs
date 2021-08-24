namespace ApiLogic.Catalog.Tree
{
    public class IndexesModel
    {
        public IndexesModel()
        {
            Indexes = 0;
        }
        
        public bool ShouldSearchEpg => Indexes.HasFlag(ElasticSearchIndexes.Epg) || Indexes.HasFlag(ElasticSearchIndexes.Common);

        public bool ShouldSearchMedia => Indexes.HasFlag(ElasticSearchIndexes.Media) || Indexes.HasFlag(ElasticSearchIndexes.Common);

        public ElasticSearchIndexes Indexes { get; set; }
    }
}