using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class AdProvider
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        public AdProvider(Tvinci.Data.Loaders.TvinciPlatform.Catalog.AdProvider adProvider)
        {
            Id = adProvider.ProviderID;
            Name = adProvider.ProviderName;
        }
    }
}
