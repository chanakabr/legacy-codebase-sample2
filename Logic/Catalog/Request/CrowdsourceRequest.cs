using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.CrowdsourceItems.Base;
using Core.Catalog.Response;
using Newtonsoft.Json;

namespace Core.Catalog.Request
{
    public class CrowdsourceRequest : BaseRequest, IRequestImp
    {
        public long LastDate { get; set; }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            CrowdsourceResponse response = null;
            List<BaseCrowdsourceItem> csItems = Crowdsourcing.GetCroudsourceItems(m_nGroupID, this.m_oFilter.m_nLanguage);
            if (LastDate != 0)
            {
                csItems = csItems.Where(item => item.TimeStamp < LastDate).ToList();
            }
            if (m_nPageSize != 0)
            {
                csItems = csItems.Take(this.m_nPageSize).ToList();
            }
            if (csItems != null && csItems.Count > 0)
            { 
                response = new CrowdsourceResponse()
                {
                    CrowdsourceItems = csItems
                };
            }

            return response;
        }
    }
}
