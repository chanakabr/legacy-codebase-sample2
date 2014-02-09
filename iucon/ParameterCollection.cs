// Copyright (c) iucon GmbH. All rights reserved.
// For more information about our work, visit http://www.iucon.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Web;
using System.Web.Script.Serialization;

namespace iucon.web.Controls
{
    public class ParameterCollection : Collection<Parameter>
    {
        public string this[string name]
        {
            get
            {
                var result = (from p in Items where p.Name == name select p).FirstOrDefault();
                if (result != null) return result.Value;

                return null;
            }
            set
            {
                var result = (from p in Items where p.Name == name select p).FirstOrDefault();
                if (result != null) 
                    result.Value = value;
            }
        }

        public ParameterCollection() :this(null)
        {

        }

        /// <summary>
        /// Initialize the collection with support to access parameters from sever
        /// </summary>
        /// <param name="controlPage"></param>
        public ParameterCollection(System.Web.UI.Page controlPage)
        {
            if (HttpContext.Current != null &&
                !string.IsNullOrEmpty(HttpContext.Current.Request.Form["__PARAMETERS"]))
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Parameter[] parameters = serializer.Deserialize<Parameter[]>(HttpContext.Current.Request.Form["__PARAMETERS"]);                
                if (parameters != null && parameters.Length > 0)
                {
                    foreach (Parameter p in parameters)
                        Add(p);
                }
            }

            syncForServerside(controlPage);
        }

        private void syncForServerside(System.Web.UI.Page controlPage)
        {
            PanelHostPage host = controlPage as PanelHostPage;

            if (host != null && host.Parameters != null && !controlPage.IsPostBack)
            {
                foreach(Parameter p in host.Parameters)
                {
                    Add(p);
                }                
            }
        }
    }
}
