using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace GracenoteFeeder
{
    public abstract class BaseGracenoteFeeder
    {

        abstract public void SaveChannel(XmlDocument xmlDoc);
    }
}    
