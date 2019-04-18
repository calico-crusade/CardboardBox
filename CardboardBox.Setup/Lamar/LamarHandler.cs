using Lamar;
using Microsoft.Extensions.DependencyInjection;

namespace CardboardBox.Setup.Lamar
{
    public interface ILamarHandler : IDependencyHandle
    {
        Container Create();
    }

    public class LamarHandler : ILamarHandler
    {
        private ServiceRegistry serviceRegistry;

        public LamarHandler(ServiceRegistry serviceRegistry)
        {
            this.serviceRegistry = serviceRegistry;
        }

        public LamarHandler()
        {
            this.serviceRegistry = new ServiceRegistry();
        }

        public void AddTransient<T1, T2>() where T2 : class, T1
                                           where T1 : class
        {
            serviceRegistry.For<T1>().Use<T2>();
        }

        public void AddSingleton<T1>(T1 item) where T1: class
        {
            serviceRegistry.For<T1>().Use(item);
        }

        public Container Create()
        {
            serviceRegistry.Scan(_ =>
            {
                _.AssembliesAndExecutablesFromApplicationBaseDirectory();
                _.TheCallingAssembly();
                _.WithDefaultConventions();
                _.SingleImplementationsOfInterface();
            });

            AddSingleton<IServiceCollection>(serviceRegistry);

            return new Container(serviceRegistry);
        }

        public T Build<T>()
        {
            return Create().QuickBuild<T>();
        }

        public static IDependencyHandle Start(ServiceRegistry services)
        {
            return new LamarHandler(services)
                .Use<IReflectionUtility, ServiceCollection.ServiceCollectionReflectionUtility>();
        }

        public static IDependencyHandle Start()
        {
            return Start(new ServiceRegistry());
        }
    }
}
