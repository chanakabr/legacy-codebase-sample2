using System;
using ApiObjects;
using Core.Profiles;
using Microsoft.Extensions.Logging;

namespace IngestHandler.Common.Repositories
{
    public interface IIngestProfileRepository
    {
        IngestProfile GetIngestProfile(int partnerId, int? ingestProfileId);
    }

    public class IngestProfileRepository : IIngestProfileRepository
    {
        private readonly ILogger<IngestProfileRepository> _logger;

        public IngestProfileRepository(ILogger<IngestProfileRepository> logger)
        {
            _logger = logger;
        }
        
        public IngestProfile GetIngestProfile(int partnerId, int? ingestProfileId)
        {
            var ingestProfile = IngestProfileManager.GetIngestProfileById(partnerId, ingestProfileId)?.Object;

            if (ingestProfile == null)
            {
                var message = $"Received bulk upload ingest event with invalid ingest profile.";
                _logger.LogError(message);
                throw new Exception(message);
            }

            return ingestProfile;
        }
    }
}