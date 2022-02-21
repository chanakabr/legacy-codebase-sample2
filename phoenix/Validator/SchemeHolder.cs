using System;
using System.Collections.Generic;
using System.Text;
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
using WebAPI.App_Start;
using WebAPI.Controllers;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.General;

namespace Validator.Managers.Scheme
{
    public class SchemeHolder
    {
        private Assembly validatorAssembly;
        

        public Assembly Assembly;
        public XmlDocument AssemblyXml;
        public List<Type> Enums = new List<Type>();
        public IEnumerable<Type> Types;
        public IEnumerable<Type> Controllers;
        public IEnumerable<Type> Exceptions;
        public Dictionary<int, ApiException.ExceptionType> Errors = new Dictionary<int, ApiException.ExceptionType>();

        public SchemeHolder()
        {
            this.Assembly = Assembly.Load("WebAPI");
            this.validatorAssembly = Assembly.GetExecutingAssembly();
            this.AssemblyXml = GetAssemblyXml();

            Load();
        }

        private XmlDocument GetAssemblyXml()
        {
            const string prefix = "file:///";
            if (validatorAssembly.CodeBase.StartsWith(prefix))
            {
                StreamReader streamReader;
                try
                {
                    var directoryName = Path.GetDirectoryName(validatorAssembly.Location);
                    var location = Path.Combine(directoryName, "WebAPI.xml");
                    streamReader = new StreamReader(location);
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

        private void Load()
        {
            Controllers = Assembly.GetTypes().Where(myType => myType.IsClass && typeof(IKalturaController).IsAssignableFrom(myType));
            Exceptions = Assembly.GetTypes().Where(myType => myType.IsClass && (myType.IsSubclassOf(typeof(ApiException)) || myType == typeof(ApiException)));

            foreach (Type exception in Exceptions.OrderBy(exception => exception.Name))
            {
                loadErrors(exception);
            }

            LoadType(typeof(KalturaApiExceptionArg));
            LoadType(typeof(KalturaClientConfiguration));
            LoadType(typeof(KalturaRequestConfiguration));
            LoadType(typeof(KalturaResponseType));

            foreach (Type controller in Controllers)
            {
                var apiExplorerSettings = controller.GetCustomAttribute<ApiExplorerSettingsAttribute>(false);
                if (apiExplorerSettings != null && apiExplorerSettings.IgnoreApi)
                    continue;

                var serviceAttribute = controller.GetCustomAttribute<ServiceAttribute>();
                if (serviceAttribute != null && serviceAttribute.IsInternal)
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
            }

            var filters = Assembly.GetTypes().Where(myType => myType.IsClass && typeof(IKalturaFilter).IsAssignableFrom(myType));
            foreach (var filter in filters)
            {
                var orderByName = filter.Name.Replace("Filter", "OrderBy");
                var orderBys = Assembly.GetTypes().Where(myType => myType.IsEnum && myType.Name == orderByName);
                foreach (Type orderBy in orderBys)
                {
                    if (!Enums.Contains(orderBy))
                        Enums.Add(orderBy);
                }
            }
            Types = FixTypeDependencies(Field.loadedTypes.Values);
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
                        if (Errors.ContainsKey((int)exceptionType.statusCode))
                        {
                            throw new Exception("Error code " + exceptionType.statusCode + " appears twice: ApiException.ApiExceptionType." + exceptionType.name);
                        }
                        else
                        {
                            Errors.Add((int)exceptionType.statusCode, type);
                        }
                    }
                    else if (type.GetType() == typeof(ApiException.ClientExceptionType))
                    {
                        ApiException.ClientExceptionType exceptionType = type as ApiException.ClientExceptionType;
                        Errors.Add((int)exceptionType.statusCode, type);
                    }
                }
            }
        }

        private void LoadType(Type type)
        {
            if (type.IsEnum && !Enums.Contains(type))
            {
                Enums.Add(type);
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

            string typeName = SchemeManager.GetTypeName(type);
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

                    var subClasses = Assembly.GetTypes().Where(myType => IsSubclassOfRawGeneric(type, myType));
                    foreach (Type subClass in subClasses)
                        LoadType(subClass);

                    LoadTypeProperties(type);
                    return;
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

        private void LoadTypeProperties(Type type)
        {
            List<PropertyInfo> properties = type.GetProperties().ToList();

            foreach (var property in properties)
            {
                if (property.DeclaringType == type)
                    LoadType(property.PropertyType);
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
    }
}
