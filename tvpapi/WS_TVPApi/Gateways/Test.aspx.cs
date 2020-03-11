using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.IO;
using System.Text;

public partial class Gateways_Test : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        HttpWebRequest HttpWReq = (HttpWebRequest)WebRequest.Create(@"http://drmil.tvinci.com/getlicense.aspx");
        HttpWReq.Method = "POST";
        HttpWReq.ContentType = "application/xml";
        HttpWReq.Headers["msprdrm_server_redirect_compat"] = "false";
        HttpWReq.Headers["msprdrm_server_exception_compat"] = "false";

        string xmlPost = "b252eb9a533c8ae37a462a267e1f2fa9";
        byte[] byteArray = Encoding.UTF8.GetBytes(xmlPost);
        HttpWReq.ContentLength = byteArray.Length;

        using (Stream stream = HttpWReq.GetRequestStream())
        {
            stream.Write(byteArray, 0, byteArray.Length);
            stream.Close();

            HttpWebResponse resp = (HttpWebResponse)HttpWReq.GetResponse();
            StreamReader responseReader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
            string res = responseReader.ReadToEnd();
        }
    }

    void RequestStreamCallback(IAsyncResult ar)
    {
        HttpWebRequest request = ar.AsyncState as HttpWebRequest;

        // populate request stream  
        request.ContentType = "text/xml";
        Stream requestStream = request.EndGetRequestStream(ar);
        StreamWriter streamWriter = new StreamWriter(requestStream, System.Text.Encoding.UTF8);

        streamWriter.Write("b252eb9a533c8ae37a462a267e1f2fa9");
        streamWriter.Close();

        // Make async call for response  
        request.BeginGetResponse(new AsyncCallback(ResponseCallback), request);
    }

    private void ResponseCallback(IAsyncResult ar)
    {
        HttpWebRequest request = ar.AsyncState as HttpWebRequest;
        WebResponse response = request.EndGetResponse(ar);
        StreamReader responseReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
        string res = responseReader.ReadToEnd();
    }

}
