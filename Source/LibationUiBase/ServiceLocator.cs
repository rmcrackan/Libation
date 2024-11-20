using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Prism.Events;

namespace LibationUiBase;

public static class ServiceLocator
{
    private static readonly ServiceCollection MutableContainer = new();
    private static ServiceProvider _container;
    
    /// <summary>
    /// Registers a type that when requested, will always return the same, single instance.
    /// </summary>
    /// <param name="t"></param>
    public static void RegisterSingleton(Type t)
    {
        CheckContainerNotBuilt();
        if (!TypeExists(t))
            MutableContainer.AddSingleton(t);
    }

    /// <summary>
    /// Registers a type that when requested, will always return the same, single instance.
    /// </summary>
    public static void RegisterSingleton<T>() where T : class
    {
        CheckContainerNotBuilt();
        if (!TypeExists(typeof(T)))
            MutableContainer.AddSingleton(typeof(T));
    }

    /// <summary>
    /// Registers a type that when requested, will always return the same, single instance.
    /// </summary>
    public static void RegisterSingleton<TInterface, TImplementation>() 
        where TInterface : class
        where TImplementation : TInterface
    {
        CheckContainerNotBuilt();
        if (!TypeExists(typeof(TInterface)))
            MutableContainer.AddSingleton(typeof(TInterface), typeof(TImplementation));
    }


    /// <summary>
    /// Registers a singleton but using a certain instance.
    /// </summary>
    /// <param name="obj"></param>
    public static void RegisterInstance<TInterface, TImplementation>(object obj)
        where TInterface : class
        where TImplementation : TInterface
    {
        CheckContainerNotBuilt();
        if (!TypeExists(typeof(TInterface)))
            MutableContainer.AddSingleton<TInterface>(_ => (TImplementation)obj);
    }

    /// <summary>
    /// Registers a type that gets a new instance every 
    /// call to Get().
    /// </summary>
    /// <param name="t"></param>
    public static void RegisterTransient(Type t)
    {
        CheckContainerNotBuilt();
        if (!TypeExists(t))
            MutableContainer.AddTransient(t);
    }

    /// <summary>
    /// Registers a type that gets a new instance every 
    /// call to Get().
    /// </summary>
    public static void RegisterTransient<T>()
    {
        CheckContainerNotBuilt();
        if (!TypeExists(typeof(T)))
            MutableContainer.AddTransient(typeof(T));
    }

    public static void RegisterTransient<TInterface, TImplementation>()
    {
        CheckContainerNotBuilt();
        if (!TypeExists(typeof(TInterface)))
            MutableContainer.AddTransient(typeof(TInterface), typeof(TImplementation));
    }



    /// <summary>
    /// Call once at app startup after add all required types.
    /// </summary>
    public static void AddCommonServicesAndBuild()
    {
        CheckContainerNotBuilt();

        // Add common, cross-platform services here.
        RegisterSingleton<IEventAggregator, EventAggregator>();

        // Finalize and build container, mutableContainer should
        // no longer be used.
        _container = MutableContainer.BuildServiceProvider();
    }

    public static T Get<T>()
    {
        CheckContainerBuilt();
        return _container.GetService<T>();
    }

    public static object Get(Type type)
    {
        CheckContainerBuilt();
        if (!TypeExists(type))
            throw new InvalidOperationException($"Type {type.Name} has not been registered.");
        return _container.GetService(type);
    }

    private static bool TypeExists(Type type)
    {
        var exists = MutableContainer.Any(sd => sd.ServiceType == type);
        if (exists) 
            Serilog.Log.Logger.Warning($"Type already registered: {type.Name}");
        return exists;
    }

    private static void CheckContainerNotBuilt()
    {
        if (_container != null)
            throw new Exception("Container already built!");
    }

    private static void CheckContainerBuilt()
    {
        if (_container == null)
            throw new Exception("Container not yet built!");
    }
}