using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Services;
using System.Web;
using System.Web.Services.Protocols;
using System.Threading;
using System.ServiceModel.Web;

using System.IO;
using KLogMonitor;
using System.Reflection;

namespace ServiceExtensions
{

    //#region TraceExtensionStream

    ///// <summary>
    ///// Special switchable stream
    ///// </summary>
    //internal class TraceExtensionStream : Stream
    //{
    //    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    //    #region Fields

    //    private Stream innerStream;
    //    private readonly Stream originalStream;

    //    #endregion

    //    #region .ctor

    //    /// <summary>
    //    /// Constructs an instance of the stream wrapping the original stream into it
    //    /// </summary>
    //    internal TraceExtensionStream(Stream originalStream)
    //    {
    //        innerStream = this.originalStream = originalStream;
    //    }

    //    #endregion

    //    #region New public members

    //    /// <summary>
    //    /// Creates a new memory stream and makes it active 
    //    /// </summary>
    //    public void SwitchToNewStream()
    //    {
    //        innerStream = new MemoryStream();
    //    }

    //    /// <summary>
    //    /// Copies data from the old stream to the new in-memory stream 
    //    /// </summary>
    //    public void CopyOldToNew()
    //    {
    //        //innerStream = new MemoryStream((int)originalStream.Length);
    //        Copy(originalStream, innerStream);
    //        innerStream.Position = 0;
    //    }

    //    /// <summary>
    //    /// Copies data from the new stream to the old stream
    //    /// </summary>
    //    public void CopyNewToOld()
    //    {
    //        Copy(innerStream, originalStream);
    //    }

    //    /// <summary>
    //    /// Returns <c>true</c> if the active inner stream is a new stream, i.e. <see cref="SwitchToNewStream"/> has been called
    //    /// </summary>
    //    public bool IsNewStream
    //    {
    //        get
    //        {
    //            return (innerStream != originalStream);
    //        }
    //    }

    //    /// <summary>
    //    /// A link to the active inner stream
    //    /// </summary>
    //    public Stream InnerStream
    //    {
    //        get { return innerStream; }
    //    }

    //    #endregion

    //    #region Private members

    //    private static void Copy(Stream from, Stream to)
    //    {
    //        const int size = 4096;
    //        byte[] bytes = new byte[4096];
    //        int numBytes;
    //        while ((numBytes = from.Read(bytes, 0, size)) > 0)
    //            to.Write(bytes, 0, numBytes);
    //    }

    //    #endregion

    //    #region Overridden members

    //    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
    //    {
    //        return innerStream.BeginRead(buffer, offset, count, callback, state);
    //    }

    //    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
    //    {
    //        return innerStream.BeginWrite(buffer, offset, count, callback, state);
    //    }

    //    protected override void Dispose(bool disposing)
    //    {
    //        try
    //        {
    //            if (disposing)
    //            {
    //                innerStream.Close();
    //            }
    //        }
    //        finally
    //        {
    //            base.Dispose(disposing);
    //        }
    //    }

    //    public override int EndRead(IAsyncResult asyncResult)
    //    {
    //        return innerStream.EndRead(asyncResult);
    //    }

    //    public override void EndWrite(IAsyncResult asyncResult)
    //    {
    //        innerStream.EndWrite(asyncResult);
    //    }

    //    public override void Flush()
    //    {
    //        innerStream.Flush();
    //    }

    //    public override int Read(byte[] buffer, int offset, int count)
    //    {
    //        return innerStream.Read(buffer, offset, count);
    //    }

    //    public override int ReadByte()
    //    {
    //        return innerStream.ReadByte();
    //    }

    //    public override long Seek(long offset, SeekOrigin origin)
    //    {
    //        return innerStream.Seek(offset, origin);
    //    }

    //    public override void SetLength(long value)
    //    {
    //        innerStream.SetLength(value);
    //    }

    //    public override void Write(byte[] buffer, int offset, int count)
    //    {
    //        innerStream.Write(buffer, offset, count);
    //    }

    //    public override void WriteByte(byte value)
    //    {
    //        innerStream.WriteByte(value);
    //    }

    //    // Properties
    //    public override bool CanRead
    //    {
    //        get
    //        {
    //            return innerStream.CanRead;
    //        }
    //    }

    //    public override bool CanSeek
    //    {
    //        get
    //        {
    //            return innerStream.CanSeek;
    //        }
    //    }

    //    public override bool CanWrite
    //    {
    //        get
    //        {
    //            return innerStream.CanWrite;
    //        }
    //    }

    //    public override long Length
    //    {
    //        get
    //        {
    //            return innerStream.Length;
    //        }
    //    }

    //    public override long Position
    //    {
    //        get
    //        {
    //            return innerStream.Position;
    //        }
    //        set
    //        {
    //            innerStream.Position = value;
    //        }
    //    }

    //    #endregion
    //}

    //#endregion
    //public class BackupWSExtension : SoapExtension
    //{
    //    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    //    TraceExtensionStream traceStream;

    //    public override Stream ChainStream(Stream stream)
    //    {
    //        traceStream = new BackupWSExtension(stream);
    //        return traceStream;
    //    }

    //    public override object GetInitializer(LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute)
    //    {
    //        return methodInfo.Name;
    //    }

    //    private bool GetTraceable(SoapMessage message)
    //    {
    //        bool retVal = false;
    //        SoapServerMessage serverMessage = message as SoapServerMessage;
    //        if (serverMessage != null)
    //        {
    //            retVal = true;
    //        }
    //        return retVal;

    //    }

    //    public override void Initialize(object initializer)
    //    {
    //        if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
    //        {
    //            Thread.CurrentThread.Name = Guid.NewGuid().ToString();
    //        }
    //    }

    //    public override object GetInitializer(Type WebServiceType)
    //    {
    //        return WebServiceType.Name;
    //    }

    //    public override void ProcessMessage(SoapMessage message)
    //    {
    //        if (GetTraceable(message))
    //        {
    //            try
    //            {

    //                switch (message.Stage)
    //                {
    //                    case SoapMessageStage.BeforeSerialize:
    //                        traceStream.SwitchToNewStream();
    //                        break;
    //                    case SoapMessageStage.AfterSerialize:
    //                        {
    //                            if (traceStream.IsNewStream)
    //                            {
    //                                traceStream.Position = 0;
    //                                WriteOutput(message);
    //                                traceStream.Position = 0;
    //                                traceStream.CopyNewToOld();
    //                            }
    //                            break;
    //                        }
    //                    case SoapMessageStage.BeforeDeserialize:
    //                        {

    //                            if (!string.IsNullOrEmpty(Thread.CurrentThread.Name))
    //                            {
    //                                if (HttpContext.Current.Items != null && !HttpContext.Current.Items.Contains(Thread.CurrentThread.Name))
    //                                {
    //                                    HttpContext.Current.Items.Add(Thread.CurrentThread.Name, DateTime.UtcNow);
    //                                }
    //                            }
    //                            traceStream.SwitchToNewStream();
    //                            traceStream.CopyOldToNew();
    //                            WriteInput(message);
    //                            traceStream.Position = 0;

    //                            break;
    //                        }
    //                    case SoapMessageStage.AfterDeserialize:
    //                        break;
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                log.Error("SOapExtension - Soap Exception :" + ex.Message, ex);
    //            }
    //        }
    //    }

    //    public void WriteOutput(SoapMessage message)
    //    {
    //        try
    //        {
    //            //BaseLog logObject = CreateLogObject(message);
    //            //logObject.Type = eLogType.SoapResponse;

    //            //var soapString = logObject.Type.ToString();
    //            //var header = Thread.CurrentThread.Name + " " + soapString + " " + message.MethodInfo.Name + " ";


    //            //if (message.Exception != null)
    //            //{
    //            //    Log(header, traceStream, logObject, message.Exception);
    //            //}
    //            //else
    //            //{
    //            //    if (logObject != null)
    //            //    {
    //            //        Log(header, traceStream, logObject);

    //            //    }
    //            //}
    //        }
    //        catch (Exception ex)
    //        {
    //            log.Error("SOapExtension - Soap Exception :" + ex.Message, ex);
    //        }

    //    }

    //    public void WriteInput(SoapMessage message)
    //    {
    //        try
    //        {
    //            //BaseLog logObject = CreateLogObject(message);

    //            //// Copy(oldStream, newStream);

    //            //if (logObject != null)
    //            //{
    //            //    string soapString = logObject.Type.ToString();
    //            //    var header = Thread.CurrentThread.Name + " " + soapString + " " + message.MethodInfo.Name + " ";

    //            //    Log(header, traceStream, logObject);
    //            //}
    //        }
    //        catch (Exception ex)
    //        {
    //            log.Error("SOapExtension - Soap Exception :" + ex.Message, ex);
    //        }
    //    }


    //    //private BaseLog CreateLogObject(SoapMessage message)
    //    //{
    //    //    try
    //    //    {
    //    //        BaseLog logObject = null;

    //    //        if (message != null)
    //    //        {
    //    //            logObject = new BaseLog();
    //    //            logObject.TimeSpan = 0;
    //    //            logObject.Method = message.MethodInfo.Name;

    //    //            logObject.Id = Thread.CurrentThread.Name;
    //    //            logObject.ObjectCreationDate = GetObjectCreationDate();
    //    //            //logObject.UserAgent = HttpContext.Current.Request.UserAgent;
    //    //            //logObject.IP = HttpContext.Current.Request.UserHostAddress;
    //    //            logObject.Type = (message is SoapServerMessage) ? eLogType.SoapRequest : eLogType.SoapResponse;
    //    //        }

    //    //        return logObject;
    //    //    }
    //    //    catch (Exception ex)
    //    //    {
    //    //        log.Error("SOapExtension - Soap Exception :" + ex.Message, ex);
    //    //        return null;
    //    //    }
    //    //}

    //    private DateTime GetObjectCreationDate()
    //    {
    //        try
    //        {
    //            DateTime startProcessTime = new DateTime();
    //            bool isParseSucceeded = false;
    //            bool isDateStoredInHttpContext = HttpContext.Current.Items != null && HttpContext.Current.Items.Contains(Thread.CurrentThread.Name);
    //            if (isDateStoredInHttpContext)
    //            {
    //                isParseSucceeded = DateTime.TryParse(HttpContext.Current.Items[Thread.CurrentThread.Name].ToString(), out startProcessTime);
    //            }
    //            else if (!isDateStoredInHttpContext || !isParseSucceeded)
    //            {
    //                startProcessTime = DateTime.UtcNow;
    //            }
    //            return startProcessTime;
    //        }
    //        catch (Exception ex)
    //        {
    //            log.Error("SOapExtension - Soap Exception :" + ex.Message, ex);
    //            return DateTime.Now;
    //        }
    //    }

    //    //void Log(string header, Stream stream, BaseLog logObject, Exception e = null)
    //    //{
    //    //    try
    //    //    {
    //    //        var sb = new StringBuilder();
    //    //        var w = new StringWriter(sb);

    //    //        stream.Position = 0;
    //    //        Copy(stream, w);
    //    //        var msg = sb.ToString();
    //    //        try
    //    //        {
    //    //            //Since we're looking at SOAP, parse the XML so it gets formatted nicely.
    //    //            var log = msg.Trim().ToString();
    //    //            if (e == null)
    //    //            {
    //    //                logObject.Info(log, true);
    //    //            }
    //    //            else
    //    //            {
    //    //                logObject.Error(e.Message, true);
    //    //            }
    //    //        }
    //    //        catch (Exception ex) //message is not valid xml
    //    //        {
    //    //            if (e == null)
    //    //            {
    //    //                logObject.Info(ex.Message, true);

    //    //            }
    //    //            else
    //    //            {
    //    //                logObject.Error(header + msg + e.Message, true);
    //    //            }
    //    //        }
    //    //        stream.Position = 0;
    //    //    }
    //    //    catch (Exception ex)
    //    //    {
    //    //        log.Error("SOapExtension - Soap Exception :" + ex.Message, ex);
    //    //    }
    //    //}

    //    void Copy(Stream from, TextWriter to)
    //    {
    //        try
    //        {
    //            var reader = new StreamReader(from);
    //            to.WriteLine(reader.ReadToEnd());
    //            to.Flush();
    //        }
    //        catch (Exception ex)
    //        {
    //            log.Error("SOapExtension - Soap Exception :" + ex.Message, ex);
    //        }
    //    }

    //    void Copy(Stream from, Stream to)
    //    {
    //        try
    //        {
    //            TextReader reader = new StreamReader(from);
    //            TextWriter writer = new StreamWriter(to);
    //            writer.WriteLine(reader.ReadToEnd());
    //            writer.Flush();
    //        }
    //        catch (Exception ex)
    //        {
    //            log.Error("SOapExtension - Soap Exception :" + ex.Message, ex);
    //        }
    //    }
    //}
}
