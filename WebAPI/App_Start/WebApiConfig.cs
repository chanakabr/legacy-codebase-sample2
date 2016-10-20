using AutoMapper;
using Microsoft.AspNet.WebApi.MessageHandlers.Compression;
using Microsoft.AspNet.WebApi.MessageHandlers.Compression.Compressors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WebAPI.App_Start;
using WebAPI.Filters;
using WebAPI.Models;
using WebAPI.Utils;
using System.Reflection;
using WebAPI.Controllers;

namespace WebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}/{action}"
                //defaults: new { id = RouteParameter.Optional }
            );

            Mapper.Initialize(cfg =>
            {

            });

            //Removing Newton and adding Jil
            config.Formatters.RemoveAt(0);
            config.Formatters.Insert(0, new JilFormatter());
            config.Formatters.Add(new CustomXmlFormatter());
            config.Formatters.Add(new CustomResponseFormatter());
            config.Formatters.Remove(GlobalConfiguration.Configuration.Formatters.XmlFormatter);

            config.Filters.Add(new RequestParser());
            config.Filters.Add(new ValidateModelAttribute());            
            config.Filters.Add(new VoidActionFilter());
            config.MessageHandlers.Add(new WrappingHandler());

            GlobalConfiguration.Configuration.MessageHandlers.Insert(0, new ServerCompressionHandler(new GZipCompressor(), new DeflateCompressor()));
        }
    }
}
