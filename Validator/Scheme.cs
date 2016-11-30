using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Linq;
using WebAPI.Models.General;
using WebAPI.ClientManagers.Client;
using WebAPI.Managers.Models;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Utils;
using WebAPI.Managers.Scheme;
using WebAPI.Filters;
using WebAPI.Exceptions;
using WebAPI.Models.Renderers;

namespace Validator.Managers.Scheme
{
    internal class Scheme
    {
        private Stream stream;
        private Assembly assembly;
        private Assembly validatorAssembly;
        private XmlDocument assemblyXml;
        private XmlWriter writer;

        private List<Type> enums = new List<Type>();
        private List<Type> types;
        private Dictionary<string, Type> loadedTypes = new Dictionary<string, Type>();
        private IEnumerable<Type> controllers;
        private IEnumerable<Type> exceptions;
        private Dictionary<int, ApiException.ExceptionType> errors = new Dictionary<int, ApiException.ExceptionType>();

        public Scheme(bool loadAll)
        {
            this.assembly = Assembly.Load("WebAPI");
            this.validatorAssembly = Assembly.GetExecutingAssembly();
            this.assemblyXml = GetAssemblyXml();

            Load(loadAll);
        }

        public Scheme() : this(false)
        {

        }

        public Scheme(Stream stream) : this()
        {
            this.stream = stream;

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };
            writer = XmlWriter.Create(stream, settings);
        }

        private void Load(bool loadAll)
        {
            controllers = assembly.GetTypes().Where(myType => myType.IsClass && myType.IsSubclassOf(typeof(ApiController)));
            exceptions = assembly.GetTypes().Where(myType => myType.IsClass && (myType.IsSubclassOf(typeof(ApiException)) || myType == typeof(ApiException)));

            foreach (Type exception in exceptions.OrderBy(exception => exception.Name))
            {
                loadErrors(exception);
            }

            foreach (Type controller in controllers)
            {
                var apiExplorerSettings = controller.GetCustomAttribute<ApiExplorerSettingsAttribute>(false);
                if (apiExplorerSettings != null && apiExplorerSettings.IgnoreApi)
                    continue;

                if (!loadAll && !SchemeManager.Validate(controller, false))
                    continue;

                var methods = controller.GetMethods();
                foreach (var method in methods)
                {
                    if (!method.IsPublic || method.DeclaringType.Namespace != "WebAPI.Controllers")
                        continue;

                    //Read only HTTP POST as we will have duplicates otherwise
                    var explorerAttr = method.GetCustomAttributes<ApiExplorerSettingsAttribute>(false);
                    if (explorerAttr.Count() > 0 && explorerAttr.First().IgnoreApi)
                        continue;

                    if (!loadAll && !SchemeManager.Validate(method, false))
                        continue;

                    foreach (var param in method.GetParameters())
                    {
                        LoadType(param.ParameterType, loadAll);
                    }

                    if (method.ReturnType != null)
                    {
                        LoadType(method.ReturnType, loadAll);
                    }
                };
            }

            var filters = assembly.GetTypes().Where(myType => myType.IsClass && typeof(IKalturaFilter).IsAssignableFrom(myType));
            foreach (var filter in filters)
            {
                var orderByName = filter.Name.Replace("Filter", "OrderBy");
                var orderBys = assembly.GetTypes().Where(myType => myType.IsEnum && myType.Name == orderByName);
                foreach (Type orderBy in orderBys)
                {
                    if (!enums.Contains(orderBy))
                        enums.Add(orderBy);
                }
            }

            List<Field> fields = new List<Field>();
            foreach (Type type in loadedTypes.Values)
            {   
                fields.Add(new Field(type));
            }

            int[] sortedFields = Sort(fields);

            List<Type> sortedTypes = new List<Type>();
            for (int i = 0; i < sortedFields.Length; i++)
            {
                var field = fields[sortedFields[i]];
                sortedTypes.Insert(0, loadedTypes.Values.Where(myType => myType.Name == field.Name).First());
            }
            types = sortedTypes;
        }

        private void loadErrors(Type exception)
        {
            FieldInfo[] fields = exception.GetFields();
            foreach (FieldInfo field in fields)
            {
                if (field.FieldType.IsSubclassOf(typeof(ApiException.ExceptionType)))
                {
                    ApiException.ExceptionType type = (ApiException.ExceptionType)field.GetValue(null);

                    if (type.GetType() == typeof(ApiException.ApiExceptionType))
                    {
                        ApiException.ApiExceptionType exceptionType = type as ApiException.ApiExceptionType;
                        errors.Add((int)exceptionType.statusCode, type);
                    }
                    else if (type.GetType() == typeof(ApiException.ClientExceptionType))
                    {
                        ApiException.ClientExceptionType exceptionType = type as ApiException.ClientExceptionType;
                        errors.Add((int)exceptionType.statusCode, type);
                    }
                }
            }
        }

        private void LoadType(Type type, bool loadAll)
        {
            if (type.IsEnum && !enums.Contains(type))
            {
                if (loadAll || SchemeManager.Validate(type, false))
                    enums.Add(type);
                return;
            }
            if (loadedTypes.ContainsKey(type.Name))
            {
                return;
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                LoadType(type.GetGenericArguments()[0], loadAll);
                return;
            }

            if (type == typeof(KalturaOTTObject))
            {
                LoadTypeProperties(type, loadAll);
                return;
            }

            string typeName = type.Name;
            if (type.IsGenericType && typeName.StartsWith("KalturaFilter"))
                typeName = "KalturaFilter";

            if (type.IsSubclassOf(typeof(KalturaOTTObject)) && !loadedTypes.ContainsKey(typeName) && (loadAll || SchemeManager.Validate(type, false)))
            {
                loadedTypes.Add(typeName, type);
                LoadType(type.BaseType, loadAll);

                var subClasses = assembly.GetTypes().Where(myType => myType.IsSubclassOf(type));
                foreach (Type subClass in subClasses)
                    LoadType(subClass, loadAll);

                LoadTypeProperties(type, loadAll);
                return;
            }

            if (type.IsArray)
            {
                LoadType(type.GetElementType(), loadAll);
            }
            else if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Dictionary<,>) || type.GetGenericTypeDefinition() == typeof(SerializableDictionary<,>)))
            {
                LoadType(type.GetGenericArguments()[1], loadAll);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                LoadType(type.GetGenericArguments()[0], loadAll);
            }
        }

        private void LoadTypeProperties(Type type, bool loadAll)
        {
            List<PropertyInfo> properties = type.GetProperties().ToList();
            foreach (var property in properties)
            {
                if (property.DeclaringType == type)
                    LoadType(property.PropertyType, loadAll);
            }
        }

        private XmlDocument GetAssemblyXml()
        {
            string assemblyFilename = validatorAssembly.CodeBase;

            const string prefix = "file:///";

            if (assemblyFilename.StartsWith(prefix))
            {
                StreamReader streamReader;

                try
                {
                    streamReader = new StreamReader(string.Format("{0}/WebAPI.xml", Path.GetDirectoryName(assemblyFilename.Substring(prefix.Length))));
                }
                catch (FileNotFoundException exception)
                {
                    throw new Exception("XML documentation not present (make sure it is turned on in project properties when building)", exception);
                }

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(streamReader);
                return xmlDocument;
            }
            else
            {
                throw new Exception("Could not ascertain assembly filename", null);
            }
        }

        private string GetAssemblyVersion()
        {
            string assemblyFilename = assembly.CodeBase;

            const string prefix = "file:///";

            if (assemblyFilename.StartsWith(prefix))
            {
                try
                {
                    string location = string.Format("{0}/WebAPI.dll", Path.GetDirectoryName(assemblyFilename.Substring(prefix.Length)));
                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(location);
                    return fileVersionInfo.FileVersion;
                }
                catch (FileNotFoundException exception)
                {
                    throw new Exception("DLL not present (make sure it is turned on in project properties when building)", exception);
                }
            }
            else
            {
                throw new Exception("Could not ascertain assembly filename", null);
            }
        }

        internal bool validate()
        {
            bool valid = true;

            foreach (Type type in enums.OrderBy(myType => myType.Name))
            {
                if (!SchemeManager.Validate(type, false))
                    valid = false;
            }

            foreach (Type type in types)
            {
                //Skip master base class
                if (type == typeof(KalturaOTTObject))
                    continue;

                if (!SchemeManager.Validate(type, false))
                    valid = false;
            }

            foreach (Type controller in controllers.OrderBy(controller => controller.Name))
            {
                var controllerAttr = controller.GetCustomAttribute<ApiExplorerSettingsAttribute>(false);

                if (controllerAttr != null && controllerAttr.IgnoreApi)
                    continue;

                if (!SchemeManager.Validate(controller, false))
                    valid = false;
            }

            SchemeManager.ValidateErrors(exceptions, false);

            return valid;
        }

        internal void write()
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("xml");
            writer.WriteAttributeString("apiVersion", GetAssemblyVersion());
            writer.WriteAttributeString("generatedDate", SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow).ToString());
            
            //Printing enums
            writer.WriteStartElement("enums");
            foreach (Type type in enums.OrderBy(myType => myType.Name))
            {
                if (!SchemeManager.Validate(type, true))
                    continue;

                writeEnum(type);
            }

            //Hardcoding the status codes
            //var statusCodes = ClientsManager.ApiClient().GetErrorCodesDictionary();
            //writer.WriteStartElement("enum");
            //writer.WriteAttributeString("name", "KalturaStatusCodes");
            //writer.WriteAttributeString("enumType", "int");
            //foreach (var enumValue in statusCodes)
            //{
            //    writer.WriteStartElement("const");
            //    writer.WriteAttributeString("name", enumValue.Key);
            //    writer.WriteAttributeString("value", enumValue.Value.ToString());
            //    writer.WriteEndElement(); // const
            //}
            //foreach (var enumValue in Enum.GetValues(typeof(StatusCode)))
            //{
            //    int eVal = (int)Enum.Parse(typeof(StatusCode), enumValue.ToString());

            //    if (statusCodes.Where(status => status.Value == eVal).Count() == 0)
            //    {
            //        writer.WriteStartElement("const");
            //        writer.WriteAttributeString("name", enumValue.ToString());
            //        writer.WriteAttributeString("value", eVal.ToString());
            //        writer.WriteEndElement(); // const
            //    }
            //}
            //writer.WriteEndElement(); // enum

            writer.WriteEndElement(); // enums

            //Running on classes
            writer.WriteStartElement("classes");
            foreach (Type type in types)
            {
                if (!SchemeManager.Validate(type, true))
                    continue;

                writeType(type);
            }

            writer.WriteEndElement(); // classes

            //Running on methods
            writer.WriteStartElement("services");
            foreach (Type controller in controllers.OrderBy(controller => controller.Name))
            {
                if (!SchemeManager.Validate(controller, true))
                    continue;

                writeService(controller);
            }
            writer.WriteEndElement(); // services


            // errors
            if (SchemeManager.ValidateErrors(exceptions, true))
            {
                writer.WriteStartElement("errors");
                foreach (Type exception in exceptions.OrderBy(exception => exception.Name))
                {
                    writeErrors(exception);
                }
                writer.WriteEndElement(); // errors
            }


            //Config section
            writer.WriteStartElement("configurations");

            writer.WriteStartElement("client");
            writer.WriteAttributeString("type", "KalturaClientConfiguration");

            writer.WriteStartElement("clientTag");
            writer.WriteAttributeString("type", "string");
            writer.WriteEndElement(); // clientTag

            writer.WriteStartElement("apiVersion");
            writer.WriteAttributeString("type", "string");
            writer.WriteEndElement(); // apiVersion

            writer.WriteEndElement(); // client


            writer.WriteStartElement("request");
            writer.WriteAttributeString("type", "KalturaRequestConfiguration");

            writer.WriteStartElement("partnerId");
            writer.WriteAttributeString("type", "int");
            writer.WriteAttributeString("description", "Impersonated partner id");
            writer.WriteEndElement(); // partnerId

            writer.WriteStartElement("userId");
            writer.WriteAttributeString("type", "int");
            writer.WriteAttributeString("description", "Impersonated user id");
            writer.WriteEndElement(); // userId

            writer.WriteStartElement("language");
            writer.WriteAttributeString("type", "int");
            writer.WriteAttributeString("description", "Content language");
            writer.WriteEndElement(); // language

            writer.WriteStartElement("ks");
            writer.WriteAttributeString("type", "string");
            writer.WriteAttributeString("alias", "sessionId");
            writer.WriteAttributeString("description", "Kaltura API session");
            writer.WriteEndElement(); // ks

            writer.WriteEndElement(); // request

            writer.WriteEndElement(); // configurations

            writer.WriteEndElement(); // xml

            writer.WriteEndDocument();
            writer.Dispose();
        }

        private int[] Sort(List<Field> fields)
        {
            TopologicalSorter topologicalSorter = new TopologicalSorter(fields.Count);
            Dictionary<string, int> _indexes = new Dictionary<string, int>();

            //add vertices
            for (int i = 0; i < fields.Count; i++)
            {
                _indexes[fields[i].Name.ToLower()] = topologicalSorter.AddVertex(i);
            }

            //add edges
            for (int i = 0; i < fields.Count; i++)
            {
                if (fields[i].DependsOn != null)
                {
                    for (int j = 0; j < fields[i].DependsOn.Count; j++)
                    {
                        var name = fields[i].DependsOn[j];
                        if (!_indexes.ContainsKey(name.ToLower()))
                            throw new Exception(string.Format("Unable to find dependency [{0}] for type [{1}]", name, fields[i].Name));

                        topologicalSorter.AddEdge(i, _indexes[name.ToLower()]);
                    }
                }
            }

            return topologicalSorter.Sort();
        }

        private void writeErrors(Type exception)
        {
            FieldInfo[] fields = exception.GetFields();
            foreach (FieldInfo field in fields)
            {
                if (field.FieldType.IsSubclassOf(typeof(ApiException.ExceptionType)))
                {
                    ApiException.ExceptionType type = (ApiException.ExceptionType)field.GetValue(null);
                    writer.WriteStartElement("error");

                    if (type.GetType() == typeof(ApiException.ApiExceptionType))
                    {
                        ApiException.ApiExceptionType exceptionType = type as ApiException.ApiExceptionType;
                        writer.WriteAttributeString("name", exceptionType.statusCode.ToString());
                        writer.WriteAttributeString("code", exceptionType.statusCode.GetHashCode().ToString());
                        writer.WriteAttributeString("message", exceptionType.message);

                        foreach (string parameter in exceptionType.parameters)
                        {
                            writer.WriteStartElement("parameter");
                            writer.WriteAttributeString("name", parameter);
                            writer.WriteEndElement(); // parameter
                        }
                    }
                    else if (type.GetType() == typeof(ApiException.ClientExceptionType))
                    {
                        ApiException.ClientExceptionType exceptionType = type as ApiException.ClientExceptionType;
                        writer.WriteAttributeString("name", exceptionType.statusCode.ToString());
                        writer.WriteAttributeString("code", exceptionType.statusCode.GetHashCode().ToString());
                    }

                    writer.WriteEndElement(); // error
                }
            }
        }

        private void writeService(Type controller)
        {
            var serviceId = SchemeManager.getServiceId(controller);

            writer.WriteStartElement("service");
            writer.WriteAttributeString("name", serviceId);
            writer.WriteAttributeString("id", serviceId.ToLower());

            var methods = controller.GetMethods().OrderBy(method => method.Name);
            foreach (var method in methods)
            {
                if (!method.IsPublic || method.DeclaringType.Namespace != "WebAPI.Controllers")
                    continue;

                //Read only HTTP POST as we will have duplicates otherwise
                var explorerAttr = method.GetCustomAttributes<ApiExplorerSettingsAttribute>(false);
                if (explorerAttr.Count() > 0 && explorerAttr.First().IgnoreApi)
                    continue;

                if (!SchemeManager.Validate(method, true))
                    continue;

                writeAction(method);
            };

            writer.WriteEndElement(); // service
        }

        private void writeAction(MethodInfo action)
        {
            RouteAttribute route = action.GetCustomAttribute<RouteAttribute>(false);
            Type controller = action.ReflectedType;
            string serviceId = SchemeManager.getServiceId(controller);
            string actionId = route.Template;

            // string routePrefix = assembly.GetType("WebAPI.Controllers.ServiceController").GetCustomAttribute<RoutePrefixAttribute>().Prefix;
            
            writer.WriteStartElement("action");
            writer.WriteAttributeString("name", actionId);
            writer.WriteAttributeString("enableInMultiRequest", "1");
            writer.WriteAttributeString("supportedRequestFormats", "json");
            writer.WriteAttributeString("supportedResponseFormats", "json,xml");
            writer.WriteAttributeString("description", getDescription(action));
            // writer.WriteAttributeString("path", string.Format("/{0}/{1}/{2}", routePrefix, serviceId, actionId));
            if (action.GetCustomAttribute<ObsoleteAttribute>() != null)
                writer.WriteAttributeString("deprecated", "1");

            foreach (var param in action.GetParameters())
            {
                writer.WriteStartElement("param");
                writer.WriteAttributeString("name", param.Name);
                appendType(param.ParameterType);

                if (param.IsOptional)
                    writer.WriteAttributeString("default", param.DefaultValue == null ? "null" : param.DefaultValue.ToString());

                writer.WriteAttributeString("description", getDescription(action, param));
                writer.WriteAttributeString("optional", param.IsOptional ? "1" : "0");
                writer.WriteEndElement(); // param
            }

            writer.WriteStartElement("result");
            if (action.ReturnType != typeof(void))
                appendType(action.ReturnType);
            writer.WriteEndElement(); // result

            IEnumerable<ThrowsAttribute> actionErrors = action.GetCustomAttributes<ThrowsAttribute>(false);
            foreach (ThrowsAttribute error in actionErrors)
            {
                writer.WriteStartElement("throws");
                if (error.ApiCode.HasValue && errors.ContainsKey((int)error.ApiCode.Value))
                {
                    ApiException.ApiExceptionType exceptionType = errors[(int)error.ApiCode.Value] as ApiException.ApiExceptionType;
                    writer.WriteAttributeString("name", exceptionType.statusCode.ToString());
                }
                else if (error.ClientCode.HasValue && errors.ContainsKey((int)error.ClientCode.Value))
                {
                    ApiException.ClientExceptionType exceptionType = errors[(int)error.ClientCode.Value] as ApiException.ClientExceptionType;
                    writer.WriteAttributeString("name", exceptionType.statusCode.ToString());
                }
                writer.WriteEndElement(); // throws
            }

            writer.WriteEndElement(); // action
        }

        private string getDescription(PropertyInfo property)
        {
            return getDescription(string.Format("//member[@name='P:{0}.{1}']", property.ReflectedType, property.Name));
        }

        private string getDescription(Type type)
        {
            return getDescription(string.Format("//member[@name='T:{0}']", type.FullName));
        }

        private string getDescription(MethodInfo method)
        {
            if (method.GetParameters().Length > 0)
            {
                return getDescription(string.Format("//member[starts-with(@name,'M:{0}.{1}(')]", method.ReflectedType.FullName, method.Name));
            }
            else
            {
                return getDescription(string.Format("//member[@name='M:{0}.{1}']", method.ReflectedType.FullName, method.Name));
            }
        }

        private string getDescription(MethodInfo method, ParameterInfo param)
        {
            return getDescription(string.Format("//member[starts-with(@name,'M:{0}.{1}')]/param[@name='{2}']", method.ReflectedType.FullName, method.Name, param.Name));
        }
        
        private string getDescription(string xPath)
        {
            var classNode = assemblyXml.SelectNodes(xPath);
            if (classNode == null || classNode.Count == 0 || classNode[0].ChildNodes == null)
                return "";

            foreach (XmlNode child in classNode[0].ChildNodes)
            {
                if (child.Name == "summary")
                    return HttpUtility.HtmlEncode(child.InnerText.Trim());
            }

            return classNode[0].InnerText.Trim();
        }
        
        private void writeType(Type type)
        {
            string typeName = type.Name;
            if (type.IsGenericType && typeName.StartsWith("KalturaFilter"))
                typeName = "KalturaFilter";

            writer.WriteStartElement("class");
            writer.WriteAttributeString("name", typeName);
            writer.WriteAttributeString("description", getDescription(type));

            if (type.BaseType != null && type.BaseType != typeof(KalturaOTTObject))
            {
                string baseType = type.BaseType.Name;
                if (type.BaseType.IsGenericType && baseType.StartsWith("KalturaFilter"))
                    baseType = "KalturaFilter";

                writer.WriteAttributeString("base", baseType);
            }

            if (type.IsInterface || type.IsAbstract)
                writer.WriteAttributeString("abstract", "1");


            List<PropertyInfo> properties = type.GetProperties().ToList();

            //Remove properties from base
            List<PropertyInfo> baseProps = type.BaseType.GetProperties().ToList();
            baseProps.RemoveAll(myProperty => myProperty.GetCustomAttribute<ObsoleteAttribute>(false) != null);
            List<string> basePropsNames = baseProps.Select(myProperty => myProperty.Name).ToList();
            properties.RemoveAll(myProperty => basePropsNames.Contains(myProperty.Name));

            foreach (var property in properties)
            {
                ObsoleteAttribute obsolete = property.GetCustomAttribute<ObsoleteAttribute>(true);
                if (obsolete != null)
                    continue;

                var dataMemberAttr = property.GetCustomAttribute<DataMemberAttribute>();
                if (dataMemberAttr == null)
                    continue;


                writer.WriteStartElement("property");
                writer.WriteAttributeString("name", dataMemberAttr.Name);

                if (typeName == "KalturaFilter" && dataMemberAttr.Name == "orderBy")
                {
                    appendType(typeof(int));
                }
                else
                {
                    appendType(property.PropertyType);
                }

                writer.WriteAttributeString("description", getDescription(property));

                SchemePropertyAttribute schemeProperty = property.GetCustomAttribute<SchemePropertyAttribute>();
                if (schemeProperty == null)
                {
                    writer.WriteAttributeString("readOnly", "0");
                    writer.WriteAttributeString("insertOnly", "0");
                }
                else
                {
                    writer.WriteAttributeString("readOnly", schemeProperty.ReadOnly ? "1" : "0");
                    writer.WriteAttributeString("insertOnly", schemeProperty.InsertOnly ? "1" : "0");
                    writer.WriteAttributeString("writeOnly", schemeProperty.WriteOnly ? "1" : "0");

                    if (schemeProperty.DynamicType != null)
                        writer.WriteAttributeString("valuesEnumType", schemeProperty.DynamicType.Name);

                    if (schemeProperty.DynamicMinInt > int.MinValue)
                        writer.WriteAttributeString("valuesMinValue", schemeProperty.DynamicMinInt.ToString());
                    if (schemeProperty.DynamicMaxInt < int.MaxValue)
                        writer.WriteAttributeString("valuesMaxValue", schemeProperty.DynamicMaxInt.ToString());

                    if (schemeProperty.RequiresPermission > 0)
                    {
                        RequestType[] validPermissions = new RequestType[] { RequestType.READ, RequestType.UPDATE, RequestType.INSERT };
                        string[] permissions = (string[]) validPermissions.Where((t, i) => ((int)t & schemeProperty.RequiresPermission) > 0).Select(t => t.ToString().ToLower()).ToArray();
                        writer.WriteAttributeString("requiresPermissions", string.Join(",", permissions));
                    }

                    if (schemeProperty.MaxInteger < int.MaxValue)
                        writer.WriteAttributeString("maxValue", schemeProperty.MaxInteger.ToString());
                    else if (schemeProperty.MaxLong < long.MaxValue)
                        writer.WriteAttributeString("maxValue", schemeProperty.MaxLong.ToString());
                    else if (schemeProperty.MaxFloat < float.MaxValue)
                        writer.WriteAttributeString("maxValue", schemeProperty.MaxFloat.ToString());

                    if (schemeProperty.MinInteger < int.MinValue)
                        writer.WriteAttributeString("minValue", schemeProperty.MinInteger.ToString());
                    else if (schemeProperty.MinLong < long.MinValue)
                        writer.WriteAttributeString("minValue", schemeProperty.MinLong.ToString());
                    else if (schemeProperty.MinFloat < float.MinValue)
                        writer.WriteAttributeString("minValue", schemeProperty.MinFloat.ToString());

                }
                writer.WriteEndElement(); // property
            }

            writer.WriteEndElement(); // class
        }

        private void writeEnum(Type type)
        {
            bool isIntEnum = type.GetCustomAttribute<KalturaIntEnumAttribute>() != null;
            var etype = isIntEnum ? "int" : "string";

            writer.WriteStartElement("enum");
            writer.WriteAttributeString("name", type.Name);
            writer.WriteAttributeString("enumType", isIntEnum ? "int" : "string");

            //Print values
            foreach (var enumValue in Enum.GetValues(type))
            {
                string eValue = isIntEnum ? ((int)Enum.Parse(type, enumValue.ToString())).ToString() : enumValue.ToString();
                writer.WriteStartElement("const");
                writer.WriteAttributeString("name", enumValue.ToString().ToUpper());
                writer.WriteAttributeString("value", eValue);
                writer.WriteEndElement(); // const
            }

            writer.WriteEndElement(); // enum
        }

        private string getTypeName(Type type)
        {
            if (type == typeof(String))
                return "string";
            if (type == typeof(DateTime))
                return "int";
            if (type == typeof(long) || type == typeof(Int64))
                return "bigint";
            if (type == typeof(Int32))
                return "int";
            if (type == typeof(double) || type == typeof(float))
                return "float";
            if (type == typeof(bool))
                return "bool";
            if (type.IsEnum)
                return type.Name;

            if (typeof(KalturaRenderer).IsAssignableFrom(type))
                return "file";

            return type.Name;
        }

        private void appendType(Type type)
        {
            //Handling nullables
            if (Nullable.GetUnderlyingType(type) != null)
            {
                type = type.GetGenericArguments()[0];
            }

            //Handling Enums
            if (type.IsEnum)
            {
                bool isIntEnum = type.GetCustomAttribute<KalturaIntEnumAttribute>() != null;
                var etype = isIntEnum ? "int" : "string";

                writer.WriteAttributeString("type", isIntEnum ? "int" : "string");
                writer.WriteAttributeString("enumType", getTypeName(type));
                return;
            }

            //Handling arrays
            if (type.IsArray)
            {
                writer.WriteAttributeString("type", "array");
                writer.WriteAttributeString("arrayType", getTypeName(type.GetElementType()));
                return;
            }

            if (type.IsGenericType)
            {

                //if List
                if (type.GetGenericArguments().Count() == 1)
                {
                    writer.WriteAttributeString("type", "array");
                    writer.WriteAttributeString("arrayType", getTypeName(type.GetGenericArguments()[0]));
                    return;
                }

                //if Dictionary
                Type dictType = typeof(SerializableDictionary<,>);
                if (type.GetGenericArguments().Count() == 2 &&
                    dictType.GetGenericArguments().Length == type.GetGenericArguments().Length &&
                    dictType.MakeGenericType(type.GetGenericArguments()) == type)
                {
                    writer.WriteAttributeString("type", "map");
                    writer.WriteAttributeString("arrayType", getTypeName(type.GetGenericArguments()[1]));
                    return;
                }

                if (type.GetGenericArguments().Count() == 2)
                    throw new Exception("Dont know how to handle");

                throw new Exception("Generic type unknown");
            }

            writer.WriteAttributeString("type", getTypeName(type));
        }

    }
}