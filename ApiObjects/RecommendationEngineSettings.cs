using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class RecommendationEngineSettings
    {
        public string key { get; set; }
        public string value { get; set; }

        public RecommendationEngineSettings()
        {

        }

        public RecommendationEngineSettings(string key, string value)
        {
            this.key = key;
            this.value = value;
        }

    }
}
