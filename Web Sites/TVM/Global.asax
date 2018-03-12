<%@ Application Language="C#" %>

<script runat="server">

    void Application_Start(object sender, EventArgs e)
    {
        TVinciShared.WS_Utils.InitTcmConfig();
        
        // set monitor and log configuration files
        KLogMonitor.KMonitor.Configure("log4net.config", KLogMonitor.KLogEnums.AppType.WS);
        KLogMonitor.KLogger.Configure("log4net.config", KLogMonitor.KLogEnums.AppType.WS);
    }

    void Application_End(object sender, EventArgs e)
    {
        
    }

    void Application_BeginRequest(object sender, EventArgs e)
    {             
        string sAppStatus = TVinciShared.WS_Utils.GetTcmConfigValue("APP_STATE");
        if (sAppStatus == "version_upload")
        {
            if (Request.RawUrl.ToUpper().IndexOf("VERSION_UPLOAD.HTM") == -1 &&
                Request.RawUrl.ToUpper().IndexOf(".JS") == -1 &&
                Request.RawUrl.ToUpper().IndexOf(".CSS") == -1 &&
                Request.RawUrl.ToUpper().IndexOf(".SWF") == -1)
            {
                Response.Redirect("VERSION_UPLOAD.HTM");
            }
        }
    }

    void Application_Error(object sender, EventArgs e)
    {
        //TVinciShared.LoginManager.LogoutFromSite("login.html");
    }

    void Session_Start(object sender, EventArgs e)
    {
        //------TVINCI----------
        Session["MSSQL_SERVER_NAME"] = "msd169.1host.co.il";
        Session["DB_NAME"] = "tvinci";
        Session["UN"] = "production";
        Session["PS"] = "lF6CZU9HIOIAGuzj";
        Session["ODBC_CACH_SEC"] = "0";
        //------MSN_BUBOT_FRONT from local----------
        //Session["MSSQL_SERVER_NAME"] = "192.117.152.87";
        //Session["DB_NAME"] = "habobot";
        //DBO
        //Session["UN"] = "habobot_dbo";
        //USER
        //Session["UN"] = "habobot_user";
        //DBO
        //Session["PS"] = "at9M9Zqo";
        //USER
        //Session["PS"] = "TlA7oSGU";

        Session["peek_date"] = "Choose a date";
        Session["Su"] = "Sun";
        Session["Mo"] = "Mon";
        Session["Tu"] = "Tue";
        Session["We"] = "Wen";
        Session["Th"] = "Thu";
        Session["Fr"] = "Fri";
        Session["Sa"] = "Sat";
        Session["MonthList"] = "Array(\"\",\"Jan\",\"Feb\",\"Mar\",\"Apr\",\"May\",\"Jun\",\"Jul\",\"Aug\",\"Sep\",\"Oct\",\"Nov\",\"Dec\")";

    }

    void Session_End(object sender, EventArgs e)
    {
        // Code that runs when a session ends. 
        // Note: The Session_End event is raised only when the sessionstate mode
        // is set to InProc in the Web.config file. If session mode is set to StateServer 
        // or SQLServer, the event is not raised.

    }

       
</script>
