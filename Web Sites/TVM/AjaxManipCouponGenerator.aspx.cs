using System;
using System.Collections.Generic;
using System.Threading;
using TVinciShared;

public partial class AjaxManipCouponGenerator : System.Web.UI.Page
{
    static Dictionary<int, Thread> ThreadDict = new Dictionary<int, Thread>();

    protected void Page_Load(object sender, EventArgs e)
    {
        string sRet = "FAIL";
        string sType = "";

        string couponGroupId = "";

        if (Request.Form["type"] != null)
            sType = Request.Form["type"].ToString();
        if (Request.Form["coupon_group_id"] != null)
            couponGroupId = Request.Form["coupon_group_id"].ToString();

        bool ok = false;
        string errorMsg = "";

        if (sType == "CouponGenerator")
        {
            CouponGenerator(couponGroupId, ref ok, ref errorMsg);
        }
        if (sType == "CouponNameGenerator")
        {
            CouponNameGenerator(couponGroupId, ref ok, ref errorMsg);
        }
        if (ok == true)
            sRet = "OK";
        else
            sRet = "FAIL";
        Response.CacheControl = "no-cache";
        Response.AddHeader("Pragma", "no-cache");
        Response.Expires = -1;
        Response.Clear();
        Response.Write(sRet + "~~|~~" + errorMsg);
    }

    private void CouponGenerator(string couponGroupId, ref bool bOK, ref string sError)
    {
        string numberOfCoupons = "";
        bool useSpecialCharacters = false;
        bool useNumbers = false;
        bool useLetters = false;

        if (Request.Form["number_of_coupons"] != null)
            numberOfCoupons = Request.Form["number_of_coupons"].ToString();
        if (Request.Form["use_special_characters"] != null)
            useSpecialCharacters = bool.Parse(Request.Form["use_special_characters"].ToString());
        if (Request.Form["use_numbers"] != null)
            useNumbers = bool.Parse(Request.Form["use_numbers"].ToString());
        if (Request.Form["use_letters"] != null)
            useLetters = bool.Parse(Request.Form["use_letters"].ToString());


        int numOfCoupons = 0;
        int.TryParse(numberOfCoupons, out numOfCoupons);
        if (numOfCoupons <= 0)
        {
            sError = "Number of coupons to generate should be higher than 0";
            bOK = false;
        }
        if (numOfCoupons > 50000)
        {
            sError = "Maximum Generation is 50000 coupons";
            bOK = false;
        }

        if (!useLetters && !useNumbers && !useSpecialCharacters)
        {
            sError = "Please choose at least one option: letter, numbers or special characters";
            bOK = false;
        }

        if (string.IsNullOrEmpty(sError))
        {
            Int32 nGroupID = TVinciShared.LoginManager.GetLoginGroupID();

            if (!ThreadDict.ContainsKey(nGroupID))
            {
                ParameterizedThreadStart start = new ParameterizedThreadStart(GenerateCoupons);

                Thread t = new Thread(start);

                ThreadDict.Add(TVinciShared.LoginManager.GetLoginGroupID(), t);

                int[] vals = new int[6];
                vals[0] = numOfCoupons;
                vals[1] = int.Parse(couponGroupId);
                vals[2] = nGroupID;
                vals[3] = useSpecialCharacters ? 1 : 0;
                vals[4] = useNumbers ? 1 : 0;
                vals[5] = useLetters ? 1 : 0;

                t.Start(vals);
                bOK = true;
            }
            else
            {
                sError = "Working";
            }
        }
    }

    private void CouponNameGenerator(string couponGroupId, ref bool bOK, ref string sError)
    {
        string couponCode = "";

        if (Request.Form["coupon_code"] != null)
            couponCode = Request.Form["coupon_code"].ToString();

        int groupId = TVinciShared.LoginManager.GetLoginGroupID();

        if (IsCouponNameValid(groupId, couponCode, couponGroupId, ref sError))
        {
            if (!ThreadDict.ContainsKey(groupId))
            {
                ParameterizedThreadStart start = new ParameterizedThreadStart(GenerateNameCoupons);

                Thread t = new Thread(start);

                ThreadDict.Add(groupId, t);

                string[] vals = new string[3];
                vals[0] = couponCode;
                vals[1] = couponGroupId;
                vals[2] = groupId.ToString();

                t.Start(vals);
                bOK = true;
            }
            else
            {
                sError = "Working";
            }
        }       
    }

    private bool IsCouponNameValid(int groupId, string couponCode, string couponGroupId, ref string sError)
    {
        if (string.IsNullOrEmpty(couponCode) || couponCode.Length > 50)
        {
            sError = "The Coupon code provided is not valid.(does not match the required number of digits).";
            return false;
        }

        couponCode = couponCode.Trim();
        if (couponCode.Contains(" "))
        {
            sError = "The Coupon code should not have spaces.";
            return false;
        }
       
        if (IsCouponcouponCodeExist(groupId, couponCode))
        {
            sError = "Coupon code already exist";
            return false;
        }

        return true;
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

        PasswordGenerator p = new PasswordGenerator();
        p.Maximum = 16;
        p.Minimum = 12;
        p.RepeatCharacters = true;
        p.ExcludeSymbols = useSpecialCharacters == 0;
        p.UseNumbers = useNumbers == 1;
        p.UseLetters = useLetters == 1;

        for (int i = 0; i < nNumberOfCoupons; i++)
        {
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

        lock (ThreadDict)
        {
            ThreadDict.Remove(nGroupID);
        }
    }

    protected void GenerateNameCoupons(object val)
    {
        string[] vals = (string[])val;

        string couponCode = vals[0];
        string nCGID = vals[1];
        int groupID = int.Parse(vals[2]);

        string query = string.Format("Insert into coupons(code, COUPON_GROUP_ID, GROUP_ID) values('{0}',{1},{2});", couponCode, nCGID, groupID);

        ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
        directQuery += query;
        directQuery.SetConnectionKey("pricing_connection");
        directQuery.Execute();
        directQuery.Finish();
        directQuery = null;

        lock (ThreadDict)
        {
            ThreadDict.Remove(groupID);
        }
    }

    private bool IsCouponcouponCodeExist(int groupId, string code)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += string.Format("SELECT TOP (1) id FROM dbo.coupons WITH(NOLOCK) WHERE   group_id = {0} "
            + " AND CODE = '{1}' AND [status] = 1", groupId, code);

        if (selectQuery.Execute("query", true) != null && selectQuery.Table("query") != null && selectQuery.Table("query").Rows.Count != 0)
        {
            return true;
        }

        selectQuery.Finish();
        selectQuery = null;

        return false;
    }
}