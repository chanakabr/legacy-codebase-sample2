using ApiObjects;
using ApiObjects.Catalog;
using Core.Catalog;
using Ingest.Clients.ClientManager;
using Ingest.Importers;
using Ingest.Models;
using KLogMonitor;
using System.Reflection;
using System.ServiceModel;

namespace Ingest
{
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any, InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]    
    public class Service : IngestService
    {
        // DO NOT IMPLEMENT ANYTHING HERE!!
        // This is a proxy class for the actual common implementation in Ingest.Common 
        // which is the base class
        // This is so that the net461 and netcore implementation will have the same source code of implementation
        // While allowing [ServiceBehavior] attribute to be defined
    }
}
