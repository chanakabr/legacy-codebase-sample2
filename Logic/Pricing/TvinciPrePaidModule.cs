using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;

namespace Core.Pricing
{
    public class TvinciPrePaidModule : BasePrePaidModule
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public TvinciPrePaidModule()
        {
        }

        public TvinciPrePaidModule(int groupID)
            : base(groupID)
        {
        }


        public override PrePaidModule GetPrePaidModuleData(int nPrePaidModuleCode, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {

            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                PrePaidModule retVal = new PrePaidModule();
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from pre_paid_modules with (nolock) where is_active=1 and status=1 and ";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id" , "=" , m_nGroupID);
                selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_GroupID, "MAIN_CONNECTION_STRING");
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPrePaidModuleCode);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        string sPriceCode = selectQuery.Table("query").DefaultView[0].Row["PRICE_CODE"].ToString();
                        string sUsageModuleCode = selectQuery.Table("query").DefaultView[0].Row["USAGE_MODULE_CODE"].ToString();
                        string sDiscountModuleCode = selectQuery.Table("query").DefaultView[0].Row["DISCOUNT_MODULE_CODE"].ToString();
                        string sCouponGroupCode = selectQuery.Table("query").DefaultView[0].Row["COUPON_GROUP_CODE"].ToString();
                        string sCreditPriceCode = selectQuery.Table("query").DefaultView[0].Row["Value_Price_Code"].ToString();
                        string sName = string.Empty;
                        if (selectQuery.Table("query").DefaultView[0].Row["NAME"] != null &&
                            selectQuery.Table("query").DefaultView[0].Row["NAME"] != DBNull.Value)
                            sName = selectQuery.Table("query").DefaultView[0].Row["NAME"].ToString();
                        int nIsFixed = int.Parse(selectQuery.Table("query").DefaultView[0].Row["Is_Fixed_Price"].ToString());
                        bool isFixed = false;
                        if (nIsFixed == 1)
                            isFixed = true;
                        //Initialize object from DB info
                        retVal.Initialize(sPriceCode, sCreditPriceCode, sUsageModuleCode, sDiscountModuleCode, sCouponGroupCode, m_GroupID, nPrePaidModuleCode, isFixed, sName, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                        return retVal;
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetPrePaidModuleData. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" G ID: ", GroupID));
                sb.Append(String.Concat(" PP MC: ", nPrePaidModuleCode));
                sb.Append(String.Concat(" Cntry Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" D Nm: ", sDEVICE_NAME));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;

            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
            return null;
        }
    }
}
