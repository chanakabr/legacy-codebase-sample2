using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WebAPI.App_Start;
using WebAPI.Models;
using WebAPI.Utils;

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
                routeTemplate: "{controller}/{id}"
                //defaults: new { id = RouteParameter.Optional }
            );

            Mapper.Initialize(cfg =>
            {
                
            });

            //Removing Newton and adding Jil
            config.Formatters.RemoveAt(0);
            config.Formatters.Insert(0, new JilFormatter());

            Mapper.CreateMap<Users.UserResponseObject, User>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.m_user.m_sSiteGUID));

            config.Filters.Add(new ValidateModelAttribute());
        }
    }
}
