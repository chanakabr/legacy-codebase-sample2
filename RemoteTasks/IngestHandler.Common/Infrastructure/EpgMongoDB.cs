using System;
using System.Collections.Generic;
using System.Reflection;
using ApiObjects.BulkUpload;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using DAL;
using IngestHandler.Common.Repositories.Models;
using MongoDB.Driver;
using OTT.Lib.MongoDB;
using Phx.Lib.Log;

namespace IngestHandler.Common.Infrastructure
{
    public class EpgMongoDB
    {
        public const int INGEST_COLLECTION_TTL_SEC = 30 * 24 * 60 * 60; // 30d ttl in sec
        public const string INGEST_STAGING_COLLECTION = "ingest_staging";
        public const string INGEST_BULK_UPLOAD_RESULTS_COLLECTION = "ingest_bulk_upload_results";
        public const string INGEST_BULK_UPLOAD_CRUD_COLLECTION = "ingest_bulk_upload_crud";
        public const string INGEST_BULK_UPLOAD_ERRORS_COLLECTION = "ingest_bulk_upload_errors";
        public const string DB_NAME = "epg";

        public static readonly MongoDbConfiguration Configuration = new MongoDbConfiguration
        {
            ConnectionString = TcmConnectionStringHelper.Instance.GetConnectionString(),
            CollectionProps = new Dictionary<string, MongoDbConfiguration.CollectionProperties>
            {
                {
                    INGEST_STAGING_COLLECTION, new MongoDbConfiguration.CollectionProperties
                    {

                        DisableLogicalDelete = true,
                        AutoTtlIndexSeconds = INGEST_COLLECTION_TTL_SEC,
                        IndexBuilderAsync = async (builder) =>
                        {
                            await builder.CreateIndexAsync(o => o.Ascending(f => f.BulkUploadId).Ascending(f => f.LinearMediaId),
                                new MongoDbCreateIndexOptions<EpgProgramBulkUploadObject>
                                {
                                    Unique = false,
                                });
                        }
                    }
                },
                {
                    INGEST_BULK_UPLOAD_RESULTS_COLLECTION, new MongoDbConfiguration.CollectionProperties
                    {
                        DisableLogicalDelete = true,
                        AutoTtlIndexSeconds = INGEST_COLLECTION_TTL_SEC,
                        IndexBuilderAsync = async (builder)=>
                        {
                            await builder.CreateIndexAsync(o => o.Ascending(f => f.BulkUploadId),
                                new MongoDbCreateIndexOptions<BulkUploadProgramAssetResult>{Unique = false});
                        }
                    }
                },
                {
                    INGEST_BULK_UPLOAD_CRUD_COLLECTION, new MongoDbConfiguration.CollectionProperties
                    {
                        DisableLogicalDelete = true,
                        AutoTtlIndexSeconds = (int)TimeSpan.FromDays(30).TotalSeconds,
                        IndexBuilderAsync = async (builder)=>
                        {
                            await builder.CreateIndexAsync(o => o.Ascending(f => f.BulkUploadId),
                                new MongoDbCreateIndexOptions<EpgProgramBulkUploadObjectDocument>{Unique = false});

                        }
                    }
                },
                {
                    INGEST_BULK_UPLOAD_ERRORS_COLLECTION, new MongoDbConfiguration.CollectionProperties
                    {
                        DisableLogicalDelete = true,
                        AutoTtlIndexSeconds = INGEST_COLLECTION_TTL_SEC,
                        IndexBuilderAsync = async (builder)=>
                        {
                            await builder.CreateIndexAsync(o => o.Ascending(f => f.BulkUploadId),
                                new MongoDbCreateIndexOptions<BulkUploadErrorDocument>{Unique = false});
                        }
                    }
                }
            }
        };
    }
}