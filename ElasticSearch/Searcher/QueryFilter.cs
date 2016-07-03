using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearch.Searcher
{
    public class QueryFilter
    {
        public QueryFilter() { }

        public BaseFilterCompositeType FilterSettings
        {
            get;
            set;
        }
        
        public bool IsEmpty()
        {
            if (FilterSettings == null)
            {
                return true;
            }
            else
            {
                return FilterSettings.IsEmpty();
            }
        }

        public override string ToString()
        {
            string sRes = string.Empty;

            if (FilterSettings != null && !FilterSettings.IsEmpty())
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\"filter\":{");
                sb.Append(FilterSettings.ToString());
                sb.Append("}");
                sRes = sb.ToString();
            }

            return sRes;
        }
    }

    public interface IESTerm
    {
        eTermType eType { get; }
        bool IsEmpty();
        string ToString();
    }

    public class ESTerm : IESTerm
    {
        public string Value { get; set; }
        public string Key { get; set; }
        public eTermType eType { get; protected set; }
        public bool isNot { get; set; }
        public float Boost { get; set; }

        bool m_bIsNumeric;

        public ESTerm(bool bIsNumeric)
        {
            eType = eTermType.TERM;
            m_bIsNumeric = bIsNumeric;
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Value) || string.IsNullOrEmpty(Key);

        }

        public override string ToString()
        {

            if (this.IsEmpty())
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            if (isNot)
                sb.Append("{\"not\":");

            sb.Append("{\"term\":{");
            sb.AppendFormat("\"{0}\":", Key);
            sb.Append("{");

            if (m_bIsNumeric)
            {
                sb.AppendFormat("\"value\":{0}", Value);
            }
            else
            {
                sb.AppendFormat("\"value\":\"{0}\"", Value);
            }

            if (Boost > 0.0f)
            {
                sb.AppendFormat(",\"boost\":{0}", Boost);
            }

            sb.Append("}}}");

            if (isNot)
                sb.Append("}");

            return sb.ToString();


            //if(this.IsEmpty())
            //    return string.Empty;

            //StringBuilder sb = new StringBuilder();


            //if (bNot)
            //    sb.Append(" { \"not\": ");

            //sb.Append("{ \"term\": { \"");
            //sb.Append(Key);
            //sb.Append("\": ");
            //if (m_bIsNumeric)
            //{
            //    sb.Append(Value);
            //}
            //else
            //{
            //    sb.AppendFormat("\"{0}\"", Value);
            //}

            //if (Boost > 0.0f)
            //{
            //    sb.AppendFormat(", \"boost\": {0}", Boost);
            //}

            //sb.Append(" } }");

            //if (bNot)
            //    sb.Append("}");

            //return  sb.ToString();

        }
    }

    public class ESTerms : IESTerm
    {
        public eTermType eType { get; protected set; }
        public List<string> Value { get; protected set; }
        public string Key { get; set; }
        public bool isNot { get; set; }

        bool m_bIsNumeric;

        public ESTerms(bool bIsNumeric)
        {
            eType = eTermType.TERMS;
            Value = new List<string>();
            m_bIsNumeric = bIsNumeric;
        }

        public bool IsEmpty()
        {
            return (string.IsNullOrEmpty(Key) || Value == null || Value.Count == 0) ? true : false;
        }

        public override string ToString()
        {
            if (this.IsEmpty())
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            if (isNot)
                sb.Append("{\"not\":");

            sb.Append("{\"terms\":{\"");
            sb.Append(Key);
            sb.Append("\":[");
            if (m_bIsNumeric)
            {
                sb.Append(Value.Aggregate((current, next) => current + "," + next));

            }
            else
            {
                string[] arrValues = new string[Value.Count];

                for (int i = 0; i < Value.Count; i++)
                {
                    arrValues[i] = string.Format("\"{0}\"", Value[i]);
                }

                sb.Append(arrValues.Aggregate((current, next) => current + "," + next));
            }
            sb.Append("]}}");

            if (isNot)
                sb.Append("}");

            return sb.ToString();

        }
    }

    /// <summary>
    /// Prefix filter part
    /// </summary>
    public class ESPrefix : IESTerm
    {
        #region IESTerm Members

        public eTermType eType
        {
            get
            {
                return eTermType.PREFIX;
            }
        }

        #endregion

        #region Properties

        public string Value
        {
            get;
            set;
        }
        public string Key
        {
            get;
            set;
        }

        public bool isNot
        {
            get;
            set;
        }

        #endregion

        #region Public Methods

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Value) || string.IsNullOrEmpty(Key);
        }

        /// <summary>
        /// Creates a filter object for ES search requests
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.IsEmpty())
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();

            if (isNot)
            {
                sb.Append("{\"not\":");
            }

            sb.Append("{\"prefix\":{");
            sb.AppendFormat("\"{0}\": \"{1}\"", Key, Value);
            sb.Append("}}");

            if (isNot)
            {
                sb.Append("}");
            }

            return sb.ToString();      
        }

        #endregion
    }

    public class ESRange : IESTerm
    {
        public eTermType eType { get; protected set; }
        public List<KeyValuePair<eRangeComp, string>> Value { get; protected set; }
        public string Key { get; set; }
        public bool isNot { get; set; }

        bool m_bIsNumeric;

        public ESRange(bool bIsNumeric)
        {
            eType = eTermType.RANGE;
            Value = new List<KeyValuePair<eRangeComp, string>>();
            m_bIsNumeric = bIsNumeric;
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Key) || Value == null || Value.Count == 0;
        }

        public override string ToString()
        {
            if (this.IsEmpty())
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            if (isNot)
                sb.Append("{\"not\":");

            sb.Append("{\"range\":{\"");
            sb.Append(Key);
            sb.Append("\":{");
            List<string> tempList = new List<string>();

            if (m_bIsNumeric)
            {
                foreach (var item in Value)
                {
                    tempList.Add(string.Format("\"{0}\":{1}", item.Key.ToString().ToLower(), item.Value));
                }
                sb.Append(tempList.Aggregate((current, next) => current + "," + next));

            }
            else
            {
                foreach (var item in Value)
                {
                    tempList.Add(string.Format("\"{0}\":\"{1}\"", item.Key.ToString().ToLower(), item.Value));
                }
                sb.Append(tempList.Aggregate((current, next) => current + "," + next));
            }
            sb.Append("}}}");

            if (isNot)
                sb.Append("}");

            return sb.ToString();

        }
    }

    public class ESWildcard : ESTerm
    {
        public ESWildcard() : base(false)
        {
            this.eType = eTermType.WILDCARD;
        }

        public override string ToString()
        {
            if (this.IsEmpty())
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            if(isNot)
                sb.Append("{\"not\":");

            sb.Append("{\"wildcard\":{");
            sb.AppendFormat("\"{0}\":",Key);
            sb.Append("{");
            sb.AppendFormat("\"value\":\"{0}\"", Value);

            if (Boost > 0.0f)
            {
                sb.AppendFormat(",\"boost\":{0}", Boost);
            }

            sb.Append("}}}");

            if (isNot)
                sb.Append("}");

            return sb.ToString();
        }
    }

    public class ESExists : IESTerm
    {
        public ESExists()
        {
            this.eType = eTermType.EXISTS;
        }

        public eTermType eType { get; protected set; }

        public bool isNot { get; set; }
        public string Value { get; set; }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Value);
        }

        public override string ToString()
        {
            if (this.IsEmpty())
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            if (isNot)
                sb.Append("{\"not\":");

            sb.Append("{\"exists\":{");
             sb.AppendFormat("\"field\":\"{0}\"", Value);

            sb.Append("}}");

            if (isNot)
                sb.Append("}");

            return sb.ToString();
        }

    }

    public enum eTermType
    {
        TERM,
        TERMS,
        RANGE,
        WILDCARD,
        BOOL_QUERY,
        MULTI_MATCH,
        EXISTS,
        MATCH,
        MATCH_ALL,
        PREFIX
    }

    public enum eRangeComp
    {
        GT,
        LT,
        GTE,
        LTE
    }
    public enum eFilterRelation
    {
        AND,
        OR
    }
}
