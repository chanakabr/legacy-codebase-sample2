using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class CountryResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public List<Country> Countries { get; set; }

        public CountryResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Countries = new List<Country>();
        }
    }

    public class Country
    {
        [JsonProperty("country_id")]
        public int Id
        {
            get;
            set;
        }
        [JsonProperty("name")]
        public string Name
        {
            get;
            set;
        }
        [JsonProperty("code")]
        public string Code
        {
            get;
            set;
        }

        public Country()
        {
            this.Id = 0;
            this.Name = string.Empty;
            this.Code = string.Empty;
        }
    }
}