using ApiObjects.Pricing;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
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
        private const string COUPON_GROUP_NOT_EXIST = "Coupon group doesn't exist";
        private const string FAILED_ERROR_FORMAT = "failed to {0}";
        private const string NAME_REQUIRED = "Name must have a value";
        private const string COUPON_CODE_NOT_IN_THE_RIGHT_LENGTH = "The Coupon code provided is not valid.(does not match the required number of digits).";
        private const string DISCOUNT_CODE_NOT_EXIST = "Discount code doen't exist";        

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

                if (string.IsNullOrEmpty(couponCode) || couponCode.Length > 50)
                {
                    log.ErrorFormat("fail GeneratePublicCode coupon code not in the right length groupId={0}, couponCode={1},couponGroupId={2} ", m_nGroupID, couponCode, couponGroupId);
                    status.Code = (int)eResponseStatus.CouponCodeNotInTheRightLength;
                    status.Message = COUPON_CODE_NOT_IN_THE_RIGHT_LENGTH;
                    return coupons;
                }

                couponCode = couponCode.Trim();

                if (couponCode.Contains(" "))
                {
                    log.ErrorFormat("fail GeneratePublicCode. coupon code should not have space. groupId={0}, couponCode={1},couponGroupId={2} ", m_nGroupID, couponCode, couponGroupId);
                    status.Code = (int)eResponseStatus.Error;
                    status.Message = "The Coupon code should not have spaces.";
                    return coupons;
                }

                // check that couponCode doesn't exists 
                bool isCouponcouponCodeExist = DAL.PricingDAL.IsCouponCodeExists(m_nGroupID, couponCode);
                if (isCouponcouponCodeExist)
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
                else
                {
                    response.Status = new Status((int)eResponseStatus.CouponGroupNotExist, COUPON_GROUP_NOT_EXIST);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to get coupons group, Id = {0}", couponsGroupId), ex);
            }

            return response;
        }

        public override CouponsGroupResponse UpdateCouponsGroup(int groupId, long id, string name, DateTime? startDate, DateTime? endDate,
            int? maxUsesNumber, int? maxUsesNumberOnRenewableSub, int? maxHouseholdUses, CouponGroupType? couponGroupType, long? discountCode)
        {
            CouponsGroupResponse response = new CouponsGroupResponse()
            {
                CouponsGroup = new CouponsGroup(),
                Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() }
            };
            try
            {
                //check couponsGroup exists
                if (!response.CouponsGroup.Initialize(Convert.ToInt32(id), groupId))
                {
                    response.Status.Code = (int)eResponseStatus.CouponGroupNotExist;
                    response.Status.Message = COUPON_GROUP_NOT_EXIST;
                    return response;
                }

                if (discountCode.HasValue)
                {
                    // check that discount code exists 
                    bool isDiscountCodeExsits = DAL.PricingDAL.IsDiscountCodeExists(m_nGroupID, discountCode.Value);
                    if (!isDiscountCodeExsits)
                    {
                        response.Status.Code = (int)eResponseStatus.DiscountCodeNotExist;
                        response.Status.Message = DISCOUNT_CODE_NOT_EXIST;
                        return response;
                    }
                }

                int maxUsesNumberToUpdate = maxUsesNumber.HasValue ? maxUsesNumber.Value : response.CouponsGroup.m_nMaxUseCountForCoupon;
                int maxHouseholdUsesToUpdate = maxHouseholdUses.HasValue ? maxHouseholdUses.Value : response.CouponsGroup.maxDomainUses;

                if(maxHouseholdUsesToUpdate > maxUsesNumberToUpdate)
                {
                    response.Status.Code = (int)eResponseStatus.Error;
                    response.Status.Message = "maxHouseholdUses value conflicts maxUsesNumber value";
                    return response;
                }

                DataTable dt = PricingDAL.UpdateCouponsGroup(groupId, id, name, startDate, endDate,
                    maxUsesNumber, maxUsesNumberOnRenewableSub, maxHouseholdUses, couponGroupType, discountCode);
                if (dt == null || dt.Rows.Count == 0)
                {
                    log.ErrorFormat("Error while UpdateCouponsGroup. groupId: {0}, CoupunGroupId: {1}", groupId, id);
                    return response;
                }

                string invalidationKey = LayeredCacheKeys.GetCouponsGroupInvalidationKey(groupId, id);
                if (!CachingProvider.LayeredCache.LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key for CouponsGroupInvalidationKey . key = {0}", invalidationKey);
                }

                invalidationKey = LayeredCacheKeys.GetCouponsGroupsInvalidationKey(groupId);
                if (!CachingProvider.LayeredCache.LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key for CouponsGroupsInvalidationKey. key = {0}", invalidationKey);
                }

                response.CouponsGroup = CreateCouponsGroup(dt.Rows[0]);
                response.Status.Code = (int)eResponseStatus.OK;
                response.Status.Message = eResponseStatus.OK.ToString();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UpdateCouponsGroup failed ex={0}, groupId={1}, couponGroupId={2}", ex, groupId, id);
            }

            return response;
        }

        private CouponsGroup CreateCouponsGroup(DataRow dataRow)
        {
            CouponsGroup couponsGroup = new CouponsGroup()
            {
                m_sGroupCode = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "ID").ToString(),
                m_dStartDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow, "START_DATE"),
                m_dEndDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow, "END_DATE"),
                m_nMaxUseCountForCoupon = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "MAX_USE_TIME"),
                m_sGroupName = ODBCWrapper.Utils.GetSafeStr(dataRow, "CODE"),
                m_nMaxRecurringUsesCountForCoupon = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "MAX_RECURRING_USES"),
                couponGroupType = (CouponGroupType)ODBCWrapper.Utils.GetIntSafeVal(dataRow, "COUPON_GROUP_TYPE"),
                maxDomainUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "DOMAIN_MAX_USES"),
                m_sDiscountCode = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "DISCOUNT_CODE").ToString(),
            };

            return couponsGroup;
        }

        public override CouponsGroupsResponse GetCouponGroups()
        {
            CouponsGroupsResponse response = new CouponsGroupsResponse();
            List<CouponsGroup> couponsGroups = new List<CouponsGroup>();

            string key = LayeredCacheKeys.GetCouponsGroupsKey(m_nGroupID);
            if (!LayeredCache.Instance.Get<List<CouponsGroup>>(key, ref couponsGroups, GetCouponsGroups, new Dictionary<string, object>() { { "groupId", m_nGroupID } },
                m_nGroupID, LayeredCacheConfigNames.GET_COUPONS_GROUP, new List<string>() { LayeredCacheKeys.GetCouponsGroupsInvalidationKey(m_nGroupID) }))
            {
                log.ErrorFormat("Failed coupons groups from LayeredCache, key: {0}", key);
            }
            else
            {
                response.CouponsGroups = couponsGroups;
                response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return response;
        }

        private static Tuple<List<CouponsGroup>, bool> GetCouponsGroups(Dictionary<string, object> funcParams)
        {
            bool result = false;
            List<CouponsGroup> couponsGroups = new List<CouponsGroup>();

            try
            {
                if (funcParams != null && funcParams.Count == 1)
                {
                    if (funcParams.ContainsKey("groupId"))
                    {
                        int? groupId = funcParams["groupId"] as int?;

                        if (groupId.HasValue)
                        {
                            DataTable dt = PricingDAL.GetGroupCouponsGroups(groupId.Value);

                            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                            {
                                CouponsGroup couponsGroup = null;
                                foreach (DataRow row in dt.Rows)
                                {
                                    string sDiscountCode = ODBCWrapper.Utils.GetSafeStr(row, "DISCOUNT_CODE");
                                    BaseDiscount t = null;
                                    Utils.GetBaseImpl(ref t, groupId.Value);

                                    couponsGroup = new CouponsGroup()
                                    {
                                        m_oDiscountCode = t.GetDiscountCodeData(sDiscountCode),
                                        m_dStartDate = ODBCWrapper.Utils.GetDateSafeVal(row, "START_DATE"),
                                        m_dEndDate = ODBCWrapper.Utils.GetDateSafeVal(row, "END_DATE"),
                                        m_nMaxUseCountForCoupon = ODBCWrapper.Utils.GetIntSafeVal(row, "MAX_USE_TIME"),
                                        m_sGroupName = ODBCWrapper.Utils.GetSafeStr(row, "CODE"),
                                        m_nFinancialEntityID = ODBCWrapper.Utils.GetIntSafeVal(row, "FINANCIAL_ENTITY_ID"),
                                        m_nMaxRecurringUsesCountForCoupon = ODBCWrapper.Utils.GetIntSafeVal(row, "MAX_RECURRING_USES"),
                                        couponGroupType = (CouponGroupType)ODBCWrapper.Utils.GetIntSafeVal(row, "COUPON_GROUP_TYPE"),
                                        maxDomainUses = ODBCWrapper.Utils.GetIntSafeVal(row, "DOMAIN_MAX_USES"),
                                        m_sGroupCode = ODBCWrapper.Utils.GetSafeStr(row, "ID"),
                                        m_sDiscountCode = sDiscountCode
                                    };

                                    couponsGroups.Add(couponsGroup);
                                }
                                result = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetCouponsGroups failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<CouponsGroup>, bool>(couponsGroups, result);
        }

        public override Status DeleteCouponsGroups(int groupId, long id)
        {
            Status status = null; new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            CouponsGroup tmp = new CouponsGroup();
            try
            {
                //check couponsGroup exists
                if (!tmp.Initialize(Convert.ToInt32(id), groupId))
                {
                    return new Status((int)eResponseStatus.CouponGroupNotExist, COUPON_GROUP_NOT_EXIST);
                }

                int res = PricingDAL.DeleteCouponsGroup(groupId, id);
                if (res == 0)
                {
                    return new Status((int)eResponseStatus.Error, string.Format(FAILED_ERROR_FORMAT, "delete"));
                }
                else if (res == -1)
                {
                    return new Status((int)eResponseStatus.CouponGroupNotExist, COUPON_GROUP_NOT_EXIST);
                }
                else
                {
                    status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }

                string invalidationKey = LayeredCacheKeys.GetCouponsGroupInvalidationKey(groupId, id);
                if (!CachingProvider.LayeredCache.LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key for CouponsGroupInvalidationKey . key = {0}", invalidationKey);
                }

                invalidationKey = LayeredCacheKeys.GetCouponsGroupsInvalidationKey(groupId);
                if (!CachingProvider.LayeredCache.LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key for CouponsGroupsInvalidationKey. key = {0}", invalidationKey);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("DeleteCouponsGroup failed ex={0}, groupId={1}, couponGroupId={2}", ex, groupId, id);
            }

            return status;
        }

        public override CouponsGroupResponse AddCouponsGroup(int groupId, string name, DateTime? startDate, DateTime? endDate, int? maxUsesNumber,
            int? maxUsesNumberOnRenewableSub, int? maxHouseholdUses, CouponGroupType? couponGroupType, long? discountCode)
        {
            CouponsGroupResponse response = new CouponsGroupResponse()
            {
                CouponsGroup = new CouponsGroup(),
                Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() }
            };
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    response.Status.Code = (int)eResponseStatus.NameRequired;
                    response.Status.Message = NAME_REQUIRED;
                    return response;
                }

                if (discountCode.HasValue)
                {
                    // check that discount code exists 
                    bool isDiscountCodeExsits = DAL.PricingDAL.IsDiscountCodeExists(m_nGroupID, discountCode.Value);
                    if (!isDiscountCodeExsits)
                    {
                        response.Status.Code = (int)eResponseStatus.DiscountCodeNotExist;
                        response.Status.Message = DISCOUNT_CODE_NOT_EXIST;
                        return response;
                    }
                }

                DataTable dt = PricingDAL.AddCouponsGroup(groupId, name, startDate, endDate,
                    maxUsesNumber, maxUsesNumberOnRenewableSub, maxHouseholdUses, couponGroupType, discountCode);
                if (dt == null || dt.Rows.Count == 0)
                {
                    log.ErrorFormat("Error while AddCouponsGroup. groupId: {0}, name: {1}", groupId, name);
                    return response;
                }

                response.CouponsGroup = CreateCouponsGroup(dt.Rows[0]);
                response.Status.Code = (int)eResponseStatus.OK;
                response.Status.Message = eResponseStatus.OK.ToString();

                string invalidationKey = LayeredCacheKeys.GetCouponsGroupInvalidationKey(groupId, int.Parse(response.CouponsGroup.m_sGroupCode));
                if (!CachingProvider.LayeredCache.LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key for CouponsGroupInvalidationKey . key = {0}", invalidationKey);
                }

                invalidationKey = LayeredCacheKeys.GetCouponsGroupsInvalidationKey(groupId);
                if (!CachingProvider.LayeredCache.LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key for CouponsGroupsInvalidationKey. key = {0}", invalidationKey);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("AddCouponsGroup failed ex={0}, groupId={1}, name={2}", ex, groupId, name);
            }

            return response;
        }
    }
}