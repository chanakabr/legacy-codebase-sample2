using ApiObjects;
using Ingest.Clients.ClientManager;
using Ingest.Models;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WS_Pricing;

namespace Ingest.Clients
{
    public class PricingClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public PricingClient()
        {
        }

        protected mdoule Pricing
        {
            get
            {
                return (Module as mdoule);
            }
        }

        internal BusinessModuleResponse InsertPricePlan(int groupId, IngestPricePlan pricePlan)
        {
            BusinessModuleResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Pricing.InsertPricePlan(groupId, pricePlan);
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
                    response = Pricing.UpdatePricePlan(groupId, pricePlan);
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
                    response = Pricing.DeletePricePlan(groupId, pricePlanCode);
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
                    response = Pricing.InsertMPP(groupId, multiPricePlan);
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
                    response = Pricing.UpdateMPP(groupId, multiPricePlan);
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
                    response = Pricing.DeleteMPP(groupId, multiPricePlanCode);
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
                    response = Pricing.InsertPPV(groupId, ppv);
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
                    response = Pricing.UpdatePPV(groupId, ppv);
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
                    response = Pricing.DeletePPV(groupId, ppvCode);
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