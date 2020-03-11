using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.CrowdsourceItems.Base;
using DAL;
using Tvinci.Core.DAL;

namespace Core.Catalog
{
    public static class Crowdsourcing
    {
        public static List<BaseCrowdsourceItem> GetCroudsourceItems(int groupId, int languageId)
        {
            return DAL.CrowdsourceDAL.GetCsList(groupId, languageId);
        }
    }
}
