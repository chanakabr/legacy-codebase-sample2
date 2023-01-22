using System.Collections.Generic;
using ApiObjects.Recordings;
using MongoDB.Driver;
using OTT.Lib.MongoDB;

namespace DAL.Recordings
{
    public static class RecordingsDbProperties
    {
        public const string RECORDINGS_DATABASE = "recordings";
        internal const string RECORDINGS_COLLECTION = "recordings";
        internal const string HOUSEHOLD_RECORDINGS_COLLECTION = "household_recordings";
        internal const string PROGRAMS_COLLECTION = "programs";
        public static readonly Dictionary<string, MongoDbConfiguration.CollectionProperties> CollectionProperties
            = new Dictionary<string, MongoDbConfiguration.CollectionProperties>
            {
                {
                    RECORDINGS_COLLECTION, new MongoDbConfiguration.CollectionProperties
                    {
                        IndexBuilder = (builder) =>
                        {
                            builder.CreateIndex(o =>
                                o.Ascending(f => f.Key), new MongoDbCreateIndexOptions<TimeBasedRecording>
                            {
                                Unique = true
                            });
                            builder.CreateIndex(o =>
                                o.Ascending(f => f.Status), new MongoDbCreateIndexOptions<TimeBasedRecording>{});
                        }
                    }
                },
                {
                    HOUSEHOLD_RECORDINGS_COLLECTION, new MongoDbConfiguration.CollectionProperties
                    {
                        IndexBuilder = (builder) =>
                        {
                            builder.CreateIndex(o =>
                                o.Ascending(f => f.HouseholdId).Ascending(f => f.RecordingKey), new MongoDbCreateIndexOptions<HouseholdRecording>
                            {
                                Unique = true
                            });
                            builder.CreateIndex(o =>
                                o.Ascending(f => f.HouseholdId).Ascending(f => f.Status), new MongoDbCreateIndexOptions<HouseholdRecording>{});
                        }
                    }
                },
                {
                    PROGRAMS_COLLECTION, new MongoDbConfiguration.CollectionProperties
                    {
                        IndexBuilder = (builder) =>
                        {
                            builder.CreateIndex(o =>
                                o.Ascending(f => f.EpgId), new MongoDbCreateIndexOptions<Program>
                            {
                                Unique = true
                            });
                            // builder.CreateIndex(o =>
                            //     o.Ascending(f => f.Id), new MongoDbCreateIndexOptions<Program>
                            // {
                            //     Unique = true
                            // });
                        }
                    }
                }
            };
    }
}