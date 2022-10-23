using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml;
using TVinciShared;
using WebAPI;
using WebAPI.Controllers;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor.Extensions;

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
        private XmlWriter writer;
        private SchemeValidator _validator;

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
            _validator = new SchemeValidator(false);
        }

        public void RemoveInvalidObjects()
        {
            _validator.Holder.Controllers = _validator.Holder.Controllers.Where(controller => _validator.ValidateType(controller, false, out var _));
            _validator.Holder.Enums = _validator.Holder.Enums.Where(type => _validator.ValidateType(type, false, out var _)).ToList();
            _validator.Holder.Types = _validator.Holder.Types.Where(type => _validator.ValidateType(type, false, out var _)).ToList();
        }

        private string GetAssemblyVersion()
        {
            const string prefix = "file:///";
            if (_validator.Holder.Assembly.CodeBase.StartsWith(prefix))
            {
                try
                {
                    var directoryName = Path.GetDirectoryName(_validator.Holder.Assembly.Location);
                    var location = Path.Combine(directoryName, "WebAPI.dll");
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
            foreach (Type type in _validator.Holder.Enums.OrderBy(myType => myType.Name))
            {
                if (!_validator.ValidateType(type, true, out var _))
                    continue;

                writeEnum(type);
            }
            writer.WriteEndElement(); // enums

            //Running on classes
            writer.WriteStartElement("classes");
            foreach (Type type in _validator.Holder.Types)
            {
                if (!_validator.ValidateType(type, true, out var _) || type.Name == "KalturaListResponseT" || type.Name == "KalturaFilterT")
                {
                    Console.WriteLine($"Info: type {type.Name} is not valid and will not consider as class");
                    continue;
                }
                    
                WriteClass(GetClassDetails(type));
            }
            writer.WriteEndElement(); // classes

            //Running on methods
            writer.WriteStartElement("services");
            foreach (Type controller in _validator.Holder.Controllers.OrderBy(controller => controller.Name))
            {
                if (controller.IsAbstract || !_validator.ValidateType(controller, true, out var _))
                    continue;

                var serviceAttribute = controller.GetCustomAttribute<ServiceAttribute>(true);
                if (serviceAttribute != null && serviceAttribute.IsInternal)
                    continue;

                WriteService(GetControllerDetails(controller));
            }
            writer.WriteEndElement(); // services


            // errors
            if (_validator.ValidateErrors(true))
            {
                writer.WriteStartElement("errors");
                foreach (Type exception in _validator.Holder.Exceptions.OrderBy(exception => exception.Name))
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
                try
                {
                    writeAction(action, controller);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception while writeAction for service. controller:{controller.ServiceId}, action:{action.LoweredName}, ex:{ex.ToString()}");
                    throw;
                }
            }
            
            writer.WriteEndElement(); // service
        }

        private void writeAction(KalturaActionDetails action, KalturaControllerDetails controller)
        {
            writer.WriteStartElement("action");
            writer.WriteAttributeString("name", action.LoweredName);
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
                if (_validator.Holder.Errors.ContainsKey((int)apiCode))
                {
                    writer.WriteStartElement("throws");
                    var exceptionType = _validator.Holder.Errors[(int)apiCode] as ApiException.ApiExceptionType;
                    writer.WriteAttributeString("name", exceptionType.name);
                    writer.WriteEndElement(); // throws apiCode
                }
            }

            foreach (var clientCode in action.ClientThrows)
            {
                if (_validator.Holder.Errors.ContainsKey((int)clientCode))
                {
                    writer.WriteStartElement("throws");
                    var exceptionType = _validator.Holder.Errors[(int)clientCode] as ApiException.ClientExceptionType;
                    writer.WriteAttributeString("name", exceptionType.statusCode.ToString());
                    writer.WriteEndElement(); // throws clientCode
                }
            }
            
            writer.WriteEndElement(); // action
        }
        
        internal string getDescription(MethodInfo method, ParameterInfo param)
        {
            return SchemeManager.GetDescriptionByAssemblyXml(string.Format("//member[starts-with(@name,'M:{0}.{1}')]/param[@name='{2}']", 
                method.ReflectedType.FullName, method.Name, param.Name), _validator.Holder.AssemblyXml);
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

                writer.WriteAttributeString("validationState", propertyDetails.SchemeProperty.ValidationState.ToString());

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

                if (propertyDetails.SchemeProperty.Pattern != null)
                    writer.WriteAttributeString("pattern", propertyDetails.SchemeProperty.Pattern);
                if (propertyDetails.SchemeProperty.MinItems >= 0)
                    writer.WriteAttributeString("minItems", propertyDetails.SchemeProperty.MinItems.ToString());
                if (propertyDetails.SchemeProperty.MaxItems >= 0)
                    writer.WriteAttributeString("maxItems", propertyDetails.SchemeProperty.MaxItems.ToString());
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
                writer.WriteAttributeString("enumType", SchemeManager.GetTypeName(type));
                return;
            }

            //Handling arrays
            if (type.IsArray)
            {
                writer.WriteAttributeString("type", "array");
                writer.WriteAttributeString("arrayType", SchemeManager.GetTypeName(type.GetElementType()));
                return;
            }

            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                
                if (genericTypeDefinition == typeof(List<>))
                {
                    writer.WriteAttributeString("type", "array");
                    writer.WriteAttributeString("arrayType", SchemeManager.GetTypeName(type.GetGenericArguments()[0]));
                    return;
                }

                if (genericTypeDefinition == typeof(Dictionary<,>) || genericTypeDefinition == typeof(SerializableDictionary<,>))
                {
                    writer.WriteAttributeString("type", "map");
                    writer.WriteAttributeString("arrayType", SchemeManager.GetTypeName(type.GetGenericArguments()[1]));
                    return;
                }
            }

            writer.WriteAttributeString("type", SchemeManager.GetTypeName(type));
        }

        private KalturaControllerDetails GetControllerDetails(Type controllerType)
        {
            var controllerDetails = new KalturaControllerDetails()
            {
                ServiceType = controllerType,
                ServiceId = SchemeManager.getServiceId(controllerType),
                IsAbstract = controllerType.IsAbstract
            };

            var methods = controllerType.GetMethods().OrderBy(method => method.Name);
            foreach (var method in methods)
            {
                if (!method.IsPublic || method.DeclaringType.Namespace != "WebAPI.Controllers")
                    continue;

                if (!_validator.ValidateMethod(method, false))
                    continue;

                //Read only HTTP POST as we will have duplicates otherwise
                var explorerAttr = method.GetCustomAttributes<ApiExplorerSettingsAttribute>(false);
                if (explorerAttr.Count() > 0 && explorerAttr.First().IgnoreApi)
                    continue;

                if (!_validator.ValidateMethod(method, true, false))
                    continue;

                controllerDetails.Actions.Add(GetActionDetails(method));
            };

            return controllerDetails;
        }

        private KalturaActionDetails GetActionDetails(MethodInfo method)
        {
            var actionDetails = new KalturaActionDetails()
            {
                RealName = method.Name,
                IsGenericMethod = method.IsGenericMethod,
                Description = SchemeManager.GetMethodDescription(method, _validator.Holder.AssemblyXml),
                IsDeprecated = method.GetCustomAttribute<ObsoleteAttribute>() != null,
                IsSessionRequired = method.GetCustomAttribute<ApiAuthorizeAttribute>() != null,
            };

            var actionAttribute = method.GetCustomAttribute<ActionAttribute>(false);
            if (actionAttribute != null)
            {
                actionDetails.LoweredName = actionAttribute.Name;
            }

            var parameters = method.GetParameters();
            foreach (var parameter in parameters)
            {
                actionDetails.Prameters.Add(GetPrameterDetails(parameter, method));
            }

            if (method.ReturnType != typeof(void))
            {
                actionDetails.ReturnedTypes = SchemeManager.GetParameterTypes(method.ReturnType);
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
                ParameterType = parameter.ParameterType,
                Name = parameter.Name,
                ParameterTypes = SchemeManager.GetParameterTypes(parameter.ParameterType),
                IsOptional = parameter.IsOptional,
                Description = GetParameterDescription(parameter, method),
                Position = parameter.Position
            };

            if (parameter.IsOptional)
            {
                prameterDetails.DefaultValue = parameter.DefaultValue == null ? "null" : (parameter.DefaultValue is bool ? parameter.DefaultValue.ToString().ToLower() : parameter.DefaultValue.ToString());
            }

            return prameterDetails;
        }

        private KalturaClassDetails GetClassDetails(Type classType)
        {
            var kalturaClassDetails = new KalturaClassDetails()
            {
                Name = SchemeManager.GetTypeName(classType),
                Description = GetTypeDescription(classType),
                IsAbstract = classType.IsInterface || classType.IsAbstract
            };

            var schemeBaseAttribute = classType.GetCustomAttribute<SchemeBaseAttribute>();
            if (schemeBaseAttribute != null)
            {
                kalturaClassDetails.BaseName = SchemeManager.GetTypeName(schemeBaseAttribute.BaseType);
            }
            else if (classType.BaseType != null && classType.BaseType != typeof(KalturaOTTObject))
            {
                kalturaClassDetails.BaseName = SchemeManager.GetTypeName(classType.BaseType);
            }

            var properties = classType.GetProperties().ToList();
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
                    propertyDetailsKalturaTranslationTokenList.Name = MultilingualStringMapper.GetMultilingualName(propertyDetailsKalturaTranslationTokenList.Name);
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

        private KalturaPropertyDetails GetPropertyDetails(PropertyInfo propertyInfo, string description)
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

        private string GetParameterDescription(ParameterInfo param, MethodInfo method)
        {
            return SchemeManager.GetDescriptionByAssemblyXml(string.Format("//member[starts-with(@name,'M:{0}.{1}')]/param[@name='{2}']", 
                method.ReflectedType.FullName, method.Name, param.Name), _validator.Holder.AssemblyXml);
        }

        private string GetTypeDescription(Type type)
        {
            return SchemeManager.GetDescriptionByAssemblyXml(string.Format("//member[@name='T:{0}']", type.FullName), _validator.Holder.AssemblyXml);
        }

        internal string GetPropertyDescription(PropertyInfo property)
        {
            try
            {
                if (property.ReflectedType.IsGenericType)
                {
                    Regex regex = new Regex(@"^[^\[]+");
                    string name = regex.Match(property.ReflectedType.FullName).Value;
                    return SchemeManager.GetDescriptionByAssemblyXml(string.Format("//member[@name='P:{0}.{1}']", name, property.Name), _validator.Holder.AssemblyXml);
                }
                else
                {
                    return SchemeManager.GetDescriptionByAssemblyXml(string.Format("//member[@name='P:{0}.{1}']", property.ReflectedType, property.Name), _validator.Holder.AssemblyXml);
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