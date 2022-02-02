namespace ApiLogic.Catalog.CatalogManagement.Models
{
    public class MetaByNameInput
    {
        public string MetaName { get; set; }
        public int GroupId { get; set; }
        public bool ShouldSearchEpg { get; set; }
        public bool ShouldSearchMedia { get; set; }
        public bool ShouldSearchRecordings { get; set; }
    }
}