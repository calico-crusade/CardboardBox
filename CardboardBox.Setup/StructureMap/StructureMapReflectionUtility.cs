using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CardboardBox.Setup.StructureMap
{
    public class StructureMapReflectionUtility : ReflectionUtility
    {
        private readonly IContainer container;

        public StructureMapReflectionUtility(IContainer container)
        {
            this.container = container;
        }

        public override T GetInstance<T>()
        {
            return container.GetInstance<T>();
        }

        public override object GetInstance(Type type)
        {
            return container.GetInstance(type);
        }

        public override IEnumerable<T> GetAllTypesOf<T>()
        {
            return container.GetAllInstances<T>().ToArray();
        }
    }
}
