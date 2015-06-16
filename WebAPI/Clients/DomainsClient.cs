using AutoMapper;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.Domains;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Clients
{
    public class DomainsClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public DomainsClient()
        { 
        }

        #region Properties

        protected WebAPI.Domains.module Domains
        {
            get
            {
                return (Module as WebAPI.Domains.module);
            }
        }

        #endregion


        internal Domain GetDomainInfo(int groupId, int domainId)
        {
            Domain result = null;
            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Domains.DomainResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.GetDomainInfo(group.DomainsCredentials.Username, group.DomainsCredentials.Password, domainId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Domains.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Domain == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            result = Mapper.Map<Domain>(response.Domain);

            return result;
        }
    }
}