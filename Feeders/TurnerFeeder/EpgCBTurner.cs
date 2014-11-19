using System;
using ApiObjects;
using Newtonsoft.Json;

namespace TurnerFeeder
{
    public class EpgCBTurner : EpgCB
    {
        [JsonProperty("subtitle")]
        public string Subtitle { get; set; }

        public EpgCBTurner()
        {
            Subtitle = string.Empty;  
        }

    }
}
