using StructureMap;
using StructureMap.Graph;
using System;
using System.Collections.Generic;

namespace CardboardBox.Setup.StructureMap
{
    public interface IStructureMapHandler : IDependencyHandle
    {
        StructureMapHandler Use<T1, T2>() where T2: T1;
        StructureMapHandler Use<T1>(T1 item);
        StructureMapHandler Use<T1>(Func<IContext, T1> item);
        StructureMapHandler Use<T1>(Func<Container, IContext, T1> item);
        StructureMapHandler AllOf<T1>();
        StructureMapHandler AllOf(Type t);
        StructureMapHandler Config(Action<ConfigurationExpression> exp);
        StructureMapHandler Scan(Action<IAssemblyScanner> scan);
        StructureMapHandler Config(Action<ConfigurationExpression, Container> exp);
        Container Create();
    }

    public class StructureMapHandler : IStructureMapHandler
    {
        private List<Action<ConfigurationExpression>> Mappers;
        private List<Action<ConfigurationExpression, Container>> Containers;
        private List<Action<IAssemblyScanner>> Scanners;

        public StructureMapHandler()
        {
            Mappers = new List<Action<ConfigurationExpression>>();
            Scanners = new List<Action<IAssemblyScanner>>();
            Containers = new List<Action<ConfigurationExpression, Container>>();
        }

        public void AddSingleton<T1>(T1 item) where T1: class
        {
            Use(item);
        }

        public void AddTransient<T1, T2>() where T2: class, T1
                                           where T1: class
        {
            Use<T1, T2>();
        }

        public StructureMapHandler Use<T1, T2>() where T2 : T1
        {
            return Config(c =>
            {
                c.For<T1>().Use<T2>();
            });
        }

        public StructureMapHandler Use<T1>(T1 item)
        {
            return Config(c =>
            {
                c.For<T1>().Use(a => item);
            });
        }

        public StructureMapHandler Use<T1>(Func<IContext, T1> item)
        {
            return Config(c =>
            {
                c.For<T1>().Use(a => item(a));
            });
        }

        public StructureMapHandler Use<T1>(Func<Container, IContext, T1> item)
        {
            return Config((c, c1) =>
            {
                c.For<T1>().Use(a => item(c1, a));
            });
        }

        public StructureMapHandler AllOf<T1>()
        {
            return Scan(c =>
            {
                c.AddAllTypesOf<T1>();
            });
        }

        public StructureMapHandler AllOf(Type t)
        {
            return Scan(c =>
            {
                c.AddAllTypesOf(t);
            });
        }

        public StructureMapHandler Config(Action<ConfigurationExpression> exp)
        {
            Mappers.Add(exp);
            return this;
        }

        public StructureMapHandler Scan(Action<IAssemblyScanner> scan)
        {
            Scanners.Add(scan);
            return this;
        }

        public StructureMapHandler Config(Action<ConfigurationExpression, Container> exp)
        {
            Containers.Add(exp);
            return this;
        }

        public Container Create()
        {
            var cont = new Container();
            AddSingleton<IContainer>(cont);

            cont.Configure(c =>
            {
                c.Scan(s =>
                {
                    s.AssembliesAndExecutablesFromApplicationBaseDirectory();
                    s.TheCallingAssembly();
                    s.WithDefaultConventions();
                    s.SingleImplementationsOfInterface();

                    foreach (var scanner in Scanners)
                        scanner?.Invoke(s);
                });

                foreach (var conf in Mappers)
                    conf?.Invoke(c);

                foreach (var conf in Containers)
                    conf?.Invoke(c, cont);
            });
            return cont;
        }

        public T Build<T>()
        {
            return Create().GetInstance<T>();
        }

        public static IDependencyHandle Start()
        {
            return new StructureMapHandler()
                        .Use<IReflectionUtility, StructureMapReflectionUtility>();
        }
    }
}
