using Phx.Lib.Appconfig;
using ElasticSearch.Common;
using EventBus.RabbitMQ;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HealthCheck
{
    public class Program
    {
        private static KLogger _Logger;

        public static int Main(string[] args)
        {
            KLogger.InitLogger("log4net.config", KLogEnums.AppType.WindowsService, @"C:\log\HealthCheck\");
            ApplicationConfiguration.Init();
            _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
            return (int)HealthCheck();
        }

        private static ExitCode HealthCheck()
        {
            _Logger.Info($"Starting health check.");
            _Logger.Info($"Checking couchbase connection...");

            var HealthCheckResults = new List<ExitCode>();
            HealthCheckResults.Add(CheckRabbitMQ());
            HealthCheckResults.Add(CheckElasticsearch());
            HealthCheckResults.Add(CheckCouchbase());
            HealthCheckResults.Add(CheckSqlServer());

            if (HealthCheckResults.All(r => r == ExitCode.Success))
            {
                _Logger.Info($"Health check passed.");
                return ExitCode.Success;
            }
            else
            {
                var firstError = HealthCheckResults.First(r => r != ExitCode.Success);
                _Logger.Info($"Health check failed, returning code for first failure: ExitCode:[{firstError}]({(int)firstError}).");
                return firstError;
            }

        }

        private static ExitCode CheckRabbitMQ()
        {
            try
            {
                _Logger.Info($"Checking Rabbit Connection...");
                using (var connections = RabbitMQPersistentConnection.GetInstanceUsingTCMConfiguration())
                using (var channel = connections.CreateModel())
                {
                    _Logger.Info($"Connected to RabbitMq on channel:[{channel}]");
                    _Logger.Info($"Disposing RabbitMq connection..");
                }
            }
            catch (Exception e)
            {
                _Logger.Error("Could not connect to RabbitMQ", e);
                return ExitCode.RabbitError;
            }
            return ExitCode.Success;
        }

        private static ExitCode CheckSqlServer()
        {
            try
            {
                _Logger.Info($"Checking SQL DB connection...");
                var q = new ODBCWrapper.SelectQuery();
                q += "select 1";
                var sqlIsSuccess = q.Execute();
                if (!sqlIsSuccess)
                {
                    _Logger.Error("Could not query SQL DB");
                    return ExitCode.MSSQLError;
                }
            }
            catch (Exception e)
            {

                _Logger.Error("Error while trying to query SQL DB", e);
                return ExitCode.MSSQLError;
            }

            return ExitCode.Success;
        }

        private static ExitCode CheckCouchbase()
        {
            try
            {
                _Logger.Info($"Checking Couchbase...");
                var cb = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.OTT_APPS);
                var cbIsSuccess = cb.Set($"HealthCheckDoc_RemoteTasksGeneralHealthCheck", "", 1);
                if (!cbIsSuccess)
                {
                    _Logger.Error("Could not set document into couchbase");
                    return ExitCode.CBError;
                }
            }
            catch (Exception e)
            {
                _Logger.Error("Error while trying to set document into couchbase", e);
                return ExitCode.CBError;
            }

            return ExitCode.Success;
        }

        private static ExitCode CheckElasticsearch()
        {
            try
            {
                _Logger.Info($"Checking Elasticsearch...");
                var es = new ElasticSearchApi(ApplicationConfiguration.Current);
                var esIsSuccess = es.HealthCheck();
                if (!esIsSuccess)
                {
                    _Logger.Error("Elasticsearch health check failed");
                    return ExitCode.ESError;
                }
            }
            catch (Exception e)
            {
                _Logger.Error("Error while trying to health check Elasticsearch", e);
                return ExitCode.ESError;
            }

            return ExitCode.Success;
        }
    }
}
