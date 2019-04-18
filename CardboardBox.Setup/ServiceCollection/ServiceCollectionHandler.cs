using Microsoft.Extensions.DependencyInjection;

namespace CardboardBox.Setup.ServiceCollection
{
    public class ServiceCollectionHandler : IDependencyHandle
    {
        private readonly IServiceCollection collection;

        public ServiceCollectionHandler(IServiceCollection collection)
        {
            this.collection = collection;
        }

        public void AddTransient<T1, T2>() where T2: class, T1
                                           where T1: class
        {
            collection.AddTransient(typeof(T1), typeof(T2));
        }

        public void AddSingleton<T1>(T1 item) where T1: class
        {
            collection.AddSingleton(typeof(T1), item);
        }

        public T Build<T>()
        {
            return collection.BuildServiceProvider()
                             .GetRequiredService<T>();
        }

        public static IDependencyHandle Start(IServiceCollection collection)
        {
            return new ServiceCollectionHandler(collection)
                        .Use(collection)
                        .Use<IReflectionUtility, ServiceCollectionReflectionUtility>();
        }
    }
}
