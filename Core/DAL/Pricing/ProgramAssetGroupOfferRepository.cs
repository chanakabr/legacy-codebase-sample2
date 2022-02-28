using ApiObjects.Pricing;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using OTT.Lib.MongoDB;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DAL.Pricing
{
    public interface IPagoRepository
    {
        long AddPago(int partnerId, ProgramAssetGroupOffer pago);

        bool UpdatePago(int partnerId, ProgramAssetGroupOffer pagoToUpdate);

        bool DeletePago(int partnerId, long id);

        bool IsPagoExists(int partnerId, long id);

        long GetPagoByExternalId(int partnerId, string externalId);

        Dictionary<long, bool> GetAllPagoIds(int partnerId);

        List<ProgramAssetGroupOffer> GetProgramAssetGroupOffersData(int partnerId, List<long> programAssetGroupOfferIds);

        long GetPagoByExternaOfferlId(int partnerId, string externaOfferId);
        void UpdatePagoVirtualAssetId(int groupId, long id, long assetId);
    }

    public class ProgramAssetGroupOfferRepository : IPagoRepository
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string CollectionName = "program_asset_group_offers";
        private const string DBName = "offers";
        private IMongoDbClientFactory _service;

        public ProgramAssetGroupOfferRepository(string connectionString)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMongoDbClientFactory(new MongoDbConfiguration
            {
                ConnectionString = connectionString,
                CollectionProps =
                {
                    {
                        CollectionName, new MongoDbConfiguration.CollectionProperties
                        {
                            DisableLogicalDelete = false,
                            DisableAutoTimestamps = false,
                            IndexBuilder = (builder) =>
                            {
                                builder.CreateIndex<ProgramAssetGroupOffer>( o =>
                                    o.Ascending(f => f.ExternalId), new MongoDbCreateIndexOptions<ProgramAssetGroupOffer>
                                    {
                                        Unique = true,
                                        PartialFilterExpression = b=> b.Type(a=>a.ExternalId,"string")
                                        });

                             builder.CreateIndex<ProgramAssetGroupOffer>( o =>
                                    o.Ascending(f => f.ExternalOfferId), new MongoDbCreateIndexOptions<ProgramAssetGroupOffer>
                                    {
                                        Unique = true,
                                        PartialFilterExpression = b=> b.Type(a=>a.ExternalOfferId,"string")
                                        });
                            }
                        }
                    }
                }
            }, DBName);

            var p = serviceCollection.BuildServiceProvider();
            _service = p.GetService<IMongoDbClientFactory>();
        }

        private long GetNextPagoId(int partnerId)
        {
            var factory = _service.NewMongoDbClient(partnerId, log);
            return factory.GetNextId(CollectionName);
        }

        public long AddPago(int partnerId, ProgramAssetGroupOffer pago)
        {
            pago.Id = GetNextPagoId(partnerId);
            pago.CreateDate = DateTime.UtcNow;
            pago.UpdateDate = DateTime.UtcNow;

            var factory = _service.NewMongoDbClient(partnerId, log);
            factory.InsertOne(CollectionName, pago);

            return pago.Id;
        }

        public bool DeletePago(int partnerId, long id)
        {
            var factory = _service.NewMongoDbClient(partnerId, log);
            try
            {
                factory.DeleteOne<ProgramAssetGroupOffer>(CollectionName, f => f.Eq(o => o.Id, id));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void UpdatePagoVirtualAssetId(int partnerId, long id, long assetId)
        {
            var factory = _service.NewMongoDbClient(partnerId, log);
            var updateResult = factory.UpdateOne<ProgramAssetGroupOffer>(
                CollectionName,
                f => f.Eq(o => o.Id, id),
                u => u.Set(o => o.VirtualAssetId, assetId));
        }

        public bool UpdatePago(int partnerId, ProgramAssetGroupOffer pago)
        {
            pago.UpdateDate = DateTime.UtcNow;

            var factory = _service.NewMongoDbClient(partnerId, log);
            var updateResult = factory.UpdateOne<ProgramAssetGroupOffer>(
                CollectionName,
                f => f.Eq(o => o.Id, pago.Id),
                u => SetUpdateExpression(pago, u));

            return updateResult.ModifiedCount > 0;
        }

        private UpdateDefinition<ProgramAssetGroupOffer> SetUpdateExpression(ProgramAssetGroupOffer pago, UpdateDefinitionBuilder<ProgramAssetGroupOffer> updateBuilder)
        {
            updateBuilder = new UpdateDefinitionBuilder<ProgramAssetGroupOffer>();
            var updates = new List<UpdateDefinition<ProgramAssetGroupOffer>>();

            UpdateIfNotNull(updateBuilder, updates, x => x.StartDate, pago.StartDate);
            UpdateIfNotNull(updateBuilder, updates, x => x.EndDate, pago.EndDate);
            UpdateIfNotNull(updateBuilder, updates, x => x.ExpiryDate, pago.ExpiryDate);
            UpdateIfNotNull(updateBuilder, updates, x => x.Description, pago.Description);
            UpdateIfNotNull(updateBuilder, updates, x => x.ExternalId, pago.ExternalId);
            UpdateIfNotNull(updateBuilder, updates, x => x.ExternalOfferId, pago.ExternalOfferId);
            UpdateIfNotNull(updateBuilder, updates, x => x.FileTypeIds, pago.FileTypeIds);
            UpdateIfNotNull(updateBuilder, updates, x => x.IsActive, pago.IsActive);
            UpdateIfNotNull(updateBuilder, updates, x => x.Name, pago.Name);
            UpdateIfNotNull(updateBuilder, updates, x => x.LastUpdaterId, pago.LastUpdaterId);
            UpdateIfNotNull(updateBuilder, updates, x => x.PriceDetailsId, pago.PriceDetailsId);
            UpdateIfNotNull(updateBuilder, updates, x => x.VirtualAssetId, pago.VirtualAssetId);
            UpdateIfNotNull(updateBuilder, updates, x => x.UpdateDate, DateTime.UtcNow);

            return updateBuilder.Combine(updates);
        }

        private static void UpdateIfNotNull<TDocument, TField>(
            UpdateDefinitionBuilder<TDocument> updateBuilder,
            List<UpdateDefinition<TDocument>> updates,
            Expression<Func<TDocument, TField>> field,
            TField value)
        {
            if (value != null)
            {
                var update = updateBuilder.Set(field, value);
                updates.Add(update);
            }
        }

        public bool IsPagoExists(int partnerId, long id)
        {
            var factory = _service.NewMongoDbClient(partnerId, log);

            return factory.Find<ProgramAssetGroupOffer>(CollectionName, f => f.Eq(o => o.Id, id)).SingleOrDefault() != null;
        }

        public long GetPagoByExternalId(int partnerId, string externalId)
        {
            var factory = _service.NewMongoDbClient(partnerId, log);

            var pago = factory.Find<ProgramAssetGroupOffer>(CollectionName, f => f.Eq(o => o.ExternalId, externalId)).SingleOrDefault();

            if (pago != null)
            {
                return pago.Id;
            }

            return 0;
        }

        public Dictionary<long, bool> GetAllPagoIds(int partnerId)
        {
            Dictionary<long, bool> res = null;
            var factory = _service.NewMongoDbClient(partnerId, log);

            var pagos = factory.Find<ProgramAssetGroupOffer>(CollectionName, f => f.Empty).ToList<ProgramAssetGroupOffer>();
            if (pagos.Count > 0)
            {
                res = new Dictionary<long, bool>();
                pagos.ForEach(pago =>
                {
                    res.Add(pago.Id, pago.IsActive.Value);
                });
            }
            return res;
        }

        public List<ProgramAssetGroupOffer> GetProgramAssetGroupOffersData(int partnerId, List<long> programAssetGroupOfferIds)
        {
            var factory = _service.NewMongoDbClient(partnerId, log);

            return factory.Find<ProgramAssetGroupOffer>(CollectionName, f => f.Where(i => programAssetGroupOfferIds.Contains(i.Id))).ToList<ProgramAssetGroupOffer>();
        }

        public long GetPagoByExternaOfferlId(int partnerId, string externaOfferId)
        {
            var factory = _service.NewMongoDbClient(partnerId, log);

            var pago = factory.Find<ProgramAssetGroupOffer>(CollectionName, f => f.Eq(o => o.ExternalOfferId, externaOfferId)).SingleOrDefault();

            if (pago != null)
            {
                return pago.Id;
            }

            return 0;
        }
    }
}