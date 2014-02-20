using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Serialization;
using Tvinci.Data.TVMDataLoader.Protocols;
using Tvinci.Data.TVMDataLoader.Protocols.TVMMenu;
using System.IO;
using ProtocolHandler;
using System.Reflection;
using System.Configuration;

public partial class tvmpro_api : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
       
        try
        {
            string sRequest = GetFormParameters();
            Logger.Logger.Log("Request Recieved", sRequest, "MediaHub");
           // Type type = Type.GetType("TVMMenu");
            string typeNS = Request.QueryString["t"];
            string baseNS = ConfigurationManager.AppSettings["BaseProtocolNamespace"];
            Type tempType = Type.GetType(string.Format("{0}, {1}", typeNS, baseNS));
            XmlSerializer xs = new XmlSerializer(tempType);
            object result = xs.Deserialize(new StringReader(sRequest));
            //Type type = result.GetType();
            Protocol protocolReq = result as Protocol;
            if (protocolReq != null)
            {
                string handleNS = ConfigurationManager.AppSettings[typeNS];
                Type handlerType = Type.GetType(handleNS);
                object oHandler = Activator.CreateInstance(handlerType);
                if (oHandler != null)
                {
                    BaseProtocolHandler<Protocol> handler = oHandler as BaseProtocolHandler<Protocol>;
                    string retVal = handler.GetSerializedResponse(protocolReq);
                    Response.Write(retVal);
                }
                

            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("Exception", ex.Message, "MediaHub");
            int i = 0;
        }

    }

    protected string GetFormParameters()
    {
        Int32 nCount = Request.TotalBytes;
        string sFormParameters = System.Text.Encoding.UTF8.GetString(Request.BinaryRead(nCount));
        return sFormParameters;
    }

    private string GetInstanceType(string xml)
    {
        string retVal = string.Empty;
        if (xml.Contains("ObjType"))
        {
            int beginIndes = xml.IndexOf("ObjType");
        }
        return retVal;
    }
}