using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

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

        if (string.IsNullOrEmpty(couponCode))
        {
            sError = "Coupon code (name) is empty";
            bOK = false;
        }

        if (string.IsNullOrEmpty(sError))
        {
            Int32 nGroupID = TVinciShared.LoginManager.GetLoginGroupID();

            if (!ThreadDict.ContainsKey(nGroupID))
            {
                ParameterizedThreadStart start = new ParameterizedThreadStart(GenerateNameCoupons);

                Thread t = new Thread(start);

                ThreadDict.Add(TVinciShared.LoginManager.GetLoginGroupID(), t);

                string[] vals = new string[3];
                vals[0] = couponCode;
                vals[1] = couponGroupId;
                vals[2] = nGroupID.ToString();

                t.Start(vals);
                bOK = true;
            }
            else
            {
                sError = "Working";
            }
        }
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
            p.RepeatCharacters = true;
            p.ExcludeSymbols = useSpecialCharacters == 0;
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

}

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
        int upperBound = passwordCharModifiedArray.GetUpperBound(0);

        int randomCharPosition = GetCryptographicRandomNumber(
            passwordCharModifiedArray.GetLowerBound(0), upperBound);

        char randomChar = passwordCharModifiedArray[randomCharPosition];

        return randomChar;
    }

    public string Generate()
    {
        // Pick random length between minimum and maximum   
        int pwdLength = GetCryptographicRandomNumber(this.Minimum,
            this.Maximum);

        SetPasswordCharArray();

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

    public void SetPasswordCharArray()
    {
        string optionalCharactersModified = optionalCharacters;
        //pwdCharArray
        if (ExcludeSymbols)
        {
            optionalCharactersModified = optionalCharactersModified.Remove(62, 4);
        }

        if (!UseNumbers)
        {
            optionalCharactersModified = optionalCharactersModified.Remove(52, 10);

        }

        if (!UseLetters)
        {
            optionalCharactersModified = optionalCharactersModified.Remove(0, 52);

        }

        passwordCharModifiedArray = optionalCharactersModified.ToArray();
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
    //optional Characters
    private string optionalCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@#$?";
    private char[] passwordCharModifiedArray;
    public bool UseNumbers { get; set; }
    public bool UseLetters { get; set; }
}