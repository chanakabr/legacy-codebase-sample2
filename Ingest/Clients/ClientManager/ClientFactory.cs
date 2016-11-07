using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ingest.Clients.ClientManager
{
    public class ClientFactory
    {
        public static BaseClient GetService(string url, ClientType clientType)
        {
            BaseClient client = null;

            switch (clientType)
            {
                case ClientType.Api:
                    client = new ApiClient() { Module = new WS_API.API() };
                    break;
                case ClientType.Pricing:
                    client = new PricingClient() { Module = new WS_Pricing.mdoule() };
                    break;
                case ClientType.Catalog:
                    client = new CatalogClient()
                    {
                        Module = new Catalog.IserviceClient(string.Empty, url)
                    };
                    break;
                default:
                    break;
            }

            if (client != null)
            {
                client.ClientType = clientType;
            }

            return client;
        }
    }
}