using ApiObjects.Base;
using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiLogic.Catalog
{
    public class CategoryItemFilter : ICrudFilter
    {
    }

    public class CategoryItemByIdInFilter : CategoryItemFilter
    {
        public string IdIn { get; set; }

        public List<long> GetIdIn()
        {
            List<long> list = new List<long>();
            if (!string.IsNullOrEmpty(IdIn))
            {
                string[] stringValues = IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    int value;
                    if (int.TryParse(stringValue, out value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new Exception("CategoryItemByIdInFilter.idIn is invalid");
                    }
                }
            }

            return list;
        }
    }



    public class CategoryItemByKsqlRootFilter : CategoryItemFilter
    {
        public string Ksql { get; set; }

        public bool RootOnly { get; set; }
    }
}