using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ApiLogic.Context;
using ApiLogic.Modules.Services;
using ApiObjects;
using ApiObjects.Segmentation;
using Core.Api;
using Core.Catalog;
using EventBus.Kafka;
using Microsoft.Extensions.Configuration;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;
using System.Linq;
using Couchbase.Configuration.Client;
using Couchbase.Core.Transcoders;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using CouchbaseManager;
using Couchbase;
using Newtonsoft.Json.Linq;

namespace Segmentation.Migrator
{
    public class Program
    {
        private static KLogger log;
        private static Configuration config;

        private const string BUCKET = "OTT_Apps";
        private const string DESIGN_DOC_NAME = "migration";
        private const string HOUSEHOLD_SEGMENTS_DESIGN_DOC = "function (doc, meta) {\n  if (meta.id.startsWith(\"household_segment\") && doc.hasOwnProperty(\"Segments\"))\n  {\n    var keys = Object.keys(doc.Segments)\n    if (keys.length > 0)\n    {\n    \temit(doc.Segments[keys[0]].GroupId, null);\n    }\n  }\n}\n                 ";
        private const string USER_SEGMENTS_DESIGN_DOC = "function (doc, meta) {\n  if (meta.id.startsWith(\"user_segment\") && doc.hasOwnProperty(\"Segments\"))\n  {\n    var objectConstructor = ({}).constructor;\n    if (doc.Segments && doc.Segments.constructor == objectConstructor)\n    {\n      var keys = Object.keys(doc.Segments)\n      if (keys.length > 0 && doc.Segments[keys[0]].GroupId)\n      {\n        emit(doc.Segments[keys[0]].GroupId, null);\n      }\n    }\n  }\n}\n                 ";
        private const string MIGRATION_DESIGN_DOC = "{\"views\":{\"household_segments\":{\"map\":\"function (doc, meta) {\\n  if (meta.id.startsWith(\\\"household_segment\\\") && doc.hasOwnProperty(\\\"Segments\\\"))\\n  {\\n    var keys = Object.keys(doc.Segments)\\n    if (keys.length > 0)\\n    {\\n    \\temit(doc.Segments[keys[0]].GroupId, null);\\n    }\\n  }\\n}\\n                 \"},\"user_segments\":{\"map\":\"function (doc, meta) {\\n  if (meta.id.startsWith(\\\"user_segment\\\") && doc.hasOwnProperty(\\\"Segments\\\"))\\n  {\\n    var objectConstructor = ({}).constructor;\\n    if (doc.Segments && doc.Segments.constructor == objectConstructor)\\n    {\\n      var keys = Object.keys(doc.Segments)\\n      if (keys.length > 0 && doc.Segments[keys[0]].GroupId)\\n      {\\n        emit(doc.Segments[keys[0]].GroupId, null);\\n      }\\n    }\\n  }\\n}\\n                 \"}}}";

        public static void Main(string[] args)
        {
            InitializeMigration();

            try
            {
                log.Info($"Starting segmentation migration for partner:[{config.PartnerId}].");

                MigrateSegmentationTypes();
                MigrateHouseholdSegments();
                MigrateUserSegments();

                log.Info($"Migration completed...");
            }
            catch (Exception e)
            {
                log.Error("unexpected error: ", e);
            }
        }

        private static void InitializeMigration()
        {
            // Init Logger
            KLogger.InitLogger("log4net.config", KLogEnums.AppType.WindowsService, @"/var/log/segmentation_migration/");
            log = new KLogger("SegmentationMigrationReader");
            log.Info("Starting segmentation migration");

            // Build configuration
            var configRoot = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables("OTT")
                .Build();

            config = configRoot.Get<Configuration>();
            ApplicationConfiguration.Init();

            var clientConfiguration = GetCouchbaseClientConfigurationFromTCM();

            // use this to initialize cluster once
            new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);

            var bucket = ClusterHelper.GetBucket(BUCKET);
            string username = clientConfiguration.BucketConfigs[BUCKET].Username;
            if (string.IsNullOrEmpty(username))
            {
                username = config.CouchbaseUserName;
            }

            var manager = bucket.CreateManager(username, clientConfiguration.BucketConfigs[BUCKET].Password);
            var getResult = manager.GetDesignDocument(DESIGN_DOC_NAME);
            if (getResult.Success)
            {
                var json = JObject.Parse(getResult.Value);

                if (json["views"] == null)
                {
                    json["views"] = new JObject();
                }

                var views = json["views"];

                if (views["household_segments"] == null)
                {
                    views["household_segments"] = new JObject();
                }

                views["household_segments"]["map"] = HOUSEHOLD_SEGMENTS_DESIGN_DOC;

                if (views["user_segments"] == null)
                {
                    views["user_segments"] = new JObject();
                }

                views["user_segments"]["map"] = USER_SEGMENTS_DESIGN_DOC;

                var updateResult = manager.UpdateDesignDocument(DESIGN_DOC_NAME, json.ToString());

                log.Debug($"update design doc result = {updateResult.Success} ; {updateResult.Message}");
            }
            else
            {
                var insertResult = manager.InsertDesignDocument(DESIGN_DOC_NAME, MIGRATION_DESIGN_DOC);
                log.Debug($"insert design doc result = {insertResult.Success} ; {insertResult.Message}");
            }

        }

        private static ClientConfiguration GetCouchbaseClientConfigurationFromTCM()
        {
            try
            {
                ClientConfiguration couchbaseConfigFromTCM = ApplicationConfiguration.Current.CouchbaseClientConfiguration.GetClientConfiguration();

                if (couchbaseConfigFromTCM != null)
                {
                    // This is here because the default constructor of ClientConfiguration adds a http://localhost:8091/pools url to the 0 index :\
                    // Sunny: somehow I got to a situation where we don't have this localhost server, so I added more conditions so we won't remove a "good" server
                    if (couchbaseConfigFromTCM.Servers != null && couchbaseConfigFromTCM.Servers.Count > 0)
                    {
                        var firstServer = couchbaseConfigFromTCM.Servers[0];

                        if (firstServer.AbsoluteUri.ToLower().Contains("localhost"))
                        {
                            couchbaseConfigFromTCM.Servers.RemoveAt(0);
                        }
                    }
                    couchbaseConfigFromTCM.Transcoder = GetTranscoder;
                    return couchbaseConfigFromTCM;
                }
            }
            catch (Exception e)
            {
                log.Warn($"Could not load couchbase configuration from TCM , trying to load it from web.config file. exception details:{e}", e);
            }

            return null;
        }

        private static ITypeTranscoder GetTranscoder()
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver()
            };
            JsonSerializerSettings deserializationSettings = new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver()
            };
            CustomSerializer serializer = new CustomSerializer(deserializationSettings, serializerSettings);
            CustomTranscoder transcoder = new CustomTranscoder(serializer);

            return transcoder;
        }
        private static void MigrateSegmentationTypes()
        {
            log.Debug($"SegmentationType: starting to get");

            int pageIndex = 0;
            int segmentationTypepageSize = config.SegmentationTypePageSize;
            if (segmentationTypepageSize <= 0)
            {
                segmentationTypepageSize = 50;
            }

            // get all segmentation types in partner - loop for other pages if needed (there aren't many segmentation types in each partner)
            var segmentationTypes = SegmentationType.ListFromCb(config.PartnerId, null, pageIndex, segmentationTypepageSize, out int totalCount);

            while (((pageIndex + 1) * segmentationTypepageSize) < totalCount)
            {
                pageIndex++;
                segmentationTypes.AddRange(SegmentationType.ListFromCb(config.PartnerId, null, pageIndex, segmentationTypepageSize, out totalCount));
            }

            // send kafka messages with each of the segmentation types
            var messageService = new SegmentationTypeCrudMessageService(
                KafkaProducerFactoryInstance.Get(),
                WebKafkaContextProvider.Instance,
                log);

            log.Debug($"SegmentationType: got {segmentationTypes.Count}, starting to publish messages");
            foreach (var segmentationType in segmentationTypes)
            {
                messageService.PublishMigrationCreateEventAsync(config.PartnerId, segmentationType).GetAwaiter().GetResult();
            }

            log.Debug($"Segmentation Types: finished publishing messages");
        }

        private static void MigrateHouseholdSegments()
        {
            log.Debug($"HouseholdSegment: starting to get");

            int pageIndex = 0;
            int pageSize = config.UserSegmentPageSize;
            if (pageSize <= 0)
            {
                pageSize = 1000;
            }

            var householdSegmentsList = HouseholdSegment.ListAll(config.PartnerId, pageIndex, pageSize, out int totalCount);

            while (((pageIndex + 1) * pageSize) < totalCount)
            {
                pageIndex++;
                householdSegmentsList.AddRange(HouseholdSegment.ListAll(config.PartnerId, pageIndex, pageSize, out _));
            }

            // send kafka messages with each of the segmentation types
            var messageService = new HouseholdSegmentCrudMessageService(
                KafkaProducerFactoryInstance.Get(),
                WebKafkaContextProvider.Instance,
                log);
            log.Debug($"HouseholdSegment: got {householdSegmentsList.Count}, starting to publish messages");

            foreach (var householdSegments in householdSegmentsList)
            {
                foreach (var householdSegment in householdSegments.Segments.Values)
                {
                    messageService.PublishMigrationCreateEventAsync(config.PartnerId, householdSegment).GetAwaiter().GetResult();
                }
            }

            log.Debug($"HouseholdSegment: finished publishing messages");
        }

        private static void MigrateUserSegments()
        {
            log.Debug($"UserSegment: starting to get");

            int pageIndex = 0;
            int pageSize = config.UserSegmentPageSize;
            if (pageSize <= 0)
            {
                pageSize = 1000;
            }

            var userSegmentsList = UserSegment.ListAll(config.PartnerId, pageIndex, pageSize, out int totalCount);


            while (((pageIndex + 1) * pageSize) < totalCount)
            {
                pageIndex++;
                userSegmentsList.AddRange(UserSegment.ListAll(config.PartnerId, pageIndex, pageSize, out _));
            }

            // send kafka messages with each of the segmentation types
            var messageService = new UserSegmentCrudMessageService(
                KafkaProducerFactoryInstance.Get(),
                WebKafkaContextProvider.Instance,
                log);
            log.Debug($"UserSegment: got {userSegmentsList.Count}, starting to publish messages");

            foreach (var userSegments in userSegmentsList.Where(o => o != null))
            {
                foreach (var userSegment in userSegments.Segments.Values)
                {
                    userSegment.UserId = userSegments.UserId;
                    messageService.PublishMigrationCreateEventAsync(config.PartnerId, userSegment).GetAwaiter().GetResult();
                }
            }

            log.Debug($"UserSegment: finished publishing messages");
        }
    }
}
