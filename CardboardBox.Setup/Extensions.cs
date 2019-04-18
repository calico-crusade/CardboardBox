using Microsoft.Extensions.DependencyInjection;

namespace CardboardBox.Setup
{
    public static class Extensions
    {
        public static IDependencyHandle Use<T1, T2>(this IDependencyHandle handle) where T2: class, T1
                                                                                   where T1: class
        {
            handle.AddTransient<T1, T2>();
            return handle;
        }

        public static IDependencyHandle Use<T1>(this IDependencyHandle handle, T1 item) where T1: class
        {
            handle.AddSingleton(item);
            return handle;
        }

        public static IDependencyHandle CardboardBox(this IServiceCollection collection)
        {
            return ServiceCollection.ServiceCollectionHandler.Start(collection);
        }
    }
}
