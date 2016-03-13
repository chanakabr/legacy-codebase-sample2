using Ingest.Clients.ClientManager;
using Ingest.Models;
using Ingest.Pricing;
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

        protected Ingest.Pricing.mdoule Pricing
        {
            get
            {
                return (Module as Ingest.Pricing.mdoule);
            }
        }

        internal Ingest.Pricing.BusinessModuleResponse InsertPricePlan(int groupId, Ingest.Pricing.IngestPricePlan pricePlan)
        {
            Ingest.Pricing.BusinessModuleResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Pricing.InsertPricePlan(groupId, pricePlan);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. ws address: {0}, exception: {1}", Pricing.Url, ex);
            }

            return response;
        }

        internal Ingest.Pricing.BusinessModuleResponse UpdatePricePlan(int groupId, Ingest.Pricing.IngestPricePlan pricePlan)
        {
            Ingest.Pricing.BusinessModuleResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Pricing.UpdatePricePlan(groupId, pricePlan);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. ws address: {0}, exception: {1}", Pricing.Url, ex);
            }

            return response;
        }

        internal Ingest.Pricing.BusinessModuleResponse DeletePricePlan(int groupId, string pricePlanCode)
        {
            Ingest.Pricing.BusinessModuleResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Pricing.DeletePricePlan(groupId, pricePlanCode);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. ws address: {0}, exception: {1}", Pricing.Url, ex);
            }

            return response;
        }

        internal Ingest.Pricing.BusinessModuleResponse InsertMultiPricePlan(int groupId, Ingest.Pricing.IngestMultiPricePlan multiPricePlan)
        {
            Ingest.Pricing.BusinessModuleResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Pricing.InsertMPP(groupId, multiPricePlan);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. ws address: {0}, exception: {1}", Pricing.Url, ex);
            }

            return response;
        }

        internal Ingest.Pricing.BusinessModuleResponse UpdateMultiPricePlan(int groupId, Ingest.Pricing.IngestMultiPricePlan multiPricePlan)
        {
            Ingest.Pricing.BusinessModuleResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Pricing.UpdateMPP(groupId, multiPricePlan);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. ws address: {0}, exception: {1}", Pricing.Url, ex);
            }

            return response;
        }

        internal Ingest.Pricing.BusinessModuleResponse DeleteMultiPricePlan(int groupId, string multiPricePlanCode)
        {
            Ingest.Pricing.BusinessModuleResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Pricing.DeleteMPP(groupId, multiPricePlanCode);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. ws address: {0}, exception: {1}", Pricing.Url, ex);
            }

            return response;
        }

        internal Ingest.Pricing.BusinessModuleResponse InsertPPV(int groupId, Ingest.Pricing.IngestPPV ppv)
        {
            Ingest.Pricing.BusinessModuleResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Pricing.InsertPPV(groupId, ppv);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. ws address: {0}, exception: {1}", Pricing.Url, ex);
            }

            return response;
        }

        internal Ingest.Pricing.BusinessModuleResponse UpdatePPV(int groupId, Ingest.Pricing.IngestPPV ppv)
        {
            Ingest.Pricing.BusinessModuleResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Pricing.UpdatePPV(groupId, ppv);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. ws address: {0}, exception: {1}", Pricing.Url, ex);
            }

            return response;
        }

        internal Ingest.Pricing.BusinessModuleResponse DeletePPV(int groupId, string ppvCode)
        {
            Ingest.Pricing.BusinessModuleResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Pricing.DeletePPV(groupId, ppvCode);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. ws address: {0}, exception: {1}", Pricing.Url, ex);
            }

            return response;
        }


    }
}