using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiLogic.IndexManager.NestData
{
    [ElasticsearchType(IdProperty = nameof(id))]
    public class NestCountry
    {
        [Text()]
        public string name { get; set; }
        [Text()]
        public string code { get; set; }
        [Number()]
        public long country_id { get; set; }
        [Text()]
        public string id { get; set; }

        public ApiObjects.Country ToApiObject()
        {
            return new ApiObjects.Country()
            {
                Code = this.code,
                Id = (int)this.country_id,
                Name = this.name,
            };
        }
    }
}
