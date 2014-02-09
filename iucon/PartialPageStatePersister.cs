// Copyright (c) iucon GmbH. All rights reserved.
// For more information about our work, visit http://www.iucon.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Collections.Specialized;
using System.IO;

namespace iucon.web.Controls
{
    public class PartialPageStatePersister : PageStatePersister
    {
        public string PageState
        {
            get;
            set;
        }

        public PartialPageStatePersister(Page page)
            : base(page)
        {
        } 

        public override void Load()
        {
            if (!string.IsNullOrEmpty(PageState))
            {
                LosFormatter format = new LosFormatter();

                try
                {
                    Pair pair = (Pair)format.Deserialize(PageState);
                    base.ViewState = pair.First;
                    base.ControlState = pair.Second;
                }
                catch (ArgumentException)
                {
                }
            }
        }

        public override void Save()
        {
            if (ViewState != null || ControlState != null)
            {
                string pageState = "";

                if ((base.ViewState != null) || (base.ControlState != null))
                {
                    LosFormatter format = new LosFormatter();
                    StringWriter writer = new StringWriter();

                    format.Serialize(writer, new Pair(ViewState, ControlState));
                    pageState = writer.ToString();
                }

                PageState = pageState;                
            } 
        }
    }
}
