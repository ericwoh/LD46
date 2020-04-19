using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "LD46/GroceryManagerSettings")]
public class GroceryManagerSettings : ScriptableObject
{
    public List<GameObject> _lPrefabGrocery;
    public int _cShelf = 3;
    public int _cSlotPerShelf = 20;

    public float _tAddGroceries = 60.0f;
    public float _tRemoveGroceries = 15.0f;
};




public class Shelf
{
    public List<GameObject> _lObjGrocery;
    public int _iShelf;
    public int _cSlot;

    public Shelf(int iShelf, int cSlot)
    {
        _lObjGrocery = new List<GameObject>();
        _iShelf = iShelf;
        _cSlot = cSlot;
    }

    public bool FTryGetNextAvailableSlot(ref int iSlot, ref int width)
    {
        width = -1;
        for (int iObj = 0; iObj < _lObjGrocery.Count; ++iObj)
        {
            GameObject obj = _lObjGrocery[iObj];
            Grocery groc = obj.GetComponent<Grocery>();
            if (groc._iSlot > iSlot)
            {
                width = groc._iSlot - iSlot;
                break;
            }
            else
            {
                iSlot = groc._iSlot + groc._width;
            }
        }

        if (iSlot < _cSlot)
        {
            if (width == -1)
            {
                width = _cSlot - iSlot;
            }

            return true;
        }

        return false;
    }

    public void AddGrocery(GameObject obj)
    {
        _lObjGrocery.Add(obj);

        _lObjGrocery.Sort(
            delegate (GameObject objLhs, GameObject objRhs)
            {
                return objLhs.GetComponent<Grocery>()._iSlot - objRhs.GetComponent<Grocery>()._iSlot;
            });
    }

    public void RemoveRandomGroceries(int cGrocery)
    {
        for (int i = 0; i < cGrocery; ++i)
        {
            int iObjRemove = Random.Range(0, _lObjGrocery.Count);
            GameObject obj = _lObjGrocery[iObjRemove];
            Object.Destroy(obj);
            _lObjGrocery.RemoveAt(iObjRemove);
        }
    }
}

public class GroceryManager
{
    private List<GameObject> _lPrefabGrocery;
    List<Shelf> _lShelf;
    float _tLastGroceryAdd = -1000.0f;
    float _tLastGroceryRemove = -1000.0f;

    float _tAddGroceries = 60.0f;
    float _tRemoveGroceries = 15.0f;

    public bool _fButtonsDisabled = false;

    public GroceryManager(GroceryManagerSettings grocmsetting)
    {
        _lPrefabGrocery = grocmsetting._lPrefabGrocery;
        _lShelf = new List<Shelf>();
        for (int iShelf = 0; iShelf < grocmsetting._cShelf; ++iShelf)
        {
            _lShelf.Add(new Shelf(iShelf, grocmsetting._cSlotPerShelf));
        }

        _tAddGroceries = grocmsetting._tAddGroceries;
        _tRemoveGroceries = grocmsetting._tRemoveGroceries;
    }

    public void tick()
    {
        if (Time.time - _tLastGroceryAdd > _tAddGroceries)
        {
            SpawnGroceries();
            _tLastGroceryAdd = Time.time;
            _tLastGroceryRemove = Time.time;
        }

        if (Time.time - _tLastGroceryRemove > _tRemoveGroceries)
        {
            ChooseAndRemoveGroceries();
            _tLastGroceryRemove = Time.time;
        }
    }

    static void Shuffle<T>(IList<T> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
            Swap(list, 0, Random.Range(0, i));
    }

    static void Swap<T>(IList<T> list, int i, int j)
    {
        var temp = list[i];
        list[i] = list[j];
        list[j] = temp;
    }

    GameObject ObjCreateGroceryBySize(int width)
    {
        if (_lPrefabGrocery.Count > 1)
            Shuffle(_lPrefabGrocery);

        foreach (GameObject obj in _lPrefabGrocery)
        {
            if (obj.GetComponent<Grocery>()._width <= width)
            {
                GameObject objNew = Object.Instantiate(obj);
                return objNew;
            }
        }

        return null;
    }

    void SpawnGroceries()
    {
        foreach (Shelf shelf in _lShelf)
        {
            int iSlot = 0;
            int width = 0;

            while (true)
            {
                if (shelf.FTryGetNextAvailableSlot(ref iSlot, ref width))
                {
                    GameObject obj = ObjCreateGroceryBySize(width);
                
                    // BB (ericw) Should we leave some empty space occasionally?

                    if (obj != null)
                    {
                        obj.GetComponent<Grocery>().SetISlotAndIShelf(iSlot, shelf._iShelf);
                        shelf.AddGrocery(obj);
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }

    void ChooseAndRemoveGroceries()
    {
        foreach (Shelf shelf in _lShelf)
        {
            // BB (ericw) Change this over time for difficulty?

            shelf.RemoveRandomGroceries(Random.Range(0, 2)); 
        }
    }
}