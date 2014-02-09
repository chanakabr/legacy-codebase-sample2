// Copyright (c) iucon GmbH. All rights reserved.
// For more information about our work, visit http://www.iucon.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Reflection;
using System.Collections;

namespace iucon.web.Controls
{
    class PageRequestManager
    {
        private object _pageRequestManager = null;

        public PageRequestManager(ScriptManager scriptManager)
        {
            PropertyInfo property = scriptManager.GetType().GetProperty("PageRequestManager", BindingFlags.NonPublic | BindingFlags.Instance);
            _pageRequestManager = property.GetValue(scriptManager, null);
        }

        public ScriptDataItem[] ScriptDataItems
        {
            get
            {
                if (_pageRequestManager != null)
                {
                    FieldInfo field = _pageRequestManager.GetType().GetField("_scriptDataItems", BindingFlags.NonPublic | BindingFlags.Instance);
                    object scriptDataItems = field.GetValue(_pageRequestManager);

                    if (scriptDataItems != null)
                    {
                        object[] array = new object[((ICollection)scriptDataItems).Count];
                        ((ICollection)scriptDataItems).CopyTo(array, 0);

                        return (from script in array select new ScriptDataItem(script)).ToArray();
                    }
                }

                return new ScriptDataItem[] { };
            }
        }
    }


    class ScriptDataItem
    {
        public string ClientID
        {
            get;
            set;
        }

        public string DataItem
        {
            get;
            set;
        }

        public bool IsJsonSerialized
        {
            get;
            set;
        }

        public ScriptDataItem(object dataItem)
        {
            PropertyInfo property = dataItem.GetType().GetProperty("Control", BindingFlags.Instance | BindingFlags.Public);
            Control control = (Control)property.GetValue(dataItem, null);
            ClientID = control.ClientID;

            property = dataItem.GetType().GetProperty("DataItem", BindingFlags.Instance | BindingFlags.Public);
            DataItem = (string)property.GetValue(dataItem, null);

            property = dataItem.GetType().GetProperty("IsJsonSerialized", BindingFlags.Instance | BindingFlags.Public);
            IsJsonSerialized = (bool)property.GetValue(dataItem, null);
        }
    }
}
