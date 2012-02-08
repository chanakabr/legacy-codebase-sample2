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

public partial class Gateways_JsonGateway : BaseGateway
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string MethodName = Request.QueryString["MethodName"];
        string Str = String.Empty;
        System.Web.Services.WebService webservice = m_MediaService;
        MethodInfo WSMethod = webservice.GetType().GetMethod(MethodName);
        if (WSMethod == null)
        {
            webservice = m_SiteService;
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
                else if (TargetParameter.ParameterType != typeof(String))
                    CallParameters[i] = TypeDeSerialize(RawParameter, TargetParameter.ParameterType);
                else
                    CallParameters[i] = RawParameter;

                Str += TargetParameter.Name + ", ";
            }
            object JSONMethodReturnValue = WSMethod.Invoke(webservice, CallParameters);
            string SerializedReturnValue = JSONSerialize(JSONMethodReturnValue);
            Context.Response.HeaderEncoding=
            Context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            Context.Response.Charset = "utf-8";
            Context.Response.Write(SerializedReturnValue);

        }
        //Response.Write(Str);
    }

    private object TypeDeSerialize(string DeserializationTarget, Type TargetType)
    {
        MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(DeserializationTarget));
        DataContractJsonSerializer serializer = new DataContractJsonSerializer(TargetType);
        object Product = serializer.ReadObject(ms);
        ms.Close();
        return Product;
    }

    private string JSONSerialize(object SerializationTarget)
    {
        DataContractJsonSerializer serializer = new DataContractJsonSerializer(SerializationTarget.GetType());
        MemoryStream ms = new MemoryStream();
        serializer.WriteObject(ms, SerializationTarget);
        string Product = Encoding.UTF8.GetString(ms.ToArray());
        //string Product = Encoding.Default.GetString(ms.ToArray());
        ms.Close();
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
        //retVal.Locale = locale;
        //return retVal;
        InitializationObject retVal = base.GetInitObj();
        retVal.UDID = Request.QueryString["UUID"];
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
        if(Request.QueryString["DomainID"] != null)
            retVal.DomainID = int.Parse(Request.QueryString["DomainID"]);

        return retVal;
    }

}