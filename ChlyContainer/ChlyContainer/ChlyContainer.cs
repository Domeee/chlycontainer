using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ChlyContainer
{
  /// <summary>
  ///   A lightweight dependency injection container.
  /// </summary>
  public class ChlyContainer : IDependencyContainer
  {
    /// <summary>
    ///   Store for registrations.
    /// </summary>
    private readonly Dictionary<Type, List<Func<ChlyContainer, Type, object>>> _store;

    /// <summary>
    ///   True if the current instance already has been disposed.
    /// </summary>
    private bool _disposed;

    /// <summary>
    ///   Initializes a new instance of the <see cref="ChlyContainer" /> class.
    /// </summary>
    public ChlyContainer()
    {
      _store = new Dictionary<Type, List<Func<ChlyContainer, Type, object>>>();
    }

    #region Resolution

    /// <summary>
    ///   Resolve an implementation.
    /// </summary>
    /// <typeparam name="T">Type registered with the container.</typeparam>
    /// <returns>The current instance of the container.</returns>
    public T Resolve<T>()
      where T : class
    {
      return (T) Resolve(typeof (T));
    }

    #endregion Resolution

    /// <summary>
    ///   Disposing event is fired when the current instance is being disposed.
    /// </summary>
    public event EventHandler Disposing;

    /// <summary>
    ///   Creates a constructor for the type by resolving each argument from the container.
    /// </summary>
    /// <param name="type">The type to create a constructor for.</param>
    /// <returns>The constructor as a delegate.</returns>
    private static Func<ChlyContainer, Type, object> GetCreator(Type type)
    {
      var ctors = type.GetConstructors();
      if (ctors == null || ctors.Length == 0) throw new Exception();

      var ctor = ctors[0];
      var containerParameter = Expression.Parameter(typeof (ChlyContainer), "container");
      var typeParameter = Expression.Parameter(typeof (Type), "type");

      // Resolve each ctor argument => THAT'S CONSTRUCTOR INJECTION
      var ctorArguments =
        ctor.GetParameters()
          .Select(p => Expression.Call(containerParameter, "Resolve", new[] {p.ParameterType}));

      var dynamicConstructor = Expression.New(ctor, ctorArguments);

      var creator = Expression.Lambda<Func<ChlyContainer, Type, object>>(dynamicConstructor, containerParameter,
        typeParameter);

      var constructorDelegate = creator.Compile();

      return constructorDelegate;
    }

    /// <summary>
    ///   Register an instance for a type.
    /// </summary>
    /// <param name="type">The type of the instance.</param>
    /// <param name="instance">The instance to register.</param>
    /// <returns>The current instance of the container.</returns>
    private IDependencyContainer RegisterInstance(Type type, object instance)
    {
      if (_disposed) throw new ObjectDisposedException(GetType().FullName);

      List<Func<ChlyContainer, Type, object>> creators;
      var exists = _store.TryGetValue(type, out creators);

      if (!exists)
      {
        _store.Add(type, creators = new List<Func<ChlyContainer, Type, object>>());
        creators.Add((c, t) => instance);
        RegisterForDisposing(instance);
      }

      return this;
    }

    /// <summary>
    ///   If instance implements <see cref="IDisposable" />, registers instance for disposing.
    /// </summary>
    /// <param name="instance">Instance to register for disposing.</param>
    private void RegisterForDisposing(object instance)
    {
      var disposable = instance as IDisposable;
      if (disposable != null)
      {
        Disposing += (sender, args) => disposable.Dispose();
      }
    }

    /// <summary>
    ///   Resolve an instance for the type.
    /// </summary>
    /// <param name="type">The type registered with the container.</param>
    /// <returns>An instance for the type.</returns>
    private object Resolve(Type type)
    {
      if (_disposed) throw new ObjectDisposedException(GetType().FullName);

      if (type == null) throw new ArgumentNullException("type");

      List<Func<ChlyContainer, Type, object>> creators;
      var exists = _store.TryGetValue(type, out creators);

      if (!exists) throw new ResolutionException(type);

      var constructor = creators.First();
      return constructor(this, type);
    }

    #region Registration

    /// <summary>
    ///   Register an implementation with its interface.
    /// </summary>
    /// <param name="from">The interface.</param>
    /// <param name="to">The type to resolve.</param>
    /// <returns>The current instance of the container.</returns>
    public IDependencyContainer Register(Type from, Type to)
    {
      if (from == null) throw new ArgumentNullException(nameof(@from));
      if (to == null) throw new ArgumentNullException(nameof(to));
      if (_disposed) throw new ObjectDisposedException(GetType().FullName);

      List<Func<ChlyContainer, Type, object>> creators;
      var exists = _store.TryGetValue(from, out creators);

      if (!exists)
      {
        _store.Add(from, creators = new List<Func<ChlyContainer, Type, object>>());
        var typeToConstructor = GetCreator(to);
        creators.Add(typeToConstructor);
      }

      return this;
    }

    /// <summary>
    ///   Register an implementation with its interface.
    /// </summary>
    /// <typeparam name="TFrom">The interface.</typeparam>
    /// <typeparam name="TTo">The type to resolve.</typeparam>
    /// <returns>The current instance of the container.</returns>
    public IDependencyContainer Register<TFrom, TTo>()
      where TFrom : class
      where TTo : class, TFrom
    {
      Register(typeof (TFrom), typeof (TTo));

      return this;
    }

    /// <summary>
    ///   Register an implementation.
    /// </summary>
    /// <typeparam name="T">The type to resolve.</typeparam>
    /// <returns>The current instance of the container.</returns>
    public IDependencyContainer Register<T>()
      where T : class
    {
      Register(typeof (T), typeof (T));

      return this;
    }

    /// <summary>
    ///   Register an existing instance.
    /// </summary>
    /// <param name="instance">The instance to register.</param>
    /// <returns>The current instance of the container.</returns>
    public IDependencyContainer RegisterInstance(object instance)
    {
      if (instance == null) throw new ArgumentNullException(nameof(instance));
      return RegisterInstance(instance.GetType(), instance);
    }

    /// <summary>
    ///   Register an existing instance of type T.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    /// <param name="instance">The instance to register.</param>
    /// <returns>The current instance of the container.</returns>
    public IDependencyContainer RegisterInstance<T>(object instance)
      where T : class
    {
      if (instance == null) throw new ArgumentNullException(nameof(instance));
      return RegisterInstance(typeof (T), instance);
    }

    /// <summary>
    ///   Register a singleton of a type.
    /// </summary>
    /// <typeparam name="T">The type to register.</typeparam>
    /// <returns>The current instance of the container.</returns>
    public IDependencyContainer RegisterSingleton<T>()
      where T : class
    {
      if (_disposed) throw new ObjectDisposedException(GetType().FullName);

      var typeOf = typeof (T);

      List<Func<ChlyContainer, Type, object>> creators;
      var exists = _store.TryGetValue(typeOf, out creators);

      if (!exists)
      {
        var creator = GetCreator(typeOf);
        var instance = creator(this, typeOf);
        _store.Add(typeOf, creators = new List<Func<ChlyContainer, Type, object>>());
        creators.Add((c, t) => instance);
        RegisterForDisposing(instance);
      }

      return this;
    }

    #endregion Registration

    #region Disposing

    /// <summary>
    ///   Disposes the current instance and all registered instances implementing <see cref="IDisposable" />.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    ///   Calls all registered delegates for the Disposing event.
    /// </summary>
    /// <param name="ea">Event arguments for the Disposing event.</param>
    protected void OnDisposing(EventArgs ea)
    {
      var handler = Disposing;
      handler?.Invoke(this, ea);
    }

    /// <summary>
    ///   Dispose pattern specific for this class. Fires the Disposing event.
    /// </summary>
    /// <param name="disposing">True if called by consumers. False if called by the <c>finalizer</c>.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (_disposed) return;

      // Free managed objects here
      if (disposing)
      {
        OnDisposing(EventArgs.Empty);
      }

      // Free unmanaged objects here
      _disposed = true;
    }

    #endregion Disposing
  }
}