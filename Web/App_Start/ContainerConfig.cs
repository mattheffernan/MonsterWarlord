using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;

namespace Web.App_Start
{
    public class ContainerConfig
    {
        public static void RegisterContainer(HttpConfiguration httpConfiguration)
        {
            var builder = new ContainerBuilder();

            // Register API controllers using assembly scanning.
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            // Register API controller dependencies per request.

            builder.RegisterGeneric(typeof(List<>)).As(typeof(IList<>));
            var container = builder.Build();

            // Set the dependency resolver implementation.

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}