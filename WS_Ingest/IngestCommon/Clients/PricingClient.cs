using ApiObjects;
using Ingest.Clients.ClientManager;
using Ingest.Models;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Ingest.Clients
{
    public class PricingClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public PricingClient()
        {
        }

        internal BusinessModuleResponse InsertPricePlan(int groupId, IngestPricePlan pricePlan)
        {
            BusinessModuleResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.InsertPricePlan(groupId, pricePlan);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {1}", ex);
            }

            return response;
        }

        internal BusinessModuleResponse UpdatePricePlan(int groupId, IngestPricePlan pricePlan)
        {
            BusinessModuleResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.UpdatePricePlan(groupId, pricePlan);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {1}", ex);
            }

            return response;
        }

        internal BusinessModuleResponse DeletePricePlan(int groupId, string pricePlanCode)
        {
            BusinessModuleResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.DeletePricePlan(groupId, pricePlanCode);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {1}", ex);
            }

            return response;
        }

        internal BusinessModuleResponse InsertMultiPricePlan(int groupId, IngestMultiPricePlan multiPricePlan)
        {
            BusinessModuleResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.InsertMPP(groupId, multiPricePlan);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {1}", ex);
            }

            return response;
        }

        internal BusinessModuleResponse UpdateMultiPricePlan(int groupId, IngestMultiPricePlan multiPricePlan)
        {
            BusinessModuleResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.UpdateMPP(groupId, multiPricePlan);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {1}", ex);
            }

            return response;
        }

        internal BusinessModuleResponse DeleteMultiPricePlan(int groupId, string multiPricePlanCode)
        {
            BusinessModuleResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.DeleteMPP(groupId, multiPricePlanCode);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {1}", ex);
            }

            return response;
        }

        internal BusinessModuleResponse InsertPPV(int groupId, IngestPPV ppv)
        {
            BusinessModuleResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.InsertPPV(groupId, ppv);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {1}", ex);
            }

            return response;
        }

        internal BusinessModuleResponse UpdatePPV(int groupId, IngestPPV ppv)
        {
            BusinessModuleResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.UpdatePPV(groupId, ppv);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {1}", ex);
            }

            return response;
        }

        internal BusinessModuleResponse DeletePPV(int groupId, string ppvCode)
        {
            BusinessModuleResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.DeletePPV(groupId, ppvCode);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {1}", ex);
            }

            return response;
        }


    }
}