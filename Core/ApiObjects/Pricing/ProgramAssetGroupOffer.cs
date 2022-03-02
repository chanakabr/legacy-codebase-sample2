using ApiObjects.Base;
using System;
using System.Collections.Generic;

namespace ApiObjects.Pricing
{
    public class ProgramAssetGroupOffer : BaseSupportsNullable
    {
        public long Id { get; set; }

        public Dictionary<string, string> Description { get; set; }       

        public string ExternalId { get; set; }

        public string ExternalOfferId { get; set; }

        public List<long> FileTypeIds { get; set; }

        public bool? IsActive { get; set; }

        public Dictionary<string, string> Name { get; set; }

        public long? PriceDetailsId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public long? VirtualAssetId { get; set; }

        public DateTime? CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public long LastUpdaterId { get; set; }

        public DateTime __updated { get; set; }
    }
}