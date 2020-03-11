using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Reflection;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;
using TVPApi;
using KLogMonitor;
using ApiObjects;
using InitializationObject = TVPApi.InitializationObject;
using TVPApi.Common;

public partial class Gateways_JsonGateway : BaseGateway
{
    private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    protected void Page_Load(object sender, EventArgs e)
    {
        string MethodName = Request.QueryString["MethodName"];
        string Str = String.Empty;
        var gateway = new JsonPostGateway();

        object webservice = gateway.GetMediaService();
        MethodInfo WSMethod = webservice.GetType().GetMethod(MethodName);
        if (WSMethod == null)
        {
            webservice = gateway.GetSiteService();
            WSMethod = webservice.GetType().GetMethod(MethodName);
        }
        if (WSMethod == null)
        {
            webservice = gateway.GetDomainService();
            WSMethod = webservice.GetType().GetMethod(MethodName);
        }
        if (WSMethod != null)
        {
            ParameterInfo[] MethodParameters = WSMethod.GetParameters();
            object[] CallParameters = new object[MethodParameters.Length];
            for (int i = 0; i < MethodParameters.Length; i++)
            {
                ParameterInfo TargetParameter = MethodParameters[i];

                //string RawParameter = Context.Request.Form[TargetParameter.Name];
                string RawParameter = Context.Request.QueryString[TargetParameter.Name];
                if (TargetParameter.ParameterType == typeof(TVPApi.InitializationObject))
                    CallParameters[i] = GetInitObj2();
                else if (TargetParameter.ParameterType == typeof(Tvinci.Data.TVMDataLoader.Protocols.MediaMark.action))
                    CallParameters[i] = parseAction(RawParameter);
                else if (TargetParameter.ParameterType == typeof(eUserAction))
                    CallParameters[i] = parseSocialAction(RawParameter);
                else if (TargetParameter.ParameterType == typeof(SocialAction))
                    CallParameters[i] = parseApiSocialAction(RawParameter);
                else if (TargetParameter.ParameterType == typeof(SocialPlatform))
                    CallParameters[i] = (SocialPlatform)Enum.Parse(typeof(SocialPlatform), RawParameter);
                else if (TargetParameter.ParameterType == typeof(SocialPlatform))
                    CallParameters[i] = parseSocialPlatform(RawParameter);
                else if (TargetParameter.ParameterType == typeof(TVPApi.ActionType))
                    CallParameters[i] = parseActionType(RawParameter);
                else if (TargetParameter.ParameterType == typeof(MediaHelper.ePeriod))
                    CallParameters[i] = parsePeriodType(RawParameter);
                else if (TargetParameter.ParameterType == typeof(TVPApi.OrderBy))
                    CallParameters[i] = parseOrderByType(RawParameter);
                else if (TargetParameter.ParameterType != typeof(String))

                    CallParameters[i] = TypeDeSerialize(RawParameter, TargetParameter.ParameterType);
                else
                    CallParameters[i] = RawParameter;

                Str += TargetParameter.Name + ", ";
            }
            object JSONMethodReturnValue = WSMethod.Invoke(webservice, CallParameters);
            string SerializedReturnValue = JSONSerialize(JSONMethodReturnValue);
            Context.Response.HeaderEncoding =
            Context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            Context.Response.Charset = "utf-8";
            Context.Response.Write(SerializedReturnValue);

        }
        //Response.Write(Str);
    }

    private TVPApi.OrderBy parseOrderByType(string param)
    {
        if (string.IsNullOrEmpty(param)) param = TVPApi.OrderBy.None.ToString();
        TVPApi.OrderBy orderBy = (TVPApi.OrderBy)Enum.Parse(typeof(TVPApi.OrderBy), param);
        return orderBy;
    }

    private MediaHelper.ePeriod parsePeriodType(string param)
    {
        MediaHelper.ePeriod period = (MediaHelper.ePeriod)Enum.Parse(typeof(MediaHelper.ePeriod), param);
        return period;
    }

    private TVPApi.ActionType parseActionType(string param)
    {
        TVPApi.ActionType action = (TVPApi.ActionType)Enum.Parse(typeof(TVPApi.ActionType), param);
        return action;
    }

    private SocialPlatform parseSocialPlatform(string param)
    {
        if (param.ToLower() == "facebook")
            return SocialPlatform.FACEBOOK;

        return SocialPlatform.UNKNOWN;
    }

    private SocialAction parseApiSocialAction(string param)
    {
        return (SocialAction)Enum.Parse(typeof(SocialAction), param);
    }

    private eUserAction parseSocialAction(string param)
    {
        if (param.ToLower() == "post")
            return eUserAction.POST;

        return eUserAction.UNKNOWN;
    }

    private Tvinci.Data.TVMDataLoader.Protocols.MediaMark.action parseAction(String param)
    {
        if (param == "finish")
            return Tvinci.Data.TVMDataLoader.Protocols.MediaMark.action.finish;
        else if (param == "first_play")
            return Tvinci.Data.TVMDataLoader.Protocols.MediaMark.action.first_play;
        else if (param == "load")
            return Tvinci.Data.TVMDataLoader.Protocols.MediaMark.action.load;
        else if (param == "none")
            return Tvinci.Data.TVMDataLoader.Protocols.MediaMark.action.none;
        else if (param == "pause")
            return Tvinci.Data.TVMDataLoader.Protocols.MediaMark.action.pause;
        else if (param == "play")
            return Tvinci.Data.TVMDataLoader.Protocols.MediaMark.action.play;
        else if (param == "stop")
            return Tvinci.Data.TVMDataLoader.Protocols.MediaMark.action.stop;
        return Tvinci.Data.TVMDataLoader.Protocols.MediaMark.action.none;
    }

    private object TypeDeSerialize(string DeserializationTarget, Type TargetType)
    {
        object Product = new object();

        using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(DeserializationTarget)))
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(TargetType);
            Product = serializer.ReadObject(ms);
        }

        return Product;
    }

    private string JSONSerialize(object SerializationTarget)
    {
        string Product = string.Empty;
        using (MemoryStream ms = new MemoryStream())
        {
            var properties = from p in SerializationTarget.GetType().GetProperties()
                             where p.PropertyType == typeof(DateTime) &&
                                   p.CanRead &&
                                   p.CanWrite
                             select p;

            foreach (var property in properties)
            {
                var value = (DateTime)property.GetValue(SerializationTarget, null);
                if (value == null || value.Kind == DateTimeKind.Unspecified)
                {
                    property.SetValue(SerializationTarget, new DateTime(1970, 1, 1), null);
                }
            }
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(SerializationTarget.GetType());
            serializer.WriteObject(ms, SerializationTarget);
            Product = Encoding.UTF8.GetString(ms.ToArray());
            //string Product = Encoding.Default.GetString(ms.ToArray());
        }
        return Product;
    }

    protected InitializationObject GetInitObj2()
    {
        //InitializationObject retVal = new InitializationObject();
        //retVal.Platform = PlatformType.STB;
        //retVal.ApiUser = "tvpapi_125";
        //retVal.ApiPass = "11111";
        //Locale locale = new Locale();
        //locale.LocaleUserState = LocaleUserState.Unknown;
        //return retVal;

        InitializationObject retVal = base.GetInitObj();
        Locale locale = new Locale();
        locale.LocaleUserState = LocaleUserState.Unknown;
        retVal.Locale = locale;

        retVal.UDID = Request.QueryString["UUID"];
        if (string.IsNullOrEmpty(retVal.UDID)) retVal.UDID = Request.QueryString["UDID"];

        retVal.ApiUser = Request.QueryString["ApiUser"];
        retVal.ApiPass = Request.QueryString["ApiPass"];

        if (Request.QueryString["Platform"] != null)
        {
            switch (Request.QueryString["Platform"].ToLower())
            {
                case "cellular":
                    retVal.Platform = PlatformType.Cellular;
                    break;
                case "connectedtv":
                    retVal.Platform = PlatformType.ConnectedTV;
                    break;
                case "stb":
                    retVal.Platform = PlatformType.STB;
                    break;
                case "web":
                    retVal.Platform = PlatformType.Web;
                    break;
                case "ipad":
                    retVal.Platform = PlatformType.iPad;
                    break;
                default:
                    retVal.Platform = PlatformType.Unknown;
                    break;
            }
        }
        retVal.SiteGuid = Request.QueryString["SiteGuid"];
        if (Request.QueryString["DomainID"] != null)
            retVal.DomainID = int.Parse(Request.QueryString["DomainID"]);

        return retVal;
    }

}