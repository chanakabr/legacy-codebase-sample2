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
using KLogMonitor;
using System.Reflection;

public partial class EPGTester : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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
        btnTestEPG.Click += new EventHandler(btnTestEPG_Click);
        btnMediaCorpEPG.Click += new EventHandler(btnMediaCorpEPG_Click);
        btnMediaCorpxmltvEPG.Click += new EventHandler(btnMediaCorpxmltvEPG_Click);
        btnMediaCorpXMLNodeEPG.Click += new EventHandler(btnMediaCorpXMLNodeEPG_Click);
        btnYesEPG.Click += new EventHandler(btnYesEPG_Click);
    }

    void btnYesEPG_Click(object sender, EventArgs e)
    {
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "154|EPG_Yes|WebURL|http://localhost/tester/EPG/Yes/Yes16012013.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "154|EPG_Yes|WebURL|http://localhost/tester/EPG/Yes/yes_27012013.xml");
        EpgFeeder.EpgFeederObj test = new EpgFeeder.EpgFeederObj(1, 2, "154|EPG_Yes|WebURL|http://localhost/tester/EPG/Yes/epgdata_29012013.xml");



        bool res = test.DoTheTask();
        this.Page.Response.Write("Yes EPG Schedule Finish : " + res.ToString());
    }

    private void LoadFile()
    {
        FtpWebResponse response = null;
        Stream stream = null;
        StreamReader reader = null;
        try
        {
            FtpWebRequest reqFTP;
            reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(string.Format("{0}", s_Path)));
            reqFTP.Credentials = NetCredential;
            reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
            response = (FtpWebResponse)reqFTP.GetResponse();
            stream = response.GetResponseStream();
            reader = new StreamReader(stream);
            string[] spleter = { " " };

            while (reader.Peek() > 0)
            {
                string filename = "";
                filename = reader.ReadLine();

                if (!string.IsNullOrEmpty(filename))
                {
                    string[] arrfilename = filename.Split(spleter, StringSplitOptions.RemoveEmptyEntries);
                    FilePathList.Add(arrfilename[arrfilename.Length - 1]);
                }
            }
        }
        catch (Exception exp)
        {
            log.Error("MediaCorp_EPG_LoadFile - " + string.Format("there an error occurred during the Load Files process,  Error : {0}", exp.Message), exp);
        }
        finally
        {
            if (response != null)
            {
                response.Close();
            }
            if (stream != null)
            {
                response.Close();
            }
            if (reader != null)
            {
                reader.Close();
            }
        }
    }

    private Stream GetFTPStreamFile(string sFileName, out FtpWebResponse response)
    {
        Stream stream = null;
        try
        {
            //create instance FTPWebRequest to the specific path. 
            FtpWebRequest reqdownloadFTP = (FtpWebRequest)WebRequest.Create(new Uri(string.Format("{0}/{1}", s_Path, sFileName)));
            //Add Network Credential
            reqdownloadFTP.Credentials = NetCredential;
            //specific the action method
            reqdownloadFTP.Method = WebRequestMethods.Ftp.DownloadFile;
            reqdownloadFTP.UseBinary = true;
            //get response 
            FtpWebResponse responseDownload = (FtpWebResponse)reqdownloadFTP.GetResponse();
            //stream response
            stream = responseDownload.GetResponseStream();
            response = responseDownload;
        }
        catch (Exception exp)
        {
            log.Error("Media Corp: EPG FTP Stream " + string.Format("there an error occurred during the Get FTP Stream file process, Stream file '{0}' , Error : {1}", sFileName, exp.Message), exp);
            response = null;

        }
        return stream;
    }
    void btnMediaCorpXMLNodeEPG_Click(object sender, EventArgs e)
    {
        NetCredential = new NetworkCredential(userName, password);
        LoadFile();
        foreach (string fname in FilePathList)
        {
            //Func<string> str = FilePathList;

            SaveMediaCorpEPGData(fname);
        }
    }
    private void SaveMediaCorpEPGData(string sFileName)
    {
        FtpWebResponse response = null;
        Stream stream = null;

        bool enabledelete = false;
        try
        {

            stream = GetFTPStreamFile(sFileName, out response);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(stream);



            //-------------------- Start Add Schedule EPG to Data Base ------------------------------//
            //
            //
            //create xml document to load FTP file

            StringReader xr = new StringReader(xmlDoc.InnerXml);
            XmlTextReader reader = new XmlTextReader(xr);

            XmlNodeList xmlnodelist = xmlDoc.GetElementsByTagName("program");

            List<FieldTypeEntity> FieldEntityMapping = new List<FieldTypeEntity>();
            List<FieldTypeEntity> tempFieldMapping = GetMapingFields();

            foreach (XmlNode node in xmlnodelist)
            {
                foreach (FieldTypeEntity item in tempFieldMapping)
                {

                    FieldTypeEntity newItem = item;

                    foreach (string XmlRefName in item.XmlReffName)
                    {
                        foreach (XmlNode multinode in node.SelectNodes(XmlRefName))
                            newItem.Value.Add(multinode.InnerXml);
                    }
                    FieldEntityMapping.Add(newItem);
                }
            }

            #region Insert Basic Field Value
            var BasicFieldEntity = from item in FieldEntityMapping
                                   where item.FieldType == FieldTypes.Basic && item.XmlReffName.Capacity > 0
                                   select item;

            ODBCWrapper.InsertQuery insertBasicProgQuery = new ODBCWrapper.InsertQuery("epg_channels_schedule");
            foreach (var item in BasicFieldEntity)
            {
                insertBasicProgQuery += ODBCWrapper.Parameter.NEW_PARAM(item.Name, "=", string.Join(" ", item.Value.ToArray()));
            }

            //Int32 channelID = GetExistChannel(progItem.channel_id);
            //Int32 nMediaID = GetExistMedia(channelID);

            //DateTime dProgStartDate = ParseEPGStrToDate(progItem.schedule_date.ToString(), progItem.start_time.ToString());
            //DateTime dProgEndDate = dProgStartDate.Add(GetProgramDuration(progItem.duration.ToString()));
            //Guid EPGGuid = Guid.NewGuid();
            //insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_IDENTIFIER", "=", EPGGuid.ToString());
            insertBasicProgQuery += ODBCWrapper.Parameter.NEW_PARAM("PIC_ID", "=", 0);
            insertBasicProgQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            insertBasicProgQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
            //insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("Media_id", "=", nMediaID);
            insertBasicProgQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 500);
            #endregion

            #region Inser Meta Field Value
            var MetaFieldEntity = from item in FieldEntityMapping
                                  where item.FieldType == FieldTypes.Meta && item.XmlReffName.Capacity > 0
                                  select item;

            ODBCWrapper.InsertQuery insertMetaProgQuery = new ODBCWrapper.InsertQuery("EPG_program_metas");
            foreach (var item in MetaFieldEntity)
            {
                //insertMetaProgQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", 148);
                insertMetaProgQuery += ODBCWrapper.Parameter.NEW_PARAM("value", "=", string.Join(" ", item.Value.ToArray()));
                insertMetaProgQuery += ODBCWrapper.Parameter.NEW_PARAM("epg_meta_id", "=", item.ID);
                insertMetaProgQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                insertMetaProgQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                insertMetaProgQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 500);

            }

            #endregion

            #region Insert Tags Field Value
            var TagFieldEntity = from item in FieldEntityMapping
                                 where item.FieldType == FieldTypes.Tag && item.XmlReffName.Capacity > 0
                                 select item;

            ODBCWrapper.InsertQuery insertTagProgQuery = new ODBCWrapper.InsertQuery("EPG_tags");
            foreach (var item in TagFieldEntity)
            {
                foreach (string Tagvalue in item.Value)
                {
                    //insertMetaProgQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", 148);
                    insertTagProgQuery += ODBCWrapper.Parameter.NEW_PARAM("value", "=", Tagvalue);
                    insertTagProgQuery += ODBCWrapper.Parameter.NEW_PARAM("epg_tag_type_id", "=", item.ID);
                    insertTagProgQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    insertTagProgQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    insertTagProgQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 500);
                }
            }

            #endregion

            //
            //
            //-------------------- End Add Schedule EPG to Data Base --------------------------------//
            if (response != null)
                response.Close();

            if (stream != null)
                stream.Close();
            //enabledelete = MoveFile(sFileName, sPath_successPath);
        }
        catch (Exception exp)
        {
            if (response != null)
                response.Close();

            if (stream != null)
                stream.Close();


        }
        finally
        {
            if (response != null)
                response.Close();

            if (stream != null)
                stream.Close();

            //delete the source file that proccess even if the proc
            if (enabledelete)
            {
                //DeleteFile(sFileName);
            }

        }


    }

    protected int GetExistEPGTagID(string value, int EPGTagTypeId)
    {
        int res = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += " select * from EPG_tags";
        selectQuery += "Where";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("Group_ID", "=", 148);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("value", "=", value);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("epg_tag_type_id", "=", EPGTagTypeId);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);

        if (selectQuery.Execute("query", true) != null)
        {
            int count = selectQuery.Table("query").DefaultView.Count;
            // res = PageUt
        }
        return res;
    }

    public enum FieldTypes
    {
        Unknown,
        Basic,
        Meta,
        Tag

    }
    public struct FieldTypeEntity
    {
        public string ID;
        public string Name;
        public List<string> XmlReffName;
        public FieldTypes FieldType;
        public List<string> Value;
    }
    protected List<FieldTypeEntity> GetMapingFields()
    {
        List<FieldTypeEntity> res = new List<FieldTypeEntity>();

        #region Basic fields definition
        ODBCWrapper.DataSetSelectQuery selectQueryBasic = new ODBCWrapper.DataSetSelectQuery();
        selectQueryBasic += " select * from lu_EPG_basics_types where status = 1 ";
        if (selectQueryBasic.Execute("query", true) != null)
        {
            int count = selectQueryBasic.Table("query").DefaultView.Count;
            if (count > 0)
            {
                int i = 0;
                foreach (DataRowView dr in selectQueryBasic.Table("query").DefaultView)
                {

                    FieldTypeEntity item = new FieldTypeEntity();
                    item.ID = ODBCWrapper.Utils.GetSafeStr(dr["ID"]);
                    item.Name = ODBCWrapper.Utils.GetSafeStr(dr["Name"]);
                    item.FieldType = FieldTypes.Basic;
                    res.Add(item);
                }
            }
        }
        #endregion

        #region Meta fields definition
        ODBCWrapper.DataSetSelectQuery selectQueryMetas = new ODBCWrapper.DataSetSelectQuery();
        selectQueryMetas += " select * from EPG_metas_types where status = 1 ";
        //selectQueryTags += PageUtils.GetAllGroupTreeStr(groupID);
        if (selectQueryMetas.Execute("query", true) != null)
        {
            int count = selectQueryMetas.Table("query").DefaultView.Count;
            if (count > 0)
            {
                int i = 0;
                foreach (DataRowView dr in selectQueryMetas.Table("query").DefaultView)
                {

                    FieldTypeEntity item = new FieldTypeEntity();
                    item.ID = ODBCWrapper.Utils.GetSafeStr(dr["ID"]);
                    item.Name = ODBCWrapper.Utils.GetSafeStr(dr["Name"]);
                    item.FieldType = FieldTypes.Meta;
                    res.Add(item);
                }
            }
        }
        #endregion

        #region Tag dields defintion
        ODBCWrapper.DataSetSelectQuery selectQueryTags = new ODBCWrapper.DataSetSelectQuery();
        selectQueryTags += " select * from EPG_tags_types where status = 1 ";
        //selectQueryTags += PageUtils.GetAllGroupTreeStr(groupID);
        if (selectQueryTags.Execute("query", true) != null)
        {
            int count = selectQueryTags.Table("query").DefaultView.Count;
            if (count > 0)
            {
                int i = 0;
                foreach (DataRowView dr in selectQueryTags.Table("query").DefaultView)
                {

                    FieldTypeEntity item = new FieldTypeEntity();
                    item.ID = ODBCWrapper.Utils.GetSafeStr(dr["ID"]);
                    item.Name = ODBCWrapper.Utils.GetSafeStr(dr["Name"]);
                    item.FieldType = FieldTypes.Meta;
                    res.Add(item);
                }
            }
        }
        #endregion

        #region Set xml refereance field name
        ODBCWrapper.DataSetSelectQuery selectQueryFieldMapping = new ODBCWrapper.DataSetSelectQuery();
        selectQueryFieldMapping += " select * from EPG_fields_mapping where status = 1 and is_active = 1";
        //selectQueryTags += PageUtils.GetAllGroupTreeStr(groupID);
        if (selectQueryFieldMapping.Execute("query", true) != null)
        {
            int count = selectQueryFieldMapping.Table("query").DefaultView.Count;
            if (count > 0)
            {
                int i = 0;
                foreach (DataRowView dr in selectQueryFieldMapping.Table("query").DefaultView)
                {
                    FieldTypes type = (FieldTypes)Enum.Parse(typeof(FieldTypes), ODBCWrapper.Utils.GetSafeStr(dr["type"]));

                    var fieldentity = (from n in res
                                       where n.FieldType == type && n.ID == ODBCWrapper.Utils.GetSafeStr(dr["field_id"])
                                       select n).First();


                    fieldentity.XmlReffName.Add(ODBCWrapper.Utils.GetSafeStr(dr["external_ref"]));


                }
            }
        }

        #endregion
        return res;
    }
    void btnMediaCorpxmltvEPG_Click(object sender, EventArgs e)
    {
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "148|EPGxmlTv|WebURL|http://localhost/tester/EPG/MediaCorp.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "148|EPGxmlTv|WebURL|http://localhost/tester/EPG/mediacorp24092012.xml");
        EpgFeeder.EpgFeederObj test = new EpgFeeder.EpgFeederObj(1, 2, "148|EPGxmlTv|WebURL|http://localhost/tester/EPG/mediacorp03102012.xml");

        bool res = test.DoTheTask();
        this.Page.Response.Write("EPG Schedule Finish : " + res.ToString());
    }

    void btnMediaCorpEPG_Click(object sender, EventArgs e)
    {
        StringBuilder paramter = new StringBuilder();

        //paramter.Append("126|");
        //paramter.Append("EPG_MediaCorp|");
        //paramter.Append("FTP|");
        //paramter.Append("ftp://tvinci.cdnetworks.net/Ipvision/MediaCorp/xmlEPG/|");
        //paramter.Append("FTPUserName;#tvinciibc|");
        //paramter.Append("FTPPassword;#fNkG8372|");
        //paramter.Append("FTPSuccessFolder;#ftp://tvinci.cdnetworks.net/Ipvision/MediaCorp/SuccessEPG/|");
        //paramter.Append("FTPFailedFolder;#ftp://tvinci.cdnetworks.net/Ipvision/MediaCorp/FailedEPG/|");

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

    void btnTestEPG_Click(object sender, EventArgs e)
    {

        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPGIBC2012_new.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/03092012_2.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/04092012_eurosport.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/BBC_Ireland.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/britisheurosport.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/Channel4.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/arte.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/france3.xml");

        //09092012 EPG 
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/09092012Arte.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/09092012BBC1Ireland.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/09092012channel4.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/OrangexmltvExample.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|tvinci.cdnetworks.net/Ipvision/MediaCorp");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/23092012Orange2EPG.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/orange02102012.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/orange16102012.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/orange16102012_2.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/UK_bleb_05112012.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/fr_05112012.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/UK_bleb_22112012.xml");


        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/fr_22112012.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/uk_02122012.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/fr_02122012.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/uk_10122012.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/fr_10122012.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/uk_17122012.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/fr_19122012.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/fr_19_1122012.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/uk_31122012.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/fr_31122012.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/fr_07012013.xml");
        //EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/uk_16012013.xml");
        //************EpgFeeder.EpgFeeder test = new EpgFeeder.EpgFeeder(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/uk_29012013.xml");
        EpgFeeder.EpgFeederObj test = new EpgFeeder.EpgFeederObj(1, 2, "126|EPGxmlTv|WebURL|http://localhost/tester/EPG/fr_29012013.xml");



        bool res = test.DoTheTask();
        this.Page.Response.Write("EPG Schedule Finish : " + res.ToString());


    }
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            FileInfo fileInf = new FileInfo("otttvinci.upload.akamai.com/l.jpg");
        }
        catch (Exception ex)
        {
            Response.Write(ex.Message);
        }
    }



}