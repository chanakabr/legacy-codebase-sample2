using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.App_Start;
using System.Collections;
using System.IO;
using WebAPI.ObjectsConvertor.Extensions;
using WebAPI.Managers;
using System.Text;

namespace Reflector
{
    class Deserializer : Base
    {
        public static string GetJsonDeserializerCSFilePath()
        {
            var currentLocation = AppDomain.CurrentDomain.BaseDirectory;
            var solutionDir = Directory.GetParent(currentLocation).Parent.Parent.Parent.Parent.Parent;
            var filePath = Path.Combine(solutionDir.FullName, "Core", "WebAPI", "Reflection", "KalturaJsonDeserializer.cs");
            return filePath;
        }

        public Deserializer() : base(GetJsonDeserializerCSFilePath())
        {
            types.Remove(typeof(KalturaOTTObject));
            types.Remove(typeof(KalturaApiExceptionArg));
            types.Remove(typeof(KalturaFilter<>));
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
            file.WriteLine("// ReSharper disable UnusedParameter.Local");
            file.WriteLine("// ReSharper disable PossibleNullReferenceException");
            file.WriteLine("// ReSharper disable AssignNullToNotNullAttribute");
            file.WriteLine("// ReSharper disable BadChildStatementIndent");
            #pragma warning disable 612
            #pragma warning disable 612
            file.WriteLine("using Newtonsoft.Json.Linq;");
            file.WriteLine("using System;");
            file.WriteLine("using System.Web;");
            file.WriteLine("using System.Collections;");
            file.WriteLine("using System.Collections.Generic;");
            file.WriteLine("using WebAPI.Managers.Scheme;");
            file.WriteLine("using WebAPI.Filters;");
            file.WriteLine("using WebAPI.Reflection;");
            file.WriteLine("using KalturaRequestContext;");
            file.WriteLine("using WebAPI.Exceptions;");
            file.WriteLine("using WebAPI.ModelsValidators;");
            file.WriteLine("using WebAPI.ObjectsConvertor.Extensions;");
            file.WriteLine("using WebAPI.ModelsFactory;");
            file.WriteLine("using WebAPI.Utils;");
            file.WriteLine("using System.Linq;");
        }

        protected override void writeBody()
        {
            wrtieUsing();
            wrtieDeserializer();
            wrtiePartialClasses();
        }

        protected override void writeFooter()
        {
        }

        private void writeDeserializeTypeProperty(string apiName, PropertyInfo property)
        {
            Type propertyType = property.PropertyType;

            file.WriteLine("                {");

            SchemePropertyAttribute schemaProperty = property.GetCustomAttribute<SchemePropertyAttribute>();
            if (schemaProperty != null)
            {
                file.WriteLine("                    " + property.Name + "SchemaProperty.Validate(\"" + apiName + "\", parameters[\"" + apiName + "\"]);");
            }

            if(propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                propertyType = Nullable.GetUnderlyingType(propertyType); ;
            }
            if (propertyType.IsPrimitive || propertyType == typeof(string))
            {
                file.WriteLine("                    " + property.Name + " = (" + propertyType.Name + ") Convert.ChangeType(parameters[\"" + apiName + "\"], typeof(" + propertyType.Name + "));");
            }
            else if (propertyType.IsEnum)
            {
                file.WriteLine($"                    if(string.IsNullOrEmpty(parameters[\"{apiName}\"].ToString()))");
                file.WriteLine("                    {");
                file.WriteLine($"                        throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, \"{apiName}\");");
                file.WriteLine("                    }");
                file.WriteLine();
                file.WriteLine("                    " + property.Name + " = (" + propertyType.Name + ") Enum.Parse(typeof(" + propertyType.Name + "), parameters[\"" + apiName + "\"].ToString(), true);");
                file.WriteLine();
                file.WriteLine("                    if (!Enum.IsDefined(typeof(" + propertyType.Name + "), " + property.Name + "))");
                file.WriteLine("                    {");
                file.WriteLine("                        throw new ArgumentException(string.Format(\"Invalid enum parameter value {0} was sent for enum type {1}\", " + property.Name + ", typeof(" + propertyType.Name + ")));");
                file.WriteLine("                    }");
            }
            else if (propertyType == typeof(DateTime))
            {
                file.WriteLine("                    " + property.Name + " = OTTObjectBuilder.longToDateTime((long) parameters[\"" + apiName + "\"]);");
            }
            else if (propertyType.IsArray)
            {
                throw new Exception("shouldn'y happen");
            }
            else if (propertyType.IsGenericType) // nullable, list or map
            {
                if (typeof(IList).IsAssignableFrom(propertyType)) // list
                {
                    Type genericParam = propertyType.GetGenericArguments()[0];
                    string genericParamName = GetTypeName(genericParam);
                    file.WriteLine("                    if (parameters[\"" + apiName + "\"] is JArray)");
                    file.WriteLine("                    {");
                    if (typeof(IKalturaOTTObject).IsAssignableFrom(genericParam))
                    {
                        file.WriteLine("                        " + property.Name + " = OTTObjectBuilder.buildList<" + genericParamName + ">(typeof(" + genericParamName + "), (JArray) parameters[\"" + apiName + "\"]);");
                    }
                    else
                    {
                        file.WriteLine("                        " + property.Name + " = OTTObjectBuilder.buildNativeList<" + genericParamName + ">(typeof(" + genericParamName + "), (JArray) parameters[\"" + apiName + "\"]);");
                    }
                    file.WriteLine("                    }");

                    file.WriteLine("                    else if (parameters[\"" + apiName + "\"] is IList)");
                    file.WriteLine("                    {");
                    if (typeof(IKalturaOTTObject).IsAssignableFrom(genericParam))
                    {
                        file.WriteLine("                        " + property.Name + " = OTTObjectBuilder.buildList(typeof(" + genericParamName + "), parameters[\"" + apiName + "\"] as object[]);");
                    }
                    else
                    {
                        file.WriteLine("                        " + property.Name + " = OTTObjectBuilder.buildNativeList(typeof(" + genericParamName + "), parameters[\"" + apiName + "\"] as object[]);");
                    }
                    file.WriteLine("                    }");
                }
                else if (typeof(IDictionary).IsAssignableFrom(propertyType)) // map
                {
                    string genericParamName = GetTypeName(propertyType.GetGenericArguments()[1]);
                    file.WriteLine("                    if (parameters[\"" + apiName + "\"] is JObject)");
                    file.WriteLine("                    {");
                    file.WriteLine("                        " + property.Name + " = OTTObjectBuilder.buildDictionary<" + genericParamName + ">(typeof(" + genericParamName + "), ((JObject) parameters[\"" + apiName + "\"]).ToObject<Dictionary<string, object>>());");
                    file.WriteLine("                    }");
                }
            }
            else if (typeof(KalturaMultilingualString).IsAssignableFrom(propertyType))
            {
                file.WriteLine("                    if (parameters[\"" + apiName + "\"] is JArray)");
                file.WriteLine("                    {");
                file.WriteLine("                        " + property.Name + " = MultilingualStringFactory.Create((JArray) parameters[\"" + apiName + "\"]);");
                file.WriteLine("                    }");
                file.WriteLine("                    else if (parameters[\"" + apiName + "\"] is IList)");
                file.WriteLine("                    {");
                file.WriteLine("                        " + property.Name + " = MultilingualStringFactory.Create((List<object>) parameters[\"" + apiName + "\"]);");
                file.WriteLine("                    }");
            }
            else // object
            {
                file.WriteLine("                    if (parameters[\"" + apiName + "\"] is JObject)");
                file.WriteLine("                    {");
                file.WriteLine("                        " + property.Name + " = (" + propertyType.Name + ") Deserializer.deserialize(typeof(" + propertyType.Name + "), ((JObject) parameters[\"" + apiName + "\"]).ToObject<Dictionary<string, object>>());");
                file.WriteLine("                    }");
                file.WriteLine("                    else if (parameters[\"" + apiName + "\"] is IDictionary)");
                file.WriteLine("                    {");
                file.WriteLine("                        " + property.Name + " = (" + propertyType.Name + ") Deserializer.deserialize(typeof(" + propertyType.Name + "), (Dictionary<string, object>) parameters[\"" + apiName + "\"]);");
                file.WriteLine("                    }");
            }

            file.WriteLine("                }");
        }

        private void wrtieDeserializeTypeSchemaProperties(Type type)
        {
            List<PropertyInfo> schemePropertyProperties = typeof(SchemePropertyAttribute).GetProperties().ToList();

            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (!property.CanWrite || property.DeclaringType != type)
                {
                    continue;
                }

                SchemePropertyAttribute schemaProperty = property.GetCustomAttribute<SchemePropertyAttribute>();
                if (schemaProperty == null)
                {
                    continue;
                }

                file.WriteLine("        private static RuntimeSchemePropertyAttribute " + property.Name + "SchemaProperty = new RuntimeSchemePropertyAttribute(\"" + type.Name + "\")");
                file.WriteLine("        {");
                schemePropertyProperties.ForEach(schemePropertyProperty => {
                    object val = schemePropertyProperty.GetValue(schemaProperty);
                    if (val != null)
                    {
                        if (schemePropertyProperty.Name != "Name" && schemePropertyProperty.Name != "TypeId")
                        {
                            if (schemePropertyProperty.PropertyType == typeof(bool))
                            {
                                file.WriteLine("            " + schemePropertyProperty.Name + " = " + val.ToString().ToLower() + ",");
                            }
                            else if (schemePropertyProperty.PropertyType == typeof(int))
                            {
                                if ((int)val != int.MinValue && (int)val != int.MaxValue)
                                {
                                    file.WriteLine("            " + schemePropertyProperty.Name + " = " + val.ToString().ToLower() + ",");
                                }
                            }
                            else if (schemePropertyProperty.PropertyType == typeof(long))
                            {
                                if ((long)val != long.MinValue && (long)val != long.MaxValue)
                                {
                                    file.WriteLine("            " + schemePropertyProperty.Name + " = " + val.ToString().ToLower() + ",");
                                }
                            }
                            else if (schemePropertyProperty.PropertyType == typeof(float))
                            {
                                if ((float)val != float.MinValue && (float)val != float.MaxValue)
                                {
                                    file.WriteLine("            " + schemePropertyProperty.Name + " = " + val.ToString().ToLower() + ",");
                                }
                            }
                            else if (schemePropertyProperty.PropertyType == typeof(string))
                            {
                                file.WriteLine("            " + schemePropertyProperty.Name + " = @\"" + val + "\",");
                            }
                            else if (schemePropertyProperty.PropertyType == typeof(Type))
                            {
                                file.WriteLine("            " + schemePropertyProperty.Name + " = typeof(" + ((Type)val).Name + "),");
                            }
                            else if (schemePropertyProperty.PropertyType == typeof(eKSValidation))
                            {
                                file.WriteLine("            " + schemePropertyProperty.Name + " = WebAPI.Managers.eKSValidation." + val.ToString() + ",");
                            }
                            else
                            {
                                file.WriteLine("            " + schemePropertyProperty.Name + " = " + val + ",");
                            }
                        }
                    }
                });

                file.WriteLine("        };");
            }
        }

        private void wrtieDeserializeTypeProperties(Type type)
        {
            PropertyInfo[] properties = type.GetProperties();
            SchemePropertyAttribute schemaProperty;
            IEnumerable<OldStandardPropertyAttribute> oldStandards;

            bool hasProperties = false;
            bool addIsOldVersion = false;

            foreach (PropertyInfo property in properties)
            {
                if (property.CanWrite && property.DeclaringType == type)
                {
                    hasProperties = true;

                    schemaProperty = property.GetCustomAttribute<SchemePropertyAttribute>();
                    if (schemaProperty != null)
                    {
                        addIsOldVersion = true;
                        break;
                    }

                    oldStandards = property.GetCustomAttributes<OldStandardPropertyAttribute>();
                    foreach (OldStandardPropertyAttribute oldStandard in oldStandards)
                    {
                        addIsOldVersion = true;
                        break;
                    }
                }
            }

            if (!hasProperties)
            {
                return;
            }

            var schemeClass = type.GetCustomAttribute<SchemeClassAttribute>();
            var hasRequired = schemeClass?.Required?.Length > 0;
            var hasOneOf = schemeClass?.OneOf?.Length > 0;
            var hasAnyOf = schemeClass?.AnyOf?.Length > 0;
            var hasMinProperties = schemeClass?.MinProperties > -1;
            var hasMaxProperties = schemeClass?.MaxProperties > -1;

            if (hasRequired || hasOneOf || hasAnyOf || hasMinProperties || hasMaxProperties)
            {
                file.WriteLine("            if (fromRequest)");
                file.WriteLine("            {");

                if (hasRequired)
                {
                    file.WriteLine("                if (parameters == null || parameters.Count == 0)");
                    file.WriteLine("                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, \"" + string.Join(",", schemeClass.Required) + "\");");
                    file.WriteLine();
                    foreach (var required in schemeClass.Required)
                    {
                        file.WriteLine($"               if (!parameters.ContainsKey(\"{required}\") || string.IsNullOrWhiteSpace(parameters[\"{required}\"]?.ToString()))");
                        file.WriteLine($"                   throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, \"{ required}\");");
                    }
                    file.WriteLine();
                }
                else if (hasOneOf || hasAnyOf || hasMinProperties)
                {
                    file.WriteLine("                if (parameters == null || parameters.Count == 0)");
                    file.WriteLine($"                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, \"{type.Name}\");");
                    file.WriteLine();
                }

                if (hasOneOf)
                {
                    file.WriteLine("                Deserializer.CheckOneOf(parameters, new[] {" + string.Join(", ", schemeClass.OneOf.Select(x => $"\"{x}\"")) + "});");
                    file.WriteLine();
                }

                if (hasAnyOf)
                {
                    var existAnyOfString = string.Join(" && ", schemeClass.AnyOf.Select(x => $"(!parameters.ContainsKey(\"{x}\") || string.IsNullOrWhiteSpace(parameters[\"{x}\"]?.ToString()))"));
                    file.WriteLine($"                if ({existAnyOfString})");
                    file.WriteLine($"                   throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, \"{string.Join(", ", schemeClass.AnyOf)}\");");
                    file.WriteLine();
                }

                if (hasMinProperties)
                {
                    file.WriteLine($"                if (parameters.Where(x => x.Key != \"objectType\").Count() < {schemeClass.MinProperties})");
                    file.WriteLine($"                    throw new BadRequestException(BadRequestException.ARGUMENT_MIN_PROPERTIES_CROSSED, \"{type.Name}\", {schemeClass.MinProperties});");
                    file.WriteLine();
                }

                if (hasMaxProperties)
                {
                    file.WriteLine($"                if (parameters?.Where(x => !Deserializer.PropertiesToIgnore.Contains(x.Key)).Count() > {schemeClass.MaxProperties})");
                    file.WriteLine($"                    throw new BadRequestException(BadRequestException.ARGUMENT_MAX_PROPERTIES_CROSSED, \"{type.Name}\", {schemeClass.MaxProperties});");
                    file.WriteLine();
                }

                file.WriteLine("            }");
            }

            file.WriteLine("            if (parameters != null)");
            file.WriteLine("            {");

            if (addIsOldVersion)
            {
                file.WriteLine(
                    "                Version currentVersion = OldStandardAttribute.getCurrentRequestVersion();");
                file.WriteLine(
                    "                bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);");
            }

            var supportsNullable = type.IsSubclassOf(typeof(KalturaOTTObjectSupportNullable));

            foreach (PropertyInfo property in properties)
            {
                if (!property.CanWrite || property.DeclaringType != type)
                {
                    continue;
                }

                string apiName = property.Name;
                DataMemberAttribute dataMember = property.GetCustomAttribute<DataMemberAttribute>();
                if (dataMember != null)
                {
                    apiName = dataMember.Name;
                }

                if (typeof(KalturaMultilingualString).IsAssignableFrom(property.PropertyType))
                {
                    apiName = MultilingualStringMapper.GetMultilingualName(apiName);
                }

                var ca = property.GetCustomAttributes<SchemePropertyAttribute>().FirstOrDefault();
                if (supportsNullable && ca != null && ca.IsNullable)
                {
                    var nullableParam = $"{apiName}__null";
                    file.WriteLine("                if (parameters.ContainsKey(\"" + nullableParam + "\") && parameters[\"" +
                                   nullableParam + "\"] != null)");
                    file.WriteLine("                {");
                    file.WriteLine("                    this.AddNullableProperty(\"" + apiName + "\");");
                    file.WriteLine("                }");

                }

                file.WriteLine("                if (parameters.ContainsKey(\"" + apiName + "\") && parameters[\"" +
                               apiName + "\"] != null)");

                writeDeserializeTypeProperty(apiName, property);
                oldStandards = property.GetCustomAttributes<OldStandardPropertyAttribute>();
                foreach (OldStandardPropertyAttribute oldStandard in oldStandards)
                {
                    apiName = oldStandard.oldName;
                    if (typeof(KalturaMultilingualString).IsAssignableFrom(property.PropertyType))
                    {
                        apiName = MultilingualStringMapper.GetMultilingualName(apiName);
                    }

                    if (oldStandard.sinceVersion == null)
                    {
                        file.WriteLine("                if (parameters.ContainsKey(\"" + apiName +
                                       "\") && parameters[\"" + apiName + "\"] != null && isOldVersion)");
                    }
                    else
                    {
                        file.WriteLine("                if (parameters.ContainsKey(\"" + apiName +
                                       "\") && parameters[\"" + apiName +
                                       "\"] != null && (isOldVersion || currentVersion.CompareTo(new Version(\"" +
                                       oldStandard.sinceVersion + "\")) < 0))");
                    }

                    writeDeserializeTypeProperty(apiName, property);
                }
            }

            file.WriteLine("            }");
        }

        private void wrtiePartialClass(Type type)
        {
            file.WriteLine("    public partial class " + GetTypeName(type, true));
            file.WriteLine("    {");
            wrtieDeserializeTypeSchemaProperties(type);
            file.WriteLine("        public " + GetTypeName(type) + "(Dictionary<string, object> parameters = null, bool fromRequest = false) : base(parameters, fromRequest)");
            file.WriteLine("        {");
            wrtieDeserializeTypeProperties(type);
            file.WriteLine("        }");
            file.WriteLine("    }");
        }

        private void wrtieNamespace(string namespaceName)
        {
            file.WriteLine("");
            file.WriteLine("namespace " + namespaceName);
            file.WriteLine("{");

            foreach (Type type in types)
            {
                if (type.Namespace == namespaceName)
                {
                    wrtiePartialClass(type);
                }
            }

            file.WriteLine("}");
        }

        private void wrtieUsing()
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

        private void wrtieDeserializer()
        {
            file.WriteLine("");
            file.WriteLine("namespace WebAPI.Reflection");
            file.WriteLine("{");
            file.WriteLine("    public class Deserializer");
            file.WriteLine("    {");
            file.WriteLine("        internal static readonly HashSet<string> PropertiesToIgnore = new HashSet<string>()");
            file.WriteLine("        {");
            file.WriteLine("            \"objectType\",");
            file.WriteLine("            \"relatedObjects\",");
            file.WriteLine("            \"orderBy\"");
            file.WriteLine("        };");
            file.WriteLine();
            file.WriteLine("        public static IKalturaOTTObject deserialize(Type type, Dictionary<string, object> parameters)");
            file.WriteLine("        {");
            file.WriteLine("            string objectType = type.Name;");
            file.WriteLine("            if (parameters.ContainsKey(\"objectType\"))");
            file.WriteLine("            {");
            file.WriteLine("                objectType = parameters[\"objectType\"].ToString();");
            file.WriteLine("            }");
            file.WriteLine("            ");
            file.WriteLine("            switch (objectType)");
            file.WriteLine("            {");

            foreach (Type type in types)
            {
                var typeName = GetTypeName(type);
                if (typeName == "KalturaListResponse" && type.IsAbstract) { continue; }

                file.WriteLine("                case \"" + typeName + "\":");
                NewObjectTypeAttribute newObjectTypeAttribue = type.GetCustomAttribute<NewObjectTypeAttribute>(false);
                if (newObjectTypeAttribue != null)
                {
                    file.WriteLine("                    return new " + GetTypeName(newObjectTypeAttribue.type) + "(parameters, true);");
                }
                else if (type.IsAbstract)
                {
                    file.WriteLine("                    throw new RequestParserException(RequestParserException.ABSTRACT_PARAMETER, objectType);");
                }
                else
                {
                    file.WriteLine("                    return new " + typeName + "(parameters, true);");
                }
                file.WriteLine("                    ");
            }

            file.WriteLine("            }");
            file.WriteLine("            ");
            file.WriteLine("            throw new RequestParserException(RequestParserException.INVALID_OBJECT_TYPE, objectType);");
            file.WriteLine("        }");
            file.WriteLine();
            file.WriteLine("        public static void CheckOneOf(Dictionary<string, object> parameters, string[] allOneOfParams)");
            file.WriteLine("        {");
            file.WriteLine("            var oneOfParameters = allOneOfParams.Where(p => parameters.TryGetValue(p, out var value) && !string.IsNullOrWhiteSpace(value.ToString())).ToList();");
            file.WriteLine();
            file.WriteLine("            if (oneOfParameters.Count == 0)");
            file.WriteLine("                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, string.Join(\", \", allOneOfParams));");
            file.WriteLine();
            file.WriteLine("            if (oneOfParameters.Count > 1)");
            file.WriteLine("                throw new BadRequestException(BadRequestException.MULTIPLE_ARGUMENTS_CONFLICTS_EACH_OTHER, string.Join(\", \", oneOfParameters));");
            file.WriteLine("        }");
            file.WriteLine("    }");
            file.WriteLine("}");
        }

        private void wrtiePartialClasses()
        {
            HashSet<string> namespaces = new HashSet<string>();
            foreach (Type type in types)
            {
                if (!namespaces.Contains(type.Namespace))
                {
                    namespaces.Add(type.Namespace);
                    wrtieNamespace(type.Namespace);
                }
            }
        }
    }
}