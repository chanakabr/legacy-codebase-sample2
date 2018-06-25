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

namespace Reflector
{
    enum PropertyType
    {
        NATIVE,
        STRING,
        OBJECT,
        GENERIC_OBJECT,
        ARRAY,
        NUMERIC_ARRAY,
        MAP
    }

    class Serializer : Base
    {
        Dictionary<string, bool> generatedTypes = new Dictionary<string, bool>();

        public Serializer() : base("..\\..\\..\\WebAPI\\Reflection\\KalturaJsonSerializer.cs", typeof(IKalturaJsonable))
        {

        }
        
        protected override void wrtieHeader()
        {
            file.WriteLine("// NOTICE: This is a generated file, to modify it, edit Program.cs in Reflector project");
            file.WriteLine("using System;");
            file.WriteLine("using System.Linq;");
            file.WriteLine("using System.Web;");
            file.WriteLine("using WebAPI.App_Start;");
            file.WriteLine("using WebAPI.EventNotifications;");
            file.WriteLine("using WebAPI.Filters;");
            file.WriteLine("using WebAPI.Managers;");
            file.WriteLine("using WebAPI.Managers.Models;");
            file.WriteLine("using WebAPI.Managers.Scheme;");
            file.WriteLine("using WebAPI.Models.API;");
            file.WriteLine("using WebAPI.Models.Billing;");
            file.WriteLine("using WebAPI.Models.Catalog;");
            file.WriteLine("using WebAPI.Models.ConditionalAccess;");
            file.WriteLine("using WebAPI.Models.DMS;");
            file.WriteLine("using WebAPI.Models.Domains;");
            file.WriteLine("using WebAPI.Models.General;");
            file.WriteLine("using WebAPI.Models.Notification;");
            file.WriteLine("using WebAPI.Models.Notifications;");
            file.WriteLine("using WebAPI.Models.Partner;");
            file.WriteLine("using WebAPI.Models.Pricing;");
            file.WriteLine("using WebAPI.Models.Social;");
            file.WriteLine("using WebAPI.Models.Users;");
            file.WriteLine("using static WebAPI.App_Start.WrappingHandler; ");
            file.WriteLine("");
            file.WriteLine("namespace WebAPI.Reflection");
            file.WriteLine("{");
            file.WriteLine("    public class KalturaJsonSerializer");
            file.WriteLine("    {");
        }

        protected override void wrtieBody()
        {
            wrtieSerializeProperties();
        }

        protected override void wrtieFooter()
        {
            file.WriteLine("    }");
            file.WriteLine("}");
        }

        private bool wrtieSerializeTypeProperties(Type type, string castobject)
        {
            bool appended = false;
            if (type.BaseType != null && type != typeof(KalturaOTTObject))
            {
                appended = wrtieSerializeTypeProperties(type.BaseType, castobject);
            }

            List<PropertyInfo> properties = type.GetProperties().ToList();
            properties.Sort(new PropertyInfoComparer());
            bool appendAppend = false;
            foreach (PropertyInfo property in properties)
            {
                if (property.DeclaringType != type)
                    continue;

                DataMemberAttribute dataMember = property.GetCustomAttribute<DataMemberAttribute>(false);
                string propertyName = castobject + "." + property.Name;

                PropertyType propertyType = typeof(IKalturaJsonable).IsAssignableFrom(property.PropertyType) ? PropertyType.OBJECT : (property.PropertyType == typeof(object) ? PropertyType.GENERIC_OBJECT : (property.PropertyType == typeof(string) ? PropertyType.STRING : PropertyType.NATIVE));
                string tab = "";
                string genericType = "";

                if (property.PropertyType.IsGenericType)
                {
                    if (property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        Type InnerType = property.PropertyType.GetGenericArguments()[0];
                        propertyType = typeof(IKalturaJsonable).IsAssignableFrom(InnerType) ? PropertyType.OBJECT : (InnerType == typeof(string) ? PropertyType.STRING : PropertyType.NATIVE);
                        tab = "    ";

                        if (!appended)
                        {
                            appendAppend = true;
                            file.WriteLine("                    append = false;");
                        }
                        file.WriteLine("                    if(" + propertyName + ".HasValue)");
                        file.WriteLine("                    {");
                        if (appendAppend)
                        {
                            file.WriteLine("                        append = true;");
                        }
                    }
                    else if (property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        propertyType = typeof(IKalturaOTTObject).IsAssignableFrom(property.PropertyType.GetGenericArguments()[0]) ? PropertyType.ARRAY : PropertyType.NUMERIC_ARRAY;
                        genericType = property.PropertyType.GetGenericArguments()[0].Name;
                        switch (genericType)
                        {
                            case "Int64":
                                genericType = "long";
                                break;

                            case "Int32":
                                genericType = "int";
                                break;
                        }
                        tab = "    ";

                        if (!appended)
                        {
                            appendAppend = true;
                            file.WriteLine("                    append = false;");
                        }
                        file.WriteLine("                    if(" + propertyName + " != null && " + propertyName + ".Count > 0)");
                        file.WriteLine("                    {");
                        if (appendAppend)
                        {
                            file.WriteLine("                        append = true;");
                        }
                    }
                    else if (property.PropertyType.GetGenericTypeDefinition() == typeof(SerializableDictionary<,>))
                    {
                        propertyType = PropertyType.MAP;
                        genericType = property.PropertyType.GetGenericArguments()[1].Name;
                        tab = "    ";

                        if (!appended)
                        {
                            appendAppend = true;
                            file.WriteLine("                    append = false;");
                        }
                        file.WriteLine("                    if(" + propertyName + " != null && " + propertyName + ".Count > 0)");
                        file.WriteLine("                    {");
                        if (appendAppend)
                        {
                            file.WriteLine("                        append = true;");
                        }
                    }
                }

                string value = "";
                switch (propertyType)
                {
                    case PropertyType.NATIVE:
                        value = propertyName;
                        break;

                    case PropertyType.STRING:
                        value = "\"\\\"\" + " + propertyName + " + \"\\\"\"";
                        break;

                    case PropertyType.OBJECT:
                        value = "Serialize(" + propertyName + ")";
                        break;

                    case PropertyType.GENERIC_OBJECT:
                        value = "(" + propertyName + " is IKalturaJsonable ? Serialize(" + propertyName + " as IKalturaJsonable) : JsonManager.GetInstance().Serialize(" + propertyName + "))";
                        break;

                    case PropertyType.ARRAY:
                        value = "\"[\" + String.Join(\", \", " + propertyName + ".Select(item => Serialize(item))) + \"]\"";
                        break;

                    case PropertyType.MAP:
                        value = "\"{\" + String.Join(\", \", " + propertyName + ".Select(pair => \"\\\"\" + pair.Key + \"\\\": \" + Serialize(pair.Value))) + \"}\"";
                        break;

                    case PropertyType.NUMERIC_ARRAY:
                        value = "\"[\" + String.Join(\", \", " + propertyName + ".Select(item => item.ToString())) + \"]\"";
                        break;
                }

                if (appended)
                {
                    if (appendAppend)
                    {
                        file.WriteLine(tab + "                    if(append)");
                        file.WriteLine(tab + "                    {");
                        file.WriteLine(tab + "                        ret += \", \";");
                        file.WriteLine(tab + "                    }");
                        file.WriteLine(tab + "                    ret += \"\\\"" + dataMember.Name + "\\\": \" + " + value + ";");
                        appendAppend = false;
                    }
                    else
                    {
                        file.WriteLine(tab + "                    ret += \", \\\"" + dataMember.Name + "\\\": \" + " + value + ";");
                    }
                }
                else
                {
                    file.WriteLine(tab + "                    ret += \"\\\"" + dataMember.Name + "\\\": \" + " + value + ";");
                }
                appended = true;

                IEnumerable<OldStandardPropertyAttribute> oldStandardProperties = property.GetCustomAttributes<OldStandardPropertyAttribute>(false);
                if (oldStandardProperties != null && oldStandardProperties.Count() > 0)
                {
                    foreach (OldStandardPropertyAttribute oldStandardProperty in oldStandardProperties)
                    {
                        if (oldStandardProperty.sinceVersion != null)
                        {
                            file.WriteLine(tab + "                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0 || currentVersion.CompareTo(new Version(\"" + oldStandardProperty.sinceVersion + "\")) > 0)");
                        }
                        else
                        {
                            file.WriteLine(tab + "                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)");
                        }
                        file.WriteLine(tab + "                    {");
                        file.WriteLine(tab + "                        ret += \", \\\"" + oldStandardProperty.oldName + "\\\": \" + " + value + ";");
                        file.WriteLine(tab + "                    }");
                    }
                }

                if (tab.Length > 0)
                {
                    file.WriteLine("                    }");
                }
            }

            return appended;
        }

        private void wrtieSerializeProperties(Type type)
        {
            string typeName = GetTypeName(type);
            if(generatedTypes.ContainsKey(typeName))
            {
                return;
            }


            string castobject = "k" + typeName.Substring(1);
            string codeTypeName = type.IsGenericType ? typeName + "<int>" : typeName;
            file.WriteLine("                case \"" + typeName + "\":");
            file.WriteLine("                    " + codeTypeName + " " + castobject + " = ottObject as " + codeTypeName + ";");
            wrtieSerializeTypeProperties(type, castobject);
            file.WriteLine("                    break;");
            file.WriteLine("                    ");

            generatedTypes.Add(typeName, true);
        }

        private void wrtieSerializeProperties()
        {
            file.WriteLine("        public static string Serialize(IKalturaJsonable ottObject)");
            file.WriteLine("        {");
            file.WriteLine("            Version currentVersion = (Version)HttpContext.Current.Items[RequestParser.REQUEST_VERSION];");
            file.WriteLine("            string ret = \"{\";");
            file.WriteLine("            bool append;");
            file.WriteLine("            switch (ottObject.GetType().Name)");
            file.WriteLine("            {");

            foreach (Type type in types)
            {
                wrtieSerializeProperties(type);
            }

            file.WriteLine("            }");
            file.WriteLine("            ret += \"}\";");
            file.WriteLine("            return ret;");
            file.WriteLine("        }");
            file.WriteLine("        ");
        }
    }
}
