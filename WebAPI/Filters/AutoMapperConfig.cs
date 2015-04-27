using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Catalog;
using WebAPI.Clients.Mapping;
using WebAPI.Models;
using WebAPI.Utils;

namespace WebAPI.Filters
{
    public class AutoMapperConfig
    {
        public static void RegisterMappings()
        {
            UsersMappings.RegisterMappings();
            CatalogMappings.RegisterMappings();
            ApiMappings.RegisterMappings();
        }
    }
}