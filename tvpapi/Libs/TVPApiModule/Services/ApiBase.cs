﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TVPApi;

namespace TVPApiModule.Services
{
    public class ApiBase
    {
        private static List<KeyValuePair<string, string>> _Platforms = new List<KeyValuePair<string, string>>();
        private static object _locker = new object();

        public string this[string _plat]
        {
            get
            {
                if (_Platforms.Count == 0)
                {
                    lock (_locker)
                    {
                        if (_Platforms.Count == 0)
                        {
                            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery(string.Concat("Driver={SQL Server};Server=", TVinciDBConfiguration.GetConfig().DBServer,
                    ";Database=", TVinciDBConfiguration.GetConfig().DBInstance,
                    ";Uid=", TVinciDBConfiguration.GetConfig().User,
                    ";Pwd=", TVinciDBConfiguration.GetConfig().Pass,
                    ";"));

                            selectQuery += "select * from lu_platform";

                            DataTable dt = selectQuery.Execute("query", true);
                            if (dt != null)
                            {
                                Int32 nCount = dt.DefaultView.Count;
                                if (nCount > 0)
                                {
                                    foreach (DataRow item in dt.Rows)
                                    {
                                        _Platforms.Add(new KeyValuePair<string, string>(item["ID"].ToString(), item["Name"].ToString()));
                                    }
                                }
                            }


                            selectQuery.Finish();
                            selectQuery = null;
                        }
                    }
                }
                return (from platform in _Platforms
                        where platform.Value == _plat
                        select platform.Key).FirstOrDefault();
            }
        }
    }
}
