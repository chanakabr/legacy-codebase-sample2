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

namespace WebAPI.Managers.Schema
{
    public class SchemaManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static void Generate(Stream stream)
        {
            var writer = new Schema(stream);
            writer.write();
        }

        public static bool Validate(bool strict)
        {
            var validator = new Schema();
            return validator.validate(strict);
        }

        public static bool Validate(Type type, bool strict)
        {
            bool valid = true;

            if (type.IsSubclassOf(typeof(ApiController)))
                return ValidateController(type, strict) && valid;

            if (!type.Name.StartsWith("Kaltura"))
            {
                Console.WriteLine(string.Format("Type {0} doesn't have Kaltura prefix", type.Name));
                valid = false;
            }

            if (type.IsEnum)
                return ValidateEnum(type, strict) && valid;

            return ValidateObject(type, strict) && valid;
        }

        private static bool hasValidationException(PropertyInfo property, ValidationType type)
        {
            object[] attributes = property.GetCustomAttributes(true);
            foreach (Attribute attribute in attributes)
            {
                if (attribute.GetType() == typeof(ValidationException))
                {
                    ValidationException exception = attribute as ValidationException;
                    if (exception.ValidationType == type)
                        return true;
                }
            }

            return false;
        }

        private static bool ValidateProperty(PropertyInfo property, bool strict)
        {
            bool valid = true;

            if (property.PropertyType.IsPrimitive && !hasValidationException(property, ValidationType.NULLABLE))
            {
                Console.WriteLine(string.Format("Property {0}.{1} ({2}) must be nullable", property.ReflectedType.Name, property.Name, property.PropertyType.Name));
                valid = false;
            }

            if (property.Name.Contains('_'))
            {
                Console.WriteLine(string.Format("Property {0}.{1} ({2}) name may not contain underscores", property.ReflectedType.Name, property.Name, property.PropertyType.Name));
                if (strict)
                    valid = false;
            }

            if (property.PropertyType == typeof(DateTime))
            {
                Console.WriteLine(string.Format("Property {0}.{1} is DateTime, use long instead", property.ReflectedType.Name, property.Name, property.PropertyType.Name));
                valid = false;
            }

            if (property.PropertyType.IsArray)
            {
                Type valueType = property.PropertyType.GetElementType();
                if (!valueType.IsSubclassOf(typeof(KalturaOTTObject)))
                {
                    Console.WriteLine(string.Format("Property {0}.{1} array must contain KalturaOTTObject objects (or something that extends it)", property.ReflectedType.Name, property.Name, property.PropertyType.Name));
                    valid = false;
                }
            }

            if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                Type valueType = property.PropertyType.GetGenericArguments()[1];
                if (!valueType.IsSubclassOf(typeof(KalturaOTTObject)))
                {
                    Console.WriteLine(string.Format("Property {0}.{1} array must contain KalturaOTTObject objects (or something that extends it)", property.ReflectedType.Name, property.Name, property.PropertyType.Name));
                    valid = false;
                }
            }

            return valid;
        }

        private static bool ValidateFilter(Type type, bool strict)
        {
            return true;
        }

        private static bool ValidateObject(Type type, bool strict)
        {
            bool valid = true;

            if (type.Name.EndsWith("ListResponse") && type != typeof(KalturaListResponse) && !type.IsSubclassOf(typeof(KalturaListResponse)))
            {
                Console.WriteLine(string.Format("List response {0} must inherit KalturaListResponse", type.Name));
                valid = false;
            }

            if (type.IsSubclassOf(typeof(KalturaFilterPager)))
            {
                Console.WriteLine(string.Format("Object {0} should not inherit KalturaFilterPager", type.Name));
                if (strict)
                    valid = false;
            }

            //if (type.IsSubclassOf(typeof(KalturaFilter)))
            //{
            //    valid = ValidateFilter(type) && valid;
            //}

            //if (type.Name.EndsWith("Filter") && !type.IsSubclassOf(typeof(KalturaFilter)))
            //{
            //    Console.WriteLine(string.Format("Filter {0} must inherit KalturaListResponse", type.Name));
            //    valid = false;
            //}

            foreach (PropertyInfo property in type.GetProperties())
            {
                valid = ValidateProperty(property, strict) && valid;
            }

            return valid;
        }

        private static bool ValidateEnum(Type type, bool strict)
        {
            return true;
        }

        private static bool ValidateController(Type type, bool strict)
        {
            return true;
        }

        internal static bool Validate(MethodInfo method, bool strict)
        {
            bool valid = true;

            var controller = method.ReflectedType;
            var serviceId = getServiceId(controller);
            var actionId = FirstCharacterToLower(method.Name);

            var attr = method.GetCustomAttribute<RouteAttribute>(false);
            if (attr == null)
            {
                Console.WriteLine(string.Format("Action {0}.{1} has no routing attribute", serviceId, actionId));
                valid = false;
            }

            return valid;
        }

        public static string getServiceId(Type controller)
        {
            return FirstCharacterToLower(controller.Name.Replace("Controller", ""));
        }

        public static string FirstCharacterToLower(string str)
        {
            if (String.IsNullOrEmpty(str) || Char.IsLower(str, 0))
                return str;

            return Char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
    }
}