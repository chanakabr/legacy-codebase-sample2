using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Validator
{
    class Program
    {
        static void Main(string[] args)
        {
            Assembly asm = Assembly.LoadFrom(args[0]);

            if (asm == null)
                throw new Exception("DLL not found");

            var tt = asm.GetType("WebAPI.Models.General.KalturaOTTObject");

            bool found = false;
            foreach (Type type in asm.GetTypes().Where(t => t.Namespace != null && t.Namespace.StartsWith("WebAPI.Models") && t.BaseType != typeof(Attribute)))
            {
                if (!type.Name.StartsWith("Kaltura"))
                {
                    Console.WriteLine(string.Format("Model {0} doesn't have Kaltura prefix", type.Name));
                    found = true;
                }
                if (!type.IsInterface && !type.IsEnum && !tt.IsAssignableFrom(type))
                {
                    Console.WriteLine(string.Format("Model {0} doesn't inherit from {1}", type.Name, tt.Name));
                    found = true;
                }

                foreach (PropertyInfo m in type.GetProperties())
                {
                    if (m.PropertyType != typeof(String) && m.PropertyType != typeof(DateTime) && m.PropertyType != typeof(long) && 
                        m.PropertyType != typeof(Int64) && m.PropertyType != typeof(Int32) &&
                        m.PropertyType != typeof(double) && m.PropertyType != typeof(float) && m.PropertyType != typeof(bool))
                    {
                        var xe = m.GetCustomAttributes<XmlElementAttribute>();

                        if (xe.Count() > 0 && !xe.First().IsNullable && m.PropertyType.BaseType.Name != "Enum")
                        {                            
                            Console.WriteLine(string.Format("Model {0} has a member {1} with missing isnullable=true", type.Name, m.Name));
                            found = true;
                        }
                    }

                    if (m.PropertyType.IsArray || (m.PropertyType.IsGenericType &&
                        m.PropertyType.GetGenericTypeDefinition() != typeof(Dictionary<,>)
                        && m.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>)
                        && m.PropertyType.GetGenericTypeDefinition().BaseType.Name != "Dictionary`2"))
                    {

                        if (m.PropertyType.GetElementType() == typeof(int) || m.PropertyType.GetElementType() == typeof(string) || m.PropertyType.GetElementType() == typeof(Boolean))
                        {
                            Console.WriteLine(string.Format("Model {0} has a primitive Array {1}", type.Name, m.Name));
                            found = true;
                        }

                        var xaAttr = m.GetCustomAttributes<XmlArrayAttribute>();
                        var xaiAttr = m.GetCustomAttributes<XmlArrayItemAttribute>();
                        if (xaAttr.FirstOrDefault() == null || xaiAttr.FirstOrDefault() == null)
                        {
                            Console.WriteLine(string.Format("Model {0} doesn't have one of the Array properties on {1}", type.Name, m.Name));
                            found = true;
                        }

                        continue;
                    }

                    var xeAttr = m.GetCustomAttributes<XmlElementAttribute>();
                    var dmAttr = m.GetCustomAttributes<DataMemberAttribute>();
                    var jpAttr = m.GetCustomAttributes<JsonPropertyAttribute>();

                    if (xeAttr.Count() == 0 || dmAttr.Count() == 0 && jpAttr.Count() == 0)
                    {
                        Console.WriteLine(string.Format("Model {0} doesn't have one of the properties on {1}", type.Name, m.Name));
                        found = true;
                    }
                    else
                    {
                        if (xeAttr.First().ElementName != dmAttr.First().Name || dmAttr.First().Name != jpAttr.First().PropertyName ||
                            xeAttr.First().ElementName != jpAttr.First().PropertyName)
                        {
                            Console.WriteLine(string.Format("Model {0} has a mismatch in properties names", m.Name));
                            found = true;
                        }
                    }

                    if (m.PropertyType == typeof(DateTime))
                    {
                        Console.WriteLine(string.Format("Model {0} is Datetime! use long", type.Name));
                        found = true;
                    }
                }
            }

            if (!found)
            {
                //Console.WriteLine("SUCCESS!");
                Environment.Exit(0);
            }
            else
                Environment.Exit(-1);
        }
    }
}
