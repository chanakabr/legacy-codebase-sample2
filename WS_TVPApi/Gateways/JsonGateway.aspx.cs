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

public partial class Gateways_JsonGateway : BaseGateway
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string broadcasterName = Request.QueryString["broadcasterName"];
        int groupID = GetGroupIDByBroadcasterName(broadcasterName);
        string MethodName = Request.QueryString["MethodName"];
        string Str = String.Empty;
        MethodInfo WSMethod = m_MediaService.GetType().GetMethod(MethodName);
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
                    CallParameters[i] = GetInitObj();
                else
                    CallParameters[i] = TypeDeSerialize(RawParameter, TargetParameter.ParameterType);

                Str += TargetParameter.Name + ", ";
            }
            object JSONMethodReturnValue = WSMethod.Invoke(m_MediaService, CallParameters);
            string SerializedReturnValue = JSONSerialize(JSONMethodReturnValue);
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
        string Product = Encoding.Default.GetString(ms.ToArray());
        ms.Close();
        return Product;
    }

}