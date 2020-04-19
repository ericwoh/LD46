using System;
using System.Collections;
using System.Collections.Generic;
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
    StoreFood,
    CollectFood,
    BuildHomes,
}

public class Grocery : MonoBehaviour
{
    public GameObject _grocuiPrefab;

    public GROCERYK _grock;
    
    // Player will set this through UI, which will then control what JOBKs get set
    public DESIGNATIONK _desk;
    JobManager _jobm;
    public JobSite _job;

    public int _width;
    public int _height;
    public int _iSlot;
    public int _iShelf;

    // Start is called before the first frame update
    void Start()
    {
        _job = null;
        _jobm = null;

        GameObject canvas = GameObject.Find("Canvas");
        GameObject objUi = Instantiate(_grocuiPrefab, canvas.GetComponent<Transform>());
        objUi.GetComponent<GroceryUi>().SetGrocery(this);
    }

    static int CFoodInitialFromGrock(GROCERYK grock)
    {
        switch (grock)
        {
            case GROCERYK.Milk:
                return 10;

            case GROCERYK.Apples:
                return 0;
        }

        return 0;
    }

    public static int CWorkRequiredFromGrock(GROCERYK grock)
    {
        switch (grock)
        {
            case GROCERYK.Milk:
                return 20;

            case GROCERYK.Apples:
                return 20;
        }

        return 0;
    }

    public static int CWarmBedsFromGrock(GROCERYK grock)
    {
        switch (grock)
        {
            case GROCERYK.Milk:
                return 5;

            case GROCERYK.Apples:
                return 5;
        }

        return 0;
    }

    public static List<DESIGNATIONK> LDeskFromGrock(GROCERYK grock)
    {
        switch (grock)
        {
            case GROCERYK.Milk:
                return new List<DESIGNATIONK> { DESIGNATIONK.None, DESIGNATIONK.BuildHomes, DESIGNATIONK.CollectFood, DESIGNATIONK.StoreFood};
            case GROCERYK.Apples:
                return new List<DESIGNATIONK> { DESIGNATIONK.None, DESIGNATIONK.BuildHomes, DESIGNATIONK.StoreFood };
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
            switch (_desk)
            {
                case DESIGNATIONK.StoreFood:
                    _job = new JobSite(JOBK.StoreFood);
                    break;
                case DESIGNATIONK.CollectFood:
                    _job = new JobSite(JOBK.CollectFood);
                    _job._mpReskCRes[RESOURCEK.Food] = CFoodInitialFromGrock(_grock);
                    break;
                case DESIGNATIONK.BuildHomes:
                    _job = new JobSite(JOBK.Build);
                    break;
            }

            _jobm.AddJob(_job);
        }

        if (_desk == DESIGNATIONK.None)
            return;

        switch (_desk)
        {
            case DESIGNATIONK.StoreFood:
                break;

            case DESIGNATIONK.CollectFood:
                break;

            case DESIGNATIONK.BuildHomes:
                if (_job._jobk == JOBK.Build)
                {
                    if (_job._mpReskCRes[RESOURCEK.Work] > CWorkRequiredFromGrock(_grock))
                    {
                        // done building!
                        _jobm.RemoveJob(_job);

                        // we can have beds now!
                        _job = new JobSite(JOBK.WarmHome);
                        _job._mpReskCRes[RESOURCEK.WarmBed] = CWarmBedsFromGrock(_grock);
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
            _jobm.RemoveJob(_job);
            _job = null;
        }
    }
}
