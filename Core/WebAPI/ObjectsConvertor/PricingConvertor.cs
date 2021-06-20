using ApiObjects;
using ApiObjects.Base;
using ApiObjects.SearchObjects;
using AutoMapper;
using Core.Catalog;
using Core.Catalog.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using TVinciShared;
using WebAPI.ClientManagers.Client;
using WebAPI.Models.Catalog;
using WebAPI.Models.Pricing;

namespace WebAPI.ObjectsConvertor
{
    public class PricingConvertor
    {
        internal static OrderObj ConvertOrderToOrderObj(KalturaPpvOrderBy orderBy)
        {
            OrderObj result = new OrderObj();

            // order results
            switch (orderBy)
            {
                case KalturaPpvOrderBy.NAME_DESC:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;

                case KalturaPpvOrderBy.NAME_ASC:
                default:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
            }

            return result;
        }
    }
}