using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace Tvinci.Web.Controls.Gallery
{    
    /// <summary>
    /// Represents gallery control which can handle inner gallery panels    
    /// </summary>
    public interface IGallery
    {        
        void ReRegisterParts();
        
        // For panel proxy support
        event EventHandler<CommandEventArgs> CommandChangedInProxy;
        void RaiseCommandChangedInProxy(CommandEventArgs e);
    }    
}
