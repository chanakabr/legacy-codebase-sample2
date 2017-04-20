using ApiObjects.Pricing;
using KLogMonitor;
using KlogMonitorHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Core.Pricing
{
    public class TvinciCoupons : BaseCoupons
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const int BULK_TO_INSERT = 100;


        public TvinciCoupons(Int32 nGroupID): base(nGroupID)
        {
        }

        public override CouponsGroup[] GetCouponGroupListForAdmin(bool isVoucher)
        {
            CouponsGroup[] ret = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("pricing_connection");
                selectQuery += "select id from coupons_groups with (nolock) where is_active=1 and status=1 and START_DATE<getdate() and (end_date is null OR end_date>getdate()) and ";
                selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        ret = new CouponsGroup[nCount];
                    for (int i = 0; i < nCount; i++)
                    {
                        ret[i] = GetCouponGroupData(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
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
            return ret;
        }

        public override CouponsGroup[] GetCouponGroupListForAdmin()
        {
            return GetCouponGroupListForAdmin(false);

        }

        public override CouponsGroup GetCouponGroupData(string sCouponGroupID)
        {
            CouponsGroup tmp = null;
            Int32 nCouponGroupID = 0;
            try
            {
                nCouponGroupID = int.Parse(sCouponGroupID);
            }
            catch
            {
                return null;
            }
            tmp = new CouponsGroup();
            tmp.Initialize(nCouponGroupID, m_nGroupID);
            return tmp;
        }

        public override CouponData GetCouponStatus(string sCouponCode)
        {
            CouponData data = new CouponData();
            Coupon coupon = new Coupon();

            if (coupon.Initialize(sCouponCode, m_nGroupID))
            {
                CouponsGroup couponsGroup = coupon.GetCouponGroup(m_nGroupID);
                data.Initialize(sCouponCode, couponsGroup, coupon.GetCouponStatus(), coupon.m_couponType, coupon.m_campaignID, coupon.m_ownerGUID, coupon.m_ownerMedia);
            }
            else
            {
                data.m_CouponStatus = CouponsStatus.NotExists;
            }

            return data;
        }

        public override CouponsStatus SetCouponUsed(string sCouponCode, string sSiteGUID, Int32 nMFID, Int32 nSubCode, Int32 nCollectionCode, int nPrePaidCode)
        {
            return Coupon.SetCouponUsed(sCouponCode, m_nGroupID, sSiteGUID,nCollectionCode, nMFID, nSubCode, nPrePaidCode);
        }

        public override List<Coupon> GenerateCoupons(int numberOfCoupons, long couponGroupId)
        {
            List<Coupon> coupons = new List<Coupon>();

            try
            {
                // check that coupon groupId exsits 
                bool isCouponGroupId = DAL.PricingDAL.IsCouponGroupExsits(m_nGroupID, couponGroupId);
                if (!isCouponGroupId)
                {
                    log.ErrorFormat("fail GenerateCoupons coupon group not exsits groupId={0}, numberOfCoupons={1},couponGroupId={2} ", m_nGroupID, numberOfCoupons, couponGroupId);
                    return new List<Coupon>();
                }
                int bulkToInsert = Utils.GetIntValFromConfig("BULK_TO_INSERT_COUPON", BULK_TO_INSERT);
                int i;

                XmlDocument xmlDoc = new XmlDocument();
                XmlNode rootNode = xmlDoc.CreateElement("root");
                xmlDoc.AppendChild(rootNode);

                CouponGenerator p = new CouponGenerator();
                p.Maximum = 16;
                p.Minimum = 12;
                p.RepeatCharacters = false;
                string couponCode = string.Empty;

                for (i = 1; i <= numberOfCoupons; i++)
                {
                    // generate coupon code
                    couponCode = p.Generate();

                    // insert it to XML        
                    Utils.BuildCouponXML(rootNode, xmlDoc, couponCode, m_nGroupID, couponGroupId);

                    if (i % bulkToInsert == 0)
                    {
                        if (!InsertCoupons(couponGroupId, coupons, xmlDoc, rootNode))
                        {
                            return new List<Coupon>();
                        }
                        xmlDoc = new XmlDocument();
                        rootNode = xmlDoc.CreateElement("root");
                        xmlDoc.AppendChild(rootNode);
                    }
                }
                // check if still rows to insert
                if ((i - 1) % bulkToInsert > 0)
                {
                    if (!InsertCoupons(couponGroupId, coupons, xmlDoc, rootNode))
                    {
                        return new List<Coupon>();
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("fail GenerateCoupons groupId={0}, numberOfCoupons={1},couponGroupId={2}, ex={3} ", m_nGroupID, numberOfCoupons, couponGroupId, ex);
                coupons = new List<Coupon>();
            }
            return coupons;
        }

        private bool InsertCoupons(long couponGroupId, List<Coupon> coupons, XmlDocument xmlDoc, XmlNode rootNode, int retry = 0 )
        {
            bool result = false;
            try
            {
                // call dal to insert roes to DB 
                DataTable dt = DAL.PricingDAL.InsertCoupons(xmlDoc);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    coupons.AddRange(dt.AsEnumerable().Select(dr => new Coupon((int)dr.Field<Int64>("id"), dr.Field<string>("code"))).ToList());
                    result = true;                   
                }
                else
                {
                    log.ErrorFormat("fail to insert coupon bulk to DB groupId={0}, numberOfCoupons={1},couponGroupId={2},xmlDoc={3} ", m_nGroupID, couponGroupId, xmlDoc.InnerXml);
                    if (retry < 3)
                    {
                        InsertCoupons(couponGroupId, coupons, xmlDoc, rootNode, ++retry);
                    }                    
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("fail to insert coupon bulk to DB groupId={0}, couponGroupId={1},xmlDoc={2}, ex={3} ", m_nGroupID, couponGroupId, xmlDoc.InnerXml, ex);
            }

            return result;
        }
    }
}
