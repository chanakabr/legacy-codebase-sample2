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

/// <summary>
/// Finds the Method By Reflection
/// </summary>
public partial class MethodFinder
{    
    #region Procedures For Types
    private delegate object FuncByType(string key,Type targetType);
    private Dictionary<System.Type, FuncByType> FuncsByTypesDic;
    #endregion   

    private T ConvertToEnum<T>(string value)
    {
        if (String.IsNullOrEmpty(value))
            return default(T);
        return (T)Enum.Parse(typeof(T), value);
    }

    public MethodFinder(System.Web.Services.WebService fromService, System.Web.Services.WebService backService)
    {
        Webservice = fromService;
        BackWebservice = backService;                
    }      

    private System.Type GetUnderLineType(System.Type type)
    {
        return type.BaseType.Name == "ValueType" ? typeof(ValueType) :
            type.BaseType.Name == "Enum" ? typeof(Enum) : type;
    }

    private void BuildParameterObjectString(System.Type paramType, ref StringBuilder root)
    {
        if (paramType.BaseType.Name == "ValueType" || paramType == typeof(String))
            root.Append("'").Append(paramType.Name).Append("'");
        else if (paramType.BaseType.Name == "Enum")
        {
            root.Append("'");
            foreach (String t in Enum.GetNames(paramType))
            {
                root.Append(t).Append("||");
            }
            root.Remove(root.Length - 2, 2);
            root.Append("'");
        }
        else
        {
            root.Append("{");
            foreach (PropertyInfo propInfo in paramType.GetProperties())
            {
                root.Append(propInfo.Name).Append(":");
                BuildParameterObjectString(propInfo.PropertyType, ref root);
            }
            root.Append("}");
        }
        root.Append(",");
    }

    private object GetParam(String key, System.Type targetType)
    {
        string result = HttpContext.Current.Request.Params[key];
        object ret;
        if (targetType == typeof(TVPApi.InitializationObject))
            ret = GetInitObject(result);
        else if (targetType == typeof(Tvinci.Data.TVMDataLoader.Protocols.MediaMark.action))
            ret = ConvertToEnum < Tvinci.Data.TVMDataLoader.Protocols.MediaMark.action>(result);
        else if (targetType == typeof(TVPPro.SiteManager.TvinciPlatform.Social.SocialAction))
            ret = ConvertToEnum<TVPPro.SiteManager.TvinciPlatform.Social.SocialAction>(result);
        else if (targetType == typeof(TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform))
            ret = ConvertToEnum<TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform>(result);
        else if (targetType == typeof(TVPApi.ActionType))
            ret = ConvertToEnum<TVPApi.ActionType>(result);
        else if (targetType != typeof(String))
            ret = TypeDeSerialize(result, targetType);
        else
            ret = result;
        return ret; 
    }

    private InitializationObject GetInitObject(string objSeri)
    {
        foreach (PlatformType t in Enum.GetValues(typeof(PlatformType)))
        {
            objSeri.Replace(t.ToString(), ((int)t).ToString());
        }
        foreach (TVPApi.LocaleUserState t in Enum.GetValues(typeof(TVPApi.LocaleUserState)))
        {
            objSeri.Replace(t.ToString(), ((int)t).ToString());
        }

        var s = new System.Web.Script.Serialization.JavaScriptSerializer();
        return s.Deserialize<InitializationObject>(objSeri);
    }

    private object TypeDeSerialize(string DeserializationTarget, Type TargetType)
    {
        MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(DeserializationTarget));
        DataContractJsonSerializer serializer = new DataContractJsonSerializer(TargetType);
        object Product = serializer.ReadObject(ms);
        ms.Close();
        return Product;
    }

}