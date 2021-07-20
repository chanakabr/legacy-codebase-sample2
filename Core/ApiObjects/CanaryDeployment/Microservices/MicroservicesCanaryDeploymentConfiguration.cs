using System;
using System.Collections.Generic;

namespace ApiObjects.CanaryDeployment.Microservices
{
    public class MicroservicesCanaryDeploymentConfiguration
    {
        public MicroservicesCanaryDeploymentDataOwnership DataOwnership  { get; set; }
        public Dictionary<string, MicroservicesCanaryDeploymentRoutingService> RoutingConfiguration { get; set; } = new Dictionary<string, MicroservicesCanaryDeploymentRoutingService>(StringComparer.OrdinalIgnoreCase);
        public MicroservicesCanaryDeploymentMigrationEvents MigrationEvents { get; set; }

        public MicroservicesCanaryDeploymentConfiguration()
        {
            DataOwnership = new MicroservicesCanaryDeploymentDataOwnership();
            MigrationEvents = new MicroservicesCanaryDeploymentMigrationEvents();
        }
    }
}
