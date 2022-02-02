using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Phx.Lib.Log;
using System.Reflection;

namespace Tvinci.Performance
{
    public enum ePerformanceSource
    {
        NDS,
        Orange,
        Site
    }

    //public class TvinciStopwatch : IDisposable
    //{
    //    private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    //    Stopwatch m_stopWatch;
    //    string Source { get; set; }
    //    String Message { get; set; }
    //    ILog Logger { get; set; }

    //    public TvinciStopwatch(ILog logger, string source, string message)
    //    {
    //        if (logger == null)
    //        {
    //            Logger = defaultLog;
    //        }
    //        else
    //        {
    //            Logger = logger;
    //        }
    //        Source = source;
    //        Message = message;
    //        m_stopWatch = Stopwatch.StartNew();
    //    }

    //    public TvinciStopwatch(ePerformanceSource source, string message)
    //        : this( defaultLog, source.ToString(),message)
    //    {
    //        // no implementation needed by design            
    //    }
        
    //    #region IDisposable Members

    //    public void Dispose()
    //    {            
    //        m_stopWatch.Stop();
    //        Source = string.IsNullOrEmpty(Source) ? "-" : Source;
    //        Message = string.IsNullOrEmpty(Message) ? "-" : Message;
    //        Logger.InfoFormat("{0},{1},{2}", Source, m_stopWatch.Elapsed, Message);

    //    }

    //    #endregion
    //}
}
