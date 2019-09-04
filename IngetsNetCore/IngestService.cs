using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiObjects;
using Core.Catalog;
using Ingest.Models;

namespace IngetsNetCore
{
    public class IngestService : Ingest.IService
    {
        public IngestResponse IngestAdiData(IngestRequest request)
        {
            throw new NotImplementedException();
        }

        public BusinessModuleIngestResponse IngestBusinessModules(string username, string password, string xml)
        {
            throw new NotImplementedException();
        }

        public IngestResponse IngestKalturaEpg(IngestRequest request)
        {
            throw new NotImplementedException();
        }

        public IngestResponse IngestTvinciData(IngestRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
