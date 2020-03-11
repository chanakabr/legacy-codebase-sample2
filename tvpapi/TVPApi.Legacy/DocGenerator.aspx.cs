using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVPApi.Common;

public partial class DocGenerator : BaseGateway
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string[] ignoreMethods = { "get_Application",
"get_Context",
"get_Session",
"get_Server",
"get_User",
"get_SoapVersion",
"add_Disposed",
"remove_Disposed",
"get_Site",
"set_Site",
"Dispose",
"get_Container",
"GetService",
"get_DesignMode",
"ToString",
"Equals",
"GetHashCode",
"GetType" };
        object[] ws = new JsonPostGateway().GetWebServices();
        foreach (var w in ws)
        {
            Response.Write("<html><body>");
            Response.Write(string.Format("<h3>{0}</h3>", w.GetType()));
            var ms = w.GetType().GetMethods().OrderBy(item => item.Name);

            foreach (var a in ms)
            {
                if (ignoreMethods.Contains(a.Name))
                    continue;

               // Response.Write("<p style='padding-bottom: 10px'>");
                Response.Write(string.Format("<div>{0}</div>", a.Name));

                //var attt = a.GetCustomAttributes(true);

                //if (attt.Count() > 0)
                //{
                //    var att = attt.Where(x => x is WebMethodAttribute).First();

                //    if (att != null)
                //    {
                //        Response.Write(string.Format("<h4>Description</h4>"));
                //        Response.Write(((System.Web.Services.WebMethodAttribute)att).Description);
                //    }


                //}
                //object[] CallParameters = new object[a.GetParameters().Length];
                //for (int i = 0; i < a.GetParameters().Length; i++)
                //{
                //    ParameterInfo TargetParameter = a.GetParameters()[i];

                //    // get the object value of the parameter
                //    CallParameters[i] = InitilizeParameter(TargetParameter.ParameterType, TargetParameter.Name);
                //}

                //// post handle request
                //string SerializedReturnValue = PostParametersInit(a.GetParameters(), CallParameters);
                //Response.Write(string.Format("<h4>Request</h4><p>{0}</p>", SerializedReturnValue));

                //for (int i = 0; i < a.GetParameters().Length; i++)
                //{
                //    ParameterInfo TargetParameter = a.GetParameters()[i];

                //    Response.Write(string.Format("<div><b>{0}</b> - {1}", TargetParameter.Name, TargetParameter.ParameterType.Name));
                //    if (attt.Where(x => x is RequestDescription && (x as RequestDescription).paramName == TargetParameter.Name).Count() > 0)
                //    {
                //        var ab = attt.Where(x => x is RequestDescription && (x as RequestDescription).paramName == TargetParameter.Name).First();
                //        Response.Write(" - <b>" + (ab as RequestDescription).paramDesc + "</b></div>");
                //    }
                //    else
                //        Response.Write("</div>");
                //}

               // Response.Write("</p>");
            }

            Response.Write("</body></html>");
        }
        return;
    }

    public string PostParametersInit(ParameterInfo[] paramInfo, object[] methodParameters)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        for (int i = 0; i < methodParameters.Length; i++)
        {
            sb.Append(@" """).Append(paramInfo[i].Name).Append(@""": ");
            //string json = String.Format("{0}{1}{0}", paramInfo[i].ParameterType.IsClass && paramInfo[i].ParameterType.Name != "String" ? "" : "", JSONSerialize(methodParameters[i]));                                
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(methodParameters[i]);
            HandleEnumInJson(ref json, paramInfo[i].ParameterType, paramInfo[i].Name, methodParameters[i], false);
            sb.Append(json).Append(",");
        }
        sb.Remove(sb.Length - 1, 1);
        sb.Append("}");

        return sb.ToString();
    }

    protected void HandleEnumInJson(ref string json, Type paramInfo, string currName, object methodParameters, bool inObject)
    {
        do
        {
            // if not object or Enum
            if (paramInfo.Name == "String" || (paramInfo.IsValueType && !paramInfo.IsEnum))
            {
                break;
            }
            //if Enum
            if (paramInfo.IsEnum)
            {
                /*Each stretegy changes the enum value differently
                 * by design -
                 * GET request - changes enum VALUES to enum NAMES.
                 * POST request - changes enum NAMES to enum VALUES.
                 **/
                ReplaceEnumValue(ref json, currName, paramInfo, methodParameters, inObject);
            }
            //if object - search for enums presence in it
            if (!paramInfo.Namespace.StartsWith("System"))
            {
                foreach (PropertyInfo propInfo in paramInfo.GetProperties())
                {
                    if (propInfo.PropertyType.BaseType != null && new List<String>() { "Enum", "Object" }.Contains(propInfo.PropertyType.BaseType.Name))
                    {
                        //search recrusivly
                        HandleEnumInJson(ref json, propInfo.PropertyType, propInfo.Name, propInfo.GetValue(methodParameters, null), true);
                    }
                }
            }

        } while (false);
    }

    protected void ReplaceEnumValue(ref string json, string replaceThisName, Type EnumType, object currValue, bool inObject)
    {

        do
        {
            if (!inObject)
            {
                ReplaceStratagy(ref json, EnumType, currValue);
                break;
            }
            /*****************/
            // re-build the json string with the updated enum VALUES|NAMES.
            Newtonsoft.Json.Linq.JObject oJson = Newtonsoft.Json.Linq.JObject.Parse(json);

            Newtonsoft.Json.Linq.JToken oCurToken = (oJson[replaceThisName] != null) ? oCurToken = oJson[replaceThisName] : oCurToken = oJson["Locale"][replaceThisName];

            int index = json.IndexOf(replaceThisName);
            //String partA = json.Substring(0, index + replaceThisName.Length +2);
            //String partB = json.Substring(partA.Length);

            //int indexOfNextProp = partB.IndexOf(",");
            //int indexOfNextObject = partB.IndexOf("}");
            //if (indexOfNextProp != -1 && indexOfNextProp < indexOfNextObject) index = indexOfNextProp;
            //else index = indexOfNextObject;



            //String ReplaceThis = partB.Substring(1, index);
            //ReplaceThis = ReplaceThis.Replace("'", string.Empty);
            //String partBWithoutValue = partB.Substring(ReplaceThis.Length);

            String replacement = String.Empty;

            ReplaceStratagy(ref replacement, EnumType, oCurToken.ToString());

            if (oJson[replaceThisName] != null)
            {
                oJson[replaceThisName] = replacement.Replace("\"", string.Empty);
            }
            else
            {
                oJson["Locale"][replaceThisName] = replacement.Replace("\"", string.Empty);
            }

            json = oJson.ToString(Newtonsoft.Json.Formatting.None);

            //json = String.Format("{0}{1}{2}", partA, replacement, partBWithoutValue);
            /*****************/
        } while (false);
    }

    protected void ReplaceStratagy(ref string json, Type EnumType, object currValue)
    {
        StringBuilder enumNames = new StringBuilder();
        foreach (String eName in Enum.GetNames(EnumType))
        {
            enumNames.Append(String.Format(@"{0}", eName)).Append(" || ");
        }
        enumNames.Remove(enumNames.Length - " || ".Length, " || ".Length);
        json = string.Concat(@"""", enumNames.ToString(), @"""");
    }

    public object InitilizeParameter(Type MethodParam, String methodName)
    {
        object requeredObjectToThisParam = CreateObjectInstance(MethodParam);
        if (!MethodParam.Namespace.StartsWith("System") && MethodParam.IsClass && !MethodParam.IsArray)
        {
            foreach (PropertyInfo propInfo in requeredObjectToThisParam.GetType().GetProperties())//check if object and properties of type objects and create them as well
            {
                if (propInfo.PropertyType.Name == "String") propInfo.SetValue(requeredObjectToThisParam, String.Empty, null);
                else if (propInfo.PropertyType.IsClass) propInfo.SetValue(requeredObjectToThisParam, InitilizeParameter(propInfo.PropertyType, ""), null);
            }
        }

        return requeredObjectToThisParam;
    }

    protected object CreateObjectInstance(Type MethodParam)
    {
        object result = null;
        do
        {
            if (MethodParam.IsByRef)// if parameter is (ref [Type] [param name])
            {
                string itsName = MethodParam.FullName.Replace("&", "");
                if (!TryFindType(itsName, out MethodParam)) return null;
            }

            if (MethodParam.IsPrimitive)
            {
                result = Activator.CreateInstance(MethodParam, true);
                break;
            }

            if (MethodParam.IsValueType && !MethodParam.IsEnum)
            {
                result = Activator.CreateInstance(MethodParam, new object[] { (object)0 });
                break;
            }

            if (MethodParam.Name == "String")
            {
                result = String.Empty;
                break;
            }

            if (MethodParam.IsArray)
            {
                Type innerType = MethodParam.GetElementType();
                string underineObjectType = MethodParam.FullName.Replace("[]", "");
                if (!TryFindType(underineObjectType, out MethodParam)) return null;
                result = Array.CreateInstance(MethodParam, 1);
                object firstElement = null;
                ConstructorInfo constructor = innerType.GetConstructors().OrderBy(c => c.GetParameters().Length).FirstOrDefault();
                if (constructor != null)
                {
                    firstElement = constructor.Invoke(new object[constructor.GetParameters().Length]);
                }
                ((Array)(result)).SetValue(firstElement, 0);
                break;
            }

            result = Activator.CreateInstance(MethodParam, true);

        } while (false);
        return result;
    }

    public bool TryFindType(string typeName, out Type t)
    {
        t = Type.GetType(typeName);
        if (t == null)
        {
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = a.GetType(typeName);
                if (t != null)
                    break;
            }
        }
        return t != null;
    }
}