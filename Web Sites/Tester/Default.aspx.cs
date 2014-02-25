using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.IO;
using System.Text;
using System.Xml;
using System.Collections;
using ConditionalAccess;
//using AbertisHPFeeder;
using TVinciShared;
using System.Globalization;
using api_ws;
using TvinciImporter;
using com.llnw.mediavault;
using Notifiers;
using System.Data;
//using GenericParsing;
using MCGroupRules;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Xsl;
using ElasticSearchFeeder;
public partial class _Default : System.Web.UI.Page
{

//    //private void SetNewIPsForModules()
//    //{
//    //    // { "173.231.146.2", "173.231.146.3", "173.231.146.4", "173.231.146.5", "173.231.146.6", "173.231.146.7", "173.231.146.8", "173.231.146.9", "173.231.146.10" };
//    //    List<string> ips = new List<string> { "10.244.134.134" };
//    //    List<string> dbNames = new List<string> { "pricing_connection", "billing_connection", "users_connection", "CA_CONNECTION_STRING", "MAIN_CONNECTION_STRING" };
//    //    foreach (string dbName in dbNames)
//    //    {

//    //        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
//    //        selectQuery.SetConnectionKey(dbName);

//    //        selectQuery += "SELECT * FROM groups_modules_ips where GROUP_ID = 147 AND IS_ACTIVE=1 AND STATUS=1";
//    //        selectQuery.Execute("ipTbl", true);
//    //        DataTable dt = selectQuery.Table("ipTbl");
//    //        selectQuery.Finish();

//    //        foreach (DataRow row in dt.Rows)
//    //        {
//    //            foreach (string ip in ips)
//    //            {
//    //                ODBCWrapper.DataSetInsertQuery insertQuery = new ODBCWrapper.DataSetInsertQuery("groups_modules_ips");
//    //                foreach (DataColumn col in dt.Columns)
//    //                {
//    //                    if (col.ColumnName != "ID" && col.ColumnName != "UPDATER_ID" && col.ColumnName != "EDITOR_REMARKS" &&
//    //                        col.ColumnName != "UPDATE_DATE" && col.ColumnName != "CREATE_DATE" && col.ColumnName != "PUBLISH_DATE")
//    //                    {
//    //                        if (col.ColumnName == "IP")
//    //                        {
//    //                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM(col.ColumnName, "=", ip);
//    //                        }
//    //                        else
//    //                        {
//    //                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM(col.ColumnName, "=", row[col.ColumnName]);
//    //                        }
//    //                    }

//    //                }
//    //                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 666);
//    //                insertQuery.SetConnectionKey(dbName);
//    //                insertQuery.Execute();
//    //                insertQuery.Finish();
//    //            }
//    //        }
//    //    }


//    //}

//    //private void SetNewGroupCloudPermissions(int groupId)
//    //{
//    //    List<string> dbNames = new List<string> { "pricing_connection", "billing_connection", "users_connection", "CA_CONNECTION_STRING", "MAIN_CONNECTION_STRING" };
//    //    foreach (string dbName in dbNames)
//    //    {

//    //        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
//    //        selectQuery.SetConnectionKey(dbName);

//    //        selectQuery += "SELECT * FROM groups_modules_ips where GROUP_ID = 134 AND IS_ACTIVE=1 AND STATUS=1";
//    //        selectQuery.Execute("ipTbl", true);
//    //        DataTable dt = selectQuery.Table("ipTbl");
//    //        selectQuery.Finish();

//    //        foreach (DataRow row in dt.Rows)
//    //        {
//    //            ODBCWrapper.DataSetInsertQuery insertQuery = new ODBCWrapper.DataSetInsertQuery("groups_modules_ips");
//    //            foreach (DataColumn col in dt.Columns)
//    //            {
//    //                if (col.ColumnName != "ID" && col.ColumnName != "UPDATER_ID" && col.ColumnName != "EDITOR_REMARKS" &&
//    //                    col.ColumnName != "UPDATE_DATE" && col.ColumnName != "CREATE_DATE" && col.ColumnName != "PUBLISH_DATE")
//    //                {
//    //                    if (col.ColumnName == "GROUP_ID")
//    //                    {
//    //                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM(col.ColumnName, "=", groupId);
//    //                    }
//    //                    else if (col.ColumnName == "USERNAME")
//    //                    {
//    //                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM(col.ColumnName, "=", row[col.ColumnName].ToString().Replace("134", groupId.ToString()));
//    //                    }
//    //                    else
//    //                    {
//    //                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM(col.ColumnName, "=", row[col.ColumnName]);
//    //                    }
//    //                }

//    //            }
//    //            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 666);
//    //            insertQuery.SetConnectionKey(dbName);
//    //            insertQuery.Execute();
//    //            insertQuery.Finish();

//    //        }
//    //    }
//    //}


//    //private void disableUsersFromCsv()
//    //{
//    //    DataTable tmpCsvDt = new DataTable();
//    //    GenericParserAdapter csvReader = new GenericParsing.GenericParserAdapter(@"D:\UsersForDeactivation.csv");
//    //    tmpCsvDt = csvReader.GetDataTable();
//    //    tmpCsvDt.Rows[0].Delete();
//    //    foreach (DataRow userrow in tmpCsvDt.Rows)
//    //    {
//    //        //int i = 0;
//    //        //int userId = int.Parse(userrow[2].ToString());
//    //        //ODBCWrapper.DataSetSelectQuery select = new ODBCWrapper.DataSetSelectQuery();
//    //        //select.SetConnectionKey("users_connection");
//    //        //select += "SELECT * FROM users_domains WHERE (";
//    //        //select += ODBCWrapper.Parameter.NEW_PARAM("USER_ID", "=", userId);
//    //        //select += " AND ";
//    //        //select += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", 147);
//    //        //select += ")";
//    //        if (select.Execute("query", true) != null)
//    //        {
//    //            int count = select.Table("query").DefaultView.Count;
//    //            if (count > 0)
//    //            {
//    //                //int domainId = int.Parse(select.Table("query").Rows[0]["domain_id"].ToString());

//    //                ////update users table
//    //                //ODBCWrapper.UpdateQuery update = new ODBCWrapper.UpdateQuery("users");
//    //                //update.SetConnectionKey("users_connection");
//    //                //update += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 0);
//    //                //update += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 0);
//    //                //update += "WHERE";
//    //                //update += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", userId);
//    //                //update.Execute();
//    //                //update.Finish();

//    //                ////update users_domains
//    //                //update = new ODBCWrapper.UpdateQuery("users_domains");
//    //                //update.SetConnectionKey("users_connection");
//    //                //update += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 0);
//    //                //update += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 0);
//    //                //update += " where ";
//    //                //update += ODBCWrapper.Parameter.NEW_PARAM("domain_id", "=", domainId);
//    //                //update.Execute();
//    //                //update.Finish();
//    //                //select.Finish();

//    //                ////update domains_devices
//    //                //select = new ODBCWrapper.DataSetSelectQuery();
//    //                //select += "SELECT * FROM domains_devices WHERE";
//    //                //select += ODBCWrapper.Parameter.NEW_PARAM("domain_id", "=", domainId);
//    //                //select += " AND ";
//    //                //select += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", 147);
//    //                //select.SetConnectionKey("users_connection");
//    //                //select.Execute("domains_devices", true);
//    //                //count = select.Table("domains_devices").DefaultView.Count;
//    //                //foreach (DataRow item in select.Table("domains_devices").Rows)
//    //                //{
//    //                //    update = new ODBCWrapper.UpdateQuery("domains_devices");
//    //                //    update.SetConnectionKey("users_connection");
//    //                //    update += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 0);
//    //                //    update += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 2);
//    //                //    update += "WHERE ";
//    //                //    update += ODBCWrapper.Parameter.NEW_PARAM("domain_id", "=", domainId);

//    //                //    update.Execute();
//    //                //    update.Finish();
//    //                //}
//    //                i++;   
//    //            }
//    //            i.ToString();
//    //        }

//    //    }
//    //}

//    //private void AddNewUser()
//    //{
//    //    Users.User u = new Users.User();
//    //    u.m_oBasicData.m_sUserName = "idan";
//    //    u.m_oBasicData.m_sPassword = "12345";
//    //    u.m_oBasicData.m_sEmail = "test@test.com";
//    //    u.Save(150);
//    //}


    protected void Page_Load(object sender, EventArgs e)
    {
      //  Billing.AdyenCreditCard adyenDD = new Billing.AdyenCreditCard(147);
      //  adyenDD.ChargeUser("391318", 2.0, "SGD", "1.1.1.1",string.Empty, 1, 0 , string.Empty);
      //  //.ChargeUser("391318 ", 2.0, "SGD", "1.1.1.1", string.Empty, 1, 0, string.Empty, 1);
      ////  adyenDD.ChargeUser("391318 ", 2.0, "SGD", "1.1.1.1", string.Empty, 1, 0, string.Empty, 1);

      //  XslTransform myXslTransform;
      //  myXslTransform = new XslTransform();
      //  myXslTransform.Load(@"F:\Tvinci\Clients\YES\Yes-ADI\Offer with bundle\test.xsl");
      //  myXslTransform.Transform(@"F:\Tvinci\Clients\YES\Yes-ADI\Offer with bundle\OfferPackage_42189_20130304150613.xml", @"F:\Tvinci\Clients\YES\Yes-ADI\Offer with bundle\out.xml");
      //  return;


      //  string sCounty = TVinciShared.WS_Utils.GetIP2CountryCode("118.200.17.214");
      //  Response.Write(sCounty);
      //  return;
       // return;
       // TvinciRenewer.Renewer.GetInstance(1976, 6000, "147||10").DoTheTask();
       // return;

       // Billing.Utils.SendPurchaseMail("Visa", "I Do I Do", "321169", 11111, "5.35", "SGD", 147);
       // API.API apiWS = new API.API();
       // apiWS.SendMailTemplate("asdfsda", "asdfasdf", new API.PurchaseMailRequest());
        //DateTime tempDatye = new DateTime(2012, 1, 31).AddMonths(1).ToString("dd/MM/yyyy");
        //Response.Write(tempDatye);
        //string sResp = "";
        ////string json = "{\"template_name\": \"purchase_147\", \"template_content\": {}, \"key\": \"ffbc104b-f619-4922-93ae-00bbf3fb7150\", \"message\": {\"text\": null, \"subject\": \"This is a test\", \"from_email\": \"arik@tvinci.com\", \"from_name\": \"Arik\", \"to\": [{\"email\": \"dubi@tvinci.com\", \"name\": \"Dubi\"}], \"headers\": {}, \"track_opens\": false, \"track_clicks\": false, \"auto_text\": false, \"url_strip_qs\": false, \"preserve_recipients\": false, \"bcc_address\": null, \"merge\": true, \"global_merge_vars\": {}, \"merge_vars\": [{\"rcpt\": \"arik@tvinci.com\", \"vars\": [{\"name\": \"FIRSTNAME\", \"content\": \"Arik\"}]}], \"tags\": {}, \"google_analytics_domain\": {}, \"google_analytics_campaign\": {}, \"attachments\": {}}}";
        ////sResp = WS_Utils.SendXMLHttpReq("https://mandrillapp.com/api/1.0/messages/send-template.json", json, null);
        ////Response.Write(sResp);
        //int i = 0;
        //APIWS.api apiWS = new APIWS.api();


        //        //MCGroupRules.MCRuleFactory mcrf = new MCRuleFactory();
        //        ////mcrf.GetRuleImplementation(135);
        //        //List<MCRule> rules = mcrf.GetGroupRules(134);
        //        //foreach (MCRule rule in rules)
        //        //{
        //        //    MCImplementationBase impBase = mcrf.GetRuleImplementation(rule);
        //        //    MCObjByTemplate obj = impBase.GetRuleObject();
        //        //}
        //        //MCObjByTemplate mcObj = new MCObjByTemplate();
        //        //JavaScriptSerializer jsSer = new JavaScriptSerializer();

        //        //MCRule r = new MCRule(134, 6, RuleType.SubscriptionTypeBeforeEnd, 10080, 287);
        //        //MCImplementationBase impBase = mcrf.GetRuleImplementation(r);
        //        //MCObjByTemplate obj = impBase.GetRuleObject();
        //        //obj.ToString();
        //        MCSocialInviteTriggered mcImp = new MCSocialInviteTriggered(134,26, 8, 0, 0, 263352);
        //        mcImp.InitMCObj();
        //        mcImp.Send();
    }


    protected void btnTestFeeder_Click(object sender, EventArgs e)
    {
        ElasticSearchFeeder.ElasticSearchAbstract es = new ElasticSearchFeeder.ElasticSearchAbstract(eESFeederType.MEDIA);
        DateTime startDate = new DateTime(2013, 12, 1);
        DateTime endDate = new DateTime(2013, 12,31);
        es.Implementer = new ElasticSearchFeeder.BaseESMedia(147, "147", true, startDate, endDate) { bSwitchIndex = true };
        es.Start();

        int k = 0;
    }
}
