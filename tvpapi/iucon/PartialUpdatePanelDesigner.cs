// Copyright (c) iucon GmbH. All rights reserved.
// For more information about our work, visit http://www.iucon.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.Design;
using System.ComponentModel;

namespace iucon.web.Controls
{
    public class PartialUpdatePanelDesigner : ControlDesigner
    {
        TemplateGroupCollection _templateCollection = null;

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            
            SetViewFlags(ViewFlags.TemplateEditing, true);
        }

        public override string GetDesignTimeHtml()
        {
            // Ensure that the control creates child components
            PartialUpdatePanel panel = (PartialUpdatePanel)Component;
            panel.EnsureChildControlsForDesigner();

            return base.GetDesignTimeHtml();
        }

        public override TemplateGroupCollection TemplateGroups
        {
            get
            {
                if (_templateCollection == null)
                {
                    _templateCollection = base.TemplateGroups;

                    PartialUpdatePanel panel = (PartialUpdatePanel)Component;
                                                        
                    // LoadingTemplate
                    TemplateGroup loadingGroup = new TemplateGroup("LoadingTemplate");
                    TemplateDefinition loadingDefinition = new TemplateDefinition(this, "LoadingTemplate", panel, "LoadingTemplate", true);                    
                    loadingGroup.AddTemplateDefinition(loadingDefinition);
                    _templateCollection.Add(loadingGroup);

                    // ErrorTemplate
                    TemplateGroup errorGroup = new TemplateGroup("ErrorTemplate");
                    TemplateDefinition errorDefinition = new TemplateDefinition(this, "ErrorTemplate", panel, "ErrorTemplate", true);
                    errorGroup.AddTemplateDefinition(errorDefinition);
                    _templateCollection.Add(errorGroup);

                    // InitialTemplate
                    TemplateGroup initialGroup = new TemplateGroup("InitialTemplate");
                    TemplateDefinition initialDefinition = new TemplateDefinition(this, "InitialTemplate", panel, "InitialTemplate", true);
                    initialGroup.AddTemplateDefinition(initialDefinition);
                    _templateCollection.Add(initialGroup);
                }

                return _templateCollection;
            }
        }
    }
}
