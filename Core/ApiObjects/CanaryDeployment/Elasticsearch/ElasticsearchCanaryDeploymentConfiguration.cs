namespace ApiObjects.CanaryDeployment.Elasticsearch
{
    public enum ElasticsearchVersion
    {
        ES_2_3,
        ES_7_13,
    }
    
    public class ElasticsearchCanaryDeploymentConfiguration
    {
        public ElasticsearchVersion ElasticsearchActiveVersion { get; set; }
        public bool EnableMigrationEvents { get; set; }
    }
}