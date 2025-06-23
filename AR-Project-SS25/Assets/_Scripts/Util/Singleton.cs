using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this as T;
        
        // Debug.Log($"Started INSTANCE {typeof(T).Name}");
    }
}

public interface ISingleton { }

public static class SingletonRegistry
{
    private static Dictionary<System.Type, ISingleton> instances = new();

    public static void Register<T>(T instance) where T : class, ISingleton
    {
        instances[typeof(T)] = instance;
    }

    public static T Get<T>() where T : class, ISingleton
    {
        return instances.TryGetValue(typeof(T), out var inst) ? inst as T : null;
    }
}
