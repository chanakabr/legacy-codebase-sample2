using ConfigurationManager;
using System;

namespace ApiLogic.Tests.ConfigurationMocks
{
    internal class MockElasticSearchConfiguration : ElasticSearchConfiguration
    {
        public MockElasticSearchConfiguration()
        {
            SetActualValue(MaxResults, 100000);
            SetActualValue(this.StatSortBulkSize, 1000);

            //if running from local env not on tests env
            //update to localhost
            var isJenkins = Environment.GetEnvironmentVariable("IS_ON_JENKINS")?.ToLower() == "true";

            if (isJenkins)
            {
                SetActualValue(URL_V2, "http://elastic02:9200");
                SetActualValue(URL_V7, "http://elastic07:9200");
            }
            else
            {
                //9201 is the port of es2 on local tests for now
                SetActualValue(URL_V2, "http://localhost:9201");
                SetActualValue(URL_V7, "http://localhost:9200");
            }
        }
    }
}
    
