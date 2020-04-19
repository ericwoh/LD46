using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "LD46/Grocery Manager Settings")]
public class GroceryManagerSettings : ScriptableObject
{
    public List<GameObject> _lPrefabGrocery;
    public int _cShelf = 3;
    public int _cSlotPerShelf = 20;

    public float _tAddGroceries = 60.0f;
    public float _tRemoveGroceries = 15.0f;
};