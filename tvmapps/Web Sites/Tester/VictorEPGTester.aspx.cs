using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EpgFeeder;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
public partial class VictorEPGTester : System.Web.UI.Page
{
    List<string> FilePathList = new List<string>();
    const string LogFileName = "EPGMediaCorp";
    string sPath_successPath;
    string sPath_FailedPath;
    string userName = "tvinci";
    string password = "321nimda";
    string s_Path = "ftp://202.172.167.13/EPGXML_NEW/";
    NetworkCredential NetCredential;
    protected override void OnInit(EventArgs e)
    {
         base.OnInit(e);
         btnMediaCorpEPG.Click += new EventHandler(btnMediaCorpEPG_Click);
         Button1.Click += new EventHandler(Button1_Click);
    }

    void Button1_Click(object sender, EventArgs e)
    {
        API.API a = new API.API();
        string[] ChannelIDs = { "54"};

        API.EPGMultiChannelProgrammeObject[] test = a.GetEPGMultiChannelProgramme("api_147", "11111", ChannelIDs, "0x0", API.EPGUnit.Days, 0, 1, 0);
        
        
        
    }

   
  
    void btnMediaCorpEPG_Click(object sender, EventArgs e)
    {
        StringBuilder paramter = new StringBuilder();

       
        paramter.Append("148|");
        paramter.Append("EPG_MediaCorp|");
        paramter.Append("FTP|");
        paramter.Append("ftp://202.172.167.13/EPGXML_NEW/|");
        paramter.Append("FTPUserName;#tvinci|");
        paramter.Append("FTPPassword;#321nimda|");
        paramter.Append("FTPSuccessFolder;#ftp://202.172.167.13/EPGXML_OLD/|");
        paramter.Append("FTPFailedFolder;#ftp://202.172.167.13/EPG_ERROR/|");


        EpgFeeder.EpgFeederObj test = new EpgFeeder.EpgFeederObj(1, 2, paramter.ToString());
        bool res = test.DoTheTask();
        this.Page.Response.Write("Media Corp EPG Schedule Finish : " + res.ToString());
    }

  
    protected void Page_Load(object sender, EventArgs e)
    {
       
    }


  
}