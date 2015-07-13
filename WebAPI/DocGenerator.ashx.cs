using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Http;
using System.Xml;

namespace WebAPI
{
    /// <summary>
    /// Summary description for DocGenerator
    /// </summary>
    public class DocGenerator : IHttpHandler
    {
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

            //Running on models first
            foreach (Type type in asm.GetTypes().Where(t => t.Namespace != null && t.Namespace.StartsWith("WebAPI.Models")))
            {
                if (type.BaseType == typeof(Enum))
                    enums.Add(type);
                else
                    classes.Add(type);
            }

            //Printing enums
            context.Response.Write("<enums>\n");
            foreach (Type t in enums)
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
                var classNode = x.SelectNodes(string.Format("//member[@name='T:{0}']", t.FullName));

                if (classNode == null)
                    continue;

                string baseName = t.BaseType != null && t.BaseType != typeof(Object) ? t.BaseType.Name : "";
                bool isAbstractOrInterface = t.IsInterface || t.IsAbstract;

                //No documentation
                if (classNode.Count == 0 || classNode[0].ChildNodes == null)
                {
                    context.Response.Write(string.Format("\t<class name='{0}' description='' base='{1}'", t.Name,
                        baseName));

                    if (isAbstractOrInterface)
                        context.Response.Write("abstract='1'");

                    context.Response.Write(">\n");
                }
                else
                {
                    foreach (XmlElement child in classNode[0].ChildNodes)
                    {
                        context.Response.Write(string.Format("\t<class name='{0}' description='{1}' base='{2}'", t.Name,
                            child.InnerText.Trim(), baseName));

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
                t.Namespace.StartsWith("WebAPI.Controllers") && t.Name.EndsWith("Controller")))
            {
                context.Response.Write(string.Format("\t<service name='{0}'>\n", controller.Name.Replace("Controller", "")));
                var methods = controller.GetMethods();
                foreach (var method in methods)
                {
                    //Read only HTTP POST as we will have duplicates otherwise
                    if (method.GetCustomAttributes<HttpGetAttribute>(false).Count() == 0)
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

                        context.Response.Write(string.Format("\t\t<action name='{0}' description='{1}'>\n", ((RouteAttribute)attr).Template, desc.Trim().Replace('\'', '"')));
                        foreach (var par in method.GetParameters())
                        {
                            var descs = x.SelectNodes(string.Format("//member[starts-with(@name,'M:{0}.{1}')]/param[@name='{2}']",
                                controller.FullName, method.Name, par.Name));

                            string pdesc = "";
                            if (descs.Count > 0)
                                pdesc = descs[0].InnerText.Trim().Replace('\'', '"');

                            //Handling nullables
                            var eType = Nullable.GetUnderlyingType(par.ParameterType);
                            string typeName = "";
                            if (eType != null)
                                typeName = getTypeFriendlyName(par.ParameterType.GetGenericArguments()[0]);
                            else
                                typeName = getTypeFriendlyName(par.ParameterType);

                            context.Response.Write(string.Format("\t\t\t<param name='{0}' type='{1}' description='{2}'/>\n", par.Name,
                                typeName, pdesc));
                        }

                        if (method.ReturnType.IsArray || method.ReturnType.IsGenericType)
                        {
                            string name = "";
                            if (method.ReturnType.IsArray)
                                name = getTypeFriendlyName(method.ReturnType.GetElementType());
                            else if (method.ReturnType.IsGenericType)
                                name = getTypeFriendlyName(method.ReturnType.GetGenericArguments()[0]);

                            context.Response.Write(string.Format("\t\t\t<result type='{0}' arrayType='{1}'/>\n", getTypeFriendlyName(method.ReturnType), name));
                        }
                        else
                            context.Response.Write(string.Format("\t\t\t<result type='{0}'/>\n", getTypeFriendlyName(method.ReturnType)));
                        context.Response.Write("\t\t</action>\n");
                    }
                };

                context.Response.Write(string.Format("\t</service>\n"));
            }
            context.Response.Write("</services>\n");
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

                    Type eType = null;
                    var dataMemberAttr = pi.GetCustomAttribute<DataMemberAttribute>();

                    if (dataMemberAttr == null)
                        continue;

                    var attr = dataMemberAttr.Name;

                    //Handling nullables
                    eType = Nullable.GetUnderlyingType(pi.PropertyType);
                    string typeName = "";
                    if (eType != null)
                        typeName = getTypeFriendlyName(pi.PropertyType.GetGenericArguments()[0]);
                    else
                        typeName = getTypeFriendlyName(pi.PropertyType);

                    if (pi.PropertyType.IsEnum || (eType != null && eType.IsEnum))
                    {                                                                    
                        context.Response.Write(string.Format("\t\t<property name='{0}' type='string' enumType='{1}' description='{2}' readOnly='0' insertOnly='0' />\n", attr,
                           typeName, pdesc));
                    }
                    else if (pi.PropertyType.IsArray || pi.PropertyType.IsGenericType)
                    {
                        string name = "";
                        if (pi.PropertyType.IsArray)
                            name = getTypeFriendlyName(pi.PropertyType.GetElementType());
                        else if (pi.PropertyType.IsGenericType)
                            name = getTypeFriendlyName(pi.PropertyType.GetGenericArguments()[0]);

                        context.Response.Write(string.Format("\t\t<property name='{0}' type='array' arrayType='{1}' description='{2}' readOnly='0' insertOnly='0' />\n", attr,
                            name, pdesc));
                    }
                    else
                    {
                        context.Response.Write(string.Format("\t\t<property name='{0}' type='{1}' description='{2}' readOnly='0' insertOnly='0' />\n", attr,
                        typeName, pdesc));
                    }
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