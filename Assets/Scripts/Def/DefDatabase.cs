using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static class that manages collections of all Defs of a given type.
/// <br/>Provides methods to look up, retrieve, and list Defs by type or name.
/// </summary>
public static class DefDatabase<T> where T : Def
{
	// Called once when accessing a DefDatabase of a specific type T for the first time
	static DefDatabase()
	{
		// Register this DefDatabase<T> type in the registry
		DefDatabaseRegistry.RegisterDefDatabase(typeof(DefDatabase<T>));
	}

	private static List<T> defsList = new List<T>();
	private static Dictionary<string, T> defsByName = new Dictionary<string, T>();

	/// <summary>
	/// Provides a list of all Defs of this particular type.
	/// </summary>
	public static List<T> AllDefs => defsList;

	/// <summary>
	/// Adds a collection of defs to this database and then binds each Def to its matching DefOf field.
	/// This binding happens after all defs in the collection have been added.
	/// Should only be called at the very start of an application.
	/// </summary>
	/// <param name="defCollection">The collection of defs to add.</param>
	public static void AddDefs(List<T> defCollection)
	{
		foreach (T def in defCollection)
		{
			if (!def.Validate())
				throw new System.Exception("Loading Defs aborted due to an invalid Def");
			if (defsByName.ContainsKey(def.DefName))
				throw new System.Exception($"Def with name {def.DefName} has already been loaded for type {def.GetType()}.");

			defsList.Add(def);
			defsByName.Add(def.DefName, def);
		}

		// After adding all defs, bind each one to its corresponding DefOf field.
		foreach (T def in defCollection)
		{
			DefDatabaseRegistry.BindDefToAllDefOfs(def);
		}
	}

	public static bool ContainsDef(string defName)
    {
		if (defName == null) return false;
		return defsByName.ContainsKey(defName);
	}

	/// <summary>
	/// Clears the database.
	/// </summary>
	public static void Clear()
    {
		defsList.Clear();
		defsByName.Clear();
    }

	/// <summary>
	/// Resolves all references of all defs within the database so references within defs to other defs can be accessed correctly.
	/// <br/>Gets called through DefDatabaseRegistry.
	/// </summary>
	public static void ResolveReferences()
    {
		foreach (T def in defsList)
		{
			try
			{
				def.ResolveReferences();
			}
			catch (System.Exception e)
			{
				throw new System.Exception("Failed to resolve references for Def '" + def.DefName + "' of type " + def.GetType() + ": " + e.Message);

			}
		}
    }

	/// <summary>
	/// Gets called through DefDatabaseRegistry after all loading steps are done and DefOfs are bound.
	/// </summary>
	public static void OnLoadingDone()
    {
		Debug.Log("DefDatabase<" + typeof(T) + "> has loaded " + defsList.Count + " Defs.");
		foreach (T def in defsList)
		{
			try
			{
				def.OnLoadingDefsDone();
			}
			catch (System.Exception e)
			{
				throw new System.Exception("Failed OnLoadingDone for Def '" + def.DefName + "' of type " + def.GetType() + ": " + e.Message);
			}
		}
	}

	/// <summary>
	/// Returns the Def of the given name. Throws an exception if it does not exist.
	/// </summary>
	public static T GetNamed(string defName)
	{
		if (defsByName.TryGetValue(defName, out var value))
		{
			return value;
		}
		throw new System.Exception(string.Concat("Failed to find ", typeof(T), " named ", defName, "."));
	}
}

