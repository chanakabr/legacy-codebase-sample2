using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ApiObjects.Response;
using Newtonsoft.Json;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models;
using WebAPI.Models.General;

namespace WebAPI.Controllers
{
    // this is a workaround to generate documentation and client libs
    // should be removed, when we'll find solution to update KalturaClient.xml with non-Phoenix endpoints

    [Service("epgServicePartnerConfiguration")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public class EpgServicePartnerConfigurationController : IKalturaController
    {
        /// <summary>
        /// Returns EPG cache service partner configurations
        /// </summary>
        /// <returns></returns>
        [Action("get")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        public static KalturaEpgServicePartnerConfiguration Get()
        {
            throw new NotImplementedException("call should go to EPG Cache service instead of Phoenix");
        }

        /// <summary>
        /// Returns EPG cache service partner configurations
        /// </summary>
        /// <param name="config"> the partner config updates </param>
        /// <returns></returns>
        [Action("update")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        public static void Update(KalturaEpgServicePartnerConfiguration config)
        {
            throw new NotImplementedException("call should go to EPG Cache service instead of Phoenix");
        }
    }
}