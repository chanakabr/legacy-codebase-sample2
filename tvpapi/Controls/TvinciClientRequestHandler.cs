using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Xml;
using KLogMonitor;


public abstract class TvinciClientRequestHandler : IHttpHandler, IRequiresSessionState
{
    private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    #region IHttpHandler Members
    public bool IsReusable
    {
        get { return true; }
    }

    public void ProcessRequest(HttpContext context)
    {
        if (context.Request["requestType"] != null)
        {
            string RequestType = context.Request["requestType"].ToString();
            string Parameters = string.Empty;
            if (context.Request["Parameters"] != null)
            {
                Parameters = context.Request["Parameters"].ToString();
            }

            if (string.IsNullOrEmpty(RequestType))
            {
                logger.Error("Cannot extract request type from HttpContext");
                SendResponse(context, string.Empty, true);
            }

            string response;
            if (ProcessClientRequest(RequestType, Parameters, out response))
            {
                SendResponse(context, response, false);
            }
            else
            {
                logger.ErrorFormat("Error processing client request, {0}, {1}", RequestType, Parameters);
                SendResponse(context, string.Empty, true);
            }
        }
        else
        {
            logger.Error("Cannot extract request type from HttpContext");
            SendResponse(context, string.Empty, true);
        }
    }
    #endregion

    private void SendResponse(HttpContext context, string theResponse, bool theError)
    {
        context.Response.ContentType = "application/xml";

        StringBuilder sb = new StringBuilder();

        sb.Append(theError ? "Error" : "Success");
        sb.Append(';');
        sb.Append(theResponse);

        context.Response.Write(sb.ToString());
        //XmlTextWriter xw = new XmlTextWriter(context.Response.OutputStream,
        //  new System.Text.UTF8Encoding());

        //xw.WriteStartElement("Status");
        //xw.WriteString(theError ? "Error" : "Success");
        //xw.WriteEndElement(); // Status

        //xw.WriteStartElement("Response");
        //xw.WriteString(theResponse);
        //xw.WriteEndElement(); // Status

        //xw.Close();
    }

    protected abstract bool ProcessClientRequest(string theAction, string theParameters, out string theResponse);
}
