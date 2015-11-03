using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using KLogMonitor;
using System.Reflection;


public partial class FBTest : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    protected void Page_Load(object sender, EventArgs e)
    {

        if (Request.QueryString != null)
        {
            string code = Request.QueryString.Get("code");
            if (!string.IsNullOrEmpty(code))
            {
                string url = string.Format("https://graph.facebook.com/oauth/access_token?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}", "397481886958054", "http://192.168.16.124/TVMTest/fbtest.aspx", "ef15dc239be08b8f1cd1fd1b7761c75c", code);
                string tokenResp = null;// WS_Utils.SendXMLHttpReq(url, string.Empty, string.Empty);
                tokenResp = tokenResp.Remove(0, 13);
                TokenTxt.Text = tokenResp;

                int status = 0;
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string userLink = string.Format("https://graph.facebook.com/me?access_token={0}", tokenResp);
                string userRetVal = SendGetHttpReq(userLink, ref status, string.Empty, string.Empty);
                FBUser fbUser = serializer.Deserialize<FBUser>(userRetVal);

                string Url = string.Format("https://graph.facebook.com/me/friends?access_token={1}", "626466025", tokenResp);
                string formId = "myForm1";

                string retVal = SendGetHttpReq(Url, ref status, string.Empty, string.Empty);
            }
        }
        //        FBFriendsContainer friends = serializer.Deserialize<FBFriendsContainer>(retVal);
        //        List<FBFriend> frindsList = friends.data.ToList<FBFriend>();
        //        StringBuilder sb = new StringBuilder();
        //        StringBuilder usersGuid = new StringBuilder();
        //        for (int i = 0; i < frindsList.Count; i++) 
        //        {
        //            if (i > 0)
        //            {
        //                sb.Append(",");
        //            }
        //            sb.AppendFormat("'{0}'",frindsList[i].id);
        //        }

        //        ODBCWrapper.DataSetSelectQuery selectQUery = new ODBCWrapper.DataSetSelectQuery();
        //        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        //        selectQuery.SetConnectionKey("users_connection_string");
        //        selectQuery += " select id,username, facebook_image, facebook_id, user_state from users where is_active = 1 and status = 1 and group_id = 109 and ";
        //        selectQuery += "facebook_id in (";
        //        selectQuery += sb.ToString();
        //        selectQuery += ")";
        //        if (selectQuery.Execute("query", true) != null)
        //        {
        //            int count = selectQuery.Table("query").DefaultView.Count;
        //            if (count > 0)
        //            {
        //                FriendsLit.Text += "<div> My Connected Friends!! <br />";
        //                for (int i = 0; i < count; i++)
        //                {
        //                    string logInState = string.Empty;
        //                    object oUS = selectQuery.Table("query").DefaultView[i].Row["user_state"];
        //                    int userStateInt = 0;

        //                    if (oUS != null && oUS != System.DBNull.Value)
        //                    {
        //                        userStateInt = int.Parse(selectQuery.Table("query").DefaultView[i].Row["user_state"].ToString());
        //                        switch (userStateInt)
        //                        {
        //                            case (2):
        //                                {
        //                                    logInState = "Logged In";
        //                                    break;
        //                                }
        //                            default:
        //                                {
        //                                    logInState = "Logged Out";
        //                                    break;
        //                                }
        //                        }
        //                    }

        //                    string imgScr = string.Format("<img src='http://graph.facebook.com/{0}/picture?type=normal' alt='{2}' title='{2}' width='100'/> Status : {1}<br />", selectQuery.Table("query").DefaultView[i].Row["facebook_id"].ToString(), logInState, selectQuery.Table("query").DefaultView[i].Row["id"].ToString());
        //                    FriendsLit.Text += imgScr;
        //                    //if (i > 0)
        //                    //{
        //                    //    usersGuid.Append(",");
        //                    //}
        //                    //usersGuid.AppendFormat("'{0}'",selectQuery.Table("query").DefaultView[0].Row["facebook_image"].ToString());
        //                }
        //                FriendsLit.Text += "</div>";
        //            }
        //        }
        //        selectQuery.Finish();
        //        selectQuery = null;
        //        //FBFriendsContainer friends = JSONHelper.Deserialise<FBFriendsContainer>(retVal);
        //        //JavaScriptSerializer
        //        //StringBuilder htmlForm = new StringBuilder();
        //        //htmlForm.AppendLine("<html>");
        //        //htmlForm.AppendLine(String.Format("<body onload='document.forms[\"{0}\"].submit()'>", formId));
        //        //htmlForm.AppendLine(String.Format("<form id='{0}' method='POST' action='{1}'>", formId, Url));
        //        //htmlForm.AppendLine(string.Format("<input type='hidden' id='access_token' value='{0}' />", tokenResp));
        //        //htmlForm.AppendLine("<input type='hidden' id='message' value='Hi!' />");
        //        //htmlForm.AppendLine("</form>");
        //        //htmlForm.AppendLine("</body>");
        //        //htmlForm.AppendLine("</html>");
        //        //WebClient wc = new WebClient();
        //        ////string msg = "Test BP!";
        //        //wc.Encoding = Encoding.UTF8;
        //       // wc.Headers["Content-type"] = "application/x-www-form-urlencoded";
        //       // string resp = wc.UploadString("https://graph.facebook.com/834978205/feed", null, string.Format("access_token={0}&link={1}", tokenResp, "http://192.168.16.1/filmo"));
        //        //HttpContext.Current.Response.Clear();
        //        //HttpContext.Current.Response.Write(htmlForm.ToString());
        //        //HttpContext.Current.Response.End();
        //       //AUthorizeChat(tokenResp);
        //    }
        //}
    }

    static public string SendGetHttpReq(string sUrl, ref Int32 nStatus, string sUserName, string sPassword)
    {
        HttpWebRequest oWebRequest = (HttpWebRequest)WebRequest.Create(sUrl);
        HttpWebResponse oWebResponse = null;
        Stream receiveStream = null;
        Int32 nStatusCode = -1;
        Encoding enc = new UTF8Encoding(false);
        try
        {
            oWebRequest.Credentials = new NetworkCredential(sUserName, sPassword);
            oWebRequest.Timeout = 1000000;
            oWebResponse = (HttpWebResponse)oWebRequest.GetResponse();
            HttpStatusCode sCode = oWebResponse.StatusCode;
            nStatusCode = GetResponseCode(sCode);
            receiveStream = oWebResponse.GetResponseStream();

            StreamReader sr = new StreamReader(receiveStream, enc);
            string resultString = sr.ReadToEnd();

            sr.Close();

            oWebResponse.Close();
            oWebRequest = null;
            oWebResponse = null;
            nStatus = nStatusCode;
            return resultString;
        }
        catch (Exception ex)
        {
            log.Error("Notifier - SendGetHttpReq exception:" + ex.Message + " to: " + sUrl, ex);
            if (oWebResponse != null)
                oWebResponse.Close();
            if (receiveStream != null)
                receiveStream.Close();
            nStatus = 404;
            return "";
        }
    }

    static protected Int32 GetResponseCode(HttpStatusCode theCode)
    {
        if (theCode == HttpStatusCode.OK)
            return 200;
        if (theCode == HttpStatusCode.NotFound)
            return 404;
        return 500;

    }

    protected void AUthorizeChat(string token)
    {
        TcpClient tcpClient = new TcpClient();
        tcpClient.Connect("chat.facebook.com", 5222);
        NetworkStream ns = tcpClient.GetStream();

        string authMsg = "<?xml version=\"1.0\"?><stream:stream xmlns=\"jabber:ClientID\" xmlns:stream=\"http://etherx.jabber.org/streams\" version=\"1.0\" to=\"chat.facebook.com\">";
        System.IO.StreamWriter sw = new System.IO.StreamWriter(ns);
        sw.WriteLine(authMsg);
        sw.Flush();
        string authMsg2 = "<auth xmlns=\"urn:ietf:params:xml:ns:xmpp-sasl\" mechanism=\"X-FACEBOOK-PLATFORM\"></auth>";
        sw.WriteLine(authMsg2);
        sw.Flush();

        byte[] serverResponseByte = new byte[1024];
        int bytesRead = 0;
        StringBuilder sb = new StringBuilder();

        while (ns.DataAvailable)
        {
            bytesRead = ns.Read(serverResponseByte, 0, serverResponseByte.Length);
            sb.Append(System.Text.Encoding.UTF8.GetString(serverResponseByte, 0, bytesRead));
        }



        string challengeVal = string.Empty;
        byte[] ChallengeByte = Convert.FromBase64String(challengeVal);
        string challengeStr = System.Text.Encoding.UTF8.GetString(ChallengeByte);
        string method = string.Empty;
        string nonce = string.Empty;

        SortedDictionary<string, string> paramsDict = new SortedDictionary<string, string>();
        paramsDict.Add("api_key", "325553384143013");
        paramsDict.Add("call_id", System.DateTime.Now.Ticks.ToString());
        paramsDict.Add("method", method);
        paramsDict.Add("nonce", "RETRIEVE NONCE FROM STRING THAT YOU RECEIVE FROM CHALLENGE");
        paramsDict.Add("access_token", token);
        paramsDict.Add("v", "1.0");

        string hash = ComputeHash(paramsDict, "dda111ac4152fc2a563a8aa954908f1c");
        StringBuilder sb2 = new StringBuilder();
        foreach (string key in paramsDict.Keys)
        {
            sb2.AppendFormat("{0}={1}&", key, paramsDict[key]);
        }
        sb2.AppendFormat(hash);

        byte[] hashedBytes = System.Text.Encoding.UTF8.GetBytes(sb2.ToString());
        string cnvertedHash = Convert.ToBase64String(hashedBytes);

        sw.WriteLine(authMsg);
        sw.Flush();

        sw.WriteLine(authMsg2);
        sw.Flush();

        string tls = "<starttls xmlns=\"urn:ietf:params:xml:ns:xmpp-tls\"/>";
        sw.WriteLine(tls);
        sw.Flush();

        string hashedResponse = string.Format("<response xmlns=\"urn:ietf:params:xml:ns:xmpp-sasl\">{0}</response>", cnvertedHash);
        sw.WriteLine(hashedResponse);
        sw.Flush();

        byte[] serverResponseByte2 = new byte[1024];
        int bytesRead2 = 0;
        StringBuilder sb3 = new StringBuilder();

        while (ns.DataAvailable)
        {
            bytesRead2 = ns.Read(serverResponseByte, 0, serverResponseByte2.Length);
            sb3.Append(System.Text.Encoding.UTF8.GetString(serverResponseByte2, 0, bytesRead2));
        }
    }

    private string ComputeHash(SortedDictionary<string, string> paramKeys, string secretKey)
    {
        StringBuilder parametersForSig = new StringBuilder();
        foreach (string key in paramKeys.Keys)
        {
            parametersForSig.Append(string.Format("{0}={1}", key, paramKeys[key]));
        }
        //For Each myKey As String In myparams.Keys
        //parametersForSig.Append(String.Format("{0}={1}", myKey, myparams(myKey)))
        //Next
        parametersForSig.Append(secretKey);
        System.Security.Cryptography.MD5CryptoServiceProvider myMD5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] data = myMD5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(parametersForSig.ToString()));
        StringBuilder md5SB = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            md5SB.Append(data[i].ToString("x2"));
        }
        return md5SB.ToString().ToLower();

    }
}


public class JSONHelper
{
    public static T Deserialise<T>(string json)
    {
        T obj = Activator.CreateInstance<T>();
        MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
        ms.Position = 0;
        DataContractJsonSerializer serialiser = new DataContractJsonSerializer(obj.GetType());

        obj = (T)serialiser.ReadObject(ms);
        ms.Close();
        return obj;
    }
}

[Serializable]
public class FBUser
{
    string m_id;
    string m_name;
    string m_first_name;

    [DataMember]
    public string name
    {
        get
        {
            return m_name;
        }
        set
        {
            m_name = value;
        }
    }

    [DataMember]
    public string id
    {
        get
        {
            return m_id;
        }
        set
        {
            m_id = value;
        }
    }

    [DataMember]
    public string first_name
    {
        get
        {
            return m_first_name;
        }
        set
        {
            m_first_name = value;
        }
    }
}


[Serializable]
public class FBFriendsContainer
{
    IEnumerable<FBFriend> m_data;
    [DataMember]
    public IEnumerable<FBFriend> data { get; set; }

    //[DataMember]
    //public IEnumerable<FBFriend> data
    //{
    //    get
    //    {
    //        return m_data;
    //    }
    //    set
    //    {
    //        m_data = value;
    //    }
    //}
}

[Serializable]
public class FBFriend
{

    string m_name;
    string m_id;
    string m_siteGuid;

    public FBFriend()
    {
    }

    public FBFriend(string id, string name)
    {
        m_name = name;
        m_id = id;

    }

    [DataMember]
    public string id
    {
        get
        {
            return m_id;
        }
        set
        {
            m_id = value;

        }
    }

    [DataMember]
    public string name
    {
        get
        {
            return m_name;
        }
        set
        {
            m_name = value;
        }
    }

    [DataMember]
    public string siteGuid
    {
        get
        {
            return m_siteGuid;
        }
        set
        {
            m_siteGuid = value;
        }
    }

}