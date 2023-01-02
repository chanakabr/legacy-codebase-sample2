using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Response;
using DAL;
using IngestHandler.Common.Infrastructure;
using IngestHandler.Common.Repositories.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OTT.Lib.MongoDB;

namespace IngestHandler.Common.Repositories
{
    public interface IBulkUploadRepository
    {
        Task InsertBulkUploadResults(long partnerId, IEnumerable<BulkUploadProgramAssetResult> resultsToInsert);
        Task InsertCrudOperations(long partnerId, long bulkUploadId, CRUDOperations<EpgProgramBulkUploadObject> crudOps);
        Task InsertErrors(long partnerId, long bulkUploadId, IEnumerable<Status> errors);

        Task<IEnumerable<BulkUploadProgramAssetResult>> GetBulkUploadResults(long partnerId, long bulkUploadId);
        Task<CRUDOperations<EpgProgramBulkUploadObject>> GetCrudOperations(long partnerId, long bulkUploadId);
        Task<IEnumerable<Status>> GetErrors(long partnerId, long bulkUploadId);

        Task<bool> IsLinearChannelOfBulkUploadInProgress(int partnerId, long bulkUploadId, long linearChannelId);
    }

    public class BulkUploadRepository : IBulkUploadRepository
    {
        private readonly IMongoDbClientFactory _mongoDbClientFactory;


        public BulkUploadRepository(IMongoDbClientFactory mongoDbClientFactory)
        {
            _mongoDbClientFactory = mongoDbClientFactory;
        }

        public async Task InsertBulkUploadResults(long partnerId, IEnumerable<BulkUploadProgramAssetResult> resultsToInsert)
        {
            var c = await _mongoDbClientFactory.NewMongoDbClientAsync((int)partnerId);
            await c.InsertManyAsync(EpgMongoDB.INGEST_BULK_UPLOAD_RESULTS_COLLECTION, resultsToInsert);
        }

        public async Task InsertCrudOperations(long partnerId, long bulkUploadId, CRUDOperations<EpgProgramBulkUploadObject> crudOps)
        {
            var c = await _mongoDbClientFactory.NewMongoDbClientAsync((int)partnerId);
            var affectedItems = crudOps.AffectedItems.Select(item => new EpgProgramBulkUploadObjectDocument(bulkUploadId, item, BulkUploadCRUDOperation.Affected)).ToList();
            var itemsToAdd = crudOps.ItemsToAdd.Select(item => new EpgProgramBulkUploadObjectDocument(bulkUploadId, item, BulkUploadCRUDOperation.Added)).ToList();
            var itemsToDelete = crudOps.ItemsToDelete.Select(item => new EpgProgramBulkUploadObjectDocument(bulkUploadId, item, BulkUploadCRUDOperation.Deleted)).ToList();
            var itemsToUpdate = crudOps.ItemsToUpdate.Select(item => new EpgProgramBulkUploadObjectDocument(bulkUploadId, item, BulkUploadCRUDOperation.Updated)).ToList();

            if (affectedItems.Any()) { await c.InsertManyAsync(EpgMongoDB.INGEST_BULK_UPLOAD_CRUD_COLLECTION, affectedItems); }

            if (itemsToAdd.Any()) { await c.InsertManyAsync(EpgMongoDB.INGEST_BULK_UPLOAD_CRUD_COLLECTION, itemsToAdd); }

            if (itemsToDelete.Any()) { await c.InsertManyAsync(EpgMongoDB.INGEST_BULK_UPLOAD_CRUD_COLLECTION, itemsToDelete); }

            if (itemsToUpdate.Any()) { await c.InsertManyAsync(EpgMongoDB.INGEST_BULK_UPLOAD_CRUD_COLLECTION, itemsToUpdate); }
        }

        public async Task InsertErrors(long partnerId, long bulkUploadId, IEnumerable<Status> errors)
        {
            var c = await _mongoDbClientFactory.NewMongoDbClientAsync((int)partnerId);
            var errorDocs = errors.Select(e => new BulkUploadErrorDocument(bulkUploadId, e));
            await c.InsertManyAsync(EpgMongoDB.INGEST_BULK_UPLOAD_ERRORS_COLLECTION, errorDocs);
        }

        public async Task<IEnumerable<BulkUploadProgramAssetResult>> GetBulkUploadResults(long partnerId, long bulkUploadId)
        {
            var mongoDbClient = await _mongoDbClientFactory.NewMongoDbClientAsync((int)partnerId);
            var res = await mongoDbClient.FindAsync<BulkUploadProgramAssetResult>(EpgMongoDB.INGEST_BULK_UPLOAD_RESULTS_COLLECTION,
                f => f.And(Builders<BulkUploadProgramAssetResult>.Filter.Eq(p => p.BulkUploadId, bulkUploadId))
            );

            var results = await res.ToListAsync();
            return results;
        }

        public async Task<CRUDOperations<EpgProgramBulkUploadObject>> GetCrudOperations(long partnerId, long bulkUploadId)
        {
            var mongoDbClient = await _mongoDbClientFactory.NewMongoDbClientAsync((int)partnerId);
            var res = await mongoDbClient.FindAsync<EpgProgramBulkUploadObjectDocument>(EpgMongoDB.INGEST_BULK_UPLOAD_CRUD_COLLECTION,
                f => f.And(Builders<EpgProgramBulkUploadObjectDocument>.Filter.Eq(p => p.BulkUploadId, bulkUploadId))
            );

            var crudOpsDocs = await res.ToListAsync();
            var crudOps = new CRUDOperations<EpgProgramBulkUploadObject>();
            crudOps.AffectedItems = crudOpsDocs.Where(c => c.Operation == BulkUploadCRUDOperation.Affected).Select(c => c.EpgProgramBulkUploadObject).ToList();
            crudOps.ItemsToAdd = crudOpsDocs.Where(c => c.Operation == BulkUploadCRUDOperation.Added).Select(c => c.EpgProgramBulkUploadObject).ToList();
            crudOps.ItemsToUpdate = crudOpsDocs.Where(c => c.Operation == BulkUploadCRUDOperation.Updated).Select(c => c.EpgProgramBulkUploadObject).ToList();
            crudOps.ItemsToDelete = crudOpsDocs.Where(c => c.Operation == BulkUploadCRUDOperation.Deleted).Select(c => c.EpgProgramBulkUploadObject).ToList();
            return crudOps;
        }

        public async Task<IEnumerable<Status>> GetErrors(long partnerId, long bulkUploadId)
        {
            var mongoDbClient = await _mongoDbClientFactory.NewMongoDbClientAsync((int)partnerId);
            var res = await mongoDbClient.FindAsync<BulkUploadErrorDocument>(EpgMongoDB.INGEST_BULK_UPLOAD_ERRORS_COLLECTION,
                f => f.And(Builders<BulkUploadErrorDocument>.Filter.Eq(p => p.BulkUploadId, bulkUploadId))
            );

            var errorDoc = await res.ToListAsync();
            var errors = errorDoc.Select(e => e.Error);
            return errors;
        }

        public async Task<bool> IsLinearChannelOfBulkUploadInProgress(int partnerId, long bulkUploadId, long linearChannelId)
        {
            var mongoDbClient = await _mongoDbClientFactory.NewMongoDbClientAsync(partnerId);
            var idempotencyDoc = new BulkUploadIdempotencyDocument(bulkUploadId, linearChannelId);
            var upsertResult = await mongoDbClient.UpdateOneAsync<BulkUploadIdempotencyDocument>(
                EpgMongoDB.INGEST_BULK_UPLOAD_IDEMPOTENT_COLLECTION,
                f => f.Eq(o => o.Id, idempotencyDoc.Id),
                u => u
                    .Set(x => x.Id, idempotencyDoc.Id)
                    .Set(x => x.BulkUploadId, idempotencyDoc.BulkUploadId)
                    .Set(x => x.LinearChannelId, idempotencyDoc.LinearChannelId),
                new MongoDbUpdateOptions { IsUpsert = true });

            return upsertResult.MatchedCount != 0;
        }
    }
}