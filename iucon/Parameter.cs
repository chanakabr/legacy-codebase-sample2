// Copyright (c) iucon GmbH. All rights reserved.
// For more information about our work, visit http://www.iucon.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iucon.web.Controls
{
    /// <summary>
    /// A wrapper for a parameter that is passed via HTTP-GET
    /// to the UserControl
    /// </summary>
    public class Parameter
    {
        public string Name
        {
            get;
            set;
        }

        public string Value
        {
            get;
            set;
        }

        public Parameter()
        {
        }

        public Parameter(string Name, string Value)
        {
            this.Name = Name;
            this.Value = Value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            Parameter p = obj as Parameter;
            if ((object)p == null) return false;

            return p.Name == this.Name;
        }
    }
}
