using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class pic_resize_tool : System.Web.UI.Page
{
    protected string GetSafeQuery(string sKey)
    {
        if (Request.QueryString[sKey] != null)
            return Request.QueryString[sKey].ToString();
        return "";
    }
    protected Int32 GetSafeInt(string sKey)
    {
        try
        {
            return int.Parse(sKey);
        }
        catch
        {
            return 0;
        }
    }

    private static byte[] ConvertImageToByteArray(System.Drawing.Image imageToConvert,System.Drawing.Imaging.ImageFormat formatOfImage)
    {
        byte[] Ret;

        try
        {

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                imageToConvert.Save(ms, formatOfImage);
                Ret = ms.ToArray();
            }
        }
        catch (Exception) { throw; }

        return Ret;
    } 


    protected void Page_Load(object sender, EventArgs e)
    {
        byte[] byteArray = { };
        string sURL = GetSafeQuery("u");
        string sWidth = GetSafeQuery("w");
        Int32 nWidth = GetSafeInt(sWidth);
        
        string sHight = GetSafeQuery("h");
        Int32 nHeight = GetSafeInt(sHight);
        
        string sCrop = GetSafeQuery("c");
        bool bCrop = false;
        if (sCrop.Trim().ToLower() == "true")
            bCrop = true;
        if (CachingManager.CachingManager.Exist("image_resize" + sURL + "_" + sWidth + "_" + sHight + "_" + sCrop) == true)
            byteArray = (byte[])(CachingManager.CachingManager.GetCachedData("image_resize" + sURL + "_" + sWidth + "_" + sHight + "_" + sCrop));

        if (sURL != "")
        {
            System.Net.HttpWebRequest httpRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(sURL);
            System.Net.HttpWebResponse httpResponse = (System.Net.HttpWebResponse)httpRequest.GetResponse();
            System.IO.Stream imageStream = httpResponse.GetResponseStream();
            System.Drawing.Image i = System.Drawing.Image.FromStream(imageStream);
            if (nHeight == 0)
                nHeight = i.Height;
            if (nWidth == 0)
                nWidth = i.Width;
            Response.ClearHeaders();
            Response.Clear();
            Response.ContentType = TVinciShared.ImageUtils.GetEncoderType(sURL);
            TVinciShared.ImageUtils.DynamicResizeImage(sURL, nWidth, nHeight, bCrop, ref i);
            Response.Expires = -1;
            byteArray = ConvertImageToByteArray(i, TVinciShared.ImageUtils.GetFileFormat(sURL));
            i.Dispose();
            Response.BinaryWrite(byteArray);
        }
        CachingManager.CachingManager.SetCachedData("image_resize" + sURL + "_" + sWidth + "_" + sHight + "_" + sCrop, byteArray, 3600, System.Web.Caching.CacheItemPriority.Normal, 0, true);
    }
}
