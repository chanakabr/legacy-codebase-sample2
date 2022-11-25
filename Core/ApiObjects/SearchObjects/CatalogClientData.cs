using System;

namespace ApiObjects.SearchObjects
{
    public class CatalogClientData
    {
        public DateTime ServerTime { get; set; }
        public string Signature { get; set; }
        public string SignString { get; set; }
    }
}