using ConfigurationManager;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiLogic.Tests.ConfigurationMocks
{
    internal class MockElasticSearchConfiguration : ElasticSearchConfiguration
    {
        public MockElasticSearchConfiguration()
        {
            SetActualValue(MaxResults, 100000);

            //if running from local env not on tests env
            //update to localhost
            var isJenkins = System.Environment.GetEnvironmentVariable("IS_ON_JENKINS")?.ToLower() == "true";

            //9201 is the port of es2 on local tests for now
            if (isJenkins)
            {
                SetActualValue(URL_V2, "http://elastic02:9201");
                SetActualValue(URL_V7_13, "http://elastic07:9200");
            }
            else
            {
                SetActualValue(URL_V2, "http://localhost:9201");
                SetActualValue(URL_V7_13, "http://localhost:9200");
            }
        }
    }
}
    
