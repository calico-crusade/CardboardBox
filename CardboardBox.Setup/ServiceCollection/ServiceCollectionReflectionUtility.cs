using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace CardboardBox.Setup.ServiceCollection
{
    public class ServiceCollectionReflectionUtility : ReflectionUtility
    {
        private IServiceProvider provider;

        public ServiceCollectionReflectionUtility(IServiceCollection collection)
        {
            provider = collection.BuildServiceProvider();
        }

        public override IEnumerable<T> GetAllTypesOf<T>()
        {
            foreach (var type in GetTypes(typeof(T)))
                yield return (T)provider.GetService(type);
        }

        public override T GetInstance<T>()
        {
            return provider.GetService<T>();
        }

        public override object GetInstance(Type type)
        {
            return provider.GetService(type);
        }
    }
}
