using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Managers.Models;
using System.Runtime.CompilerServices;

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
        private static readonly HashSet<string> RetrievedPropertiesToSkip = new HashSet<string> {"TotalCount", "objectType", "Metas", "Tags"};
        
        public static string GetJsonSerializerCSFilePath()
        {
            var currentLocation = AppDomain.CurrentDomain.BaseDirectory;
            var solutionDir = Directory.GetParent(currentLocation).Parent.Parent.Parent.Parent.Parent;
            var filePath = Path.Combine(solutionDir.FullName, "Core", "WebAPI", "Reflection", "KalturaJsonSerializer.cs");
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
            file.WriteLine("// disable compiler warning due to generation of empty usages ot unused vars");
            file.WriteLine("// ReSharper disable CheckNamespace");
            file.WriteLine("// ReSharper disable NotAccessedVariable");
            file.WriteLine("// ReSharper disable UnusedVariable");
            file.WriteLine("// ReSharper disable RedundantAssignment");
            file.WriteLine("// ReSharper disable PossibleMultipleEnumeration");
            file.WriteLine("#pragma warning disable 168");
            file.WriteLine("#pragma warning disable 219");
            file.WriteLine("#pragma warning disable 612");
            file.WriteLine("using System;");
            file.WriteLine("using System.Collections.Generic;");
            file.WriteLine("using System.Linq;");
            file.WriteLine("using System.Net;");
            file.WriteLine("using System.Text;");
            file.WriteLine("using System.Web;");
            file.WriteLine("using KalturaRequestContext;");
            file.WriteLine("using TVinciShared;");
            file.WriteLine("using WebAPI.Managers.Scheme;");
            file.WriteLine("using WebAPI.Filters;");
            file.WriteLine("using WebAPI.Managers;");
            file.WriteLine("using WebAPI.Reflection;");
            file.WriteLine("using WebAPI.Utils;");
            file.WriteLine("using WebAPI.ModelsValidators;");
            file.WriteLine("using WebAPI.ObjectsConvertor.Extensions;");
        }

        protected override void writeBody()
        {
            writeUsing();
            WriteNamespaces();
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

        private void WriteNamespaces()
        {
            var namespaces = new List<Type>(types).Concat(enums).GroupBy(x => x.Namespace);
            foreach (var namespaceWithTypes in namespaces)
            {
                WriteNamespace(namespaceWithTypes.Key, namespaceWithTypes.AsEnumerable());
            }
        }

        private void WriteNamespace(string namespaceName, IEnumerable<Type> namespaceTypes)
        {
            file.WriteLine("");
            file.WriteLine("namespace " + namespaceName);
            file.WriteLine("{");

            foreach (var type in namespaceTypes)
            {
                if (type.IsClass)
                {
                    WritePartialClass(type);
                }
                else if (type.IsEnum)
                {
                    WriteEnumExtensions(type);
                }
            }

            file.WriteLine("}");
        }

        private void WritePartialClass(Type type)
        {
            string typeName = GetTypeName(type, true);
            file.WriteLine("    public partial class " + typeName);
            file.WriteLine("    {");
            file.WriteLine("        protected override Dictionary<string, string> PropertiesToJson(Version currentVersion, bool omitObsolete, bool responseProfile = false)");
            file.WriteLine("        {");
            file.WriteLine("            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);");
            file.WriteLine("            Dictionary<string, string> ret = base.PropertiesToJson(currentVersion, omitObsolete, responseProfile);");
            file.WriteLine("            string propertyValue = null;");
            if (typeName != "KalturaListResponse")
            {
                file.WriteLine("            IEnumerable<string> retrievedProperties = null;");
                file.WriteLine("            if (responseProfile)");
                file.WriteLine("            {");
                file.WriteLine("                retrievedProperties = Utils.Utils.GetOnDemandResponseProfileProperties();");
                file.WriteLine("            }");
            }
            writeSerializeTypeProperties(type, SerializeType.JSON);
            file.WriteLine("            return ret;");
            file.WriteLine("        }");
            
            file.WriteLine("        ");
            file.WriteLine("        public override ISet<string> AppendPropertiesAsJson(StringBuilder stringBuilder, Version currentVersion, bool omitObsolete, bool responseProfile = false)");
            file.WriteLine("        {");
            file.WriteLine("            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);");
            file.WriteLine("            var keys = base.AppendPropertiesAsJson(stringBuilder, currentVersion, omitObsolete, responseProfile);");
            
            if (typeName != "KalturaListResponse")
            {
                file.WriteLine("            IEnumerable<string> retrievedProperties = null;");
                file.WriteLine("            if (responseProfile)");
                file.WriteLine("            {");
                file.WriteLine("                retrievedProperties = Utils.Utils.GetOnDemandResponseProfileProperties();");
                file.WriteLine("            }");
            }
            WriteSerializeTypeProperties(type, SerializeType.JSON);
            file.WriteLine("            return keys;");
            file.WriteLine("        }");

            file.WriteLine("        ");
            file.WriteLine("        protected override Dictionary<string, string> PropertiesToXml(Version currentVersion, bool omitObsolete, bool responseProfile = false)");
            file.WriteLine("        {");
            file.WriteLine("            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);");
            file.WriteLine("            Dictionary<string, string> ret = base.PropertiesToXml(currentVersion, omitObsolete, responseProfile);");
            file.WriteLine("            string propertyValue;");
            if (typeName != "KalturaListResponse")
            {
                file.WriteLine("            IEnumerable<string> retrievedProperties = null;");
                file.WriteLine("            if (responseProfile)");
                file.WriteLine("            {");
                file.WriteLine("                retrievedProperties = Utils.Utils.GetOnDemandResponseProfileProperties();");
                file.WriteLine("            }");
            }
            writeSerializeTypeProperties(type, SerializeType.XML);
            file.WriteLine("            return ret;");
            file.WriteLine("        }");
            
            file.WriteLine("        ");
            file.WriteLine("        public override ISet<string> AppendPropertiesAsXml(StringBuilder stringBuilder, Version currentVersion, bool omitObsolete, bool responseProfile = false)");
            file.WriteLine("        {");
            file.WriteLine("            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);");
            file.WriteLine("            var keys = base.AppendPropertiesAsXml(stringBuilder, currentVersion, omitObsolete, responseProfile);");
            
            if (typeName != "KalturaListResponse")
            {
                file.WriteLine("            IEnumerable<string> retrievedProperties = null;");
                file.WriteLine("            if (responseProfile)");
                file.WriteLine("            {");
                file.WriteLine("                retrievedProperties = Utils.Utils.GetOnDemandResponseProfileProperties();");
                file.WriteLine("            }");
            }
            WriteSerializeTypeProperties(type, SerializeType.XML);
            file.WriteLine("            return keys;");
            file.WriteLine("        }");

            file.WriteLine("    }");
            file.WriteLine();
        }

        private void WriteEnumExtensions(Type type)
        {
            var typeName = GetTypeName(type, true);
            file.WriteLine($"    public static class {typeName}Extensions");
            file.WriteLine("    {");
            file.WriteLine($"        public static string ToSerializedString(this {typeName} value)");
            file.WriteLine("        {");
            file.WriteLine("            switch (value)");
            file.WriteLine("            {");
            foreach (var enumValue in type.GetEnumValues())
            {
                file.WriteLine($"                case {typeName}.{enumValue}:");
                file.WriteLine($"                    return \"{Enum.GetName(type, enumValue)}\";");
            }

            file.WriteLine("                default:");
            file.WriteLine("                    return string.Empty;");
            file.WriteLine("            }");
            file.WriteLine("        }");
            file.WriteLine("    }");
            file.WriteLine();
        }

        private void writeSerializeTypeProperties(Type type, SerializeType serializeType)
        {
            var name = type.Name;

            List<PropertyInfo> properties = type.GetProperties().ToList();
            properties.Sort(new PropertyInfoComparer());

            if (properties.Any(doesPropertyRequiresReadPermission))
            {
                file.WriteLine("            var requestType = HttpContext.Current.Items.ContainsKey(RequestContextConstants.REQUEST_TYPE) ? (RequestType?)HttpContext.Current.Items[RequestContextConstants.REQUEST_TYPE] : null;");
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
                    if (onlyNewStandard.SinceVersion == null)
                    {
                        conditions.Add("!isOldVersion");
                    }
                    else
                    {
                        conditions.Add("OnlyNewStandardAttribute.IsNew(\"" + onlyNewStandard.SinceVersion + "\", currentVersion)");
                    }
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
                    if ((serializeType == SerializeType.JSON && property.PropertyType.GetMethod("ToCustomJson") != null) || 
                        (serializeType == SerializeType.XML && property.PropertyType.GetMethod("ToCustomXml") != null))
                    {
                        propertyType = PropertyType.CUSTOM;
                    }
                    else
                    {
                        var extensionMethods = GetExtensionMethods(property.PropertyType.Assembly, property.PropertyType);
                        if ((serializeType == SerializeType.JSON && extensionMethods.Any(x => x.Name == "ToCustomJson")) ||
                            (serializeType == SerializeType.XML && extensionMethods.Any(x => x.Name == "ToCustomXml")))
                        {
                            propertyType = PropertyType.CUSTOM;
                        }
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

                // responseProfile
                if (!RetrievedPropertiesToSkip.Contains(propertyName) && property.DeclaringType.BaseType.Name != "KalturaListResponse")
                {
                    conditions.Add("(retrievedProperties == null || retrievedProperties.Contains(\"" + dataMember.Name + "\"))");
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
                            if (property.DeclaringType.BaseType.Name == "KalturaListResponse")
                            {
                                file.WriteLine(tab + "            propertyValue = \"[\" + String.Join(\", \", " + propertyName + ".Select(item => item.ToJson(currentVersion, omitObsolete, true))) + \"]\";");
                            }
                            else
                            {
                                file.WriteLine(tab + "            propertyValue = \"[\" + String.Join(\", \", " + propertyName + ".Select(item => item.ToJson(currentVersion, omitObsolete))) + \"]\";");

                            }
                        }
                        else
                        {
                            if (property.DeclaringType.BaseType.Name == "KalturaListResponse")
                            {
                                file.WriteLine(tab + "            propertyValue = " + propertyName + ".Count > 0 ? \"<item>\" + String.Join(\"</item><item>\", " + propertyName + ".Select(item => item.ToXml(currentVersion, omitObsolete, true))) + \"</item>\": \"\";");
                            }
                            else
                            {
                                file.WriteLine(tab + "            propertyValue = " + propertyName + ".Count > 0 ? \"<item>\" + String.Join(\"</item><item>\", " + propertyName + ".Select(item => item.ToXml(currentVersion, omitObsolete))) + \"</item>\": \"\";");
                            }
                        }
                        break;

                    case PropertyType.MAP: //TODO: irena - case sensitivity 
                        if (serializeType == SerializeType.JSON)
                        {
                            if (propertyName == "Metas" || propertyName == "Tags")
                            {
                                // filter metas / tags from profile 
                                file.WriteLine(tab + "            propertyValue = null;");
                                file.WriteLine(tab + "            if (retrievedProperties != null && !retrievedProperties.Contains(\"" + dataMember.Name + "\"))");
                                file.WriteLine(tab + "            {");
                                file.WriteLine(tab + "                  var valuesToFilter = retrievedProperties.Where(rp => rp.StartsWith(\"" + dataMember.Name + ".\")).Select(p => p.Replace(\"" + dataMember.Name + ".\", \"\"));");
                                file.WriteLine(tab + "                  var filteredValues = " + propertyName + ".Where(pair => valuesToFilter.Contains(pair.Key));");
                                file.WriteLine(tab + "                  if (valuesToFilter.Any())");
                                file.WriteLine(tab + "                  {");
                                file.WriteLine(tab + "                      propertyValue = \"{\" + String.Join(\", \", filteredValues.Select(pair => \"\\\"\" + pair.Key + \"\\\": \" + pair.Value.ToJson(currentVersion, omitObsolete))) + \"}\";");
                                file.WriteLine(tab + "                  }");
                                file.WriteLine(tab + "            }");
                                file.WriteLine(tab + "            else");
                                file.WriteLine(tab + "            {");
                                file.WriteLine(tab + "                  propertyValue = \"{\" + String.Join(\", \", " + propertyName + ".Select(pair => \"\\\"\" + pair.Key + \"\\\": \" + pair.Value.ToJson(currentVersion, omitObsolete))) + \"}\";");
                                file.WriteLine(tab + "            }");
                            }
                            else
                            {
                                file.WriteLine(tab + "            propertyValue = \"{\" + String.Join(\", \", " + propertyName + ".Select(pair => \"\\\"\" + pair.Key + \"\\\": \" + pair.Value.ToJson(currentVersion, omitObsolete))) + \"}\";");

                            }
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
                        if (propertyName == "Metas" || propertyName == "Tags")
                        {
                            file.WriteLine(tab + "            if(propertyValue != null)");
                            file.WriteLine(tab + "            {");
                            file.WriteLine(tab + "               ret.Add(\"" + dataMember.Name + "\", \"\\\"" + dataMember.Name + "\\\": \" + " + value + ");");
                            file.WriteLine(tab + "            }");
                        }
                        else
                        {
                            file.WriteLine(tab + "            ret.Add(\"" + dataMember.Name + "\", \"\\\"" + dataMember.Name + "\\\": \" + " + value + ");");
                        }
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

        private void WriteSerializeTypeProperties(Type type, SerializeType serializeType)
        {
            var properties = type.GetProperties().ToList();
            properties.Sort(new PropertyInfoComparer());

            if (properties.Any(doesPropertyRequiresReadPermission))
            {
                file.WriteLine("            var requestType = HttpContext.Current.Items.ContainsKey(RequestContextConstants.REQUEST_TYPE) ? (RequestType?)HttpContext.Current.Items[RequestContextConstants.REQUEST_TYPE] : null;");
            }

            file.Write(Environment.NewLine);

            foreach (var property in properties)
            {
                if (property.DeclaringType != type)
                {
                    continue;
                }

                var dataMember = property.GetCustomAttribute<DataMemberAttribute>(false);
                if (dataMember == null)
                {
                    continue;
                }

                var propertyType = GetPropertyType(property, serializeType);
                var conditions = GetConditions(type, property, propertyType, dataMember.Name);

                var tab = string.Empty;
                if (conditions.Count > 0)
                {
                    tab = "    ";
                    file.WriteLine($"{tab}        if({string.Join(" && ", conditions)})");
                    file.WriteLine($"{tab}        {{");
                }

                if (serializeType == SerializeType.JSON)
                {
                    if (propertyType == PropertyType.ARRAY
                        || propertyType == PropertyType.NUMERIC_ARRAY
                        || propertyType == PropertyType.MAP)
                    {
                        file.WriteLine($"{tab}            var isFirstItem = true;");
                    }

                    WriteJsonDataMember($"{tab}            ", propertyType, property, dataMember.Name);
                }
                else
                {
                    WriteXmlDataMember($"{tab}            ", propertyType, property, dataMember.Name);
                }

                IEnumerable<OldStandardPropertyAttribute> oldStandardProperties = property
                    .GetCustomAttributes<OldStandardPropertyAttribute>(false)
                    .ToArray();
                foreach (OldStandardPropertyAttribute oldStandardProperty in oldStandardProperties)
                {
                    file.WriteLine();
                    if (oldStandardProperty.sinceVersion != null)
                    {
                        file.WriteLine($"{tab}            if (currentVersion == null || isOldVersion || currentVersion.CompareTo(new Version(\"{oldStandardProperty.sinceVersion}\")) > 0)");
                    }
                    else
                    {
                        file.WriteLine($"{tab}            if (currentVersion == null || isOldVersion)");
                    }

                    file.WriteLine($"{tab}            {{");
                    if (serializeType == SerializeType.JSON)
                    {
                        WriteJsonDataMember($"{tab}                ", propertyType, property, oldStandardProperty.oldName);
                    }
                    else
                    {
                        WriteXmlDataMember($"{tab}                ", propertyType, property, dataMember.Name);
                    }

                    file.WriteLine($"{tab}            }}");
                }

                if (conditions.Count > 0)
                {
                    file.WriteLine($"{tab}        }}");
                }
                file.WriteLine();
            }
        }

        private IReadOnlyCollection<MethodInfo> GetExtensionMethods(Assembly assembly, Type extendedType)
        {
            var extensionMethods = new List<MethodInfo>();

            foreach (var type in assembly.GetTypes())
            {
                if (type.IsDefined(typeof(ExtensionAttribute), false))
                {
                    foreach (var mi in type.GetMethods())
                    {
                        if (mi.IsDefined(typeof(ExtensionAttribute), false) && mi.GetParameters()[0].ParameterType == extendedType)
                        {
                            extensionMethods.Add(mi);
                        }
                    }
                }
            }

            return extensionMethods;
        }

        private PropertyType GetPropertyType(PropertyInfo propertyInfo, SerializeType serializeType)
        {
            var propertyType = PropertyType.NATIVE;

            if (typeof(IKalturaSerializable).IsAssignableFrom(propertyInfo.PropertyType))
            {
                propertyType = PropertyType.OBJECT;
                if ((serializeType == SerializeType.JSON && propertyInfo.PropertyType.GetMethod("ToCustomJson") != null) || 
                    (serializeType == SerializeType.XML && propertyInfo.PropertyType.GetMethod("ToCustomXml") != null))
                {
                    propertyType = PropertyType.CUSTOM;
                }
                else
                {
                    var extensionMethods = GetExtensionMethods(propertyInfo.PropertyType.Assembly, propertyInfo.PropertyType);
                    if ((serializeType == SerializeType.JSON && extensionMethods.Any(x => x.Name == "ToCustomJson")) ||
                        (serializeType == SerializeType.XML && extensionMethods.Any(x => x.Name == "ToCustomXml")))
                    {
                        propertyType = PropertyType.CUSTOM;
                    }
                }
            }
            else if (propertyInfo.PropertyType == typeof(string))
            {
                propertyType = PropertyType.STRING;
            }
            else if (propertyInfo.PropertyType == typeof(bool))
            {
                propertyType = PropertyType.BOOLEAN;
            }
            else if (propertyInfo.PropertyType.IsGenericType)
            {
                if (propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var innerType = propertyInfo.PropertyType.GetGenericArguments()[0];
                    if (innerType.IsEnum)
                    {
                        propertyType = PropertyType.ENUM;
                    }
                    else if (innerType == typeof(bool))
                    {
                        propertyType = PropertyType.BOOLEAN;
                    }
                    else
                    {
                        propertyType = PropertyType.NATIVE;
                    }
                }
                else if (propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    propertyType = typeof(IKalturaOTTObject).IsAssignableFrom(propertyInfo.PropertyType.GetGenericArguments()[0]) ? PropertyType.ARRAY : PropertyType.NUMERIC_ARRAY;
                }
                else if (propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(SerializableDictionary<,>))
                {
                    propertyType = PropertyType.MAP;
                }
            }
            else if (propertyInfo.PropertyType.IsEnum)
            {
                propertyType = PropertyType.ENUM;
            }

            return propertyType;
        }

        private IReadOnlyCollection<string> GetConditions(Type type, PropertyInfo propertyInfo, PropertyType propertyType, string dataMemberName)
        {
            var conditions = new List<string>();

            if (type.BaseType != null && type.BaseType.GetProperty(propertyInfo.Name) != null)
            {
                conditions.Add("!keys.Contains(\"" + dataMemberName + "\")");
            }

            var onlyNewStandard = propertyInfo.GetCustomAttribute<OnlyNewStandardAttribute>(false);
            if (onlyNewStandard != null)
            {
                if (onlyNewStandard.SinceVersion == null)
                {
                    conditions.Add("!isOldVersion");
                }
                else
                {
                    conditions.Add("OnlyNewStandardAttribute.IsNew(\"" + onlyNewStandard.SinceVersion + "\", currentVersion)");
                }
            }

            if (IsObsolete(propertyInfo))
            {
                conditions.Add("!omitObsolete");
            }

            var deprecationVersion = DeprecationVersion(propertyInfo);
            if (deprecationVersion != null)
            {
                conditions.Add("!DeprecatedAttribute.IsDeprecated(\"" + deprecationVersion + "\", currentVersion)");
            }

            if (propertyInfo.PropertyType.IsGenericType)
            {
                if (propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    conditions.Add(propertyInfo.Name + ".HasValue");
                }
                else if (propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    conditions.Add(propertyInfo.Name + " != null");
                }
                else if (propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(SerializableDictionary<,>))
                {
                    conditions.Add(propertyInfo.Name + " != null");
                }
            }

            if (propertyType == PropertyType.STRING || propertyType == PropertyType.OBJECT)
            {
                conditions.Add(propertyInfo.Name + " != null");
            }

            if (doesPropertyRequiresReadPermission(propertyInfo))
            {
                conditions.Add($"(requestType != RequestType.READ || RolesManager.IsPropertyPermitted(\"{type.Name}\", \"{propertyInfo.Name}\", requestType.Value))");
            }

            // responseProfile
            if (!RetrievedPropertiesToSkip.Contains(propertyInfo.Name) && propertyInfo.DeclaringType.BaseType.Name != "KalturaListResponse")
            {
                conditions.Add("(retrievedProperties == null || retrievedProperties.Contains(\"" + dataMemberName + "\"))");
            }

            return conditions;
        }

        private void WriteJsonDataMember(string tab, PropertyType propertyType, PropertyInfo propertyInfo, string dataMemberName)
        {
            switch (propertyType)
            {
                case PropertyType.NATIVE:
                    var nativeValue = propertyInfo.Name;
                    WriteSimpleJsonDataMember(tab, dataMemberName, nativeValue, false);
                    break;

                case PropertyType.ENUM:
                    var enumValue = propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)
                        ? $"{propertyInfo.Name}.Value.ToSerializedString()"
                        : $"{propertyInfo.Name}.ToSerializedString()";
                    WriteSimpleJsonDataMember(tab, dataMemberName, enumValue, true);
                    break;

                case PropertyType.BOOLEAN:
                    var booleanValue = $"{propertyInfo.Name} == true ? \"true\" : \"false\"";
                    WriteSimpleJsonDataMember(tab, dataMemberName, booleanValue, false);
                    break;

                case PropertyType.STRING:
                    WriteStringJsonDataMember(tab, dataMemberName, propertyInfo.Name);
                    break;

                case PropertyType.OBJECT:
                    WriteObjectJsonDataMember(tab, dataMemberName, propertyInfo.Name);
                    break;

                case PropertyType.CUSTOM:
                    WriteCustomJsonDataMember(tab, dataMemberName, propertyInfo.Name);
                    break;

                case PropertyType.ARRAY:
                    var responseProfileParameter = propertyInfo.DeclaringType?.BaseType?.Name == "KalturaListResponse"
                        ? "true"
                        : "false";
                    WriteArrayJsonDataMember(tab, dataMemberName, propertyInfo.Name, $"item.AppendAsJson(stringBuilder, currentVersion, omitObsolete, {responseProfileParameter});");
                    break;

                case PropertyType.NUMERIC_ARRAY:
                    WriteArrayJsonDataMember(tab, dataMemberName, propertyInfo.Name, "stringBuilder.Append(item);");
                    break;

                case PropertyType.MAP: //TODO: irena - case sensitivity
                    WriteMapJsonDataMember(tab, dataMemberName, propertyInfo.Name);
                    break;
            }
        }

        private void WriteSimpleJsonDataMember(string tab, string dataMemberName, string dataMemberValue, bool useQuotes)
        {
            WriteAddCommaIfRequired(tab);
            var quotesHolder = useQuotes
                ? "\\\""
                : string.Empty;
            file.WriteLine($"{tab}stringBuilder.Append(\"\\\"{dataMemberName}\\\":{quotesHolder}\");");
            file.WriteLine($"{tab}stringBuilder.Append({dataMemberValue});");
            if (useQuotes)
            {
                file.WriteLine($"{tab}stringBuilder.Append(\"{quotesHolder}\");");
            }

            file.WriteLine($"{tab}keys.Add(\"{dataMemberName}\");");
        }

        private void WriteStringJsonDataMember(string tab, string dataMemberName, string dataMemberValue)
        {
            WriteAddCommaIfRequired(tab);
            file.WriteLine($"{tab}stringBuilder.Append(\"\\\"{dataMemberName}\\\":\");");
            file.WriteLine($"{tab}stringBuilder.AppendEscapedJsonString({dataMemberValue});");
            file.WriteLine($"{tab}keys.Add(\"{dataMemberName}\");");
        }

        private void WriteObjectJsonDataMember(string tab, string dataMemberName, string propertyName)
        {
            WriteAddCommaIfRequired(tab);
            file.WriteLine($"{tab}stringBuilder.Append($\"\\\"{dataMemberName}\\\":\");");
            file.WriteLine($"{tab}{propertyName}.AppendAsJson(stringBuilder, currentVersion, omitObsolete);");
            file.WriteLine($"{tab}keys.Add(\"{dataMemberName}\");");
        }

        private void WriteCustomJsonDataMember(string tab, string dataMemberName, string propertyName)
        {
            file.WriteLine($"{tab}{propertyName}.AppendAsJson(stringBuilder, currentVersion, omitObsolete, \"{dataMemberName}\", keys.Count > 0);");
            file.WriteLine($"{tab}keys.Add(\"{dataMemberName}\");");
        }

        private void WriteArrayJsonDataMember(string tab, string dataMemberName, string propertyName, string itemWriteInstruction)
        {
            WriteAddCommaIfRequired(tab);
            file.WriteLine($"{tab}stringBuilder.Append(\"\\\"{dataMemberName}\\\":[\");");
            WriteCollectionAsJson(tab, propertyName, itemWriteInstruction);
            file.WriteLine($"{tab}stringBuilder.Append(\"]\");");
            file.WriteLine($"{tab}keys.Add(\"{dataMemberName}\");");
        }

        private void WriteMapJsonDataMember(string tab, string dataMemberName, string propertyName)
        {
            if (propertyName == "Metas" || propertyName == "Tags")
            {
                // filter metas / tags from profile 
                file.WriteLine($"{tab}if (retrievedProperties != null && !retrievedProperties.Contains(\"{dataMemberName}\"))");
                file.WriteLine($"{tab}{{");
                file.WriteLine($"{tab}    var valuesToFilter = retrievedProperties.Where(rp => rp.StartsWith(\"{dataMemberName}.\")).Select(p => p.Replace(\"{dataMemberName}.\", \"\"));");
                file.WriteLine($"{tab}    var filteredValues = {propertyName}.Where(pair => valuesToFilter.Contains(pair.Key));");
                file.WriteLine($"{tab}    if (valuesToFilter.Any())");
                file.WriteLine($"{tab}    {{");
                WriteAddCommaIfRequired($"{tab}        ");
                file.WriteLine($"{tab}        stringBuilder.Append(\"\\\"{dataMemberName}\\\":\");");
                file.WriteLine($"{tab}        stringBuilder.Append(\"{{\");");
                WriteMapAsJson($"{tab}        ", "filteredValues");
                file.WriteLine($"{tab}        stringBuilder.Append(\"}}\");");
                file.WriteLine($"{tab}    }}");
                file.WriteLine($"{tab}}}");
                file.WriteLine($"{tab}else");
                file.WriteLine($"{tab}{{");
                WriteAddCommaIfRequired($"{tab}    ");
                file.WriteLine($"{tab}    stringBuilder.Append(\"\\\"{dataMemberName}\\\":\");");
                file.WriteLine($"{tab}    stringBuilder.Append(\"{{\");");
                WriteMapAsJson($"{tab}    ", propertyName);
                file.WriteLine($"{tab}    stringBuilder.Append(\"}}\");");
                file.WriteLine($"{tab}}}");
            }
            else
            {
                WriteAddCommaIfRequired(tab);
                file.WriteLine($"{tab}stringBuilder.Append(\"\\\"{dataMemberName}\\\":\");");
                file.WriteLine($"{tab}stringBuilder.Append(\"{{\");");
                WriteMapAsJson($"{tab}", propertyName);
                file.WriteLine($"{tab}stringBuilder.Append(\"}}\");");
            }

            file.WriteLine($"{tab}keys.Add(\"{dataMemberName}\");");
        }

        private void WriteCollectionAsJson(string tab, string collectionVariableName, string itemWriteInstruction)
        {
            file.WriteLine($"{tab}isFirstItem = true;");
            file.WriteLine($"{tab}foreach (var item in {collectionVariableName})");
            file.WriteLine($"{tab}{{");
            file.WriteLine($"{tab}    if (!isFirstItem)");
            file.WriteLine($"{tab}    {{");
            file.WriteLine($"{tab}        stringBuilder.Append(\",\");");
            file.WriteLine($"{tab}    }}");
            file.WriteLine($"{tab}    {itemWriteInstruction}");
            file.WriteLine($"{tab}    isFirstItem = false;");
            file.WriteLine($"{tab}}}");
        }

        private void WriteMapAsJson(string tab, string collectionVariableName)
        {
            file.WriteLine($"{tab}isFirstItem = true;");
            file.WriteLine($"{tab}foreach (var item in {collectionVariableName})");
            file.WriteLine($"{tab}{{");
            file.WriteLine($"{tab}    if (!isFirstItem)");
            file.WriteLine($"{tab}    {{");
            file.WriteLine($"{tab}        stringBuilder.Append(\",\");");
            file.WriteLine($"{tab}    }}");
            file.WriteLine($"{tab}    stringBuilder.Append(\"\\\"\");");
            file.WriteLine($"{tab}    stringBuilder.Append(item.Key);");
            file.WriteLine($"{tab}    stringBuilder.Append(\"\\\":\");");
            file.WriteLine($"{tab}    item.Value.AppendAsJson(stringBuilder, currentVersion, omitObsolete);");
            file.WriteLine($"{tab}    isFirstItem = false;");
            file.WriteLine($"{tab}}}");
        }
        
        private void WriteXmlDataMember(string tab, PropertyType propertyType, PropertyInfo propertyInfo, string dataMemberName)
        {
            switch (propertyType)
            {
                case PropertyType.NATIVE:
                    var nativeValue = propertyInfo.Name;
                    WriteSimpleXmlDataMember(tab, dataMemberName, nativeValue, false);
                    break;

                case PropertyType.ENUM:
                    var enumValue = propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)
                        ? $"{propertyInfo.Name}.Value.ToSerializedString()"
                        : $"{propertyInfo.Name}.ToSerializedString()";
                    WriteSimpleXmlDataMember(tab, dataMemberName, enumValue, true);
                    break;

                case PropertyType.BOOLEAN:
                    var booleanValue = $"{propertyInfo.Name} == true ? \"true\" : \"false\"";
                    WriteSimpleXmlDataMember(tab, dataMemberName, booleanValue, false);
                    break;

                case PropertyType.STRING:
                    WriteStringXmlDataMember(tab, dataMemberName, propertyInfo.Name);
                    break;

                case PropertyType.OBJECT:
                    WriteObjectXmlDataMember(tab, dataMemberName, propertyInfo.Name);
                    break;

                case PropertyType.CUSTOM:
                    WriteCustomXmlDataMember(tab, dataMemberName, propertyInfo.Name);
                    break;

                case PropertyType.ARRAY:
                    var responseProfileParameter = propertyInfo.DeclaringType?.BaseType?.Name == "KalturaListResponse"
                        ? "true"
                        : "false";
                    WriteArrayXmlDataMember(tab, dataMemberName, propertyInfo.Name, $"item.AppendAsXml(stringBuilder, currentVersion, omitObsolete, {responseProfileParameter});");
                    break;

                case PropertyType.NUMERIC_ARRAY:
                    WriteArrayXmlDataMember(tab, dataMemberName, propertyInfo.Name, "stringBuilder.Append(item);");
                    break;

                case PropertyType.MAP: //TODO: irena - case sensitivity
                    WriteMapXmlDataMember(tab, dataMemberName, propertyInfo.Name);
                    break;
            }
        }

        private void WriteSimpleXmlDataMember(string tab, string dataMemberName, string dataMemberValue, bool useQuotes)
        {
            var quotesHolder = useQuotes
                ? "\\\""
                : string.Empty;
            file.WriteLine($"{tab}stringBuilder.Append(\"<{dataMemberName}>{quotesHolder}\");");
            file.WriteLine($"{tab}stringBuilder.Append({dataMemberValue});");
            file.WriteLine($"{tab}stringBuilder.Append(\"{quotesHolder}</{dataMemberName}>\");");
            file.WriteLine($"{tab}keys.Add(\"{dataMemberName}\");");
        }

        private void WriteStringXmlDataMember(string tab, string dataMemberName, string dataMemberValue)
        {
            file.WriteLine($"{tab}stringBuilder.Append(\"<{dataMemberName}>\");");
            file.WriteLine($"{tab}stringBuilder.AppendEscapedXmlString({dataMemberValue});");
            file.WriteLine($"{tab}stringBuilder.Append(\"</{dataMemberName}>\");");
            file.WriteLine($"{tab}keys.Add(\"{dataMemberName}\");");
        }

        private void WriteObjectXmlDataMember(string tab, string dataMemberName, string propertyName)
        {
            file.WriteLine($"{tab}stringBuilder.Append(\"<{dataMemberName}>\");");
            file.WriteLine($"{tab}stringBuilder.Append({propertyName}.ToXml(currentVersion, omitObsolete));");
            file.WriteLine($"{tab}stringBuilder.Append(\"</{dataMemberName}>\");");
            file.WriteLine($"{tab}keys.Add(\"{dataMemberName}\");");
        }

        private void WriteCustomXmlDataMember(string tab, string dataMemberName, string propertyName)
        {
            file.WriteLine($"{tab}{propertyName}.AppendAsXml(stringBuilder, currentVersion, omitObsolete, \"{dataMemberName}\");");
            file.WriteLine($"{tab}keys.Add(\"{dataMemberName}\");");
        }

        private void WriteArrayXmlDataMember(string tab, string dataMemberName, string propertyName, string itemWriteInstruction)
        {
            file.WriteLine($"{tab}stringBuilder.Append(\"<{dataMemberName}>\");");
            file.WriteLine($"{tab}foreach (var item in {propertyName})");
            file.WriteLine($"{tab}{{");
            file.WriteLine($"{tab}    stringBuilder.Append(\"<item>\");");
            file.WriteLine($"{tab}    {itemWriteInstruction}");
            file.WriteLine($"{tab}    stringBuilder.Append(\"</item>\");");
            file.WriteLine($"{tab}}}");
            file.WriteLine($"{tab}stringBuilder.Append(\"</{dataMemberName}>\");");
            file.WriteLine($"{tab}keys.Add(\"{dataMemberName}\");");
        }

        private void WriteMapXmlDataMember(string tab, string dataMemberName, string propertyName)
        {            
            file.WriteLine($"{tab}stringBuilder.Append(\"<{dataMemberName}>\");");
            file.WriteLine($"{tab}foreach (var item in {propertyName})");
            file.WriteLine($"{tab}{{");
            file.WriteLine($"{tab}    stringBuilder.Append(\"<item>\");");
            file.WriteLine($"{tab}    stringBuilder.Append(\"<itemKey>\");");
            file.WriteLine($"{tab}    stringBuilder.Append(item.Key);");
            file.WriteLine($"{tab}    stringBuilder.Append(\"</itemKey>\");");
            file.WriteLine($"{tab}    item.Value.AppendAsXml(stringBuilder, currentVersion, omitObsolete);");
            file.WriteLine($"{tab}    stringBuilder.Append(\"</item>\");");
            file.WriteLine($"{tab}}}");
            file.WriteLine($"{tab}stringBuilder.Append(\"</{dataMemberName}>\");");
            file.WriteLine($"{tab}keys.Add(\"{dataMemberName}\");");
        }

        private void WriteAddCommaIfRequired(string tab)
        {
            file.WriteLine($"{tab}if (keys.Count > 0)");
            file.WriteLine($"{tab}{{");
            file.WriteLine($"{tab}    stringBuilder.Append(\",\");");
            file.WriteLine($"{tab}}}");
        }
    }
}
