using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// The central registry that contains references to all DefDatabases (1 for each Def-type).
/// <br/>Functions that should be run on all DefDatases should only be executed through this registry.
/// </summary>
public static class DefDatabaseRegistry
{
    // Stores all DefDatabase types registered
    private static readonly List<Type> registeredDefDatabases = new List<Type>();

    // Cached list of DefOf classes so that we don't search for them every time.
    private static List<Type> cachedDefOfClasses;

    /// <summary>
    /// Adds all Defs that are defined in the framework and are useful for all projects to their respective DefDatabases.
    /// </summary>
    public static void AddAllDefs()
    {
        ClearAllDatabases();
    }

    // Called when a DefDatabase<T> type is accessed for the first time
    public static void RegisterDefDatabase(Type defDatabaseType)
    {
        if (!registeredDefDatabases.Contains(defDatabaseType))
        {
            registeredDefDatabases.Add(defDatabaseType);
        }
    }

    // Calls Clear on each registered DefDatabase type
    public static void ClearAllDatabases()
    {
        foreach (Type defDatabaseType in registeredDefDatabases)
        {
            // Invoke the static ResolveReferences method
            MethodInfo resolveMethod = defDatabaseType.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
            resolveMethod?.Invoke(null, null);
        }
    }

    // Calls ResolveReferences on each registered DefDatabase type
    public static void ResolveAllReferences()
    {
        foreach (Type defDatabaseType in registeredDefDatabases)
        {
            // Invoke the static ResolveReferences method
            MethodInfo resolveMethod = defDatabaseType.GetMethod("ResolveReferences", BindingFlags.Static | BindingFlags.Public);
            resolveMethod?.Invoke(null, null);
        }
    }

    // Calls OnLoadingDone on each registered DefDatabase type
    public static void OnLoadingDone()
    {
        foreach (Type defDatabaseType in registeredDefDatabases)
        {
            // Invoke the static OnLoadingDone method
            MethodInfo resolveMethod = defDatabaseType.GetMethod("OnLoadingDone", BindingFlags.Static | BindingFlags.Public);
            resolveMethod?.Invoke(null, null);
        }

        DefDumpUtility.DumpAllDefs();
    }

    /// <summary>
    /// Searches all assemblies for types marked with the DefOf attribute and returns them.
    /// The result is cached since these types do not change at runtime.
    /// </summary>
    /// <returns>A list of Types that have the DefOf attribute.</returns>
    private static List<Type> GetAllDefOfClasses()
    {
        if (cachedDefOfClasses == null)
        {
            cachedDefOfClasses = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.GetCustomAttributes(typeof(DefOf), inherit: false).Any())
                    {
                        cachedDefOfClasses.Add(type);
                    }
                }
            }
        }
        return cachedDefOfClasses;
    }

    /// <summary>
    /// Immediately binds the provided Def instance to all static fields in DefOf classes
    /// where the field name matches the Def's DefName and the field type is compatible with the Def.
    /// This ensures that as soon as a Def is loaded, it is available via its corresponding DefOf.
    /// </summary>
    /// <param name="def">The Def instance to bind.</param>
    public static void BindDefToAllDefOfs(Def def)
    {
        foreach (Type defOfClass in GetAllDefOfClasses())
        {
            FieldInfo[] fields = defOfClass.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                // If the field name matches the Def's DefName and the field's type is compatible with the def's type...
                if (field.Name == def.DefName && field.FieldType.IsAssignableFrom(def.GetType()))
                {
                    field.SetValue(null, def);
                }
            }
        }
    }
}

