using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Configuration;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.IO.Compression;
using KLogMonitor;
using System.Reflection;

namespace Financial
{
    public class Utils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        static public double GetDoubleSafeVal(ref ODBCWrapper.DataSetSelectQuery selectQuery, string sField, Int32 nIndex)
        {
            try
            {
                if (selectQuery.Table("query").DefaultView[nIndex].Row[sField] != null &&
                    selectQuery.Table("query").DefaultView[nIndex].Row[sField] != DBNull.Value)
                    return double.Parse(selectQuery.Table("query").DefaultView[nIndex].Row[sField].ToString());
                return 0.0;
            }
            catch
            {
                return 0.0;
            }
        }

        static public string GetStrSafeVal(ref ODBCWrapper.DataSetSelectQuery selectQuery, string sField, Int32 nIndex)
        {
            try
            {
                if (selectQuery.Table("query").DefaultView[nIndex].Row[sField] != null &&
                    selectQuery.Table("query").DefaultView[nIndex].Row[sField] != DBNull.Value)
                    return selectQuery.Table("query").DefaultView[nIndex].Row[sField].ToString();
                return "";
            }
            catch
            {
                return "";
            }
        }

        static public Int32 GetIntSafeVal(ref ODBCWrapper.DataSetSelectQuery selectQuery, string sField, Int32 nIndex)
        {
            try
            {
                if (selectQuery.Table("query").DefaultView[nIndex].Row[sField] != null &&
                    selectQuery.Table("query").DefaultView[nIndex].Row[sField] != DBNull.Value)
                    return int.Parse(selectQuery.Table("query").DefaultView[nIndex].Row[sField].ToString());
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        static public DateTime GetDateSafeVal(ref ODBCWrapper.DataSetSelectQuery selectQuery, string sField, Int32 nIndex)
        {
            try
            {
                if (selectQuery.Table("query").DefaultView[nIndex].Row[sField] != null &&
                    selectQuery.Table("query").DefaultView[nIndex].Row[sField] != DBNull.Value)
                    return (DateTime)(selectQuery.Table("query").DefaultView[nIndex].Row[sField]);
                return new DateTime(2000, 1, 1);
            }
            catch
            {
                return new DateTime(2000, 1, 1);
            }
        }

        static public string GetNodeValue(ref XmlNode theItem, string sXpath)
        {
            string sNodeVal = "";

            XmlNode theNodeVal = null;
            if (sXpath != "")
                theNodeVal = theItem.SelectSingleNode(sXpath);
            else
                theNodeVal = theItem;
            if (theNodeVal != null && theNodeVal.FirstChild != null)
                sNodeVal = theNodeVal.FirstChild.Value;
            return sNodeVal;
        }

        static public string GetItemParameterVal(ref XmlNode theNode, string sParameterName)
        {
            string sVal = "";
            if (theNode != null)
            {
                XmlAttributeCollection theAttr = theNode.Attributes;
                if (theAttr != null)
                {
                    Int32 nCount = theAttr.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        string sName = theAttr[i].Name.ToLower();
                        if (sName.ToLower().Trim() == sParameterName.ToLower().Trim())
                        {
                            sVal = theAttr[i].Value.ToString();
                            break;
                        }
                    }
                }
            }
            return sVal;
        }

        static public string GetNodeParameterVal(ref XmlNode theNode, string sXpath, string sParameterName)
        {
            string sVal = "";
            XmlNode theRoot = theNode.SelectSingleNode(sXpath);
            if (theRoot != null)
            {
                XmlAttributeCollection theAttr = theRoot.Attributes;
                if (theAttr != null)
                {
                    Int32 nCount = theAttr.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        string sName = theAttr[i].Name.ToLower();
                        if (sName.ToLower().Trim() == sParameterName.ToLower().Trim())
                        {
                            sVal = theAttr[i].Value.ToString();
                            break;
                        }
                    }
                }
            }
            return sVal;
        }

        static public string GetWSURL(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }

        static public TvinciPricing.PPVModule GetPPVModule(Int32 nGroupID, Int32 nPPVM, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            TvinciPricing.PPVModule thePPVModule = null;

            string sWSUserName = "";
            string sWSPass = "";
            TvinciPricing.mdoule m = new Financial.TvinciPricing.mdoule();
            if (GetWSURL("pricing_ws") != "")
                m.Url = GetWSURL("pricing_ws");

            if (CachingManager.CachingManager.Exist("GetPPVModuleData" + nPPVM + "_" + nGroupID.ToString()) == true)
                thePPVModule = (TvinciPricing.PPVModule)(CachingManager.CachingManager.GetCachedData("GetPPVModuleData" + nPPVM + "_" + nGroupID.ToString()));
            else
            {
                TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetPPVModuleData", "pricing", "1.1.1.1", ref sWSUserName, ref sWSPass);
                thePPVModule = m.GetPPVModuleData(sWSUserName, sWSPass, nPPVM.ToString(), sCountryCd2, sLanguageCode3, sDeviceName);
                CachingManager.CachingManager.SetCachedData("GetPPVModuleData" + nPPVM + "_" + nGroupID.ToString(), thePPVModule, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
            }

            return thePPVModule;
        }

        static public TvinciPricing.PrePaidModule GetPrePaidModule(Int32 nGroupID, Int32 nPPM, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            TvinciPricing.PrePaidModule thePPModule = null;

            string sWSUserName = "";
            string sWSPass = "";
            TvinciPricing.mdoule m = new Financial.TvinciPricing.mdoule();
            if (GetWSURL("pricing_ws") != "")
                m.Url = GetWSURL("pricing_ws");

            if (CachingManager.CachingManager.Exist("GetPrePaidModuleData" + nPPM + "_" + nGroupID.ToString()) == true)
                thePPModule = (TvinciPricing.PrePaidModule)(CachingManager.CachingManager.GetCachedData("GetPrePaidModuleData" + nPPM + "_" + nGroupID.ToString()));
            else
            {
                TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetPrePaidModuleData", "pricing", "1.1.1.1", ref sWSUserName, ref sWSPass);
                thePPModule = m.GetPrePaidModuleData(sWSUserName, sWSPass, nPPM, "", "", "");
                CachingManager.CachingManager.SetCachedData("GetPrePaidModuleData" + nPPM + "_" + nGroupID.ToString(), thePPModule, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
            }

            return thePPModule;
        }

        static public TvinciPricing.CouponsGroup GetCouponGroup(Int32 nGroupID, Int32 nCouponGroupID)
        {

            TvinciPricing.CouponsGroup theCoupon = null;

            string sWSUserName = "";
            string sWSPass = "";
            TvinciPricing.mdoule m = new Financial.TvinciPricing.mdoule();
            if (Utils.GetWSURL("pricing_staging_ws") != "")
                m.Url = Utils.GetWSURL("pricing_staging_ws");

            if (CachingManager.CachingManager.Exist("GetCouponGroupData" + nCouponGroupID + "_" + nGroupID.ToString()) == true)
                theCoupon = (TvinciPricing.CouponsGroup)(CachingManager.CachingManager.GetCachedData("GetCouponGroupData" + nCouponGroupID + "_" + nGroupID.ToString()));
            else
            {
                TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetCouponGroupData", "pricing", "1.1.1.1", ref sWSUserName, ref sWSPass);
                theCoupon = m.GetCouponGroupData(sWSUserName, sWSPass, nCouponGroupID.ToString());
                CachingManager.CachingManager.SetCachedData("GetCouponGroupData" + nCouponGroupID + "_" + nGroupID.ToString(), theCoupon, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
            }

            return theCoupon;
        }

        static public TvinciPricing.CouponData GetCouponStatus(Int32 nGroupID, string sCouponCode)
        {

            TvinciPricing.CouponData theCoupon = null;

            string sWSUserName = "";
            string sWSPass = "";
            TvinciPricing.mdoule m = new Financial.TvinciPricing.mdoule();
            if (Utils.GetWSURL("pricing_ws") != "")
                m.Url = Utils.GetWSURL("pricing_ws");

            if (CachingManager.CachingManager.Exist("GetCouponStatus" + sCouponCode + "_" + nGroupID.ToString()) == true)
                theCoupon = (TvinciPricing.CouponData)(CachingManager.CachingManager.GetCachedData("GetCouponStatus" + sCouponCode + "_" + nGroupID.ToString()));
            else
            {
                TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetCouponStatus", "pricing", "1.1.1.1", ref sWSUserName, ref sWSPass);
                theCoupon = m.GetCouponStatus(sWSUserName, sWSPass, sCouponCode);
                CachingManager.CachingManager.SetCachedData("GetCouponStatus" + sCouponCode + "_" + nGroupID.ToString(), theCoupon, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
            }

            return theCoupon;
        }


        public static string SaveZipFile(string[] sfilesEnntries, string path)
        {
            ZipOutputStream zipOutput = null;
            string zipFileName = string.Empty;
            try
            {
                if (sfilesEnntries == null || sfilesEnntries.Length == 0)
                {
                    return string.Empty;
                }

                //Sanitize inputs
                string stDirToZip = Path.GetFullPath(path);
                string stZipPath = string.Empty;
                foreach (string zipName in sfilesEnntries)
                {
                    if (zipName.Contains("Financial_Report"))
                    {
                        stZipPath = Path.GetFullPath(System.IO.Path.Combine(path, zipName));
                        zipFileName = zipName.Replace(".xml", ".zip");
                    }
                }
                if (stZipPath == string.Empty)
                {
                    stZipPath = Path.GetFullPath(System.IO.Path.Combine(path, sfilesEnntries[0]));

                }
                stZipPath = stZipPath.Replace(".xml", ".zip");

                if (File.Exists(stZipPath))
                    File.Delete(stZipPath);

                ICSharpCode.SharpZipLib.Checksums.Crc32 crc = new ICSharpCode.SharpZipLib.Checksums.Crc32();
                zipOutput = new ZipOutputStream(File.Create(stZipPath));
                zipOutput.SetLevel(9); // 0 - store only to 9 - means best compression

                foreach (string fi in sfilesEnntries)
                {
                    FileStream fs = File.OpenRead(System.IO.Path.Combine(path, fi));

                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);

                    //Create the right arborescence within the archive                  
                    ZipEntry entry = new ZipEntry(fi);
                    entry.DateTime = DateTime.Now;

                    // set Size and the crc, because the information
                    // about the size and crc should be stored in the header
                    // if it is not set it is automatically written in the footer.
                    // (in this case size == crc == -1 in the header)
                    // Some ZIP programs have problems with zip files that don't store
                    // the size and crc in the header.
                    entry.Size = fs.Length;
                    fs.Close();

                    crc.Reset();
                    crc.Update(buffer);

                    entry.Crc = crc.Value;

                    zipOutput.PutNextEntry(entry);

                    zipOutput.Write(buffer, 0, buffer.Length);
                    File.Delete(System.IO.Path.Combine(path, fi)); //delete original file
                }
                zipOutput.Finish();
                zipOutput.Close();
                zipOutput = null;
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                zipOutput.Finish();
                zipOutput.Close();
                zipOutput = null;
            }
            return zipFileName;
        }

        public static DateTime GetDateForBillingTransaction(int nGroupID, int nBillingTransactionID)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

            DateTime dDate = DateTime.MaxValue;

            //Get all billing transaction                     
            selectQuery += " select top 1 create_date from billing_transactions where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", ">", nBillingTransactionID); // last billing transaction that was calculate 
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += " order by id asc";
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0) // get the start date , occurding to the billing transaction _id
                {
                    dDate = Utils.GetDateSafeVal(ref selectQuery, "create_date", 0);
                }
            }

            selectQuery.Finish();
            selectQuery = null;

            return dDate;

        }
    }
}
