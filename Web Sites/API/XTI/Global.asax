<%@ Application Language="C#" %>

<script runat="server">

    void Application_Start(object sender, EventArgs e) 
    {
        // Code that runs on application startup

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
        // Code that runs when a new session is started
        //------TVINCI----------
        Session["MSSQL_SERVER_NAME"] = "msd101.1host.co.il";
        Session["DB_NAME"] = "tvinci";
        Session["UN"] = "production";//guy
        Session["PS"] = "lF6CZU9HIOIAGuzj";//ktaufjxhxnt
        Session["ODBC_CACH_SEC"] = "0";
        
        //Session["O_ENVIRONMENT"] = "test";
        Session["O_ENVIRONMENT"] = "prod";
    }

    void Session_End(object sender, EventArgs e) 
    {
        // Code that runs when a session ends. 
        // Note: The Session_End event is raised only when the sessionstate mode
        // is set to InProc in the Web.config file. If session mode is set to StateServer 
        // or SQLServer, the event is not raised.

    }
       
</script>
