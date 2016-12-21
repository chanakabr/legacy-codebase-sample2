using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;
using System.Text;
using TVPApi;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;
using TVPPro.SiteManager;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using TVPApiModule.Manager;
using System.Configuration;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Objects.Authorization;

/// <summary>
/// Finds the Method By Reflection
/// </summary>
public partial class MethodFinder
{
    #region Parameters Initilization Strategy
    /// <summary>
    /// Strategy common function
    /// Holds functions to handle string(JSON) and object's
    /// </summary>
    private abstract class ParameterInitBase
    {
        /// <summary>
        /// Method to be execute when load parameters object values
        /// </summary>
        /// <param name="MethodParam">the type of the parameter</param>
        /// <param name="methodName">the parameter name</param>
        /// <returns>return the object representing the value for the parameter</returns>
        public abstract object InitilizeParameter(Type MethodParam, String methodName);
        /// <summary>
        /// Method to be executed when finished loading all values for all parameter.
        /// </summary>
        /// <param name="executer">the object that holds the services to execute</param>
        /// <param name="paramInfo">the list of parameters to send to method</param>
        /// <param name="methodParameters">list of parameters values</param>
        /// <returns></returns>
        public abstract string PostParametersInit(MethodFinder executer, ParameterInfo[] paramInfo, object[] methodParameters);
        /// <summary>
        /// converts a json represantation of an object (String form) to its object form.
        /// </summary>
        /// <param name="DeserializationTarget"></param>
        /// <param name="TargetType"></param>
        /// <returns></returns>
        protected object TypeDeSerialize(string DeserializationTarget, Type TargetType)
        {
            object Product = null;
            do
            {
                // for string
                if (TargetType.Name == "String")
                {
                    Product = DeserializationTarget;
                    break;
                }

                // for Date
                if (TargetType.FullName == "System.DateTime")
                {
                    Product = Convert.ToDateTime(DeserializationTarget);
                    break;
                }

                if (TargetType.IsByRef)
                {
                    string itsName = TargetType.FullName.Replace("&", "");
                    TargetType = Type.GetType(itsName);
                }

                if (TargetType.IsArray) //Array
                {
                    JavaScriptSerializer ser = new JavaScriptSerializer();
                    try
                    {
                        if (TargetType.ToString().Equals("System.String[]"))
                        {
                            Product = Array.ConvertAll<object, string>((object[])ser.DeserializeObject(DeserializationTarget), Convert.ToString);
                        }
                        else if (TargetType.ToString().Equals("System.Int64[]"))
                        {
                            Product = Array.ConvertAll<object, long>((object[])ser.DeserializeObject(DeserializationTarget), Convert.ToInt64);
                        }
                        else if (TargetType.ToString().Equals("System.Int32[]"))
                        {
                            Product = Array.ConvertAll<object, int>((object[])ser.DeserializeObject(DeserializationTarget), Convert.ToInt32);
                        }
                        else
                        {
                            Product = ser.GetType().GetMethod("Deserialize", new Type[] { typeof(string) }).
                                MakeGenericMethod(TargetType).Invoke(ser, new object[] { DeserializationTarget });
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("", ex);
                    }
                }
                else
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(DeserializationTarget)))
                        {
                            DataContractJsonSerializer serializer = new DataContractJsonSerializer(TargetType);
                            Product = serializer.ReadObject(ms);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("", ex);
                        //XmlSerializer serializer = new XmlSerializer(TargetType);
                        //using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(DeserializationTarget)))
                        //{
                        //    Product = serializer.Deserialize(ms);
                        //    //using (XmlWriter xw = XmlWriter.Create(ms))
                        //    //{

                        //    //    Product = CreateObjectInstance(TargetType);


                        //    //}
                        //}                        
                        ////System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(TargetType);                                                

                        JavaScriptSerializer serializer = new JavaScriptSerializer();

                        var dict = serializer.Deserialize<Dictionary<string, object>>(DeserializationTarget);
                        Product = CreateObjectInstance(TargetType);

                        Parse(dict, Product);

                        //DeserializationTarget = serializer.Serialize(parsedDictionary);

                        //using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(DeserializationTarget)))
                        //{
                        //    DataContractJsonSerializer ser = new DataContractJsonSerializer(TargetType);
                        //    Product = ser.ReadObject(ms);
                        //}

                        //Product = typeof(JavaScriptSerializer).GetMethod("Deserialize").MakeGenericMethod(TargetType).Invoke(serializer, new object[] { DeserializationTarget });
                    }
                }
            } while (false);
            return Product;
        }

        private void Parse(Dictionary<string, object> dic, object product)
        {
            if (dic is Dictionary<string, object>)
            {
                foreach (PropertyInfo propInfo in product.GetType().GetProperties())//check if object and properties of type objects and create them as well
                {
                    if (propInfo.PropertyType.IsClass && propInfo.PropertyType.Name != "String")
                    {
                        string key = string.Format("{0}Field", propInfo.Name);
                        if (dic.ContainsKey(key))
                        {
                            propInfo.SetValue(product, CreateObjectInstance(propInfo.PropertyType), null);
                            Parse(dic[key] as Dictionary<string, object>, propInfo.GetValue(product, null));
                        }
                        else
                        {
                            key = propInfo.Name;
                            if (dic.ContainsKey(key))
                            {
                                propInfo.SetValue(product, CreateObjectInstance(propInfo.PropertyType), null);
                                Parse(dic[key] as Dictionary<string, object>, propInfo.GetValue(product, null));
                            }
                        }
                    }
                    else
                    {
                        string key = string.Format("{0}Field", propInfo.Name);
                        if (dic.ContainsKey(key))
                        {
                            propInfo.SetValue(product, dic[key] as object, null);
                        }
                        else
                        {
                            key = propInfo.Name;
                            if (dic.ContainsKey(key))
                            {
                                propInfo.SetValue(product, dic[key] as object, null);
                            }
                        }
                    }
                }
            }

            //Dictionary<string, object> result = new Dictionary<string, object>();
            //if (dic is Dictionary<string, object>)
            //{
            //    foreach (string key in dic.Keys)
            //    {
            //        string paraseKey = Regex.Replace(key, @"field$", String.Empty, RegexOptions.IgnoreCase);
            //        Dictionary<string, object> parsedKeyResult = Parse(dic[key] as Dictionary<string, object>);

            //        result.Add(paraseKey, parsedKeyResult ?? dic[key]);
            //    }
            //}
            //else
            //{
            //    result = null;
            //}
            //return result;
        }

        protected T ConvertTotype<T>(object objToConvert)
        {
            return (T)objToConvert;
        }

        /// <summary>
        /// Searches the reuqested type
        /// </summary>
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

        /// <summary>
        /// Create a default instance of the givien type
        /// </summary>
        /// <param name="MethodParam"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Convert an object to its json represantation (string form)
        /// </summary>
        /// <param name="SerializationTarget"></param>
        /// <returns></returns>
        protected virtual string JSONSerialize(object SerializationTarget)
        {
            string Product = String.Empty;
            do
            {
                if (SerializationTarget == null)
                    break;
                //using (MemoryStream ms = new MemoryStream())
                //{
                //    DataContractJsonSerializer serializer = new DataContractJsonSerializer(SerializationTarget.GetType());
                //    serializer.WriteObject(ms, SerializationTarget);
                //    Product = Encoding.UTF8.GetString(ms.ToArray());
                //}
                Product = Newtonsoft.Json.JsonConvert.SerializeObject(SerializationTarget);
            } while (false);
            return Product;
        }
        /// <summary>
        /// Since enum names are been included in json string (not values of enums)
        /// we need to change the enum name (present in json string) to its equivalent enum value by type.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="paramInfo"></param>
        /// <param name="currName"></param>
        /// <param name="methodParameters"></param>
        /// <param name="inObject">whether the function searches in objects or simple types</param>
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

        /// <summary>
        /// Replaces the enum in string
        /// </summary>
        /// <param name="json"></param>
        /// <param name="replaceThisName"></param>
        /// <param name="EnumType"></param>
        /// <param name="currValue"></param>
        /// <param name="inObject"></param>
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
        /// <summary>
        /// strategy method - each child implements its version of replaceing the enum
        /// See comments on method [HandleEnumInJson]
        /// </summary>
        /// <param name="json"></param>
        /// <param name="EnumType"></param>
        /// <param name="currValue"></param>
        protected abstract void ReplaceStratagy(ref string json, Type EnumType, object currValue);
    }

    private class ParameterDefaultInit : ParameterInitBase
    {

        public override object InitilizeParameter(Type MethodParam, String methodName)
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

        protected override string JSONSerialize(object SerializationTarget)
        {
            StringBuilder requeredObjectToThisParam = new StringBuilder();
            Type myType = SerializationTarget.GetType();

            if (!myType.Namespace.StartsWith("System") && myType.IsClass && !myType.IsArray)
            {
                requeredObjectToThisParam.Append("{");
                foreach (PropertyInfo propInfo in SerializationTarget.GetType().GetProperties())//check if object and properties of type objects and create them as well
                {
                    do
                    {
                        requeredObjectToThisParam.Append(String.Format(@"""{0}"":", propInfo.Name));
                        if ((propInfo.PropertyType.IsPrimitive || propInfo.PropertyType.IsValueType) && !propInfo.PropertyType.IsEnum)
                        {
                            requeredObjectToThisParam.Append(String.Format(@"""{0}""", propInfo.PropertyType.Name));
                            break;
                        }
                        if (propInfo.PropertyType.Name == "String")
                        {
                            requeredObjectToThisParam.Append(@"""String""");
                            break;
                        }
                        if (propInfo.PropertyType.IsArray)
                        {
                            requeredObjectToThisParam.Append(String.Format(@"[""{0}""]", propInfo.PropertyType.Name.Replace("[]", "")));
                            break;
                        }
                        if (propInfo.PropertyType.IsClass)
                        {
                            requeredObjectToThisParam.Append(this.JSONSerialize(propInfo.GetValue(SerializationTarget, null)));
                            break;
                        }
                    } while (false);
                    requeredObjectToThisParam.Append(",");
                }
                requeredObjectToThisParam.Remove(requeredObjectToThisParam.Length - 1, 1);
                requeredObjectToThisParam.Append("}");
            }
            else
            {
                do
                {
                    if ((myType.IsPrimitive || myType.IsValueType) && !myType.IsEnum)
                    {
                        requeredObjectToThisParam.Append(String.Format(@"""{0}""", myType.Name));
                        break;
                    }
                    if (myType.Name == "String")
                    {
                        requeredObjectToThisParam.Append(@"""String""");
                        break;
                    }
                    if (myType.IsArray)
                    {
                        requeredObjectToThisParam.Append(String.Format(@"[""{0}""]", myType.Name.Replace("[]", "")));
                        break;
                    }
                } while (false);
            }

            return requeredObjectToThisParam.ToString();
        }

        /// <summary>
        /// convert all objects to json format
        /// </summary>
        /// <param name="executer"></param>
        /// <param name="paramInfo"></param>
        /// <param name="methodParameters"></param>
        /// <returns></returns>
        public override string PostParametersInit(MethodFinder executer, ParameterInfo[] paramInfo, object[] methodParameters)
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

        protected override void ReplaceStratagy(ref string json, Type EnumType, object currValue)
        {
            StringBuilder enumNames = new StringBuilder();
            foreach (String eName in Enum.GetNames(EnumType))
            {
                enumNames.Append(String.Format(@"{0}", eName)).Append(" || ");
            }
            enumNames.Remove(enumNames.Length - " || ".Length, " || ".Length);
            json = string.Concat(@"""", enumNames.ToString(), @"""");
        }
    }

    private class ParameterJsonInit : ParameterInitBase
    {
        private static List<string> _authorizationUnsupportedGroupsPlatforms = string.IsNullOrEmpty(ConfigurationManager.AppSettings["Authorization.UnsupportedGroupsPlatforms"]) ? null : ConfigurationManager.AppSettings["Authorization.UnsupportedGroupsPlatforms"].Split(',').ToList();

        /// <summary>
        /// enumerate over the parameter of type Object to check it has properties of type enum
        /// (recrusivly)
        /// </summary>
        /// <param name="json"></param>
        /// <param name="MethodParam"></param>
        private void InspectObjectForEnums(ref string json, Type MethodParam, string methodParamName)
        {
            do
            {
                if (MethodParam.IsEnum)
                {
                    ReplaceEnumValue(ref json, methodParamName, MethodParam, null, false);
                }
                if (MethodParam.Name != "String" && MethodParam.IsClass)
                {
                    foreach (PropertyInfo propInfo in MethodParam.GetProperties())
                    {
                        if (propInfo.PropertyType.IsEnum)
                        {
                            ReplaceEnumValue(ref json, propInfo.Name, propInfo.PropertyType, null, true);
                        }
                        else if (propInfo.PropertyType.Name != "String" && propInfo.PropertyType.IsClass)
                        {
                            InspectObjectForEnums(ref json, propInfo.PropertyType, propInfo.Name);
                        }
                    }
                }
            } while (false);
        }

        public override object InitilizeParameter(Type MethodParam, String methodName)
        {
            if (HttpContext.Current.Items.Contains(methodName))
            {
                string paramValues = HttpContext.Current.Items[methodName].ToString();//.Replace("'","\"");

                InspectObjectForEnums(ref paramValues, MethodParam, methodName);//replace enum values before deserialize

                object ret = TypeDeSerialize(paramValues, MethodParam);

                return ret;
            }
            else if ((HttpContext.Current.Items.Keys.Cast<string>().ToList()).Contains(methodName, StringComparer.OrdinalIgnoreCase))
            {
                string paramValues = (from key in HttpContext.Current.Items.Keys.Cast<string>().ToList()
                                      where string.Compare(key, methodName, true) == 0
                                      select HttpContext.Current.Items[key]).FirstOrDefault<object>().ToString();

                InspectObjectForEnums(ref paramValues, MethodParam, methodName);//replace enum values before deserialize

                object ret = TypeDeSerialize(paramValues, MethodParam);



                return ret;
            }
            else
            {
                if (MethodParam.Equals(typeof(int))) return 0;
                if (MethodParam.Equals(typeof(string))) return string.Empty;
                if (MethodParam.IsValueType && MethodParam.IsPrimitive) return Activator.CreateInstance(MethodParam, false);
                //throw new Exception(string.Format("Error with '{0}' parameter.", methodName));
                return null;
            }
        }

        public override string PostParametersInit(MethodFinder executer, ParameterInfo[] paramInfo, object[] methodParameters)
        {
            // validate authorization token:
            InitializationObject initObj = (InitializationObject)methodParameters.Where(p => p is InitializationObject).First();
            int groupID = ConnectionHelper.GetGroupID("tvpapi", executer.m_MetodInfo.Name, initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            string platform = initObj.Platform.ToString();
            string groupPlatformPair = string.Format("{0}_{1}", groupID, platform); // build the configuration value
            if (_authorizationUnsupportedGroupsPlatforms == null || !_authorizationUnsupportedGroupsPlatforms.Contains(groupPlatformPair)) // authorization supported
            {
                string siteGuid = null;
                bool isAdmin = false;
                // validate unauthorized methods and extract relevant siteGuid
                if (executer.m_MetodInfo.Name != "RefreshAccessToken" && !AuthorizationManager.Instance.IsAccessTokenValid(initObj.Token, initObj.DomainID, groupID, initObj.Platform, initObj.UDID, out siteGuid, out isAdmin))
                {
                    return null;
                }
                if (executer.m_MetodInfo.Name != "RefreshAccessToken" && executer.m_MetodInfo.GetCustomAttributes(typeof(PrivateMethodAttribute), false).Length > 0 && string.IsNullOrEmpty(siteGuid))
                {
                    AuthorizationManager.Instance.returnError(401, null);
                    return null;
                }
                // add "tokenization" to context for later validations (only if not admin)
                if (!isAdmin)
                {
                    if (executer.m_MetodInfo.Name != "RefreshAccessToken")
                    {
                        // override siteGuid in initObj
                        initObj.SiteGuid = siteGuid;
                    }
                    HttpContext.Current.Items.Add("tokenization", null);
                }
            }

            object result = executer.ExecuteMethod(methodParameters);
            string convertedToJsonResult = base.JSONSerialize(result);

            return convertedToJsonResult;
        }

        protected override void ReplaceStratagy(ref string json, Type EnumType, object currValue)
        {
            int tEnum = 0;
            if (currValue != null && int.TryParse(currValue.ToString(), out tEnum))
            {
                currValue = Enum.Parse(EnumType, currValue.ToString());
            }

            foreach (Enum e in Enum.GetValues(EnumType))
            {
                if (currValue == null)
                {
                    if (e.ToString() == json)
                    {
                        json = Convert.ToInt32(e).ToString();
                        break;
                    }
                }
                else if (e.ToString() == currValue.ToString())//.Replace("\"",""))
                {
                    json = Convert.ToInt32(e).ToString();
                    break;
                }
            }
        }
    }

    #endregion

    /// <summary>
    /// Handles all error generated
    /// </summary>
    /// <param name="msg"></param>
    protected void ErrorHandler(string msg)
    {
        String msgFormat = "{ \"Error\": \"" + msg + "\" }";

        WriteResponseBackToClient(msgFormat);
    }

    /// <summary>
    /// Checks that all initial parameters have been supplied correctly by client.
    /// 1. Method specified in request query string - 'm'
    /// 2. the method exists in any of the supplied web-services
    /// 
    /// when all requirements (at this point) have met, then extract the Method and its Parameters by Reflection
    /// </summary>
    /// <returns></returns>
    private bool VerifyAllParametersCheck()
    {
        do
        {
            string methodName = HttpContext.Current.Request.QueryString["m"];

            if (String.IsNullOrEmpty(methodName))
            {
                ErrorHandler("Method Name does NOT included in Query String.. Please add to URL: [URL]?m={method_name}");
                break;
            }

            foreach (System.Web.Services.WebService service in BackWebservice)
            {
                m_MetodInfo = service.GetType().GetMethod(methodName);
                if (m_MetodInfo != null)
                {
                    Webservice = service;
                    break;
                }
            }

            if (m_MetodInfo == null)
            {
                ErrorHandler(String.Format("The method you specified[ {0} ] is NOT part of this Services..", methodName));
                break;
            }

            MethodParameters = m_MetodInfo.GetParameters();
            return true;
        } while (false);

        return false;
    }

    /// <summary>
    /// Desides which strategy to use in order to fulfill the request,
    /// decision is made by request type
    /// if GET: the user is asking to know whats the structure the parameters (JSON) the method requiers.
    /// if POST: the user asking to execute the requested method.
    /// </summary>
    /// <returns></returns>
    private ParameterInitBase GetExecuter()
    {
        ParameterInitBase _strategy = null;

        if (IsPost)
            _strategy = new ParameterJsonInit();
        else
            _strategy = new ParameterDefaultInit();

        return _strategy;
    }

    public object ExecuteMethod(object[] methodParameters)
    {
        object JSONMethodReturnValue = m_MetodInfo.Invoke(Webservice, methodParameters);
        return JSONMethodReturnValue;
    }
}