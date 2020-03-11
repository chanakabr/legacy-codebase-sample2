using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearch.Searcher
{
    [JsonObject()]
    public class ESFunctionScore : IESTerm
    {
        [JsonIgnore()]
        public eTermType eType
        {
            get
            {
                return eTermType.FUNCTION_SCORE;
            }
        }

        [JsonIgnore()]
        public string query;
        [JsonProperty(PropertyName = "query")]
        public JObject queryObject;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string boost;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? weight;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ESFunctionScoreRandomScore random_score;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ESFunctionScoreScriptScore script_score;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ESFunctionScoreFunction> functions;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, ItemConverterType = typeof(StringEnumConverter))]
        public eFunctionScoreBoostMode? boost_mode;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? max_boost;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, ItemConverterType = typeof(StringEnumConverter))]
        public eFunctionScoreScoreMode? score_mode;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? min_score;

        public bool IsEmpty()
        {
            return false;
        }

        public override string ToString()
        {
            JObject jObject = new JObject();
            queryObject = JObject.Parse(query);
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new StringEnumConverter());
            JObject thisObject = JObject.FromObject(this, serializer);

            jObject.Add("function_score", thisObject);
            return jObject.ToString(Formatting.Indented, new StringEnumConverter());
        }
    }

    public enum eFunctionScoreType
    {
        weight,
        script_score,
        random_score,
        field_value_factor
    }

    public enum eFunctionScoreBoostMode
    {
        multiply,
        replace,
        sum,
        avg,
        max,
        min
    }

    public enum eFunctionScoreScoreMode
    {
        multiply,
        sum,
        avg,
        max,
        min,
        first
    }

    public class ESFunctionScoreFunction
    {
        [JsonProperty(PropertyName = "filter", NullValueHandling = NullValueHandling.Ignore)]
        public JObject filter;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double weight;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ESFunctionScoreRandomScore random_score;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ESFunctionScoreScriptScore script_score;

        public ESFunctionScoreFunction(ESTerm filterTerm)
        {
            filter = new JObject();
            filter["term"] = new JObject();
            filter["term"][filterTerm.Key.ToLower()] = filterTerm.Value.ToLower();
        }
    }

    [JsonObject()]
    public class ESFunctionScoreRandomScore
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int seed;
    }

    public class ESFunctionScoreFieldValueFactor
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string field;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double factor;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double missing;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public eFieldValueFactorModifier modifier;
    }

    public enum eFieldValueFactorModifier
    {
        none,
        log,
        log1p,
        log2p,
        ln,
        ln1p,
        ln2p,
        square,
        sqrt,
        reciprocal
    }

    public class ESFunctionScoreScriptScore
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string script;
    }
}