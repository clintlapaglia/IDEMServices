using IDEMServicePrototype.Shared.Resolvers;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace IDEMServicePrototype.App_Start
{
    public static class DependencyInjectionConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var container = new UnityContainer();
            //container.RegisterType<IProductRepository, ProductRepository>(new HierarchicalLifetimeManager());
            config.DependencyResolver = new UnityResolver(container);
        }
    }
}