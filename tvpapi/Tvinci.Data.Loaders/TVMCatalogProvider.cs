using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using TVPPro.Configuration.PlatformServices;
using Phx.Lib.Log;
using System.Reflection;
using Core.Catalog.Response;
using Core.Catalog.Request;

namespace Tvinci.Data.Loaders
{
    [Serializable]
    public class TVMCatalogProvider : Provider
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        //private static TvinciPlatform.Catalog.IserviceClient m_oClient = null;

        public TVMCatalogProvider()
        {
            //if (!string.IsNullOrEmpty(endPointAddress))
            //{
            //    if (m_oClient == null)
            //    {
            //        try
            //        {
            //            //m_EndPoint = endPointAddress;
            //            //m_oClient = new IserviceClient(string.Empty, m_EndPoint);
            //        }
            //        catch (Exception ex)
            //        {
            //            logger.Error("Exception on Catalog Client creation", ex);
            //            m_oClient = null;
            //        }

            //        FailOverManager.Instance.SafeModeStarted += () => { if (m_oClient != null) m_oClient.Close(); m_oClient = null; };
            //        FailOverManager.Instance.SafeModeEnded += () => { if (m_oClient == null) m_oClient = new IserviceClient(string.Empty, m_EndPoint); };
            //    }
            //}
        }

        public override eProviderResult TryExecuteGetMediasByIDs(MediasProtocolRequest request, out MediaResponse response)
        {
            try
            {
                if (!FailOverManager.Instance.SafeMode)
                {
                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        response = request.GetMediasByIDs(request);
                    }

                    FailOverManager.Instance.AddRequest(true);
                    return eProviderResult.Success;
                }
                else
                {
                    response = null;
                    return eProviderResult.SafeMode;
                }
            }
            catch (Exception ex)
            {
                if (!FailOverManager.Instance.SafeMode)
                    FailOverManager.Instance.AddRequest(false);

                logger.Error("Exception in CatalogGetMediasByIDs", ex);
                response = null;
                if (ex is TimeoutException)
                    return eProviderResult.TimeOut;
                return eProviderResult.Fail;
            }

        }

        public override eProviderResult TryExecuteGetBaseResponse(BaseRequest request, out BaseResponse response)
        {
            try
            {
                if (!FailOverManager.Instance.SafeMode)
                {
                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        response = request.GetResponse(request);
                    }

                    FailOverManager.Instance.AddRequest(true);
                    return eProviderResult.Success;
                }
                else
                {
                    response = null;
                    return eProviderResult.SafeMode;
                }
            }
            catch (Exception ex)
            {
                if (!FailOverManager.Instance.SafeMode)
                    FailOverManager.Instance.AddRequest(false);
                logger.Error("Exception in CatalogGetBaseRsponse", ex);
                response = null;
                if (ex is TimeoutException)
                    return eProviderResult.TimeOut;
                return eProviderResult.Fail;
            }
        }

        public override eProviderResult TryExecuteGetProgramsByIDs(EpgProgramDetailsRequest request, out EpgProgramResponse response)
        {
            try
            {
                if (!FailOverManager.Instance.SafeMode)
                {
                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        response = request.GetProgramsByIDs(request);
                    }

                    FailOverManager.Instance.AddRequest(true);
                    return eProviderResult.Success;
                }
                else
                {
                    response = null;
                    return eProviderResult.SafeMode;
                }
            }
            catch (Exception ex)
            {
                if (!FailOverManager.Instance.SafeMode)
                    FailOverManager.Instance.AddRequest(false);
                logger.Error("Exception in TryExecuteGetProgramsByIDs", ex);
                response = null;
                if (ex is TimeoutException)
                    return eProviderResult.TimeOut;
                return eProviderResult.Fail;
            }
        }
    }
}
