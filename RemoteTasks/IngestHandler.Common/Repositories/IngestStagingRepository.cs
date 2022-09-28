using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiObjects.BulkUpload;
using DAL;
using IngestHandler.Common.Infrastructure;
using MongoDB.Driver;
using OTT.Lib.MongoDB;

namespace IngestHandler.Common.Repositories
{
    public interface IIngestStagingRepository
    {
        Task InsertProgramsToStagingCollection(int partnerId, IEnumerable<EpgProgramBulkUploadObject> programsToStage);
        Task<IEnumerable<EpgProgramBulkUploadObject>> GetProgramsFromStagingCollection(long partnerId, long bulkUploadId, long linearChannelId);
    }

    public class IngestStagingRepository : IIngestStagingRepository
    {
        private readonly IMongoDbClientFactory _mongoDbClientFactory;
        

        public IngestStagingRepository(IMongoDbClientFactory mongoDbClientFactory)
        {
            _mongoDbClientFactory = mongoDbClientFactory;
        }

        public async Task InsertProgramsToStagingCollection(int partnerId, IEnumerable<EpgProgramBulkUploadObject> programsToStage)
        {
            var mongoDbClient = await _mongoDbClientFactory.NewMongoDbClientAsync(partnerId);
            await mongoDbClient.InsertManyAsync(EpgMongoDB.INGEST_STAGING_COLLECTION, programsToStage);
        }
        
        public async Task<IEnumerable<EpgProgramBulkUploadObject>> GetProgramsFromStagingCollection(long partnerId, long bulkUploadId, long linearChannelId)
        {
            var mongoDbClient = await _mongoDbClientFactory.NewMongoDbClientAsync((int)partnerId);
            var res = await mongoDbClient.FindAsync<EpgProgramBulkUploadObject>(EpgMongoDB.INGEST_STAGING_COLLECTION,
                f => f.And(Builders<EpgProgramBulkUploadObject>.Filter.Eq(p => p.BulkUploadId, bulkUploadId),
                    Builders<EpgProgramBulkUploadObject>.Filter.Eq(p => p.ChannelId, linearChannelId))
            );

            var stagedPrograms = await res.ToListAsync();
            return stagedPrograms;

        }
        
    }
}