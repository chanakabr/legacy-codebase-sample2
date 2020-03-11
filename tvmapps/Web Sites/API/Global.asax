<%@ Application Language="C#" %>

<script runat="server">

    void Application_Start(object sender, EventArgs e)
    {
        TVinciShared.WS_Utils.InitTcmConfig();
    }

    void Application_End(object sender, EventArgs e)
    {
        //  Code that runs on application shutdown

    }

    void Application_Error(object sender, EventArgs e)
    {
        // Code that runs when an unhandled error occurs

    }

    void Session_Start(object sender, EventArgs e)
    {
        //------TVINCI----------
        Session["MSSQL_SERVER_NAME"] = "msd169.1host.co.il";
        Session["DB_NAME"] = "tvinci";
        Session["UN"] = "production";//guy
        Session["PS"] = "lF6CZU9HIOIAGuzj";//ktaufjxhxnt
        Session["ODBC_CACH_SEC"] = "3600";
        Session["ODBC_CACH_SEC_FIX"] = "3600";

        //Session["MSSQL_SERVER_NAME"] = "msd132.1host.co.il";
        //Session["DB_NAME"] = "mtvroot_db";
        //Session["UN"] = "mtvroot_dbalogin";
        //Session["PS"] = "UJgsFGXS387G";
        
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
