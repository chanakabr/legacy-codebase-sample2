namespace ApiObjects
{
    public class CatalogPartnerConfig
    {
        public bool? SingleMultilingualMode { get; set; }

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

            return needToUpdate;
        }
    }   
}