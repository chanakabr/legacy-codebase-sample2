namespace Core.Catalog.CatalogManagement
{
    public class ImageType
    {
        public long Id { get; set; }
        public string Name { get; set; }        
        public string SystemName { get; set; }
        public long RatioId{ get; set; }
        public string HelpText { get; set; }
        public long? DefaultImageId { get; set; }
    }
}
