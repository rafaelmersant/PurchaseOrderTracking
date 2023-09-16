using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace OrdenCompra
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            //routes.MapMvcAttributeRoutes();
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
             name: "Articulos",
             url: "Articulos/{action}/{id}",
             defaults: new { controller = "Article", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
             name: "Suplidores",
             url: "Suplidores/{action}/{id}",
             defaults: new { controller = "Provider", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "ContenedorEstatus",
            url: "ContenedorEstatus/{action}/{id}",
            defaults: new { controller = "ContainerStatus", action = "Index", id = UrlParameter.Optional }
           );

            routes.MapRoute(
           name: "OrdenEstatus",
           url: "OrdenEstatus/{action}/{id}",
           defaults: new { controller = "OrdenEstatus", action = "Index", id = UrlParameter.Optional }
          );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
