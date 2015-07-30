using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml;
using WebAPI.Models.General;

namespace WebAPI
{
    /// <summary>
    /// XXX: SHOULD BE REFACTORED SOMETIME TO USE XMLDOCUMENT...
    /// </summary>
    public class ApiSchema : IHttpHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static XmlDocument XMLFromAssemblyNonCached(Assembly assembly)
        {
            string assemblyFilename = assembly.CodeBase;

            const string prefix = "file:///";

            if (assemblyFilename.StartsWith(prefix))
            {
                StreamReader streamReader;

                try
                {
                    streamReader = new StreamReader(Path.ChangeExtension(assemblyFilename.Substring(prefix.Length), ".xml"));
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

        public void ProcessRequest(HttpContext context)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            var x = XMLFromAssemblyNonCached(asm);
            List<Type> enums = new List<Type>();
            List<Type> classes = new List<Type>();

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

            context.Response.Write(string.Format("<?xml version=\"1.0\"?>\n<xml apiVersion=\"{0}\" generatedDate=\"{1}\">\n",
                fvi.FileVersion, Utils.SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow)));

            List<Type> types = asm.GetTypes().Where(t => t.Namespace != null && t.Namespace.StartsWith("WebAPI.Models")).ToList();
            List<Type> sortedTypes = new List<Type>();

            var classesTypes = types.Where(yy => !yy.IsEnum);
            //Ordering by dependency
            foreach (Type tp in classesTypes)
                sortedTypes.Add(tp);

            foreach (Type tp in classesTypes)
            {                
                //No need to handle
                if (tp.BaseType == typeof(Object))
                    continue;

                sortedTypes.Remove(tp);
                if (tp.BaseType != null)
                {
                    int idx = sortedTypes.FindIndex(xx => xx == tp.BaseType);
                    sortedTypes.Insert(idx, tp);
                }
                else
                    sortedTypes.Insert(0, tp);
            }

            //Running on models first
            foreach (Type type in sortedTypes)
            {
                if (type.BaseType == typeof(Enum))
                    enums.Add(type);
                else
                    classes.Add(type);
            }

            classes.Reverse();

            //Printing enums
            context.Response.Write("<enums>\n");
            foreach (Type t in enums.OrderBy(c => c.Name))
            {
                context.Response.Write(string.Format("\t<enum name='{0}' enumType='string'>\n", t.Name));

                //Print values
                foreach (var v in Enum.GetValues(t))
                    context.Response.Write(string.Format("\t\t<const name='{0}' />\n", v));

                context.Response.Write("\t</enum>\n");
            }
            context.Response.Write("</enums>\n");

            //Running on classes
            context.Response.Write("<classes>\n");
            foreach (Type t in classes)
            {
                //Skip master base class
                if (t == typeof(KalturaOTTObject))
                    continue;

                var classNode = x.SelectNodes(string.Format("//member[@name='T:{0}']", t.FullName));

                if (classNode == null)
                    continue;

                string baseName = t.BaseType != null && t.BaseType != typeof(Object) && t.BaseType != typeof(KalturaOTTObject) ?
                    string.Format("base='{0}'", t.BaseType.Name) : "";

                bool isAbstractOrInterface = t.IsInterface || t.IsAbstract;


                //No documentation
                if (classNode.Count == 0 || classNode[0].ChildNodes == null)
                {
                    log.Error("Empty description - " + t.Name);
                    context.Response.Write(string.Format("\t<class name='{0}' description='' {1}", t.Name,
                        baseName));

                    if (isAbstractOrInterface)
                        context.Response.Write("abstract='1'");

                    context.Response.Write(">\n");
                }
                else
                {
                    foreach (XmlElement child in classNode[0].ChildNodes)
                    {
                        context.Response.Write(string.Format("\t<class name='{0}' description='{1}' {2}", t.Name,
                            HttpUtility.HtmlEncode(child.InnerText.Trim()), baseName));

                        if (isAbstractOrInterface)
                            context.Response.Write("abstract='1'");

                        context.Response.Write(">\n");
                    }
                }

                List<PropertyInfo> props = t.GetProperties().ToList();

                //Remove properties from base
                if (t.BaseType != null)
                {
                    PropertyInfo[] baseProps = t.BaseType.GetProperties();
                    props.RemoveAll(xx => baseProps.Where(y => y.Name == xx.Name).Count() > 0);
                }

                runOnProperties(context, x, t.FullName, props.ToArray());

                context.Response.Write("\t</class>\n");
            }

            context.Response.Write("</classes>\n");

            //Running on methods
            context.Response.Write("<services>\n");
            foreach (Type controller in asm.GetTypes().Where(t => t.Namespace != null &&
                t.Namespace.StartsWith("WebAPI.Controllers") && t.Name.EndsWith("Controller")).OrderBy(c => c.Name))
            {
                context.Response.Write(string.Format("\t<service name='{0}'>\n", controller.Name.Replace("Controller", "")));
                var methods = controller.GetMethods().OrderBy(z => z.Name);
                foreach (var method in methods)
                {
                    //Read only HTTP POST as we will have duplicates otherwise
                    var explorerAttr = method.GetCustomAttributes<ApiExplorerSettingsAttribute>(false);
                    if (explorerAttr.Count() > 0 && explorerAttr.First().IgnoreApi)
                        continue;

                    var attr = method.GetCustomAttribute<RouteAttribute>(false);
                    if (attr != null)
                    {
                        var classNode = x.SelectNodes(string.Format("//member[starts-with(@name,'M:{0}.{1}')]", controller.FullName, method.Name));

                        string desc = "";
                        //No documentation
                        if (classNode.Count > 0 && classNode[0].ChildNodes != null)
                        {
                            for (int i = 0; i < classNode[0].ChildNodes.Count; i++)
                            {
                                if (classNode[0].ChildNodes[i].Name == "summary")
                                {
                                    desc = classNode[0].ChildNodes[i].InnerText.Trim();
                                    break;
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(desc))
                            log.Error("Empty description in method - " + method.Name);

                        string deprecatedAttr = "";
                        if (method.GetCustomAttribute<ObsoleteAttribute>() != null)
                            deprecatedAttr = string.Format("deprecated='1'");

                        context.Response.Write(string.Format("\t\t<action name='{0}' enableInMultiRequest='0' supportedRequestFormats='json' supportedResponseFormats='json,xml' description='{1}' {2}>\n",
                            method.Name,
                            HttpUtility.HtmlEncode(desc.Trim().Replace('\'', '"')), deprecatedAttr));

                        foreach (var par in method.GetParameters())
                        {
                            var descs = x.SelectNodes(string.Format("//member[starts-with(@name,'M:{0}.{1}')]/param[@name='{2}']",
                                controller.FullName, method.Name, par.Name));

                            string pdesc = "";
                            if (descs.Count > 0)
                                pdesc = descs[0].InnerText.Trim().Replace('\'', '"');

                            if (string.IsNullOrEmpty(pdesc))
                                log.Error("Empty description in method " + method + " parameter - " + par.Name);

                            context.Response.Write(string.Format("\t\t\t<param name='{0}' {1} description='{2}'/>\n", par.Name,
                                getTypeAndArray(par.ParameterType), HttpUtility.HtmlEncode(pdesc)));
                        }

                        context.Response.Write(string.Format("\t\t\t<result {0}/>\n", getTypeAndArray(method.ReturnType)));
                        context.Response.Write("\t\t</action>\n");
                    }
                };

                context.Response.Write(string.Format("\t</service>\n"));
            }
            context.Response.Write("</services>\n");

            //Config section
            context.Response.Write("<configurations>\n");
            context.Response.Write("\t<client type='KalturaClientConfiguration'>\n");
            context.Response.Write("\t\t<clientTag type='string' />\n\t\t<apiVersion type='string'/>\n");
            context.Response.Write("\t</client>\n");
            context.Response.Write("\t<request type='KalturaRequestConfiguration'>\n");
            context.Response.Write("\t\t<partnerId type='int' description='Impersonated partner id'/>\n");
            context.Response.Write("\t\t<ks type='string' alias='sessionId' description='Kaltura API session'/>\n");
            context.Response.Write("\t</request>\n");
            context.Response.Write("</configurations>\n");

            context.Response.Write("</xml>");
        }

        private string getTypeAndArray(Type type)
        {
            bool isNullable = false;
            //Handling nullables
            if (Nullable.GetUnderlyingType(type) != null)
            {
                type = type.GetGenericArguments()[0];
                isNullable = true;
            }

            //Handling Enums
            if (type.IsEnum)
            {
                return string.Format("type='string' enumType='{0}'", getTypeFriendlyName(type));
            }
            //Handling arrays
            else if (type.IsArray || type.IsGenericType)
            {
                string name = "";
                if (type.IsArray)
                    name = getTypeFriendlyName(type.GetElementType());
                else if (type.IsGenericType)
                {
                    //if List
                    if (type.GetGenericArguments().Count() == 1)
                        name = getTypeFriendlyName(type.GetGenericArguments()[0]);
                    //if Dictionary
                    else if (type.GetGenericArguments().Count() == 2)
                        name = "map";
                    else
                        throw new Exception("Generic type unknown");
                }

                return string.Format("type='array' arrayType='{1}'", getTypeFriendlyName(type), name);
            }

            return string.Format("type='{0}'{1}default='{2}'", getTypeFriendlyName(type), isNullable ? " optional='1' " : " ", getDefaultForType(type));
        }

        private string getDefaultForType(Type type)
        {
            if (type == typeof(String))
                return "null";
            if (type == typeof(DateTime))
                return "0";
            if (type == typeof(long) || type == typeof(Int64))
                return "0";
            if (type == typeof(Int32))
                return "0";
            if (type == typeof(double) || type == typeof(float))
                return "0";
            if (type == typeof(bool))
                return "false";

            return "null";
        }

        private void runOnProperties(HttpContext context, XmlDocument x, string className, MemberInfo[] members)
        {
            foreach (var property in members)
            {
                if (property is PropertyInfo)
                {
                    var pi = (PropertyInfo)property;

                    var descs = x.SelectNodes(string.Format("//member[@name='P:{0}.{1}']//summary",
                                className, pi.Name));

                    string pdesc = "";
                    if (descs.Count > 0)
                        pdesc = descs[0].InnerText.Trim().Replace('\'', '"');

                    //Type eType = null;
                    var dataMemberAttr = pi.GetCustomAttribute<DataMemberAttribute>();

                    if (dataMemberAttr == null)
                        continue;

                    var attr = dataMemberAttr.Name;

                    context.Response.Write(string.Format("\t\t<property name='{0}' {1} description='{2}' readOnly='0' insertOnly='0' />\n", attr,
                           getTypeAndArray(pi.PropertyType), HttpUtility.HtmlEncode(pdesc)));
                }
                else
                {
                    throw new Exception("Unable to generate");
                }
            }
        }

        private static string getTypeFriendlyName(Type type)
        {
            if (type == typeof(String))
                return "string";
            if (type == typeof(DateTime))
                return "time";
            if (type == typeof(long) || type == typeof(Int64))
                return "bigint";
            if (type == typeof(Int32))
                return "int";
            if (type == typeof(double) || type == typeof(float))
                return "float";
            if (type == typeof(bool))
                return "bool";

            return type.Name;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}