using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using keygen4Lib;
using System.Net;

public partial class Filmo : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        //users.UsersService us = new users.UsersService();
       // us.Url = "http://localhost/TVMUsers/module.asmx";
        //us.ForgotPassword("users_109", "11111", "arik@tvinci.com");
       // FilmoFeeder.Feeder.UpdatePersonGUID("C:\\temp\\FilmoPerson\\Persons.xls");
        //FilmoFeeder.Feeder.ActualWork("C:\\temp\\FilmoReg", "C:\\temp\\FilmoPerson");
///        FilmoFeeder.Feeder.ActualWork("C:\\temp\\FilmoReg", "");
        //CDNetworksVault.MediaVault m = new CDNetworksVault.MediaVault("filmofvs", "guest", 7200);
        //Users.TvinciUsers t = new Users.TvinciUsers(109);
        //t.ResendActivationMail("ari@tvinci.com");
        //string sURL = m.GetURL("rtmp://filmotechfvs.cdnetworks.net/filmotechfvs/flashstream/mp4:filmotechfvs-origin.cdnetworks.net/AFL4DEHARDEWE-HRE0000EF63_73977000_77075040_16_09.mp4");
        /*
        string sMediaUrl = "rtmp://filmotechfvs.cdnetworks.net/filmotechfvs/flashstream/mp4:filmotechfvs-origin.cdnetworks.net/AFL4DEHARDEWE-HRE0000EF63_73977000_77075040_16_09.mp4";
        string sIP = TVinciShared.PageUtils.GetCallerIP();
        System.Uri u = new Uri(sMediaUrl);
        string[] sSegments = u.Segments;
        string sPath = "";
        string sSchema = u.Scheme;
        string sHost = u.Host;
        string sFileName = "";
        for (int i =0; i < sSegments.Length; i++)
        {
            string sSeg = sSegments[i];
            if (i != sSegments.Length - 1)
                sPath += sSeg;
            else
                sFileName = sSeg;
        }
        string sRtmp = sSchema + "://" + sHost + sPath;
        string sContent_url = sRtmp + sFileName; 
        string sUser_id = "guest"; 
        string nTTl = "7200"; 
        Ikeygen authobj = (Ikeygen)Server.CreateObject("keygen4.keygen"); 
        IPHostEntry host = Dns.Resolve(Dns.GetHostName()); 
        string sServer_ip = host.AddressList[0].ToString();
        string sPrivateKey = "filmofvs";
        String authkey = authobj.GetCode(sContent_url, sUser_id, nTTl, sIP, sServer_ip, sPrivateKey); 
        string sRet = sRtmp + "?key=" + authkey + "/" + sFileName;
        */         
    }
}
