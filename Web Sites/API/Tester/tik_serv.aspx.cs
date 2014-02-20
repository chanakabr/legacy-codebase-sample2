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
using System.Security.Cryptography;
using System.Text;

public partial class tik_serv : System.Web.UI.Page
{
    protected void RequestToken()
    {
        tik_bill.Service t = new tik_bill.Service();
        tik_bill.CreditCard c = new tik_bill.CreditCard();
        c.CardNumber = "5326110340922882";
        c.CvvCode = 142;
        c.ExpireMonth = 2;
        c.ExpireYear = 2011;
        c.Name = "Guy Barkan";
        string sCustom = "30";
        //string sCustom = "bbb";
        string sSecret = "G20b6D62C81Bx7";
        string sIP = "80.179.194.132";
        string sHash = sCustom + sSecret;
        MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
        md5Provider = new MD5CryptoServiceProvider();
        byte[] originalBytes = UTF8Encoding.Default.GetBytes(sHash);
        byte[] encodedBytes = md5Provider.ComputeHash(originalBytes);
        string sHased = BitConverter.ToString(encodedBytes).Replace("-", "").ToLower();
        tik_bill.Response resp = t.RequestCreditCardToken(c, sCustom, sHased);
    }

    

    protected void Page_Load(object sender, EventArgs e)
    {
        
    }
}
