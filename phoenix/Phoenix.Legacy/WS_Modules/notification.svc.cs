using ApiObjects;
using ApiObjects.Notification;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using Core.Notification;
using KLogMonitor;
using KlogMonitorHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Web;

namespace WS_Notification
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]

    public class NotificationService : WebAPI.WebServices.NotificationService
    {
        // DO NOT IMPLEMENT ANYTHING HERE!!
        // This is a proxy class for the actual common implementation in WebApi 
        // which is the base class
        // This is so that the net461 and netcore implementation will have the same source code of implementation
        // While allowing [ServiceBehavior] attribute to be defined
    }
}