using System;
using System.Collections.Generic;

namespace ApiObjects.CanaryDeployment
{
    public class CanaryDeploymentConfiguration
    {
        public CanaryDeploymentDataOwnership DataOwnership  { get; set; }
        public Dictionary<string, CanaryDeploymentRoutingService> RoutingConfiguration { get; set; } = new Dictionary<string, CanaryDeploymentRoutingService>(StringComparer.OrdinalIgnoreCase);
        public CanaryDeploymentMigrationEvents MigrationEvents { get; set; }

        public CanaryDeploymentConfiguration()
        {
            DataOwnership = new CanaryDeploymentDataOwnership();
            MigrationEvents = new CanaryDeploymentMigrationEvents();
        }
    }
}
