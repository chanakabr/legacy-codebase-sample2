using ApiObjects.Pricing;
using ApiObjects.Response;
using DAL;
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
        private const string COUPON_GROUP_NOT_FOUND = "Coupon group identifier wasn't found";
        private const string COUPON_CODE_ALREADY_EXISTS = "Coupon code already exist";



        public TvinciCoupons(Int32 nGroupID) : base(nGroupID)
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

            Int32.TryParse(sCouponGroupID, out nCouponGroupID);

            if (nCouponGroupID > 0)
            {
                tmp = new CouponsGroup();
                tmp.Initialize(nCouponGroupID, m_nGroupID);
            }
                return tmp;
        }

        public override CouponData GetCouponStatus(string sCouponCode, long domainId)
        {
            CouponData data = new CouponData();
            Coupon coupon = new Coupon();
            int? totalUses = null, leftUses = null;

            if (coupon.Initialize(sCouponCode, m_nGroupID))
            {
                CouponsGroup couponsGroup = CouponsGroup.GetCouponsGroup(coupon.couponsGroupId, m_nGroupID);

                CouponsStatus status = coupon.GetCouponStatus(m_nGroupID, couponsGroup);
                if (status == CouponsStatus.Valid)
                {
                    int uses = coupon.useCount;
                    if (couponsGroup.maxDomainUses > 0)
                    {
                        totalUses = couponsGroup.maxDomainUses;
                        int domainUses = PricingDAL.GetCouponDomainUses(m_nGroupID, coupon.m_nCouponID, domainId);
                        if (domainUses >= couponsGroup.maxDomainUses)
                        {
                            status = CouponsStatus.AllreadyUsed;
                        }
                        else
                        {
                            leftUses = couponsGroup.maxDomainUses - domainUses;
                        }
                    }
                }

                data.Initialize(sCouponCode, couponsGroup, status, coupon.m_couponType, coupon.m_campaignID, coupon.m_ownerGUID, coupon.m_ownerMedia, leftUses, totalUses);
            }
            else
            {
                data.m_CouponStatus = CouponsStatus.NotExists;
            }

            return data;
        }

        public override CouponsStatus SetCouponUsed(string sCouponCode, string sSiteGUID, Int32 nMFID, Int32 nSubCode, Int32 nCollectionCode, int nPrePaidCode, long domainId)
        {
            return Coupon.SetCouponUsed(sCouponCode, m_nGroupID, sSiteGUID, nCollectionCode, nMFID, nSubCode, nPrePaidCode, domainId);
        }

        public override List<Coupon> GenerateCoupons(int numberOfCoupons, long couponGroupId, out ApiObjects.Response.Status status, bool useLetters = true, bool useNumbers = true, bool useSpecialCharacters = true)
        {
            status = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
            List<Coupon> coupons = new List<Coupon>();

            try
            {
                // check that coupon groupId exsits 
                bool isCouponGroupId = DAL.PricingDAL.IsCouponGroupExsits(m_nGroupID, couponGroupId);
                if (!isCouponGroupId)
                {
                    log.ErrorFormat("fail GenerateCoupons coupon group not exists groupId={0}, numberOfCoupons={1},couponGroupId={2} ", m_nGroupID, numberOfCoupons, couponGroupId);
                    status.Code = (int)eResponseStatus.InvalidCouponGroup;
                    status.Message = COUPON_GROUP_NOT_FOUND;
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
                p.UseLetters = useLetters;
                p.UseNumbers = useNumbers;
                p.ExcludeSymbols = !useSpecialCharacters;

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
            status.Code = (int)eResponseStatus.OK;
            status.Message = eResponseStatus.OK.ToString();
            return coupons;
        }

        private bool InsertCoupons(long couponGroupId, List<Coupon> coupons, XmlDocument xmlDoc, XmlNode rootNode, int retry = 0)
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

        public override CouponDataResponse ValidateCouponForSubscription(int groupId, int subscriptionId, string couponCode, long domainId)
        {
            CouponDataResponse response = new CouponDataResponse();
            try
            {
                ApiObjects.Response.Status validateCoupon = Utils.ValidateCouponForSubscription((long)subscriptionId, groupId, couponCode);
                response.Status = validateCoupon;

                if (response.Status.Code != (int)eResponseStatus.OK)
                {
                    return response;
                }
                else
                {
                    response.Coupon = GetCouponStatus(couponCode, domainId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("fail to Validate Coupon For Subscription groupId={0}, subscriptionId={1},couponCode={2}, ex={3} ", m_nGroupID, subscriptionId, couponCode, ex);
            }

            return response;
        }

        public override List<Coupon> GeneratePublicCode(int groupId, long couponGroupId, string couponCode, out ApiObjects.Response.Status status)
        {
            status = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
            List<Coupon> coupons = new List<Coupon>();

            try
            {
                // check that coupon groupId exists 
                bool isCouponGroupId = DAL.PricingDAL.IsCouponGroupExsits(m_nGroupID, couponGroupId);
                if (!isCouponGroupId)
                {
                    log.ErrorFormat("fail GeneratePublicCode coupon group not exists groupId={0}, couponCode={1},couponGroupId={2} ", m_nGroupID, couponCode, couponGroupId);
                    status.Code = (int)eResponseStatus.InvalidCouponGroup;
                    status.Message = COUPON_GROUP_NOT_FOUND;
                    return coupons;
                }

                // check that couponCode doesn't exists 
                bool isCouponcouponCodeExist = DAL.PricingDAL.IsCouponCodeExists(m_nGroupID, couponCode);
                if (!isCouponcouponCodeExist)
                {
                    log.ErrorFormat("fail GeneratePublicCode coupon group not exists groupId={0}, couponCode={1},couponGroupId={2} ", m_nGroupID, couponCode, couponGroupId);
                    status.Code = (int)eResponseStatus.CouponCodeAlreadyExists;
                    status.Message = COUPON_CODE_ALREADY_EXISTS;
                    return coupons;
                }

                XmlDocument xmlDoc = new XmlDocument();
                XmlNode rootNode = xmlDoc.CreateElement("root");
                xmlDoc.AppendChild(rootNode);

                // insert it to XML        
                Utils.BuildCouponXML(rootNode, xmlDoc, couponCode, m_nGroupID, couponGroupId);

                if (!InsertCoupons(couponGroupId, coupons, xmlDoc, rootNode))
                {
                    return coupons;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("fail GeneratePublicCode groupId={0}, numberOfCoupons={1},couponCode={2}, ex={3} ", m_nGroupID, couponCode, couponGroupId, ex);
            }

            status.Code = (int)eResponseStatus.OK;
            status.Message = eResponseStatus.OK.ToString();
            return coupons;
        }

        public override CouponsGroupResponse GetCouponGroupData(long couponsGroupId)
        {
            CouponsGroupResponse response = new CouponsGroupResponse();
            try
            {
                response.CouponsGroup = new CouponsGroup();
                if (response.CouponsGroup.Initialize((int)couponsGroupId, m_nGroupID))
                {
                    response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to get coupons group, Id = {0}", couponsGroupId), ex);
            }

            return response;
        }
    }
}
