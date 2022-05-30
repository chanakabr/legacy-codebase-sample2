using System.Collections.Generic;
using ApiObjects.Pricing;
using OTT.Lib.MongoDB;

namespace DAL.Pricing
{
    public static class DatabaseProperties
    {
        public const string DATABASE = "offers";
        internal const string PAGO_COLLECTION = "program_asset_group_offers";
        public static readonly Dictionary<string, MongoDbConfiguration.CollectionProperties> CollectionProperties
            = new Dictionary<string, MongoDbConfiguration.CollectionProperties>
            {
                {
                    PAGO_COLLECTION, new MongoDbConfiguration.CollectionProperties
                    {
                        DisableLogicalDelete = false,
                        DisableAutoTimestamps = false,
                        IndexBuilder = (builder) =>
                        {
                            builder.CreateIndex(o =>
                                o.Ascending(f => f.ExternalId), new MongoDbCreateIndexOptions<ProgramAssetGroupOffer>
                            {
                                Unique = true,
                                PartialFilterExpression = b => b.Exists(a => a.ExternalId) & b.Type(a => a.ExternalId, "string") & b.Gt<object>(a => a.ExternalId, 0)
                            });

                            builder.CreateIndex(o =>
                                o.Ascending(f => f.ExternalOfferId), new MongoDbCreateIndexOptions<ProgramAssetGroupOffer>
                            {
                                Unique = true,
                                PartialFilterExpression = b => b.Exists(a => a.ExternalOfferId) & b.Type(a => a.ExternalOfferId, "string") & b.Gt<object>(a => a.ExternalOfferId, 0)
                            });
                        }
                    }
                }
            };
    }
}