// Copyright (c) iucon GmbH. All rights reserved.
// For more information about our work, visit http://www.iucon.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Collections.Specialized;
using System.Web;

namespace iucon.web.Controls
{
    internal class ScriptRenderer
    {
        #region ScriptBlock

        class ScriptBlock
        {
            public string Type { get; set; }
            public string Script { get; set; }
            public string Url { get; set; }
            public string ClientID { get; set; }            
        }

        #endregion

        private string _controlClientID = null;
        private ScriptManager _scriptManager = null;

        public ScriptRenderer(ScriptManager ScriptManager, string ControlClientID)
        {
            _controlClientID = ControlClientID;
            _scriptManager = ScriptManager;
        }

        /// <summary>
        /// Renders all registered JScript elements into a hidden div
        /// The contents of the hidden div are executed with Sys._ScriptLoader
        /// </summary>
        public string GetScriptBlock()
        {
            List<ScriptBlock> list = new List<ScriptBlock>();
            foreach (RegisteredScript script in _scriptManager.GetRegisteredStartupScripts())
                if (script.AddScriptTags)
                    list.Add(AppendScriptBlock("scriptStartupBlock", script));

            List<string> scriptUrls = new List<string>();
            foreach (RegisteredScript script in _scriptManager.GetRegisteredClientScriptBlocks())
            {
                if (script.AddScriptTags)
                    list.Add(AppendScriptBlock("scriptBlock", script));
                else if (script.ScriptType == RegisteredScriptType.ClientScriptInclude)
                {
                    // prevent adding the same script-Url multiple times
                    if (!scriptUrls.Contains(script.Url))
                    {
                        list.Add(AppendScriptBlock("clientScriptInclude", script));
                        scriptUrls.Add(script.Url);
                    }
                }
            }

            // register DataItems
            PageRequestManager requestManager = new PageRequestManager(_scriptManager);
            foreach (ScriptDataItem dataItem in requestManager.ScriptDataItems)
                list.Add(AppendDataItem(dataItem));            

            if (list.Count > 0)
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("<div style=\"display: none;\" id=\"{0}_SCRIPTS\">", _controlClientID);
                sb.Append(serializer.Serialize(list));
                sb.Append("</div>");

                return sb.ToString();
            }

            return string.Empty;
        }

        private ScriptBlock AppendDataItem(ScriptDataItem dataItem)
        {
            ScriptBlock block = new ScriptBlock();
            block.Type = dataItem.IsJsonSerialized ? "dataItemJson" : "dataItem";
            block.Script = dataItem.DataItem;
            block.ClientID = dataItem.ClientID;

            return block;
        }

        private ScriptBlock AppendScriptBlock(string type, RegisteredScript script)
        {
            ScriptBlock block = new ScriptBlock();
            block.Type = type;
            block.Script = script.Script;
            block.Url = script.Url;

            return block;
        }
    }
}
