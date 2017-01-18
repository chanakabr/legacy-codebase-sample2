using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects;
using System.Data;
using DAL;
using ApiObjects.Pricing;

namespace Core.Pricing
{
    [Serializable]
    public class Collection : PPVModule
    {
        #region Member
        //The codes which identify which medias are relevant to the subscription (int Tvinci it is the channels)
        public BundleCodeContainer[] m_sCodes;
        public DateTime m_dStartDate;
        public DateTime m_dEndDate;
        public Int32[] m_sFileTypes;
        public PriceCode m_oCollectionPriceCode;
        public DiscountModule m_oExtDisountModule;
        public LanguageContainer[] m_sName;
        public UsageModule m_oCollectionUsageModule;
        public int m_fictivicMediaID;
        public string m_ProductCode = string.Empty;
        public string m_CollectionCode;

        #endregion

        #region Ctr
        public Collection()
            : base()
        {
            m_sName = null;
            m_oCollectionUsageModule = null;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Get Fictivic Media ID
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="colCode"></param>
        private void GetFictivicMediaID(int groupID, int colecctionCode)
        {
            int fictivicGroupID = 0;
            int fictivicMediaID = 0;
            string paramName = string.Empty;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            ODBCWrapper.DataSetSelectQuery mediaSelectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select FICTIVIC_MEDIA_META_NAME, FICTIVIC_GROUP_ID from groups_parameters with (nolock)";
                selectQuery += " where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", groupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        paramName = selectQuery.Table("query").DefaultView[0].Row["FICTIVIC_MEDIA_META_NAME"].ToString();
                        fictivicGroupID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["FICTIVIC_GROUP_ID"].ToString());
                    }
                }

                if (fictivicGroupID > 0)
                {
                    mediaSelectQuery = new ODBCWrapper.DataSetSelectQuery();
                    mediaSelectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                    mediaSelectQuery += "select id from media";
                    mediaSelectQuery += " where ";
                    mediaSelectQuery += ODBCWrapper.Parameter.NEW_PARAM(paramName, "=", colecctionCode);
                    mediaSelectQuery += " and ";
                    mediaSelectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", fictivicGroupID);
                    if (mediaSelectQuery.Execute("query", true) != null)
                    {
                        int count = mediaSelectQuery.Table("query").DefaultView.Count;
                        if (count > 0)
                        {
                            fictivicMediaID = int.Parse(mediaSelectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                        }
                    }
                }
                m_fictivicMediaID = fictivicMediaID;
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
                if (mediaSelectQuery != null)
                {
                    mediaSelectQuery.Finish();
                }
            }
        }

        public void Initialize(PriceCode oPriceCode, UsageModule oUsageModule,
            DiscountModule oDiscountModule, CouponsGroup oCouponsGroup, LanguageContainer[] sDescriptions,
            string sCollectionCode, BundleCodeContainer[] sCodes, DateTime dStart, DateTime dEnd,
            Int32[] sFileTypes, LanguageContainer[] sName, PriceCode colPriceCode, UsageModule oColUsageModule, string sObjectVirtualName)
        {
            Initialize(0, oPriceCode, oUsageModule,
            oDiscountModule, oCouponsGroup, sDescriptions,
            sCollectionCode, sCodes, dStart, dEnd,
            sFileTypes, sName, colPriceCode, oColUsageModule, sObjectVirtualName);
        }

        public void Initialize(PriceCode oPriceCode, UsageModule oUsageModule,
            DiscountModule oDiscountModule, DiscountModule extDisountModule, CouponsGroup oCouponsGroup, LanguageContainer[] sDescriptions,
            string sCollectionCode, BundleCodeContainer[] sCodes, DateTime dStart, DateTime dEnd,
            Int32[] sFileTypes, bool bIsRecurring, Int32 nNumOfRecPeriods, LanguageContainer[] sName, PriceCode colPriceCode, UsageModule oColUsageModule, string sObjectVirtualName)
        {
            Initialize(0, oPriceCode, oUsageModule,
            oDiscountModule, oCouponsGroup, sDescriptions,
            sCollectionCode, sCodes, dStart, dEnd,
            sFileTypes, sName, colPriceCode, oColUsageModule, sObjectVirtualName);
            m_oExtDisountModule = extDisountModule;
        }

        public void Initialize(Int32 nGroupID, PriceCode oPriceCode, UsageModule oUsageModule,
            DiscountModule oDiscountModule, CouponsGroup oCouponsGroup, LanguageContainer[] sDescriptions,
            string sCollectionCode, BundleCodeContainer[] sCodes, DateTime dStart, DateTime dEnd,
            Int32[] sFileTypes, LanguageContainer[] sName, PriceCode colPriceCode, UsageModule oColUsageModule, string sObjectVirtualName)
        {
            base.Initialize(oPriceCode, oUsageModule, oDiscountModule, oCouponsGroup, sDescriptions,
                sCollectionCode, false, sObjectVirtualName, null, false);

            m_CollectionCode = sCollectionCode;
            m_ProductCode = "";
            m_sCodes = sCodes;
            m_dStartDate = dStart;
            m_dEndDate = dEnd;
            m_sFileTypes = sFileTypes;
            m_sName = sName;
            m_oCollectionPriceCode = colPriceCode;
            m_oCollectionUsageModule = oColUsageModule;
            GetFictivicMediaID(nGroupID, int.Parse(sCollectionCode));
        }

        public void Initialize(string sPriceCode, string sUsageModuleCode,
            string sDiscountModuleCode, string sCouponGroupCode, LanguageContainer[] sDescriptions, Int32 nGroupID,
            string sCollectionCode, BundleCodeContainer[] sCodes, DateTime dStart, DateTime dEnd,
            Int32[] sFileTypes, LanguageContainer[] sName, string colPriceCode,
            string sColUsageModule, string sObjectVirtualName,
            string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            base.Initialize(sPriceCode, sUsageModuleCode, sDiscountModuleCode, sCouponGroupCode,
                sDescriptions, nGroupID, sCollectionCode, false, sObjectVirtualName,
                sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, null, false, string.Empty);

            if (sColUsageModule != "")
            {
                BaseUsageModule um = null;
                Utils.GetBaseImpl(ref um, nGroupID);
                if (um != null)
                    m_oCollectionUsageModule = um.GetUsageModuleData(sColUsageModule);
                else
                    m_oCollectionUsageModule = null;
            }
            else
                m_oCollectionUsageModule = null;

            m_CollectionCode = sCollectionCode;
            m_ProductCode = "";
            m_sCodes = sCodes;
            m_dStartDate = dStart;
            m_dEndDate = dEnd;
            m_sFileTypes = sFileTypes;
            m_sName = sName;

            GetFictivicMediaID(nGroupID, int.Parse(sCollectionCode));

            if (colPriceCode != "")
            {
                BasePricing p = null;
                Utils.GetBaseImpl(ref p, nGroupID);
                if (p != null)
                    m_oCollectionPriceCode = p.GetPriceCodeData(colPriceCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                else
                    m_oCollectionPriceCode = null;
            }
            else
                m_oCollectionPriceCode = null;
        }

        public void Initialize(string sPriceCode, string sUsageModuleCode,
           string sDiscountModuleCode, string sCouponGroupCode, LanguageContainer[] sDescriptions, Int32 nGroupID,
           string sCollectionCode, BundleCodeContainer[] sCodes, DateTime dStart, DateTime dEnd,
           Int32[] sFileTypes, LanguageContainer[] sName, string colPriceCode,
           string sColUsageModule, string sObjectVirtualName,
           string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sProductCode, string sExtDiscount)
        {
            base.Initialize(sPriceCode, sUsageModuleCode, sDiscountModuleCode, sCouponGroupCode,
                sDescriptions, nGroupID, sCollectionCode, false, sObjectVirtualName,
                sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, null, false, string.Empty);

            if (sColUsageModule != "")
            {
                BaseUsageModule um = null;
                Utils.GetBaseImpl(ref um, nGroupID);
                if (um != null)
                    m_oCollectionUsageModule = um.GetUsageModuleData(sColUsageModule);
                else
                    m_oCollectionUsageModule = null;
            }
            else
                m_oCollectionUsageModule = null;

            m_CollectionCode = sCollectionCode;
            m_ProductCode = sProductCode;
            m_sCodes = sCodes;
            m_dStartDate = dStart;
            m_dEndDate = dEnd;
            m_sFileTypes = sFileTypes;
            m_sName = sName;

            GetFictivicMediaID(nGroupID, int.Parse(sCollectionCode));

            if (colPriceCode != "")
            {
                BasePricing p = null;
                Utils.GetBaseImpl(ref p, nGroupID);
                if (p != null)
                    m_oCollectionPriceCode = p.GetPriceCodeData(colPriceCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                else
                    m_oCollectionPriceCode = null;
            }
            else
                m_oCollectionPriceCode = null;


            if (!string.IsNullOrEmpty(sExtDiscount))
            {
                BaseDiscount d = null;
                Utils.GetBaseImpl(ref d, nGroupID);
                if (d != null)
                    m_oExtDisountModule = d.GetDiscountCodeData(sExtDiscount);
                else
                    m_oExtDisountModule = null;
            }
            else
                m_oExtDisountModule = null;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Collection. ");
            sb.Append(String.Concat(" PPV Obj Code: ", m_sObjectCode != null ? m_sObjectCode : "null"));
            sb.Append(String.Concat(" Coll Code: ", m_CollectionCode != null ? m_CollectionCode : "null"));

            return sb.ToString();
        }

        #endregion

    }
}

