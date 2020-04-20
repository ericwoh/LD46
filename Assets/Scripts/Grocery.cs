using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public enum GROCERYK // tag = grock
{
    None,
    Milk,
    Apples,
    Broccoli,
    Jar,
    Eggs,
}

public enum DESIGNATIONK // tag = desk
{
    None,

    CollectFood,
    StoreFood,
    BuildHomes,

    Max,
}

public class Grocery : MonoBehaviour
{
    public GameObject _grocuiPrefab;

    public GROCERYK _grock;
    
    // Player will set this through UI, which will then control what JOBKs get set
    public DESIGNATIONK _desk;
    JobManager _jobm;
    public JobSite _job;

    public Dictionary<RESOURCEK, int> _mpReskCRes;

    public int _width;
    public int _height;
    public int _iSlot;
    public int _iShelf;

    GameObject _objUi = null;
    GameObject _objBuilding = null;

    Color _colorBase = Color.white;

    // Start is called before the first frame update
    void Start()
    {
        _job = null;
        _jobm = null;

        GameObject canvas = GameObject.Find("Canvas");
        _objUi = Instantiate(_grocuiPrefab, canvas.GetComponent<Transform>());
        _objUi.GetComponent<GroceryUi>().SetGrocery(this);

        _mpReskCRes = new Dictionary<RESOURCEK, int>();
        _mpReskCRes[RESOURCEK.Food] = 0;
        _mpReskCRes[RESOURCEK.WarmBed] = 0;
        _mpReskCRes[RESOURCEK.Work] = 0;

        switch (_grock)
        {
            case GROCERYK.Apples:
            case GROCERYK.Broccoli:
                _mpReskCRes[RESOURCEK.Food] = _width * _height * CFoodPerSlotFromGrock(_grock); ;
                break;
            case GROCERYK.Milk:
            case GROCERYK.Jar:
            case GROCERYK.Eggs:
                break;
        }
    }

    public void SetFade(float uFade)
    {
        if (_job != null && _job._jobk == JOBK.CollectFood)
        {
            int cFoodMax = _width * _height * CFoodPerSlotFromGrock(_grock);
            int cFood = _job._mpReskCRes[RESOURCEK.Food];
        
            if (cFood > 0)
                _colorBase = Color.Lerp(Color.white, new Color(0.7f, 0.5f, 0.2f), 1.0f - (float)cFood / (float)cFoodMax);
            else
                _colorBase = new Color(0.5f, 0.3f, 0.1f);
        }

        float u = Mathf.Lerp(0.6f, 1.0f, uFade);
        GetComponent<SpriteRenderer>().color = new Color(_colorBase.r * u, _colorBase.g * u, _colorBase.b * u);
    }

    private void OnDestroy()
    {
        Destroy(_objUi);
        if (_objBuilding != null)
            Destroy(_objBuilding);

        if (_job != null)
        {
            _jobm.RemoveJob(_job);
        }
    }

    public static int CWorkRequiredPerSlotFromGrock(GROCERYK grock)
    {
        switch (grock)
        {
            case GROCERYK.Milk:
            case GROCERYK.Apples:
            case GROCERYK.Broccoli:
            case GROCERYK.Jar:
            case GROCERYK.Eggs:
                return 3;
        }

        return 0;
    }

    public static int CWarmBedsPerSlotFromGrock(GROCERYK grock)
    {
        switch (grock)
        {
            case GROCERYK.Milk:
            case GROCERYK.Apples:
            case GROCERYK.Broccoli:
            case GROCERYK.Jar:
            case GROCERYK.Eggs:
                return 1;
        }

        return 0;
    }

    public static int CFoodPerSlotFromGrock(GROCERYK grock)
    {
        switch (grock)
        {
            case GROCERYK.Milk:
            case GROCERYK.Apples:
            case GROCERYK.Broccoli:
            case GROCERYK.Jar:
            case GROCERYK.Eggs:
                return 5;
        }

        return 0;
    }

    public static List<DESIGNATIONK> LDeskFromGrock(GROCERYK grock)
    {
        switch (grock)
        {
            case GROCERYK.Milk:
            case GROCERYK.Jar:
            case GROCERYK.Eggs:
                return new List<DESIGNATIONK> { DESIGNATIONK.None, DESIGNATIONK.BuildHomes, DESIGNATIONK.StoreFood };
            case GROCERYK.Apples:
            case GROCERYK.Broccoli:
                return new List<DESIGNATIONK> { DESIGNATIONK.None, DESIGNATIONK.CollectFood, DESIGNATIONK.BuildHomes, DESIGNATIONK.StoreFood };
        }

        return new List<DESIGNATIONK> { DESIGNATIONK.None };
    }

    // Update is called once per frame
    void Update()
    {
        if (_jobm == null)
            _jobm = GameObject.Find("Game Manager").GetComponent<Game>()._jobManager;


        if (_job == null && _desk != DESIGNATIONK.None)
        {
            Vector3 vecDoorOffset = new Vector3((int)(((_width * 0.5f) - 1)) + 0.5f, 0.0f, 0.0f);
            switch (_desk)
            {
                case DESIGNATIONK.StoreFood:
                    _job = new JobSite(JOBK.StoreFood, transform, vecDoorOffset);
                    _job._mpReskCResLimit[RESOURCEK.Food] = _width * _height * CFoodPerSlotFromGrock(_grock);
                    break;
                case DESIGNATIONK.CollectFood:
                    _job = new JobSite(JOBK.CollectFood, transform, vecDoorOffset);
                    _job._mpReskCRes[RESOURCEK.Food] = _mpReskCRes[RESOURCEK.Food];
                    _job._mpReskCResLimit[RESOURCEK.Food] = _width * _height * CFoodPerSlotFromGrock(_grock);
                    break;
                case DESIGNATIONK.BuildHomes:
                    _job = new JobSite(JOBK.Build, transform, vecDoorOffset);
                    _job._mpReskCResLimit[RESOURCEK.Work] = _width * _height * CWorkRequiredPerSlotFromGrock(_grock);
                    break;
            }

            _jobm.AddJob(_job);
            Debug.Log("Job Manager adding Job: ");
        }

        if (_desk == DESIGNATIONK.None)
            return;

        switch (_desk)
        {
            case DESIGNATIONK.StoreFood:
                {
                    Debug.Assert(_job._jobk == JOBK.StoreFood);

                    GroceryManager grocm = GameObject.Find("Game Manager").GetComponent<Game>()._grocm;

                    if (_objBuilding == null)
                    {
                        GameObject prefabBuildingType = grocm._lPrefabBuildingType[UnityEngine.Random.Range(0, grocm._lPrefabBuildingType.Count)];
                        _objBuilding = Instantiate(prefabBuildingType, transform.position, transform.rotation, transform);
                        _objBuilding.GetComponent<Building>().mBuildingWidth = _width;
                        _objBuilding.GetComponent<Building>().mBuildingHeight = _height;
                        _objBuilding.GetComponent<Building>().ClearAndRefresh();
                    }

                    int cSlot = _objBuilding.GetComponent<Building>().NumSlots();
                    int cSlotMax = _objBuilding.GetComponent<Building>().NumSlotsMax();
                    int cFoodPerSlot = CFoodPerSlotFromGrock(_grock);

                    if (_job._mpReskCRes[RESOURCEK.Food] >= (cSlot + 1) * cFoodPerSlot)
                    {
                        _objBuilding.GetComponent<Building>().BuildNewModule();
                    }
                }
                break;

            case DESIGNATIONK.CollectFood:
                break;

            case DESIGNATIONK.BuildHomes:
                if (_job._jobk == JOBK.Build)
                {
                    GroceryManager grocm = GameObject.Find("Game Manager").GetComponent<Game>()._grocm;

                    if (_objBuilding == null)
                    {
                        GameObject prefabBuildingType = grocm._lPrefabBuildingType[UnityEngine.Random.Range(0, grocm._lPrefabBuildingType.Count)];
                        _objBuilding = Instantiate(prefabBuildingType, transform.position, transform.rotation, transform);
                        _objBuilding.GetComponent<Building>().mBuildingWidth = _width;
                        _objBuilding.GetComponent<Building>().mBuildingHeight = _height;
                        _objBuilding.GetComponent<Building>().ClearAndRefresh();
                    }

                    Building building = _objBuilding.GetComponent<Building>();
                    int cSlot = building.NumSlots();
                    int cSlotMax = building.NumSlotsMax();
                    int cWorkRequiredPerSlot = CWorkRequiredPerSlotFromGrock(_grock);

                    if (_job._mpReskCRes[RESOURCEK.Work] > (cSlot + 1) * cWorkRequiredPerSlot)
                    {
                        building.BuildNewModule();
                    }

                    if (_job._mpReskCRes[RESOURCEK.Work] >= cSlotMax * cWorkRequiredPerSlot)
                    {
                        while (building.NumSlots() != building.NumSlotsMax())
                        {
                            building.BuildNewModule();
                        }

                        // done building!
                        _jobm.RemoveJob(_job);

                        // we can have beds now!
                        Vector3 vecDoorOffset = new Vector3((int)(((_width * 0.5f) - 1)) + 0.5f, 0.0f, 0.0f);
                        _job = new JobSite(JOBK.WarmHome, transform, vecDoorOffset);
                        _job._mpReskCRes[RESOURCEK.WarmBed] = CWarmBedsPerSlotFromGrock(_grock) * cSlot;

                        Debug.Log("Job Manager adding Job: ");
                        _jobm.AddJob(_job);
                    }
                }
                break;
        }
    }

    public void SetISlotAndIShelf(int iSlot, int iShelf)
    {
        _iSlot = iSlot;
        _iShelf = iShelf;

        float y = 0.0f;

        // too tired to do real math...

        switch (_iShelf)
        {
            case 0: y = -7.0f; break;
            case 1: y = -2.0f; break;
            case 2: y =  3.0f; break;
        }

        GetComponent<Transform>().position = new Vector3(_iSlot - 7.5f, y, 10.0f);
    }

    public void SetDesignation(DESIGNATIONK desk)
    {
        _desk = desk;

        if (_job != null)
        {
            switch (_job._jobk)
            {
                case JOBK.Build:
                    break;
                case JOBK.StoreFood:
                    _mpReskCRes[RESOURCEK.Food] = _job._mpReskCRes[RESOURCEK.Food];
                    break;
                case JOBK.CollectFood:
                    _mpReskCRes[RESOURCEK.Food] = _job._mpReskCRes[RESOURCEK.Food];
                    break;
                case JOBK.WarmHome:
                    break;
            }

            _jobm.RemoveJob(_job);
            _job = null;
        }

        if (_objBuilding != null)
        {
            Destroy(_objBuilding);
            _objBuilding = null;
        }
    }
}
