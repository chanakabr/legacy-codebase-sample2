using ApiObjects;
using ApiObjects.Catalog;
using Core.Catalog;
using Core.Catalog.Cache;
using Core.Catalog.Controller;
using Core.Catalog.Request;
using Core.Catalog.Response;
using GroupsCacheManager;
using KLogMonitor;
using KlogMonitorHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using WebAPI.WebServices;

namespace WS_Catalog
{
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any, InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class Service : CatalogService
    {
        // DO NOT IMPLEMENT ANYTHING HERE!!
        // This is a proxy class for the actual common implementation in WebApi 
        // which is the base class
        // This is so that the net461 and netcore implementation will have the same source code of implementation
        // While allowing [ServiceBehavior] attribute to be defined
    }
}
