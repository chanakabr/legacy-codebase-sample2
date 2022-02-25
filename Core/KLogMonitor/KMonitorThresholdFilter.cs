using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;


namespace KLogMonitor
{
    /// <summary>
    /// This class is here because Amit and his devops clan refuse to grasp that helm charts should
    /// be managed by the version of the relevant component and that namespaces change.
    ///
    /// Because of that we need to keep the old namespace of `KLogMonitor.KMonitorThresholdFilter, KLogMonitor`
    /// available so that he can keep his precious helm charts unchanged.
    ///
    /// If you read this and helm charts are finally part of this repo and you can chage the log4net.conifg file
    /// in the configmap yml file plz update the <filter> tag in there and delete this project.
    ///
    /// And also plz send some spam mail to Amit 
    /// </summary>
    public class KMonitorThresholdFilter : Phx.Lib.Log.KMonitorThresholdFilter
    {
        
    }
}