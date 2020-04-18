using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scriptable object base class that can be used to define
/// well-defined sets of objects through data. For example,
/// AllActiveNPCs can be used to list all NPCs, and NPCs would 
/// register/unregister on Enable/Disable.
/// It's like a singleton but better, and fully data-driven.
/// Refer to YouTube talk for reference: https://www.youtube.com/watch?v=raQ3iHhE_Kk
/// </summary>
/// <typeparam name="T">Type of the object in this set</typeparam>
public abstract class RuntimeSet<T> : ScriptableObject
{
    // actual list of objects in this set
    public List<T> mItems = new List<T>();

    /// <summary>
    /// Public method used to add an item from the list
    /// </summary>
    /// <param name="t">Item to be added if present</param>
    public void Add(T t)
    {
        if (!mItems.Contains(t)) mItems.Add(t);
    }

    /// <summary>
    /// Public method used to remove an item from the list
    /// </summary>
    /// <param name="t">Item to be removed if present</param>
    public void Remove(T t)
    {
        if (mItems.Contains(t)) mItems.Remove(t);
    }
}