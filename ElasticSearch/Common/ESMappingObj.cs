using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearch.Common
{
    public class ESMappingObj
    {
        protected List<IMappingProperty> m_lProperties { get; set; }
        protected ESRouting m_oRouting { get; set; }
        protected string m_sName;
        
        public ESMappingObj(string sName) {
            m_sName = sName;
            m_lProperties = new List<IMappingProperty>();
        }

        public void SetRouting(ESRouting routing)
        {
            if (routing != null)
                m_oRouting = routing;
        }

        public void AddProperty(IMappingProperty property)
        {
            if (property != null)
                m_lProperties.Add(property);
        }

        public void AddProperties(List<IMappingProperty> lProperties)
        {
            if (lProperties != null && lProperties.Count > 0)
                m_lProperties.AddRange(lProperties);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.AppendFormat("\"{0}\":",m_sName);
            sb.Append("{");

            if (m_oRouting != null)
            {
                sb.AppendFormat("{0},", m_oRouting.ToString());
            }

            sb.Append("\"properties\": {");
            if (m_lProperties != null && m_lProperties.Count > 0)
            {
                sb.Append(string.Join(",", m_lProperties));
            }

            sb.Append("}");

            sb.Append("}}");


            return sb.ToString();
        }
    }

    public interface IMappingProperty
    {
        eESFieldType type { get; }
        string name { get; }
    }

    public class MultiFieldMappingPropertyV1 : IMappingProperty
    {
        public eESFieldType type { get; protected set; }
        public string null_value { get; set; }
        public string analyzer { get; set; }
        public string name { get; set; }
        public bool analyzed { get; set; }
        public bool store { get; set; }

        public List<IMappingProperty> fields { get; protected set; }

        public MultiFieldMappingPropertyV1()
        {
            type = eESFieldType.MULTI_FIELD;
            null_value = string.Empty;
            analyzer = string.Empty;
            analyzed = false;
            store = true;
            name = string.Empty;
            fields = new List<IMappingProperty>();
        }

        public void AddField(IMappingProperty property)
        {
            if(property != null) 
                fields.Add(property);
        }
        public void AddFields(List<IMappingProperty> properties)
        {
            if (properties != null && properties.Count > 0)
                fields.AddRange(properties);
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("\"{0}\":", name);
            sb.Append("{");

            sb.AppendFormat("\"type\": \"{0}\"", type.ToString().ToLower());

            if (fields.Count > 0)
            {
                sb.Append(",\"fields\": {");

                sb.Append(string.Join(",", fields));
                sb.Append("}");
            }

            sb.Append("}");

            return sb.ToString();
        }
    }

    public class InnerMappingPropertyV1 : IMappingProperty
    {
        public eESFieldType type { get; protected set; }
        public string name { get; set; }
        public List<IMappingProperty> properties { get; protected set; }
       
        public bool isNested
        {
            get
            {
                return (type == eESFieldType.NESTED) ? true : false;
            }
            set
            {
                if (value == true)
                {
                    type = eESFieldType.NESTED;
                }
                else
                {
                    type = eESFieldType.INNER;
                }
            }
        }

        public InnerMappingPropertyV1(bool isNested=false)
        {
            this.isNested = isNested;
            name = string.Empty;
            properties = new List<IMappingProperty>();
        }

        public void AddProperty(IMappingProperty property)
        {
            if (property != null)
                properties.Add(property);
        }
        public void AddProperties(List<IMappingProperty> properties)
        {
            if (properties != null && properties.Count > 0)
                properties.AddRange(properties);
        }
        public override string ToString()
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("\"{0}\":", name);
            sb.Append("{");

            if(isNested)
                sb.AppendFormat("\"type\": \"{0}\",", type.ToString().ToLower());

            sb.Append("\"properties\": {");

            if (properties != null)
            {
                sb.Append(string.Join(",", properties));
            }

            sb.Append("}");


            sb.Append("}");

            return sb.ToString();
        }

    }

    public class InnerMappingPropertyV2 : IMappingProperty
    {
        public eESFieldType type
        {
            get;
            protected set;
        }
        public string name
        {
            get;
            set;
        }
        public List<IMappingProperty> properties
        {
            get;
            protected set;
        }

        public bool isNested
        {
            get
            {
                return (type == eESFieldType.NESTED) ? true : false;
            }
            set
            {
                if (value == true)
                {
                    type = eESFieldType.NESTED;
                }
                else
                {
                    type = eESFieldType.INNER;
                }
            }
        }

        public InnerMappingPropertyV2(bool isNested = false)
        {
            this.isNested = isNested;
            name = string.Empty;
            properties = new List<IMappingProperty>();
        }

        public void AddProperty(IMappingProperty property)
        {
            if (property != null)
                properties.Add(property);
        }
        public void AddProperties(List<IMappingProperty> properties)
        {
            if (properties != null && properties.Count > 0)
                properties.AddRange(properties);
        }
        public override string ToString()
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("\"{0}\":", name);
            sb.Append("{");

            if (isNested)
                sb.AppendFormat("\"type\": \"{0}\",", type.ToString().ToLower());

            sb.Append("\"properties\": {");

            if (properties != null)
            {
                sb.Append(string.Join(",", properties));
            }

            sb.Append("}");


            sb.Append("}");

            return sb.ToString();
        }

    }

    public class BasicMappingPropertyV1 : IMappingProperty
    {
        public eESFieldType type { get; set; }
        public string null_value { get; set; }
        public string search_analyzer { get; set; }
        public string index_analyzer { get; set; }
        public string name { get; set; }
        public bool analyzed { get; set; }
        public bool store { get; set; }

        public BasicMappingPropertyV1()
        {
            type = eESFieldType.INTEGER;
            null_value = string.Empty;
            analyzed = false;
            search_analyzer = string.Empty;
            index_analyzer = string.Empty;
            store = true;
            name = string.Empty;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (string.IsNullOrEmpty(name))
                return string.Empty;

            sb.AppendFormat("\"{0}\":", name);
            sb.Append("{");
            sb.AppendFormat("\"type\": \"{0}\"", type.ToString().ToLower());

            if (type == eESFieldType.STRING)
            {
                sb.AppendFormat(",\"null_value\": \"{0}\"", null_value);
            }

            if (analyzed)
            {
                if(!string.IsNullOrEmpty(search_analyzer))
                    sb.AppendFormat(",\"search_analyzer\": \"{0}\"", search_analyzer);

                if (!string.IsNullOrEmpty(index_analyzer))
                    sb.AppendFormat(",\"index_analyzer\": \"{0}\"", index_analyzer);
            }
            else
            {
                sb.AppendFormat(",\"index\": \"not_analyzed\"");
            }

            sb.Append("}");

            return sb.ToString();
        }
    }

    public class BasicMappingPropertyV2 : IMappingProperty
    {
        public eESFieldType type
        {
            get;
            set;
        }
        public string null_value
        {
            get;
            set;
        }
        public string search_analyzer
        {
            get;
            set;
        }
        public string analyzer
        {
            get;
            set;
        }
        public string name
        {
            get;
            set;
        }
        public bool store
        {
            get;
            set;
        }
        public eMappingIndex index
        {
            get;
            set;
        }
        public string format
        {
            get;
            set;
        }

        public BasicMappingPropertyV2()
        {
            type = eESFieldType.INTEGER;
            null_value = string.Empty;
            index = eMappingIndex.not_analyzed;
            search_analyzer = string.Empty;
            analyzer = string.Empty;
            store = true;
            name = string.Empty;
            format = string.Empty;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (string.IsNullOrEmpty(name))
                return string.Empty;

            sb.AppendFormat("\"{0}\":", name);
            sb.Append("{");
            sb.AppendFormat("\"type\": \"{0}\"", type.ToString().ToLower());

            if (type == eESFieldType.STRING)
            {
                sb.AppendFormat(",\"null_value\": \"{0}\"", null_value);
            }

            if (!string.IsNullOrEmpty(format))
            {
                sb.AppendFormat(",\"format\": \"{0}\"", format);
            }

            sb.AppendFormat(",\"index\": \"{0}\"", index);

            if (index == eMappingIndex.analyzed)
            {
                if (!string.IsNullOrEmpty(search_analyzer))
                    sb.AppendFormat(",\"search_analyzer\": \"{0}\"", search_analyzer);

                if (!string.IsNullOrEmpty(analyzer))
                    sb.AppendFormat(",\"analyzer\": \"{0}\"", analyzer);
            }

            sb.Append("}");

            return sb.ToString();
        }
    }

    public class FieldsMappingPropertyV2 : BasicMappingPropertyV2
    {
        public List<IMappingProperty> fields
        {
            get;
            protected set;
        }

        public FieldsMappingPropertyV2()
        {
            type = eESFieldType.STRING;
            null_value = string.Empty;
            analyzer = string.Empty;
            index = eMappingIndex.no;
            store = true;
            name = string.Empty;
            fields = new List<IMappingProperty>();
        }

        public void AddField(IMappingProperty property)
        {
            if (property != null)
                fields.Add(property);
        }
        public void AddFields(List<IMappingProperty> properties)
        {
            if (properties != null && properties.Count > 0)
                fields.AddRange(properties);
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("\"{0}\":", name);
            sb.Append("{");

            sb.AppendFormat("\"type\": \"{0}\"", type.ToString().ToLower());

            if (type == eESFieldType.STRING)
            {
                sb.AppendFormat(",\"null_value\": \"{0}\"", null_value);
            }

            if (!string.IsNullOrEmpty(format))
            {
                sb.AppendFormat(",\"format\": \"{0}\"", format);
            }

            sb.AppendFormat(",\"index\": \"{0}\"", index);

            if (index == eMappingIndex.analyzed)
            {
                if (!string.IsNullOrEmpty(search_analyzer))
                    sb.AppendFormat(",\"search_analyzer\": \"{0}\"", search_analyzer);

                if (!string.IsNullOrEmpty(analyzer))
                    sb.AppendFormat(",\"analyzer\": \"{0}\"", analyzer);
            }

            if (fields.Count > 0)
            {
                sb.Append(",\"fields\": {");

                sb.Append(string.Join(",", fields));
                sb.Append("}");
            }

            sb.Append("}");

            return sb.ToString();
        }
    }

    public enum eESFieldType
    {
        INTEGER,
        LONG,
        DOUBLE,
        STRING,
        BOOLEAN,
        NESTED,
        INNER,
        MULTI_FIELD,
        DATE
    }

    public enum eMappingIndex
    {
        no,
        not_analyzed,
        analyzed
    }
}
