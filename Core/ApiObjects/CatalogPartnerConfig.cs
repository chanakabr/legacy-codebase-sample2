namespace ApiObjects
{
    public class CatalogPartnerConfig
    {
        public bool? SingleMultilingualMode { get; set; }
        
        public CategoryManagement CategoryManagement { get; set; }

        public bool? EpgMultilingualFallbackSupport { get; set; }

        public bool? UploadExportDatalake { get; set; }

        public bool SetUnchangedProperties(CatalogPartnerConfig oldConfig)
        {
            var needToUpdate = false;
            if (this.SingleMultilingualMode.HasValue)
            {
                needToUpdate = true;
            }
            else
            {
                this.SingleMultilingualMode = oldConfig.SingleMultilingualMode;
            }

            if (this.CategoryManagement != null)
            {
                needToUpdate = true;
            }
            else
            {
                this.CategoryManagement = oldConfig.CategoryManagement;
            }

            if (this.EpgMultilingualFallbackSupport.HasValue)
            {
                needToUpdate = true;
            }
            else
            {
                this.EpgMultilingualFallbackSupport = oldConfig.EpgMultilingualFallbackSupport;
            }

            if (this.UploadExportDatalake.HasValue)
            {
                needToUpdate = true;
            }
            else
            {
                this.UploadExportDatalake = oldConfig.UploadExportDatalake;
            }

            return needToUpdate;
        }
    }   
}