using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Controllers;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using WebAPI.Models.Renderers;
using WebAPI.Managers.Models;
using Validator.Managers.Scheme;

namespace Reflector
{
    enum SerializeType
    {
        JSON,
        XML
    }

    enum PropertyType
    {
        NATIVE,
        STRING,
        OBJECT,
        ARRAY,
        NUMERIC_ARRAY,
        MAP,
        ENUM,
        BOOLEAN,
        CUSTOM
    }

    class Serializer : Base
    {
        public static string GetJsonSerializerCSFilePath()
        {
            var currentLocation = AppDomain.CurrentDomain.BaseDirectory;
            var solutionDir = Directory.GetParent(currentLocation).Parent.Parent.Parent.Parent;
            var filePath = Path.Combine(solutionDir.FullName, @"..\Core\WebAPI\\KalturaJsonSerializer.cs");
            return filePath;
        }

        public Serializer() : base(GetJsonSerializerCSFilePath(), typeof(IKalturaSerializable))
        {
            types.Remove(typeof(StatusWrapper));
            types.Remove(typeof(KalturaSerializable));
            types.Remove(typeof(KalturaMultilingualString));
        }
        
        protected override void writeHeader()
        {
            file.WriteLine("// NOTICE: This is a generated file, to modify it, edit Program.cs in Reflector project");
            file.WriteLine("using System;");
            file.WriteLine("using System.Linq;");
            file.WriteLine("using System.Web;");
            file.WriteLine("using System.Collections.Generic;");
            file.WriteLine("using WebAPI.Managers.Scheme;");
            file.WriteLine("using WebAPI.Filters;");
            file.WriteLine("using WebAPI.Managers;");
            file.WriteLine("using TVinciShared;");
        }

        protected override void writeBody()
        {
            writeUsing();
            writePartialClasses();
        }

        protected override void writeFooter()
        {
        }

        private bool IsObsolete(PropertyInfo property)
        {
            return (property.GetCustomAttribute<ObsoleteAttribute>() != null);
        }

        private string DeprecationVersion(PropertyInfo property)
        {
            DeprecatedAttribute deprecated = property.GetCustomAttribute<DeprecatedAttribute>();
            if (deprecated != null)
            {
                return deprecated.SinceVersion;
            }
            return null;
        }
        
        private static bool doesPropertyRequiresReadPermission(PropertyInfo arg)
        {
            return arg.CustomAttributes.Any(ca =>
                    ca.AttributeType.IsEquivalentTo(typeof(SchemePropertyAttribute))
                    && ca.NamedArguments.Any(na => na.MemberName.Equals("RequiresPermission") && na.TypedValue.Value.Equals(1)));
        }
        
        private void writeUsing()
        {
            HashSet<string> namespaces = new HashSet<string>();
            foreach (Type type in types)
            {
                if (!namespaces.Contains(type.Namespace))
                {
                    namespaces.Add(type.Namespace);
                    file.WriteLine("using " + type.Namespace + ";");
                }
            }
        }

        private void writePartialClasses()
        {
            HashSet<string> namespaces = new HashSet<string>();
            foreach (Type type in types)
            {
                if (!namespaces.Contains(type.Namespace))
                {
                    namespaces.Add(type.Namespace);
                    writeNamespace(type.Namespace);
                }
            }
        }

        private void writeNamespace(string namespaceName)
        {
            file.WriteLine("");
            file.WriteLine("namespace " + namespaceName);
            file.WriteLine("{");

            foreach (Type type in types)
            {
                if (type.Namespace == namespaceName)
                {
                    writePartialClass(type);
                }
            }

            file.WriteLine("}");
        }

        private void writePartialClass(Type type)
        {
            file.WriteLine("    public partial class " + GetTypeName(type, true));
            file.WriteLine("    {");
            file.WriteLine("        protected override Dictionary<string, string> PropertiesToJson(Version currentVersion, bool omitObsolete)");
            file.WriteLine("        {");
            file.WriteLine("            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);");
            file.WriteLine("            Dictionary<string, string> ret = base.PropertiesToJson(currentVersion, omitObsolete);");
            file.WriteLine("            string propertyValue;");
            writeSerializeTypeProperties(type, SerializeType.JSON);
            file.WriteLine("            return ret;");
            file.WriteLine("        }");
            file.WriteLine("        ");
            file.WriteLine("        protected override Dictionary<string, string> PropertiesToXml(Version currentVersion, bool omitObsolete)");
            file.WriteLine("        {");
            file.WriteLine("            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);");
            file.WriteLine("            Dictionary<string, string> ret = base.PropertiesToXml(currentVersion, omitObsolete);");
            file.WriteLine("            string propertyValue;");
            writeSerializeTypeProperties(type, SerializeType.XML);
            file.WriteLine("            return ret;");
            file.WriteLine("        }");
            file.WriteLine("    }");
        }

        private void writeSerializeTypeProperties(Type type, SerializeType serializeType)
        {
            var name = type.Name;

            List<PropertyInfo> properties = type.GetProperties().ToList();
            properties.Sort(new PropertyInfoComparer());

            if (properties.Any(doesPropertyRequiresReadPermission))
            {
                file.WriteLine("            var requestType = HttpContext.Current.Items.ContainsKey(RequestContext.REQUEST_TYPE) ? (RequestType?)HttpContext.Current.Items[RequestContext.REQUEST_TYPE] : null;");
            }

            file.Write(Environment.NewLine);

            foreach (PropertyInfo property in properties)
            {
                if (property.DeclaringType != type)
                    continue;

                DataMemberAttribute dataMember = property.GetCustomAttribute<DataMemberAttribute>(false);
                if (dataMember == null) { continue; }

                string propertyName = property.Name;
                List<string> conditions = new List<string>();

                if (type.BaseType != null && type.BaseType.GetProperty(propertyName) != null)
                {
                    conditions.Add("!ret.ContainsKey(\"" + dataMember.Name + "\")");
                }

                OnlyNewStandardAttribute onlyNewStandard = property.GetCustomAttribute<OnlyNewStandardAttribute>(false);
                if (onlyNewStandard != null)
                {
                    conditions.Add("!isOldVersion");
                }

                if (IsObsolete(property))
                {
                    conditions.Add("!omitObsolete");
                }

                string deprecationVersion = DeprecationVersion(property);
                if (deprecationVersion != null)
                {
                    conditions.Add("!DeprecatedAttribute.IsDeprecated(\"" + deprecationVersion + "\", currentVersion)");
                }

                PropertyType propertyType = PropertyType.NATIVE;
                string tab = "";
                string realType = property.PropertyType.Name;

                if (typeof(IKalturaSerializable).IsAssignableFrom(property.PropertyType))
                {
                    propertyType = PropertyType.OBJECT;
                    if ((serializeType == SerializeType.JSON && property.PropertyType.GetMethod("ToCustomJson") != null) || (serializeType == SerializeType.XML && property.PropertyType.GetMethod("ToCustomXml") != null))
                    {
                        propertyType = PropertyType.CUSTOM;
                    }
                }
                else if (property.PropertyType == typeof(string))
                {
                    propertyType = PropertyType.STRING;
                }
                else if (property.PropertyType == typeof(bool))
                {
                    propertyType = PropertyType.BOOLEAN;
                }
                else if (property.PropertyType.IsGenericType)
                {
                    if (property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        Type InnerType = property.PropertyType.GetGenericArguments()[0];
                        realType = InnerType.Name;
                        if (InnerType.IsEnum)
                        {
                            propertyType = PropertyType.ENUM;
                        }
                        else if (InnerType == typeof(bool))
                        {
                            propertyType = PropertyType.BOOLEAN;
                        }
                        else
                        {
                            propertyType = PropertyType.NATIVE;
                        }
                        conditions.Add(propertyName + ".HasValue");
                    }
                    else if (property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        propertyType = typeof(IKalturaOTTObject).IsAssignableFrom(property.PropertyType.GetGenericArguments()[0]) ? PropertyType.ARRAY : PropertyType.NUMERIC_ARRAY;
                        realType = property.PropertyType.GetGenericArguments()[0].Name;
                        switch (realType)
                        {
                            case "Int64":
                                realType = "long";
                                break;

                            case "Int32":
                                realType = "int";
                                break;
                        }
                        conditions.Add(propertyName + " != null");
                    }
                    else if (property.PropertyType.GetGenericTypeDefinition() == typeof(SerializableDictionary<,>))
                    {
                        propertyType = PropertyType.MAP;
                        realType = property.PropertyType.GetGenericArguments()[1].Name;
                        conditions.Add(propertyName + " != null");
                    }
                }
                else if (property.PropertyType.IsEnum)
                {
                    propertyType = PropertyType.ENUM;
                }

                if (propertyType == PropertyType.STRING || propertyType == PropertyType.OBJECT)
                {
                    conditions.Add(propertyName + " != null");
                }

                if (doesPropertyRequiresReadPermission(property))
                {
                    conditions.Add(string.Format("(requestType != RequestType.READ || RolesManager.IsPropertyPermitted(\"{0}\", \"{1}\", requestType.Value))", type.Name, propertyName));
                }

                if (conditions.Count > 0)
                {
                    tab = "    ";
                    file.WriteLine("            if(" + String.Join(" && ", conditions) + ")");
                    file.WriteLine("            {");
                }

                string value = "propertyValue";
                switch (propertyType)
                {
                    case PropertyType.NATIVE:
                        value = propertyName;
                        break;

                    case PropertyType.ENUM:
                        if (serializeType == SerializeType.JSON)
                        {
                            value = "\"\\\"\" + Enum.GetName(typeof(" + realType + "), " + propertyName + ") + \"\\\"\"";
                        }
                        else
                        {
                            value = "\"\" + Enum.GetName(typeof(" + realType + "), " + propertyName + ") + \"\"";
                        }
                        break;

                    case PropertyType.BOOLEAN:
                        value = propertyName + ".ToString().ToLower()";
                        break;

                    case PropertyType.STRING:
                        if (serializeType == SerializeType.JSON)
                        {
                            value = "\"\\\"\" + EscapeJson(" + propertyName + ") + \"\\\"\"";
                        }
                        else
                        {
                            value = "EscapeXml(" + propertyName + ")";
                        }
                        break;

                    case PropertyType.OBJECT:
                        if (serializeType == SerializeType.JSON)
                        {
                            file.WriteLine(tab + "            propertyValue = " + propertyName + ".ToJson(currentVersion, omitObsolete);");
                        }
                        else
                        {
                            file.WriteLine(tab + "            propertyValue = " + propertyName + ".ToXml(currentVersion, omitObsolete);");
                        }
                        break;

                    case PropertyType.ARRAY:
                        if (serializeType == SerializeType.JSON)
                        {
                            file.WriteLine(tab + "            propertyValue = \"[\" + String.Join(\", \", " + propertyName + ".Select(item => item.ToJson(currentVersion, omitObsolete))) + \"]\";");
                        }
                        else
                        {
                            file.WriteLine(tab + "            propertyValue = " + propertyName + ".Count > 0 ? \"<item>\" + String.Join(\"</item><item>\", " + propertyName + ".Select(item => item.ToXml(currentVersion, omitObsolete))) + \"</item>\": \"\";");
                        }
                        break;

                    case PropertyType.MAP:
                        if (serializeType == SerializeType.JSON)
                        {
                            file.WriteLine(tab + "            propertyValue = \"{\" + String.Join(\", \", " + propertyName + ".Select(pair => \"\\\"\" + pair.Key + \"\\\": \" + pair.Value.ToJson(currentVersion, omitObsolete))) + \"}\";");
                        }
                        else
                        {
                            file.WriteLine(tab + "            propertyValue = " + propertyName + ".Count > 0 ? \"<item>\" + String.Join(\"</item><item>\", " + propertyName + ".Select(pair => \"<itemKey>\" + pair.Key + \"</itemKey>\" + pair.Value.ToXml(currentVersion, omitObsolete))) + \"</item>\" : \"\";");
                        }
                        break;

                    case PropertyType.NUMERIC_ARRAY:
                        if (serializeType == SerializeType.JSON)
                        {
                            file.WriteLine(tab + "            propertyValue = \"[\" + String.Join(\", \", " + propertyName + ".Select(item => item.ToString())) + \"]\";");
                        }
                        else
                        {
                            file.WriteLine(tab + "            propertyValue = " + propertyName + ".Count > 0 ? \"<item>\" + String.Join(\"</item><item>\", " + propertyName + ".Select(item => item.ToString())) + \"</item>\" : \"\";");
                        }
                        break;
                }

                if (serializeType == SerializeType.JSON)
                {
                    if (propertyType == PropertyType.CUSTOM)
                    {
                        file.WriteLine(tab + "            propertyValue = " + propertyName + ".ToCustomJson(currentVersion, omitObsolete, \"" + dataMember.Name + "\");");
                        file.WriteLine(tab + "            if(propertyValue != null)");
                        file.WriteLine(tab + "            {");
                        file.WriteLine(tab + "                ret.Add(\"" + dataMember.Name + "\", propertyValue);");
                        file.WriteLine(tab + "            }");
                    }
                    else
                    {
                        file.WriteLine(tab + "            ret.Add(\"" + dataMember.Name + "\", \"\\\"" + dataMember.Name + "\\\": \" + " + value + ");");
                    }
                }
                else
                {
                    if (propertyType == PropertyType.CUSTOM)
                    {
                        file.WriteLine(tab + "            ret.Add(\"" + dataMember.Name + "\", " + propertyName + ".ToCustomXml(currentVersion, omitObsolete, \"" + dataMember.Name + "\"));");
                    }
                    else
                    {
                        file.WriteLine(tab + "            ret.Add(\"" + dataMember.Name + "\", \"<" + dataMember.Name + ">\" + " + value + " + \"</" + dataMember.Name + ">\");");
                    }
                }

                IEnumerable<OldStandardPropertyAttribute> oldStandardProperties = property.GetCustomAttributes<OldStandardPropertyAttribute>(false);
                if (oldStandardProperties != null && oldStandardProperties.Count() > 0)
                {
                    foreach (OldStandardPropertyAttribute oldStandardProperty in oldStandardProperties)
                    {
                        if (oldStandardProperty.sinceVersion != null)
                        {
                            file.WriteLine(tab + "            if (currentVersion == null || isOldVersion || currentVersion.CompareTo(new Version(\"" + oldStandardProperty.sinceVersion + "\")) > 0)");
                        }
                        else
                        {
                            file.WriteLine(tab + "            if (currentVersion == null || isOldVersion)");
                        }
                        file.WriteLine(tab + "            {");
                        if (serializeType == SerializeType.JSON)
                        {
                            if (propertyType == PropertyType.CUSTOM)
                            {
                                file.WriteLine(tab + "            propertyValue = " + propertyName + ".ToCustomJson(currentVersion, omitObsolete, \"" + oldStandardProperty.oldName + "\");");
                                file.WriteLine(tab + "            if(propertyValue != null)");
                                file.WriteLine(tab + "            {");
                                file.WriteLine(tab + "                ret.Add(\"" + oldStandardProperty.oldName + "\", propertyValue);");
                                file.WriteLine(tab + "            }");
                            }
                            else
                            {
                                file.WriteLine(tab + "                ret.Add(\"" + oldStandardProperty.oldName + "\", \"\\\"" + oldStandardProperty.oldName + "\\\": \" + " + value + ");");
                            }
                        }
                        else
                        {
                            if (propertyType == PropertyType.CUSTOM)
                            {
                                file.WriteLine(tab + "            ret.Add(\"" + oldStandardProperty.oldName + "\", " + propertyName + ".ToCustomXml(currentVersion, omitObsolete, \"" + oldStandardProperty.oldName + "\"));");
                            }
                            else
                            {
                                file.WriteLine(tab + "            ret.Add(\"" + oldStandardProperty.oldName + "\", \"<" + oldStandardProperty.oldName + ">\" + " + value + " + \"</" + oldStandardProperty.oldName + ">\");");
                            }
                        }
                        file.WriteLine(tab + "            }");
                    }
                }

                if (tab.Length > 0)
                {
                    file.WriteLine("            }");
                }
            }
        }
    }
}
