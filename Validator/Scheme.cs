using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using WebAPI.App_Start;
using WebAPI.Controllers;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Renderers;
using Newtonsoft.Json;
using TVinciShared;
using WebAPI.Models.API;
using WebAPI;
using Microsoft.AspNetCore.Mvc;

namespace Validator.Managers.Scheme
{
    internal class Field
    {
        static public Dictionary<string, Field> loadedTypes = new Dictionary<string, Field>();

        public Type type;
        private List<string> _dependsOn;

        public Field(Type type)
        {
            this.type = type;
            _dependsOn = new List<string>();

            AddDependencies(type, false);
        }

        private void AddDependencies(Type type, bool ignoreCurrent = true)
        {
            if (ignoreCurrent && type.Name.Equals(this.type.Name))
                return;

            if (_dependsOn.Contains(type.Name))
                return;

            if (type == typeof(KalturaOTTObject))
                return;

            if (type.IsSubclassOf(typeof(KalturaOTTObject)))
            {
                if (!type.Name.Equals(this.type.Name))
                    _dependsOn.Add(type.Name);

                SchemeBaseAttribute schemeBaseAttribute = type.GetCustomAttribute<SchemeBaseAttribute>();
                if (schemeBaseAttribute != null)
                    AddDependencies(schemeBaseAttribute.BaseType);

                if (type.BaseType != null)
                    AddDependencies(type.BaseType);

                foreach (var property in type.GetProperties())
                {
                    AddDependencies(property.PropertyType);
                }
            }
            else if (type.IsInterface)
            {
                SchemeBaseAttribute schemeBaseAttribute = type.GetCustomAttribute<SchemeBaseAttribute>();
                if (schemeBaseAttribute != null)
                    AddDependencies(schemeBaseAttribute.BaseType);
            }
            else if (type.IsGenericType)
            {
                foreach (var genericArgument in type.GetGenericArguments())
                {
                    AddDependencies(genericArgument);
                }
            }
            else if (type.IsArray)
            {
                var arrType = type.GetElementType();
                AddDependencies(arrType);
            }
        }

        public IEnumerable<Field> getDependencies()
        {
            return loadedTypes.Values.Where(field => _dependsOn.Contains(field.Name));
        }

        public string Name
        {
            get
            {
                return type.Name;
            }
        }
    }

    internal class Scheme
    {
        private Stream stream;
        private Assembly assembly;
        private Assembly validatorAssembly;
        private XmlDocument assemblyXml;
        private XmlWriter writer;

        private List<Type> enums = new List<Type>();
        private IEnumerable<Type> types;
        private IEnumerable<Type> controllers;
        private IEnumerable<Type> exceptions;
        private Dictionary<int, ApiException.ExceptionType> errors = new Dictionary<int, ApiException.ExceptionType>();

        private static Scheme scheme;
        public static Scheme getInstance()
        {
            if (scheme != null)
                return scheme;

            scheme = new Scheme();
            return scheme;
        }

        protected Scheme()
        {
            this.assembly = Assembly.Load("WebAPI");
            this.validatorAssembly = Assembly.GetExecutingAssembly();
            this.assemblyXml = GetAssemblyXml();

            Load();
        }

        public void RemoveInvalidObjects()
        {
            controllers = controllers.Where(controller => SchemeManager.Validate(controller, false, assemblyXml));
            enums = enums.Where(type => SchemeManager.Validate(type, false, assemblyXml)).ToList();
            types = types.Where(type => SchemeManager.Validate(type, false, assemblyXml)).ToList();
        }

        private void Load()
        {
            controllers = assembly.GetTypes().Where(myType => myType.IsClass && typeof(IKalturaController).IsAssignableFrom(myType));
            exceptions = assembly.GetTypes().Where(myType => myType.IsClass && (myType.IsSubclassOf(typeof(ApiException)) || myType == typeof(ApiException)));

            foreach (Type exception in exceptions.OrderBy(exception => exception.Name))
            {
                loadErrors(exception);
            }

            LoadType(typeof(KalturaApiExceptionArg));
            LoadType(typeof(KalturaClientConfiguration));
            LoadType(typeof(KalturaRequestConfiguration));
            LoadType(typeof(KalturaResponseType));

            foreach (Type controller in controllers)
            {
                var apiExplorerSettings = controller.GetCustomAttribute<ApiExplorerSettingsAttribute>(false);
                if (apiExplorerSettings != null && apiExplorerSettings.IgnoreApi)
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

                    foreach (var param in method.GetParameters())
                    {
                        LoadType(param.ParameterType);
                    }

                    if (method.ReturnType != null)
                    {
                        LoadType(method.ReturnType);
                    }
                };

                if (SchemeManager.IsCrudController(controller, out Dictionary<string, CrudActionAttribute> crudActionAttributes, out Dictionary<string, MethodInfo> crudActions))
                {
                    foreach (var crudActionAttribute in crudActionAttributes)
                    {
                        foreach (var param in crudActions[crudActionAttribute.Key].GetParameters())
                        {
                            LoadType(param.ParameterType);
                        }

                        if (crudActions[crudActionAttribute.Key].ReturnType != null)
                        {
                            LoadType(crudActions[crudActionAttribute.Key].ReturnType);
                        }
                    }
                }
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
            types = FixTypeDependencies(Field.loadedTypes.Values);
        }

        private IEnumerable<Type> FixTypeDependencies(IEnumerable<Field> input)
        {
            return FixTypeDependencies(input, new List<Type>(), new List<string>());
        }

        private IEnumerable<Type> FixTypeDependencies(IEnumerable<Field> input, List<Type> output, List<string> added)
        {
            foreach (Field field in input)
            {
                if (output.Contains(field.type))
                    continue;       // already added

                if (added.Contains(field.Name))
                    continue;

                added.Add(field.Name);
                IEnumerable<Field> dependencies = field.getDependencies();
                FixTypeDependencies(dependencies, output, added);

                output.Add(field.type);
            }
            return output;
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
                        if (errors.ContainsKey((int)exceptionType.statusCode))
                        {
                            throw new Exception("Error code " + exceptionType.statusCode + " appears twice: ApiException.ApiExceptionType." + exceptionType.name);
                        }
                        else
                        {
                            errors.Add((int)exceptionType.statusCode, type);
                        }
                    }
                    else if (type.GetType() == typeof(ApiException.ClientExceptionType))
                    {
                        ApiException.ClientExceptionType exceptionType = type as ApiException.ClientExceptionType;
                        errors.Add((int)exceptionType.statusCode, type);
                    }
                }
            }
        }

        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic.Name.Equals(cur.Name))
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        private void LoadType(Type type)
        {
            if (type.IsEnum && !enums.Contains(type))
            {
                enums.Add(type);
                return;
            }

            if (Field.loadedTypes.ContainsKey(type.Name))
            {
                return;
            }

            if (type == typeof(KalturaOTTObject))
            {
                LoadTypeProperties(type);
                return;
            }

            string typeName = GetTypeName(type);
            if (typeof(IKalturaOTTObject).IsAssignableFrom(type) && !Field.loadedTypes.ContainsKey(typeName))
            {
                Field.loadedTypes.Add(typeName, new Field(type));

                SchemeBaseAttribute schemeBaseAttribute = type.GetCustomAttribute<SchemeBaseAttribute>();
                if (schemeBaseAttribute != null)
                {
                    LoadType(schemeBaseAttribute.BaseType);
                }

                if (type.IsClass)
                {
                    LoadType(type.BaseType);

                    var subClasses = assembly.GetTypes().Where(myType => IsSubclassOfRawGeneric(type, myType));
                    foreach (Type subClass in subClasses)
                        LoadType(subClass);

                    LoadTypeProperties(type);
                }
                return;
            }

            if (type.IsArray)
            {
                LoadType(type.GetElementType());
            }
            else if (type.IsGenericType)
            {
                foreach (Type GenericArgument in type.GetGenericArguments())
                {
                    LoadType(GenericArgument);
                }
            }
        }

        private void LoadTypeProperties(Type type)
        {
            List<PropertyInfo> properties = type.GetProperties().ToList();
            
            foreach (var property in properties)
            {
                if (property.DeclaringType == type)
                    LoadType(property.PropertyType);
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

            try
            {
                foreach (Type enumType in enums.OrderBy(myType => myType.Name))
                {
                    if (!SchemeManager.Validate(enumType, false, assemblyXml))
                        valid = false;
                }

                foreach (Type type in types)
                {
                    //Skip master base class
                    if (type == typeof(KalturaOTTObject))
                        continue;

                    if (!SchemeManager.Validate(type, false, assemblyXml))
                        valid = false;
                }

                foreach (Type controller in controllers.OrderBy(controller => controller.Name))
                {
                    var controllerAttr = controller.GetCustomAttribute<ApiExplorerSettingsAttribute>(false);

                    if (controllerAttr != null && controllerAttr.IgnoreApi)
                        continue;

                    if (!SchemeManager.Validate(controller, false, assemblyXml))
                        valid = false;
                }

                SchemeManager.ValidateErrors(exceptions, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error: there was an exception in Scheme.validate(): {0}", ex.ToString()));
                valid = false;
                throw;
            }
            
            return valid;
        }
        
        internal void write(Stream stream)
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

            writer.WriteStartDocument();
            writer.WriteStartElement("xml");
            writer.WriteAttributeString("apiVersion", GetAssemblyVersion());
            writer.WriteAttributeString("generatedDate", DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow).ToString());

            //Printing enums
            writer.WriteStartElement("enums");
            foreach (Type type in enums.OrderBy(myType => myType.Name))
            {
                if (!SchemeManager.Validate(type, true, assemblyXml))
                    continue;

                writeEnum(type);
            }
            writer.WriteEndElement(); // enums

            //Running on classes
            writer.WriteStartElement("classes");
            foreach (Type type in types)
            {
                if (!SchemeManager.Validate(type, true, assemblyXml) || type.Name == "KalturaListResponseT" || type.Name == "KalturaFilterT")
                    continue;
                
                WriteClass(GetClassDetails(type));
            }
            writer.WriteEndElement(); // classes

            //Running on methods
            writer.WriteStartElement("services");
            foreach (Type controller in controllers.OrderBy(controller => controller.Name))
            {
                if (!SchemeManager.Validate(controller, true, assemblyXml) || controller.IsAbstract)
                    continue;
                
                WriteService(GetControllerDetails(controller));
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
            writer.WriteAttributeString("description", "Client tag");
            writer.WriteEndElement(); // clientTag

            writer.WriteStartElement("apiVersion");
            writer.WriteAttributeString("type", "string");
            writer.WriteAttributeString("description", "API Version");
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
            writer.WriteAttributeString("type", "string");
            writer.WriteAttributeString("description", "Content language");
            writer.WriteEndElement(); // language

            writer.WriteStartElement("currency");
            writer.WriteAttributeString("type", "string");
            writer.WriteAttributeString("description", "Content currency");
            writer.WriteEndElement(); // currency

            writer.WriteStartElement("ks");
            writer.WriteAttributeString("type", "string");
            writer.WriteAttributeString("alias", "sessionId");
            writer.WriteAttributeString("description", "Kaltura API session");
            writer.WriteEndElement(); // ks

            writer.WriteStartElement("responseProfile");
            writer.WriteAttributeString("type", "KalturaBaseResponseProfile");
            writer.WriteAttributeString("volatile", "1");
            writer.WriteAttributeString("description", "Response profile - this attribute will be automatically unset after every API call");
            writer.WriteEndElement(); // responseProfile

            writer.WriteStartElement("abortOnError");
            writer.WriteAttributeString("type", "bool");
            writer.WriteAttributeString("description", "Abort the Multireuqset call if any error occurs in one of the requests");
            writer.WriteEndElement(); // abortOnError

            writer.WriteStartElement("abortAllOnError");
            writer.WriteAttributeString("type", "bool");
            writer.WriteAttributeString("description", "Abort all following requests if current request has an error");
            writer.WriteEndElement(); // abortAllOnError

            writer.WriteStartElement("skipCondition");
            writer.WriteAttributeString("type", "KalturaSkipCondition");
            writer.WriteAttributeString("description", "Skip current request according to skip condition");
            writer.WriteEndElement(); // skipCondition

            writer.WriteEndElement(); // request

            writer.WriteEndElement(); // configurations

            writer.WriteEndElement(); // xml

            writer.WriteEndDocument();
            writer.Dispose();
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
                        writer.WriteAttributeString("name", exceptionType.name);
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
                        writer.WriteAttributeString("description", exceptionType.description);
                    }

                    writer.WriteEndElement(); // error
                }
            }
        }

        private void WriteService(KalturaControllerDetails controller)
        {
            writer.WriteStartElement("service");
            writer.WriteAttributeString("name", controller.ServiceId);
            writer.WriteAttributeString("id", controller.ServiceId.ToLower());

            foreach (var action in controller.Actions)
            {
                writeAction(action, controller);
            }
            
            writer.WriteEndElement(); // service
        }

        private void writeAction(KalturaActionDetails action, KalturaControllerDetails controller)
        {
            writer.WriteStartElement("action");
            writer.WriteAttributeString("name", action.Name);
            writer.WriteAttributeString("enableInMultiRequest", action.IsGenericMethod ? "0" : "1");
            writer.WriteAttributeString("supportedRequestFormats", "json");
            writer.WriteAttributeString("supportedResponseFormats", "json,xml");
            writer.WriteAttributeString("description", action.Description);
            
            if (action.IsDeprecated)
            {
                writer.WriteAttributeString("deprecated", "1");
            }

            writer.WriteAttributeString("sessionRequired", action.IsSessionRequired ? "always" : "none");

            foreach (var param in action.Prameters)
            {
                writer.WriteStartElement("param");
                writer.WriteAttributeString("name", param.Name);
                foreach (var prameterType in param.ParameterTypes)
                {
                    writer.WriteAttributeString(prameterType.Key, prameterType.Value);
                }
                
                if (param.IsOptional)
                {
                    writer.WriteAttributeString("default", param.DefaultValue);
                }

                writer.WriteAttributeString("description", param.Description);
                writer.WriteAttributeString("optional", param.IsOptional ? "1" : "0");
                writer.WriteEndElement(); // param
            }

            writer.WriteStartElement("result");
            foreach (var returnedType in action.ReturnedTypes)
            {
                writer.WriteAttributeString(returnedType.Key, returnedType.Value);
            }
            writer.WriteEndElement(); // result

            // write throws
            foreach (var apiCode in action.ApiThrows)
            {
                if (errors.ContainsKey((int)apiCode))
                {
                    writer.WriteStartElement("throws");
                    var exceptionType = errors[(int)apiCode] as ApiException.ApiExceptionType;
                    writer.WriteAttributeString("name", exceptionType.name);
                    writer.WriteEndElement(); // throws apiCode
                }
            }

            foreach (var clientCode in action.ClientThrows)
            {
                if (errors.ContainsKey((int)clientCode))
                {
                    writer.WriteStartElement("throws");
                    var exceptionType = errors[(int)clientCode] as ApiException.ClientExceptionType;
                    writer.WriteAttributeString("name", exceptionType.statusCode.ToString());
                    writer.WriteEndElement(); // throws clientCode
                }
            }
            
            writer.WriteEndElement(); // action
        }
        
        internal string getDescription(MethodInfo method, ParameterInfo param)
        {
            return SchemeManager.GetDescriptionByAssemblyXml(string.Format("//member[starts-with(@name,'M:{0}.{1}')]/param[@name='{2}']", method.ReflectedType.FullName, method.Name, param.Name), assemblyXml);
        }
        
        private void WriteClass(KalturaClassDetails classDetails)
        {
            writer.WriteStartElement("class");
            writer.WriteAttributeString("name", classDetails.Name);
            writer.WriteAttributeString("description", classDetails.Description);
            if (!string.IsNullOrEmpty(classDetails.BaseName))
            {
                writer.WriteAttributeString("base", classDetails.BaseName);
            }
            
            if (classDetails.IsAbstract)
            {
                writer.WriteAttributeString("abstract", "1");
            }
            
            foreach (var property in classDetails.Properties)
            {
                WriteProperty(classDetails.Name, property);
            }

            writer.WriteEndElement(); // class
        }

        private void WriteProperty(string typeName, KalturaPropertyDetails propertyDetails)
        {
            if (propertyDetails.Obsolete != null || 
                propertyDetails.JsonIgnore != null || 
                propertyDetails.Deprecated != null || 
                propertyDetails.DataMember == null)
            {
                return;
            }
            
            writer.WriteStartElement("property");
            writer.WriteAttributeString("name", propertyDetails.Name);
            AppendType(propertyDetails.PropertyType);

            writer.WriteAttributeString("description", propertyDetails.Description);

            if (propertyDetails.SchemeProperty == null)
            {
                writer.WriteAttributeString("readOnly", "0");
                writer.WriteAttributeString("insertOnly", "0");
            }
            else
            {
                writer.WriteAttributeString("readOnly", propertyDetails.SchemeProperty.ReadOnly ? "1" : "0");
                writer.WriteAttributeString("insertOnly", propertyDetails.SchemeProperty.InsertOnly ? "1" : "0");
                writer.WriteAttributeString("writeOnly", propertyDetails.SchemeProperty.WriteOnly ? "1" : "0");
                writer.WriteAttributeString("nullable", propertyDetails.SchemeProperty.IsNullable ? "1" : "0");

                if (propertyDetails.SchemeProperty.DynamicType != null)
                    writer.WriteAttributeString("valuesEnumType", propertyDetails.SchemeProperty.DynamicType.Name);

                if (propertyDetails.SchemeProperty.DynamicMinInt > int.MinValue)
                    writer.WriteAttributeString("valuesMinValue", propertyDetails.SchemeProperty.DynamicMinInt.ToString());
                if (propertyDetails.SchemeProperty.DynamicMaxInt < int.MaxValue)
                    writer.WriteAttributeString("valuesMaxValue", propertyDetails.SchemeProperty.DynamicMaxInt.ToString());

                if (propertyDetails.SchemeProperty.RequiresPermission > 0)
                {
                    var validPermissions = new RequestType[] { RequestType.READ, RequestType.UPDATE, RequestType.INSERT };
                    var permissions = validPermissions.Where((t, i) => ((int)t & propertyDetails.SchemeProperty.RequiresPermission) > 0).Select(t => t.ToString().ToLower()).ToArray();
                    writer.WriteAttributeString("requiresPermissions", string.Join(",", permissions));
                }

                if (propertyDetails.SchemeProperty.MaxInteger < int.MaxValue)
                    writer.WriteAttributeString("maxValue", propertyDetails.SchemeProperty.MaxInteger.ToString());
                else if (propertyDetails.SchemeProperty.MaxLong < long.MaxValue)
                    writer.WriteAttributeString("maxValue", propertyDetails.SchemeProperty.MaxLong.ToString());
                else if (propertyDetails.SchemeProperty.MaxFloat < float.MaxValue)
                    writer.WriteAttributeString("maxValue", propertyDetails.SchemeProperty.MaxFloat.ToString());

                if (propertyDetails.SchemeProperty.MinInteger < int.MinValue)
                    writer.WriteAttributeString("minValue", propertyDetails.SchemeProperty.MinInteger.ToString());
                else if (propertyDetails.SchemeProperty.MinLong < long.MinValue)
                    writer.WriteAttributeString("minValue", propertyDetails.SchemeProperty.MinLong.ToString());
                else if (propertyDetails.SchemeProperty.MinFloat < float.MinValue)
                    writer.WriteAttributeString("minValue", propertyDetails.SchemeProperty.MinFloat.ToString());

            }
            writer.WriteEndElement(); // property
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
                MemberInfo[] memberInfo = type.GetMember(enumValue.ToString());
                object[] attributes = memberInfo[0].GetCustomAttributes(typeof(ObsoleteAttribute), false);
                if (attributes.Length > 0)
                {
                    continue;
                }

                string eValue = isIntEnum ? ((int)Enum.Parse(type, enumValue.ToString())).ToString() : enumValue.ToString();
                writer.WriteStartElement("const");
                writer.WriteAttributeString("name", enumValue.ToString().ToUpper());
                writer.WriteAttributeString("value", eValue);
                writer.WriteEndElement(); // const
            }

            writer.WriteEndElement(); // enum
        }
        
        private void AppendType(Type type)
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
                writer.WriteAttributeString("enumType", GetTypeName(type));
                return;
            }

            //Handling arrays
            if (type.IsArray)
            {
                writer.WriteAttributeString("type", "array");
                writer.WriteAttributeString("arrayType", GetTypeName(type.GetElementType()));
                return;
            }

            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                
                if (genericTypeDefinition == typeof(List<>))
                {
                    writer.WriteAttributeString("type", "array");
                    writer.WriteAttributeString("arrayType", GetTypeName(type.GetGenericArguments()[0]));
                    return;
                }

                if (genericTypeDefinition == typeof(Dictionary<,>) || genericTypeDefinition == typeof(SerializableDictionary<,>))
                {
                    writer.WriteAttributeString("type", "map");
                    writer.WriteAttributeString("arrayType", GetTypeName(type.GetGenericArguments()[1]));
                    return;
                }
            }

            writer.WriteAttributeString("type", GetTypeName(type));
        }

        private string GetTypeName(Type type)
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

            if (type == typeof(KalturaOTTFile))
                return "file";

            if (typeof(KalturaRenderer).IsAssignableFrom(type))
                return "file";

            Regex regex = new Regex("^[^`]+");
            Match match = regex.Match(type.Name);
            return match.Value;
        }

        private KalturaControllerDetails GetControllerDetails(Type controllerType)
        {
            var controllerDetails = new KalturaControllerDetails()
            {
                ServiceId = SchemeManager.getServiceId(controllerType),
            };

            if (SchemeManager.IsCrudController(controllerType, out Dictionary<string, CrudActionAttribute> crudActionAttributes, out Dictionary<string, MethodInfo> crudActions))
            {
                controllerDetails.IsCrudController = true;
                foreach (var crudActionAttribute in crudActionAttributes)
                {
                    if (crudActions.ContainsKey(crudActionAttribute.Key))
                    {
                        controllerDetails.Actions.Add(GetCrudActionDetails(crudActionAttribute.Value, crudActions[crudActionAttribute.Key]));
                    }
                }
            }

            var methods = controllerType.GetMethods().OrderBy(method => method.Name);
            foreach (var method in methods)
            {
                if (!method.IsPublic || method.DeclaringType.Namespace != "WebAPI.Controllers")
                    continue;

                if (!SchemeManager.ValidateMethod(method, false, assemblyXml))
                    continue;

                //Read only HTTP POST as we will have duplicates otherwise
                var explorerAttr = method.GetCustomAttributes<ApiExplorerSettingsAttribute>(false);
                if (explorerAttr.Count() > 0 && explorerAttr.First().IgnoreApi)
                    continue;

                controllerDetails.Actions.Add(GetActionDetails(method));
            };

            return controllerDetails;
        }

        private KalturaActionDetails GetCrudActionDetails(CrudActionAttribute actionAttribute, MethodInfo method)
        {
            var crudActionDetails = new KalturaActionDetails()
            {
                Name = actionAttribute.GetName(),
                Description = actionAttribute.Summary,
                IsGenericMethod = false,
                IsDeprecated = false,
                IsSessionRequired = true,
            };

            var parameters = method.GetParameters();
            foreach (var parameter in parameters)
            {
                crudActionDetails.Prameters.Add(GetCrudPrameterDetails(parameter, method, actionAttribute));
            }

            if (method.ReturnType != typeof(void))
            {
                crudActionDetails.ReturnedTypes = GetParameterTypes(method.ReturnType);
            }

            // write throws
            if (actionAttribute.ApiThrows != null && actionAttribute.ApiThrows.Length > 0)
            {
                crudActionDetails.ApiThrows.AddRange(actionAttribute.ApiThrows);
            }

            if ((actionAttribute.ClientThrows != null && actionAttribute.ClientThrows.Length > 0))
            {
                crudActionDetails.ClientThrows.AddRange(actionAttribute.ClientThrows);
            }

            return crudActionDetails;
        }

        private KalturaActionDetails GetActionDetails(MethodInfo method)
        {
            var actionDetails = new KalturaActionDetails()
            {
                IsGenericMethod = method.IsGenericMethod,
                Description = SchemeManager.GetMethodDescription(method, assemblyXml),
                IsDeprecated = method.GetCustomAttribute<ObsoleteAttribute>() != null,
                IsSessionRequired = method.GetCustomAttribute<ApiAuthorizeAttribute>() != null,
            };

            var actionAttribute = method.GetCustomAttribute<ActionAttribute>(false);
            if (actionAttribute != null)
            {
                actionDetails.Name = actionAttribute.Name;
            }

            var parameters = method.GetParameters();
            foreach (var parameter in parameters)
            {
                actionDetails.Prameters.Add(GetPrameterDetails(parameter, method));
            }

            if (method.ReturnType != typeof(void))
            {
                actionDetails.ReturnedTypes = GetParameterTypes(method.ReturnType);
            }

            IEnumerable<ThrowsAttribute> actionErrors = method.GetCustomAttributes<ThrowsAttribute>(false);
            foreach (ThrowsAttribute error in actionErrors)
            {
                if (error.ApiCode.HasValue)
                {
                    actionDetails.ApiThrows.Add(error.ApiCode.Value);
                }
                else if (error.ClientCode.HasValue)
                {
                    actionDetails.ClientThrows.Add(error.ClientCode.Value);
                }
            }

            return actionDetails;
        }

        private KalturaPrameterDetails GetPrameterDetails(ParameterInfo parameter, MethodInfo method)
        {
            var prameterDetails = new KalturaPrameterDetails()
            {
                Name = parameter.Name,
                ParameterTypes = GetParameterTypes(parameter.ParameterType),
                IsOptional = parameter.IsOptional,
                Description = GetParameterDescription(parameter, method)
            };

            if (parameter.IsOptional)
            {
                prameterDetails.DefaultValue = parameter.DefaultValue == null ? "null" : (parameter.DefaultValue is bool ? parameter.DefaultValue.ToString().ToLower() : parameter.DefaultValue.ToString());
            }

            return prameterDetails;
        }

        private KalturaPrameterDetails GetCrudPrameterDetails(ParameterInfo parameter, MethodInfo method, CrudActionAttribute actionAttribute)
        {
            var prameterDetails = new KalturaPrameterDetails()
            {
                Name = parameter.Name,
                ParameterTypes = GetParameterTypes(parameter.ParameterType),
                IsOptional = SchemeManager.IsParameterOptional(parameter, actionAttribute.GetOptionalParameters()),
                Description = actionAttribute.GetDescription(parameter.Name),
            };

            if (parameter.IsOptional)
            {
                prameterDetails.DefaultValue = SchemeManager.VarToString(parameter.DefaultValue);
            }

            return prameterDetails;
        }

        private KalturaClassDetails GetClassDetails(Type classType)
        {
            var kalturaClassDetails = new KalturaClassDetails()
            {
                Name = GetTypeName(classType),
                Description = GetTypeDescription(classType),
                IsAbstract = classType.IsInterface || classType.IsAbstract
            };

            var schemeBaseAttribute = classType.GetCustomAttribute<SchemeBaseAttribute>();
            if (schemeBaseAttribute != null)
            {
                kalturaClassDetails.BaseName = GetTypeName(schemeBaseAttribute.BaseType);
            }
            else if (classType.BaseType != null && classType.BaseType != typeof(KalturaOTTObject))
            {
                kalturaClassDetails.BaseName = GetTypeName(classType.BaseType);
            }

            var properties = classType.GetProperties().ToList();
            ListResponseAttribute listResponseAttribute = null;
            if (classType.BaseType != null)
            {
                //Remove properties from base
                var baseProps = classType.BaseType.GetProperties().ToList();
                baseProps.RemoveAll(myProperty => myProperty.GetCustomAttribute<ObsoleteAttribute>(false) != null);
                if (baseProps.Count > 0)
                {
                    var basePropsNames = new HashSet<string>(baseProps.Select(myProperty => myProperty.Name));
                    properties.RemoveAll(myProperty => basePropsNames.Contains(myProperty.Name));
                }
            }

            var propertyToDescription = new Dictionary<PropertyInfo, string>();
            foreach (var property in properties)
            {
                propertyToDescription.Add(property, GetPropertyDescription(property));
            }

            if (SchemeManager.IsGenericListResponse(classType, out listResponseAttribute, out PropertyInfo objectsProperty))
            {
                if (objectsProperty != null)
                {
                    string description = string.Empty;
                    if (listResponseAttribute != null)
                    {
                        description = listResponseAttribute.ObjectsDescription;
                    }
                    else
                    {
                        description = GetPropertyDescription(objectsProperty);
                    }

                    propertyToDescription.Add(objectsProperty, description);
                }
            }

            kalturaClassDetails.Properties = GetPropertiesDetails(propertyToDescription, kalturaClassDetails.Name);

            return kalturaClassDetails;
        }

        private List<KalturaPropertyDetails> GetPropertiesDetails(Dictionary<PropertyInfo, string> propertiesToDescription, string typeName)
        {
            var propertiesDetails = new List<KalturaPropertyDetails>();
            foreach (var property in propertiesToDescription)
            {
                if (property.Key.PropertyType == typeof(KalturaMultilingualString))
                {
                    var propertyDetailsString = GetPropertyDetails(property.Key, property.Value);
                    if (propertyDetailsString.SchemeProperty == null)
                    {
                        propertyDetailsString.SchemeProperty = new SchemePropertyAttribute();
                    }
                    propertyDetailsString.SchemeProperty.ReadOnly = true;
                    propertyDetailsString.PropertyType = typeof(string);
                    propertiesDetails.Add(propertyDetailsString);

                    var propertyDetailsKalturaTranslationTokenList = GetPropertyDetails(property.Key, property.Value);
                    propertyDetailsKalturaTranslationTokenList.PropertyType = typeof(List<KalturaTranslationToken>);
                    propertyDetailsKalturaTranslationTokenList.Name = KalturaMultilingualString.GetMultilingualName(propertyDetailsKalturaTranslationTokenList.Name);
                    propertiesDetails.Add(propertyDetailsKalturaTranslationTokenList);
                }
                else
                {
                    var propertyDetails = GetPropertyDetails(property.Key, property.Value);
                    if (typeName == "KalturaFilter" && propertyDetails.DataMember != null && propertyDetails.DataMember.Name == "orderBy")
                    {
                        propertyDetails.PropertyType = typeof(string);
                    }
                    else
                    {
                        propertyDetails.PropertyType = property.Key.PropertyType;
                    }

                    propertiesDetails.Add(propertyDetails);
                }
            }

            return propertiesDetails;
        }

        private static KalturaPropertyDetails GetPropertyDetails(PropertyInfo propertyInfo, string description)
        {
            var propertyDetails = new KalturaPropertyDetails()
            {
                Obsolete = propertyInfo.GetCustomAttribute<ObsoleteAttribute>(true),
                JsonIgnore = propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>(true),
                Deprecated = propertyInfo.GetCustomAttribute<DeprecatedAttribute>(true),
                DataMember = propertyInfo.GetCustomAttribute<DataMemberAttribute>(),
                SchemeProperty = propertyInfo.GetCustomAttribute<SchemePropertyAttribute>(),
                Description = description
            };

            propertyDetails.Name = propertyDetails.DataMember != null ? propertyDetails.DataMember.Name : null;

            return propertyDetails;
        }

        private Dictionary<string, string> GetParameterTypes(Type type)
        {
            var parameterTypes = new Dictionary<string, string>();

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
                parameterTypes.Add("type", isIntEnum ? "int" : "string");
                parameterTypes.Add("enumType", GetTypeName(type));
                return parameterTypes;
            }

            //Handling arrays
            if (type.IsArray)
            {
                parameterTypes.Add("type", "array");
                parameterTypes.Add("arrayType", GetTypeName(type.GetElementType()));
                return parameterTypes;
            }

            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();

                if (genericTypeDefinition == typeof(List<>))
                {
                    parameterTypes.Add("type", "array");
                    parameterTypes.Add("arrayType", GetTypeName(type.GetGenericArguments()[0]));
                    return parameterTypes;
                }

                if (genericTypeDefinition == typeof(Dictionary<,>) || genericTypeDefinition == typeof(SerializableDictionary<,>))
                {
                    parameterTypes.Add("type", "map");
                    parameterTypes.Add("arrayType", GetTypeName(type.GetGenericArguments()[1]));
                    return parameterTypes;
                }
            }

            parameterTypes.Add("type", GetTypeName(type));
            return parameterTypes;
        }

        private string GetParameterDescription(ParameterInfo param, MethodInfo method)
        {
            return SchemeManager.GetDescriptionByAssemblyXml(string.Format("//member[starts-with(@name,'M:{0}.{1}')]/param[@name='{2}']", method.ReflectedType.FullName, method.Name, param.Name), assemblyXml);
        }

        private string GetTypeDescription(Type type)
        {
            return SchemeManager.GetDescriptionByAssemblyXml(string.Format("//member[@name='T:{0}']", type.FullName), assemblyXml);
        }

        internal string GetPropertyDescription(PropertyInfo property)
        {
            try
            {
                if (property.ReflectedType.IsGenericType)
                {
                    Regex regex = new Regex(@"^[^\[]+");
                    string name = regex.Match(property.ReflectedType.FullName).Value;
                    return SchemeManager.GetDescriptionByAssemblyXml(string.Format("//member[@name='P:{0}.{1}']", name, property.Name), assemblyXml);
                }
                else
                {
                    return SchemeManager.GetDescriptionByAssemblyXml(string.Format("//member[@name='P:{0}.{1}']", property.ReflectedType, property.Name), assemblyXml);
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
                //throw;
            }
        }
    }
}