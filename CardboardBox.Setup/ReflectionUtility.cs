using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CardboardBox.Setup
{
    public interface IReflectionUtility
    {
        /// <summary>
        /// Gets all types that implement the specified type
        /// </summary>
        /// <param name="implementedInterface">The specified type</param>
        /// <returns>All types that implement the specified type</returns>
        IEnumerable<Type> GetTypes(Type implementedInterface);

        /// <summary>
        /// Converts an object of one type to another
        /// </summary>
        /// <param name="obj">The object to convert</param>
        /// <param name="toType">The type to convert to</param>
        /// <returns>The converted object</returns>
        object ChangeType(object obj, Type toType);

        /// <summary>
        /// Converts an object of one type to another
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="obj">The object to convert</param>
        /// <returns>The converted object</returns>
        T ChangeType<T>(object obj);

        /// <summary>
        /// Gets all types that implement a specified type
        /// </summary>
        /// <typeparam name="T">The specified type</typeparam>
        /// <returns>All instances of the specified type</returns>
        IEnumerable<T> GetAllTypesOf<T>();

        /// <summary>
        /// Creates an instance of the object using dependency injection
        /// </summary>
        /// <typeparam name="T">The type to create</typeparam>
        /// <returns>The created type</returns>
        T GetInstance<T>();

        /// <summary>
        /// Creates an instance of the object using dependency injection
        /// </summary>
        /// <param name="type">The type to create</param>
        /// <returns>The created type</returns>
        object GetInstance(Type type);

        /// <summary>
        /// Exectues a method and passes in the arguments specified or attempts to resolve arguments using dependency injection
        /// </summary>
        /// <param name="info">The method to execute</param>
        /// <param name="def">The context in which to execute the method</param>
        /// <param name="error">If an error occurred during execution</param>
        /// <param name="defaultparameters">The parameters that the method could possibly take</param>
        /// <returns>The return object of the method or an exception if "error" is true</returns>
        object ExecuteDynamicMethod(MethodInfo info, object def, out bool error, params object[] defaultparameters);

        /// <summary>
        /// Executes a method using the defined parameters
        /// </summary>
        /// <param name="info">The method to execute</param>
        /// <param name="def">The context in which to execute the method</param>
        /// <param name="error">If an error occurred during execution</param>
        /// <param name="pars">The parameters to execute the method with</param>
        /// <returns>The return object of the method or an exception if "error" is true</returns>
        object ExecuteMethod(MethodInfo info, object def, out bool error, params object[] pars);
    }

    public abstract class ReflectionUtility : IReflectionUtility
    {
        public static Encoding Encoder = Encoding.UTF8;

        public abstract T GetInstance<T>();
        public abstract object GetInstance(Type type);
        public abstract IEnumerable<T> GetAllTypesOf<T>();

        public virtual object ChangeType(object obj, Type toType)
        {
            if (obj == null)
                return null;

            var fromType = obj.GetType();

            var to = Nullable.GetUnderlyingType(toType) ?? toType;
            var from = Nullable.GetUnderlyingType(fromType) ?? fromType;

            if (to == from)
                return obj;

            if (to.IsEnum)
            {
                return Enum.ToObject(to, Convert.ChangeType(obj, to.GetEnumUnderlyingType()));
            }

            if (from == typeof(byte[]) && to == typeof(string))
            {
                return Encoder.GetString((byte[])obj);
            }

            if (to == typeof(byte[]) && from == typeof(string))
            {
                return Encoder.GetBytes((string)obj);
            }

            return Convert.ChangeType(obj, to);
        }

        public virtual T ChangeType<T>(object obj)
        {
            return (T)ChangeType(obj, typeof(T));
        }

        public virtual IEnumerable<Type> GetTypes(Type implementedInterface)
        {
            var assembly = Assembly.GetEntryAssembly();
            var alreadyLoaded = new List<string>
            {
                assembly.FullName
            };

            foreach (var type in assembly.DefinedTypes)
            {
                if (type.IsInterface || type.IsAbstract)
                    continue;

                if (type.ImplementedInterfaces.Contains(implementedInterface))
                    yield return type;
            }

            var assems = assembly.GetReferencedAssemblies()
                .Select(t => t.FullName)
                .Except(alreadyLoaded)
                .ToArray();
            foreach (var ass in assems)
            {
                foreach (var type in GetTypes(implementedInterface, ass, alreadyLoaded))
                {
                    yield return type;
                }
            }
        }

        private IEnumerable<Type> GetTypes(Type implementedInterface, string assembly, List<string> alreadyLoaded)
        {
            if (alreadyLoaded.Contains(assembly))
                yield break;

            alreadyLoaded.Add(assembly);
            var asml = Assembly.Load(assembly);
            foreach (var type in asml.DefinedTypes)
            {
                if (type.IsInterface || type.IsAbstract)
                    continue;

                if (type.ImplementedInterfaces.Contains(implementedInterface))
                    yield return type;
            }

            var assems = asml.GetReferencedAssemblies()
                .Select(t => t.FullName)
                .Except(alreadyLoaded)
                .ToArray();
            foreach (var ass in assems)
            {
                foreach (var type in GetTypes(implementedInterface, ass, alreadyLoaded))
                {
                    yield return type;
                }
            }

        }

        public virtual object ExecuteDynamicMethod(MethodInfo info, object def, out bool error, params object[] defaultparameters)
        {
            try
            {
                error = false;
                if (info == null)
                    return null;

                var pars = info.GetParameters();

                if (pars.Length <= 0)
                    return ExecuteMethod(info, def, out error);

                var args = new object[pars.Length];

                for (var i = 0; i < pars.Length; i++)
                {
                    var par = pars[i];

                    var pt = par.ParameterType;

                    var fit = defaultparameters.FirstOrDefault(t => pt.IsAssignableFrom(t.GetType()));

                    if (fit != null)
                    {
                        args[i] = fit;
                        continue;
                    }

                    var next = GetInstance(pt);
                    if (next != null)
                    {
                        args[i] = next;
                        continue;
                    }

                    args[i] = pt.IsValueType ? Activator.CreateInstance(pt) : null;
                }

                return ExecuteMethod(info, def, out error, args);
            }
            catch (Exception ex)
            {
                error = true;
                return ex;
            }
        }

        public virtual object ExecuteMethod(MethodInfo info, object def, out bool error, params object[] pars)
        {
            try
            {
                error = false;
                return info.Invoke(def, pars);
            }
            catch (Exception ex)
            {
                error = true;
                return ex;
            }
        }
    }
}
