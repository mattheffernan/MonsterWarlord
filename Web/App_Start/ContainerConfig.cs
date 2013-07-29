using System.Collections.Generic;
using System.Data.Entity;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Data.Context;
using SharpRepository.Ef5Repository;
using SharpRepository.Repository;

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

            builder.RegisterGeneric(typeof(Ef5Repository<>)).As(typeof(IRepository<>));
            builder.RegisterType<MonsterWarlordContext>().As<DbContext>();
            var container = builder.Build();

            // Set the dependency resolver implementation.

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}