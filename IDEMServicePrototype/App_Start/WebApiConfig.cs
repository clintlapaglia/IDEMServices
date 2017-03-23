using Newtonsoft.Json;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace IDEMServicePrototype.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "ApiVersion1",
                routeTemplate: "api/v1/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "ApiVersion2",
                routeTemplate: "api/v2/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            jsonFormatter.SerializerSettings.Formatting = Formatting.Indented;
            jsonFormatter.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            jsonFormatter.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
        }
    }
}