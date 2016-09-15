using ApiObjects.Response;
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
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }
}