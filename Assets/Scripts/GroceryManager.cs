using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

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
                iSlot = Mathf.Max(iSlot, groc._iSlot + groc._width);
            }
        }

        if (iSlot < _cSlot)
        {
            int rand = Random.Range(0, 2);
            if (rand != 0)
            {
                iSlot++;
                width--;
            }

            if (width < 0)
            {
                width = _cSlot - iSlot;
            }

            return iSlot < _cSlot;
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
        int cSkip = 0;

        for (int i = 0; i < cGrocery; ++i)
        {
            int iObjRemove = Random.Range(0, _lObjGrocery.Count);
            if (iObjRemove < _lObjGrocery.Count)
            {
                GameObject obj = _lObjGrocery[iObjRemove];

                if (cSkip < 3 && obj.GetComponent<Grocery>()._desk != DESIGNATIONK.None)
                {
                    // Try and avoid nuking things the player is using, but only a little bit

                    i--;
                    cSkip++;
                    continue;
                }

                obj.GetComponent<Grocery>().SetDying();
            }
        }
    }
}

public class GroceryManager
{
    private List<GameObject> _lPrefabGrocery;
    public List<GameObject> _lPrefabBuildingType;

    List<Shelf> _lShelf;
    float _tLastGroceryAdd = -1000.0f;
    float _tLastGroceryRemove = -1000.0f;

    float _tAddGroceries = 60.0f;
    float _tRemoveGroceries = 15.0f;

    int _cSlotPerShelf;

    public bool _fButtonsDisabled = false;
    bool _fFirstTime = true;

    public GroceryManager(GroceryManagerSettings grocmsetting)
    {
        _lPrefabGrocery = grocmsetting._lPrefabGrocery;
        _lPrefabBuildingType = grocmsetting._lPrefabBuildingType;

        _lShelf = new List<Shelf>();
        for (int iShelf = 0; iShelf < grocmsetting._cShelf; ++iShelf)
        {
            _lShelf.Add(new Shelf(iShelf, grocmsetting._cSlotPerShelf));
        }

        _cSlotPerShelf = grocmsetting._cSlotPerShelf;

        _tAddGroceries = grocmsetting._tAddGroceries;
        _tRemoveGroceries = grocmsetting._tRemoveGroceries;
    }

    public void tick()
    {
        if (_fFirstTime)
        {
            // make sure the player at least gets a chance at some food

            _fFirstTime = false;

            foreach (Shelf shelf in _lShelf)
            {
                int cFood = Random.Range(0, 1);
                for (int iFood = 0; iFood < cFood; ++iFood)
                {
                    GameObject obj = ObjCreateGroceryBySize(1000, true);
                    obj.GetComponent<Grocery>().SetISlotAndIShelf(Random.Range(0, _cSlotPerShelf - obj.GetComponent<Grocery>()._width), shelf._iShelf);
                    shelf.AddGrocery(obj);
                }
            }
        }

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

    GameObject ObjCreateGroceryBySize(int width, bool fFoodOnly)
    {
        if (_lPrefabGrocery.Count > 1)
            Shuffle(_lPrefabGrocery);

        foreach (GameObject obj in _lPrefabGrocery)
        {
            Grocery groc = obj.GetComponent<Grocery>();

            if (fFoodOnly)
            {
                switch (groc._grock)
                {
                    case GROCERYK.Apples:
                    case GROCERYK.Broccoli:
                        break;
                    default:
                        continue;
                }
            }

            if (groc._width <= width)
            {
                GameObject objNew = Object.Instantiate(obj);
                return objNew;
            }
        }

        return null;
    }

    public void DestroyGrocery(GameObject obj)
    {
        foreach (Shelf shelf in _lShelf)
        {
            if (shelf._lObjGrocery.Contains(obj))
            {
                shelf._lObjGrocery.Remove(obj);
                Object.Destroy(obj);
            }
        }
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
                    GameObject obj = ObjCreateGroceryBySize(width, false);

                    if (obj != null)
                    {
                        obj.GetComponent<Grocery>().SetISlotAndIShelf(iSlot, shelf._iShelf);
                        shelf.AddGrocery(obj);
                    }
                    else
                    {
                        iSlot++;
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