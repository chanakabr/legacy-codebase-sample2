using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using DAL;
using ApiObjects;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Pricing;

namespace Core.Pricing
{
    public class TvinciCollection : BaseCollection
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public TvinciCollection(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        public override Collection GetCollectionData(string sCollectionCode, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, bool bGetAlsoUnActive)
        {
            Collection tmpSubscription = new Collection();
            try
            {
                int nCollectionCode = 0;
                int? nIsActive = 1;

                if (!bGetAlsoUnActive)
                {
                    // use optimized flow which reduces the num of calls to the db.
                    Collection[] res = GetCollectionsData(new string[1] { sCollectionCode }, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                    if (res != null && res.Length > 0)
                        return res[0];
                    return null;
                }
                else
                {
                    nIsActive = null;
                }

                if (!Int32.TryParse(sCollectionCode, out nCollectionCode) || nCollectionCode == 0)
                {
                    return null;
                }
                DataSet dsCollections = PricingDAL.Get_CollectionData(m_nGroupID, nIsActive, nCollectionCode);
                List<Collection> collectionsList = CreateCollectionsListFromDataSet(dsCollections, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                if (collectionsList != null && collectionsList.Count > 0)
                {
                    tmpSubscription = collectionsList[0];
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetCollectionData.");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Coll Code: ", sCollectionCode));
                sb.Append(String.Concat(" Cntry Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" D Name: ", sDEVICE_NAME));
                sb.Append(String.Concat(" GetAlsoUnactive: ", bGetAlsoUnActive.ToString().ToLower()));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;
            }

            return tmpSubscription;
        }

        private List<Collection> CreateCollectionsListFromDataSet(DataSet dsCollections, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            List<Collection> retList = new List<Collection>();
            Collection tmpCollection = new Collection();

            if (dsCollections != null && dsCollections.Tables.Count > 0)
            {
                int nCount = dsCollections.Tables[0].DefaultView.Count;

                if (nCount > 0)
                {
                    DataTable dtCollection = dsCollections.Tables[0];
                    foreach (DataRow collectionRow in dtCollection.Rows)
                    {
                        int nCollectionCode = ODBCWrapper.Utils.GetIntSafeVal(collectionRow["ID"]);

                        tmpCollection = CreateCollectionObject(collectionRow, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, GetCollectionDescription(nCollectionCode),
                                                                   GetCollectionsChannels(nCollectionCode, m_nGroupID), GetCollectionName(nCollectionCode));
                        retList.Add(tmpCollection);
                    }

                }
            }
            return retList;
        }

        static protected LanguageContainer[] GetCollectionName(Int32 nCollectionID)
        {
            LanguageContainer[] theContainer = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from collection_names with (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("collection_id", "=", nCollectionID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        theContainer = new LanguageContainer[nCount];
                    }
                    Int32 nIndex = 0;
                    for (int i = 0; i < nCount; i++)
                    {
                        string sLang = selectQuery.Table("query").DefaultView[i].Row["language_code3"].ToString();
                        string sVal = selectQuery.Table("query").DefaultView[i].Row["description"].ToString();
                        LanguageContainer t = new LanguageContainer();
                        t.Initialize(sLang, sVal);
                        theContainer[nIndex] = t;
                        nIndex++;
                    }
                }
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
            return theContainer;
        }

        static protected BundleCodeContainer[] GetCollectionsChannels(Int32 nCollectionCode, Int32 nGroupID)
        {
            BundleCodeContainer[] theContainer = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select CHANNEL_ID from collections_channels with (nolock) where is_active=1 and status=1 and ";
                selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(nGroupID, "MAIN_CONNECTION_STRING");
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COLLECTION_ID", "=", nCollectionCode);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        theContainer = new BundleCodeContainer[nCount];
                    }
                    Int32 nIndex = 0;
                    for (int i = 0; i < nCount; i++)
                    {
                        string sID = selectQuery.Table("query").DefaultView[i].Row["channel_id"].ToString();
                        string sVal = "";
                        BundleCodeContainer t = new BundleCodeContainer();
                        t.Initialize(sID, sVal);
                        theContainer[nIndex] = t;
                        nIndex++;
                    }
                }

            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
            return theContainer;
        }

        static protected LanguageContainer[] GetCollectionDescription(Int32 nCollectionID)
        {
            LanguageContainer[] theContainer = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from collection_descriptions with (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("collection_id", "=", nCollectionID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        theContainer = new LanguageContainer[nCount];
                    }
                    Int32 nIndex = 0;
                    for (int i = 0; i < nCount; i++)
                    {
                        string sLang = selectQuery.Table("query").DefaultView[i].Row["language_code3"].ToString();
                        string sVal = selectQuery.Table("query").DefaultView[i].Row["description"].ToString();
                        LanguageContainer t = new LanguageContainer();
                        t.Initialize(sLang, sVal);
                        theContainer[nIndex] = t;
                        nIndex++;
                    }
                }

            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
            return theContainer;
        }

        private Collection CreateCollectionObject(DataRow collectionRow, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME,
                                                      LanguageContainer[] collectionDescription, BundleCodeContainer[] collectionChannels,
                                                      LanguageContainer[] collectionName)
        {
            Collection retCollection = new Collection();

            int nCollectionRowCode = ODBCWrapper.Utils.GetIntSafeVal(collectionRow["ID"]);
            string sPriceCode = ODBCWrapper.Utils.GetSafeStr(collectionRow["PRICE_ID"]);
            string sUsageModuleCode = ODBCWrapper.Utils.GetSafeStr(collectionRow["USAGE_MODULE_ID"]);
            string sDiscountModuleCode = ODBCWrapper.Utils.GetSafeStr(collectionRow["DISCOUNT_ID"]);
            string sName = ODBCWrapper.Utils.GetSafeStr(collectionRow["NAME"]);
            string sCouponGroupCode = ODBCWrapper.Utils.GetSafeStr(collectionRow["COUPON_GROUP_CODE"]);

            DateTime dStart = new DateTime(2000, 1, 1);
            DateTime dEnd = new DateTime(2099, 1, 1);
            if (collectionRow["START_DATE"] != null && collectionRow["START_DATE"] != DBNull.Value)
            {
                dStart = (DateTime)(collectionRow["START_DATE"]);
            }

            if (collectionRow["END_DATE"] != null && collectionRow["END_DATE"] != DBNull.Value)
            {
                dEnd = (DateTime)(collectionRow["END_DATE"]);
            }

            retCollection.Initialize(sPriceCode, sUsageModuleCode, sDiscountModuleCode, sCouponGroupCode, collectionDescription, m_nGroupID,
                                           nCollectionRowCode.ToString(), collectionChannels, dStart, dEnd, null, collectionName, sPriceCode, sUsageModuleCode, sName,
                                           sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

            return retCollection;
        }

        public override Collection[] GetCollectionsData(string[] oCollCodes, string sCountryCd, string sLanguageCode, string sDeviceName)
        {
            Collection[] res = null;
            try
            {
                if (oCollCodes != null && oCollCodes.Length > 0)
                {
                    List<long> lCollCodes = new List<long>(oCollCodes.Length);
                    for (int i = 0; i < oCollCodes.Length; i++)
                    {
                        long temp = 0;
                        if (Int64.TryParse(oCollCodes[i], out temp) && temp > 0)
                        {
                            lCollCodes.Add(temp);
                        }
                        else
                        {
                            log.Error("Error - " + string.Format("Failed to parse: {0} into long", oCollCodes[i]));
                        }
                    } // end for

                    if (lCollCodes.Count > 0)
                    {
                        DataSet ds = PricingDAL.Get_CollectionsData(m_nGroupID, lCollCodes);
                        if (IsCollsDataSetValid(ds))
                        {
                            Dictionary<long, List<LanguageContainer>> collsDescriptionsMapping = ExtractCollectionsDescriptions(ds);
                            Dictionary<long, List<BundleCodeContainer>> collsChannelsMapping = ExtractCollectionsChannels(ds);
                            Dictionary<long, List<LanguageContainer>> collsNamesMapping = ExtractCollectionsNames(ds);
                            res = CreateCollections(ds, collsDescriptionsMapping, collsChannelsMapping, collsNamesMapping,
                                sCountryCd, sLanguageCode, sDeviceName).ToArray();

                        }
                        else
                        {
                            log.Error("Error - " + GetGetCollectionsDataErrMsg("Colls DataSet is invalid. ", lCollCodes));
                        }
                    }
                    else
                    {
                        log.Error("Error - " + GetGetCollectionsDataErrMsg("Colls list is empty. ", lCollCodes));
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetCollectionsData. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Cntry Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLanguageCode));
                sb.Append(String.Concat(" D Name: ", sDeviceName));
                if (oCollCodes != null && oCollCodes.Length > 0)
                {
                    sb.Append(" Coll Codes: ");
                    for (int i = 0; i < oCollCodes.Length; i++)
                    {
                        sb.Append(String.Concat(oCollCodes[i], "; "));
                    }
                }
                else
                {
                    sb.Append(String.Concat(" Coll Codes is empty ."));
                }
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;
            }

            return res;
        }

        private List<Collection> CreateCollections(DataSet ds, Dictionary<long, List<LanguageContainer>> collsDescriptionsMapping, Dictionary<long, List<BundleCodeContainer>> collsChannelsMapping, Dictionary<long, List<LanguageContainer>> collsNamesMapping, string sCountryCd, string sLanguageCode, string sDeviceName)
        {
            DataTable dt = ds.Tables[0];
            List<Collection> res = null;
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                res = new List<Collection>(dt.Rows.Count);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    long lCollCode = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["ID"]);

                    LanguageContainer[] descs = null;

                    if (collsDescriptionsMapping.ContainsKey(lCollCode))
                    {
                        descs = collsDescriptionsMapping[lCollCode].ToArray();
                    }

                    BundleCodeContainer[] channels = null;

                    if (collsChannelsMapping.ContainsKey(lCollCode))
                    {
                        channels = collsChannelsMapping[lCollCode].ToArray();
                    }

                    LanguageContainer[] names = null;

                    if (collsNamesMapping.ContainsKey(lCollCode))
                    {
                        names = collsNamesMapping[lCollCode].ToArray();
                    }

                    res.Add(CreateCollectionObject(dt.Rows[i], sCountryCd, sLanguageCode, sDeviceName, descs, channels, names));
                }
            }
            else
            {
                res = new List<Collection>(0);
            }

            return res;
        }


        private string GetGetCollectionsDataErrMsg(string sMsg, List<long> lCollCodes)
        {
            StringBuilder sb = new StringBuilder(String.Concat(sMsg, ". "));
            if (lCollCodes != null && lCollCodes.Count > 0)
            {
                sb.Append("CollCodes: ");
                for (int i = 0; i < lCollCodes.Count; i++)
                {
                    sb.Append(String.Concat(lCollCodes, ","));
                }
            }
            else
            {
                sb.Append("CollCodes is null or empty.");
            }

            return sb.ToString();
        }

        private Dictionary<long, List<LanguageContainer>> ExtractCollectionsNames(DataSet ds)
        {
            Dictionary<long, List<LanguageContainer>> res = new Dictionary<long, List<LanguageContainer>>();
            DataTable dt = ds.Tables[3];
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    long lSubID = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[i]["COLLECTION_ID"]);
                    string sLanguageCode = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["language_code3"]);
                    string sDesc = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["description"]);
                    LanguageContainer lc = new LanguageContainer();
                    lc.Initialize(sLanguageCode, sDesc);
                    if (res.ContainsKey(lSubID))
                    {
                        res[lSubID].Add(lc);
                    }
                    else
                    {
                        res.Add(lSubID, new List<LanguageContainer>() { lc });
                    }
                }
            }

            return res;
        }

        private Dictionary<long, List<LanguageContainer>> ExtractCollectionsDescriptions(DataSet ds)
        {
            Dictionary<long, List<LanguageContainer>> res = new Dictionary<long, List<LanguageContainer>>();
            DataTable dt = ds.Tables[1];
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    long lSubID = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[i]["collection_id"]);
                    string sLanguageCode = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["language_code3"]);
                    string sDesc = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["description"]);
                    LanguageContainer lc = new LanguageContainer();
                    lc.Initialize(sLanguageCode, sDesc);
                    if (res.ContainsKey(lSubID))
                    {
                        res[lSubID].Add(lc);
                    }
                    else
                    {
                        res.Add(lSubID, new List<LanguageContainer>() { lc });
                    }
                }
            }

            return res;
        }

        private Dictionary<long, List<BundleCodeContainer>> ExtractCollectionsChannels(DataSet ds)
        {
            Dictionary<long, List<BundleCodeContainer>> res = new Dictionary<long, List<BundleCodeContainer>>();
            DataTable dt = ds.Tables[2];
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    long lSubID = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[i]["COLLECTION_ID"]);
                    long lChannelID = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[i]["CHANNEL_ID"]);
                    BundleCodeContainer bcc = new BundleCodeContainer();
                    bcc.Initialize(lChannelID + "", string.Empty);
                    if (res.ContainsKey(lSubID))
                    {
                        res[lSubID].Add(bcc);
                    }
                    else
                    {
                        res.Add(lSubID, new List<BundleCodeContainer>() { bcc });
                    }
                }
            }

            return res;
        }

        private bool IsCollsDataSetValid(DataSet ds)
        {
            return ds != null && ds.Tables != null && ds.Tables.Count == 4;
        }
    }
}
