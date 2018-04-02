using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using TVinciShared;

public class PasswordGenerator
{
    public PasswordGenerator()
    {
        this.Minimum = DefaultMinimum;
        this.Maximum = DefaultMaximum;
        this.ConsecutiveCharacters = false;
        this.RepeatCharacters = true;
        this.ExcludeSymbols = false;
        this.Exclusions = null;

        rng = new RNGCryptoServiceProvider();
    }

    protected int GetCryptographicRandomNumber(int lBound, int uBound)
    {
        // Assumes lBound >= 0 && lBound < uBound
        // returns an int >= lBound and < uBound
        uint urndnum;
        byte[] rndnum = new Byte[4];
        if (lBound == uBound - 1)
        {
            // test for degenerate case where only lBound can be returned
            return lBound;
        }

        uint xcludeRndBase = (uint.MaxValue -
            (uint.MaxValue % (uint)(uBound - lBound)));

        do
        {
            rng.GetBytes(rndnum);
            urndnum = System.BitConverter.ToUInt32(rndnum, 0);
        } while (urndnum >= xcludeRndBase);

        return (int)(urndnum % (uBound - lBound)) + lBound;
    }

    protected char GetRandomCharacter()
    {
        int upperBound = pwdCharArray.GetUpperBound(0);

        if (true == this.ExcludeSymbols)
        {
            upperBound = PasswordGenerator.UBoundDigit;
        }

        int randomCharPosition = GetCryptographicRandomNumber(
            pwdCharArray.GetLowerBound(0), upperBound);

        char randomChar = pwdCharArray[randomCharPosition];

        return randomChar;
    }

    public string Generate()
    {
        // Pick random length between minimum and maximum   
        int pwdLength = GetCryptographicRandomNumber(this.Minimum,
            this.Maximum);

        System.Text.StringBuilder pwdBuffer = new System.Text.StringBuilder();
        pwdBuffer.Capacity = this.Maximum;

        // Generate random characters
        char lastCharacter, nextCharacter;

        // Initial dummy character flag
        lastCharacter = nextCharacter = '\n';

        for (int i = 0; i < pwdLength; i++)
        {
            nextCharacter = GetRandomCharacter();

            if (false == this.ConsecutiveCharacters)
            {
                while (lastCharacter == nextCharacter)
                {
                    nextCharacter = GetRandomCharacter();
                }
            }

            if (false == this.RepeatCharacters)
            {
                string temp = pwdBuffer.ToString();
                int duplicateIndex = temp.IndexOf(nextCharacter);
                while (-1 != duplicateIndex)
                {
                    nextCharacter = GetRandomCharacter();
                    duplicateIndex = temp.IndexOf(nextCharacter);
                }
            }

            if ((null != this.Exclusions))
            {
                while (-1 != this.Exclusions.IndexOf(nextCharacter))
                {
                    nextCharacter = GetRandomCharacter();
                }
            }

            pwdBuffer.Append(nextCharacter);
            lastCharacter = nextCharacter;
        }

        if (null != pwdBuffer)
        {
            return pwdBuffer.ToString();
        }
        else
        {
            return String.Empty;
        }
    }

    public string Exclusions
    {
        get { return this.exclusionSet; }
        set { this.exclusionSet = value; }
    }

    public int Minimum
    {
        get { return this.minSize; }
        set
        {
            this.minSize = value;
            if (PasswordGenerator.DefaultMinimum > this.minSize)
            {
                this.minSize = PasswordGenerator.DefaultMinimum;
            }
        }
    }

    public int Maximum
    {
        get { return this.maxSize; }
        set
        {
            this.maxSize = value;
            if (this.minSize >= this.maxSize)
            {
                this.maxSize = PasswordGenerator.DefaultMaximum;
            }
        }
    }

    public bool ExcludeSymbols
    {
        get { return this.hasSymbols; }
        set { this.hasSymbols = value; }
    }

    public bool RepeatCharacters
    {
        get { return this.hasRepeating; }
        set { this.hasRepeating = value; }
    }

    public bool ConsecutiveCharacters
    {
        get { return this.hasConsecutive; }
        set { this.hasConsecutive = value; }
    }

    private const int DefaultMinimum = 6;
    private const int DefaultMaximum = 10;
    private const int UBoundDigit = 61;

    private RNGCryptoServiceProvider rng;
    private int minSize;
    private int maxSize;
    private bool hasRepeating;
    private bool hasConsecutive;
    private bool hasSymbols;
    private string exclusionSet;
    private char[] pwdCharArray = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@#$?".ToCharArray();
    public bool useSpecialCharacters { get; set; }
    public bool UseNumbers { get; set; }
    public bool UseLetters { get; set; }
}

public partial class adm_coupons_list_generator : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    static Dictionary<int, Thread> ThreadDict = new Dictionary<int, Thread>();

    static public bool IsTvinciImpl()
    {
        Int32 nImplID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select * from groups_modules_implementations where is_active=1 and status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", 3);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                nImplID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IMPLEMENTATION_ID"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        if (nImplID == 1)
            return true;
        return false;
    }


    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            if (IsTvinciImpl() == false)
            {
                Server.Transfer("adm_module_not_implemented.aspx");
                return;
            }

            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID, "adm_coupons_groups.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                string sNumberOfCoupons = Request.Form["0_val"].ToString();
                int useSpecialCharacters = 0;
                int useNumbers = 0;
                int useletters = 0;
                if (!string.IsNullOrEmpty(Request.Form["1_val"]))
                {
                    useSpecialCharacters = 1;
                }
                if (!string.IsNullOrEmpty(Request.Form["2_val"]))
                {
                    useNumbers = 1;
                }
                if (!string.IsNullOrEmpty(Request.Form["3_val"]))
                {
                    useletters = 1;
                }
                //string sMax = Request.Form["1_val"].ToString();
                if (sNumberOfCoupons == "")
                {
                    Session["error_msg"] = "Please enter numbers";
                }
                try
                {
                    Int32 nNumberOfCoupons = int.Parse(sNumberOfCoupons);
                    //Int32 nMax = int.Parse(sMax);
                    //if (nMax <= nMin)
                    //{
                    //Session["error_msg"] = "Minimum number should be lower then the maximum number";
                    //}
                    //else
                    //{
                    if (nNumberOfCoupons > 50000)
                        Session["error_msg"] = "Maximum Generation is 50000 coupons";
                    else
                    {

                        Int32 nGroupID = LoginManager.GetLoginGroupID();

                        if (!ThreadDict.ContainsKey(nGroupID))
                        {
                            ParameterizedThreadStart start = new ParameterizedThreadStart(GenerateCoupons);

                            Thread t = new Thread(start);

                            ThreadDict.Add(LoginManager.GetLoginGroupID(), t);

                            int[] vals = new int[6];
                            vals[0] = nNumberOfCoupons;
                            vals[1] = int.Parse(Session["coupon_group_id"].ToString());
                            vals[2] = nGroupID;
                            vals[3] = useSpecialCharacters;
                            vals[4] = useNumbers;
                            vals[5] = useletters;

                            t.Start(vals);

                        }
                        else
                        {
                            Response.Write("Working");
                        }


                        /*
                        for (int i = 0; i < nNumberOfCoupons; i++)
                        {
                            PasswordGenerator p = new PasswordGenerator();
                            p.Maximum = 16;
                            p.Minimum = 12;
                            p.RepeatCharacters = false;
                            string sPass = p.Generate();
                            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("coupons");
                            insertQuery.SetConnectionKey("pricing_connection");
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CODE", "=", sPass);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUPON_GROUP_ID", "=", Session["coupon_group_id"].ToString());
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
                            insertQuery.Execute();
                            insertQuery.Finish();
                            insertQuery = null;
                            System.Threading.Thread.Sleep(50);
                        }
                        Response.Redirect("adm_coupons_list.aspx?coupon_group_id=" + Session["coupon_group_id"].ToString());
                        */
                    }
                    //}
                }
                catch (Exception ex)
                {
                    Session["error_msg"] = ex.Message + "|" + ex.StackTrace;
                }
                return;
            }
            else
            {
                Session["error_msg"] = null;
            }

            if (Request.QueryString["coupon_group_id"] != null &&
                Request.QueryString["coupon_group_id"].ToString() != "")
            {
                Session["coupon_group_id"] = int.Parse(Request.QueryString["coupon_group_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("coupons_groups", "group_id", int.Parse(Session["coupon_group_id"].ToString()), "pricing_connection").ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
        }

    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Coupons Generator";
        if (Session["coupon_group_id"] != null && Session["coupon_group_id"].ToString() != "" && Session["coupon_group_id"].ToString() != "0")
        {
            sRet += " (";
            sRet += ODBCWrapper.Utils.GetTableSingleVal("coupons_groups", "code", int.Parse(Session["coupon_group_id"].ToString()), "pricing_connection");
            sRet += ")";
        }
        Response.Write(sRet);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();

        if (ThreadDict.ContainsKey(nGroupID))
        {
            return "<div>Working... Please check again later</div>";
        }

        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        string sBack = "adm_coupons_groups.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("coupons_groups", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortIntField dr_min = new DataRecordShortIntField(true, 9, 9);
        dr_min.Initialize("Number of coupons to generate", "adm_table_header_nbg", "FormInput", "MAX_USE_TIME", true);
        theRecord.AddRecord(dr_min);

        DataRecordCheckBoxField drcheckBoxField = new DataRecordCheckBoxField(true);
        drcheckBoxField.Initialize("Use special characters", "adm_table_header_nbg", "FormInput", "USE_SPECIAL_CHARACTERS", false);
        drcheckBoxField.SetDefault(0);
        theRecord.AddRecord(drcheckBoxField);

        drcheckBoxField = new DataRecordCheckBoxField(true);
        drcheckBoxField.Initialize("Use numbers", "adm_table_header_nbg", "FormInput", "USE_NUMBERS", false);
        drcheckBoxField.SetDefault(0);
        theRecord.AddRecord(drcheckBoxField);

        drcheckBoxField = new DataRecordCheckBoxField(true);
        drcheckBoxField.Initialize("Use letters", "adm_table_header_nbg", "FormInput", "USE_LETTERS", false);
        drcheckBoxField.SetDefault(0);
        theRecord.AddRecord(drcheckBoxField);

        /*
        DataRecordShortIntField dr_max = new DataRecordShortIntField(true, 9, 9);
        dr_max.Initialize("Maximum Coupon Number", "adm_table_header_nbg", "FormInput", "MAX_USE_TIME", true);
        theRecord.AddRecord(dr_max);
        */
        string sTable = theRecord.GetTableHTML("adm_coupons_list_generator.aspx?submited=1");

        return sTable;
    }




    protected void GenerateCoupons(object val)
    {
        int[] vals = (int[])val;

        Int32 nNumberOfCoupons = vals[0];
        Int32 nCGID = vals[1];
        Int32 nGroupID = vals[2];
        int useSpecialCharacters = vals[3];
        int useNumbers = vals[4];
        int useLetters = vals[5];

        string query = string.Empty;
        int nCounter = 0;
        int nInsertMax = 300;

        for (int i = 0; i < nNumberOfCoupons; i++)
        {
            PasswordGenerator p = new PasswordGenerator();
            p.Maximum = 16;
            p.Minimum = 12;
            p.RepeatCharacters = false;
            p.useSpecialCharacters = useSpecialCharacters == 1;
            p.UseNumbers = useNumbers == 1;
            p.UseLetters = useLetters == 1;

            string sPass = p.Generate();

            query += string.Format("Insert into coupons(code, COUPON_GROUP_ID, GROUP_ID) values('{0}',{1},{2});", sPass, nCGID, nGroupID);
            nCounter++;

            if (nCounter == nInsertMax || (i + 1) == nNumberOfCoupons)
            {
                ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                directQuery += query;
                directQuery.SetConnectionKey("pricing_connection");
                directQuery.Execute();
                directQuery.Finish();
                directQuery = null;

                nCounter = 0;
                query = string.Empty;
            }

        }

        //Thread.Sleep(10000);

        lock (ThreadDict)
        {
            ThreadDict.Remove(nGroupID);
        }
    }
}
