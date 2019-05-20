using ApiObjects.Catalog;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Catalog.CatalogManagement;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tvinci.Core.DAL;

namespace ExcelManager
{
    // TODO SHIR - delete (+ ALL RELATED .CS) WHEN FINISH BEO-5472 
    public class ExcelManager
    {
        private static readonly string excelTemplateDir = string.Format("{0}ExcelTemplates\\", AppDomain.CurrentDomain.BaseDirectory);
       
        private ExcelManager()
        {
            Directory.CreateDirectory(excelTemplateDir);
        }

        #region Public Methods
        
       
        #endregion

       
    }
}
