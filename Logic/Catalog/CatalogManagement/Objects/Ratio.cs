using ApiObjects.Response;

namespace Core.Catalog.CatalogManagement
{
    public class Ratio
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int PrecisionPrecentage { get; set; }
    }

    public class RatioResponse
    {
        public Status Status { get; set; }

        public Ratio Ratio { get; set; }

        public RatioResponse()
        {
            Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
        }
    }
}
