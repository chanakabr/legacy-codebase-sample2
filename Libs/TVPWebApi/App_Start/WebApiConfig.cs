using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using TVPWebApi.Models;

namespace TVPWebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //Services
            
            config.Routes.MapHttpRoute(
                name: "ServicesApi",
                routeTemplate: "api/{action}",
                defaults: new { controller = "Services" }
            );


            //Users

            config.Routes.MapHttpRoute(
                name: "UsersActionApi",
                routeTemplate: "api/users/{site_guid}/{action}/{media_id}",
                defaults: new { controller = "Users", media_id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "UsersGetApi",
                routeTemplate: "api/users/{site_guid}",
                defaults: new { controller = "Users", action = "Get" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) }
            );

            config.Routes.MapHttpRoute(
                name: "UsersPutApi",
                routeTemplate: "api/users/{site_guid}",
                defaults: new { controller = "Users", action = "Put" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Put) }
            );

            config.Routes.MapHttpRoute(
                name: "UsersDeleteApi",
                routeTemplate: "api/users/{site_guid}",
                defaults: new { controller = "Users", action = "Delete" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Delete) }
            );

            //Channels

            config.Routes.MapHttpRoute(
                name: "ChannelApi",
                routeTemplate: "api/channels/{channel_id}",
                defaults: new { controller = "Channels", channel_id = RouteParameter.Optional }
            );


            //Medias

            config.Routes.MapHttpRoute(
                name: "MediaFilesActionApi",
                routeTemplate: "api/medias/files/{file_id}/{action}",
                defaults: new { controller = "Medias" }
            );

            config.Routes.MapHttpRoute(
                name: "MediasActionApi",
                routeTemplate: "api/medias/{media_id}/{action}",
                defaults: new { controller = "Medias" }
            );

            config.Routes.MapHttpRoute(
                name: "MediasGetApi",
                routeTemplate: "api/medias/{media_id}",
                defaults: new { controller = "Medias", action = "Get" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) }
            );

            config.Routes.MapHttpRoute(
                name: "MediasPutApi",
                routeTemplate: "api/medias/{media_id}",
                defaults: new { controller = "Medias", action = "Put" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Put) }
            );

            config.Routes.MapHttpRoute(
                name: "MediasDeleteApi",
                routeTemplate: "api/medias/{media_id}",
                defaults: new { controller = "Medias", action = "Delete" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Delete) }
            );

            //General

            config.Routes.MapHttpRoute(
                name: "PostApi",
                routeTemplate: "api/{controller}",
                defaults: new { action = "Post" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Post) }
            );

            config.Filters.Add(new GenericExceptionFilter());
            //config.Formatters.Insert(0, new JsonNetFormatter());
        }
    }
}
