using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChlyContainer
{
    /// <summary>
    /// The DependencyContainer interface defines the contract the the <see cref="ChlyContainer" /> implementation.
    /// </summary>
    public interface IDependencyContainer : IDisposable
    {
        /// <summary>
        /// Used to register an implementation with its interface.
        /// </summary>
        /// <param name="from">The interface.</param>
        /// <param name="to">The type to resolve.</param>
        /// <returns>The current instance of the container.</returns>
        IDependencyContainer Register(Type from, Type to);

        /// <summary>
        /// Used to register an implementation with its interface.
        /// </summary>
        /// <typeparam name="TFrom">The interface.</typeparam>
        /// <typeparam name="TTo">The type to resolve.</typeparam>
        /// <returns>The current instance of the container.</returns>
        IDependencyContainer Register<TFrom, TTo>()
            where TFrom : class
            where TTo : class, TFrom;

        /// <summary>
        /// Used to register an implementation.
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <returns>The current instance of the container.</returns>
        IDependencyContainer Register<T>()
            where T : class;

        /// <summary>
        /// Used to register an existing instance.
        /// </summary>
        /// <param name="instance">The instance to register.</param>
        /// <returns>The current instance of the container.</returns>
        IDependencyContainer RegisterInstance(object instance);

        /// <summary>
        /// Used to register an existing instance of type T.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="instance">The instance to register.</param>
        /// <returns>The current instance of the container.</returns>
        IDependencyContainer RegisterInstance<T>(object instance)
            where T : class;

        /// <summary>
        /// Used to register a singleton of a type.
        /// </summary>
        /// <typeparam name="T">The type to register.</typeparam>
        /// <returns>The current instance of the container.</returns>
        IDependencyContainer RegisterSingleton<T>()
            where T : class;

        /// <summary>
        /// Used to resolve an implementation.
        /// </summary>
        /// <typeparam name="T">Type registered with the container.</typeparam>
        /// <returns>The current instance of the container.</returns>
        T Resolve<T>()
            where T : class;
    }
}
