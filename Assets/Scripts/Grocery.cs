using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public enum GROCERYK // tag = grock
{
    Milk,
    Beer,
    //Cheese,
    //Veggies,
    //Condiments,
    //Fruit,
    //Yogurt,
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
    public GameObject _prefabDesignationButton;
    public GROCERYK _grock;
    
    // Player will set this through UI, which will then control what JOBKs get set
    public DESIGNATIONK _desk;
    JobManager _jobm;
    JobSite _job;

    List<GameObject> _lObjButton;

    public int _width;
    public int _iSlot;
    public int _iShelf;

    // Start is called before the first frame update
    void Start()
    {
        //_desk = DESIGNATIONK.None;
        _job = null;
        _jobm = null;

        _lObjButton = new List<GameObject>();
    }

    static int CFoodInitialFromGrock(GROCERYK grock)
    {
        switch (grock)
        {
            case GROCERYK.Milk:
                return 10;

            case GROCERYK.Beer:
                return 0;
        }

        return 0;
    }

    static int CWorkRequiredFromGrock(GROCERYK grock)
    {
        switch (grock)
        {
            case GROCERYK.Milk:
                return 20;

            case GROCERYK.Beer:
                return 20;
        }

        return 0;
    }

    static int CWarmBedsFromGrock(GROCERYK grock)
    {
        switch (grock)
        {
            case GROCERYK.Milk:
                return 5;

            case GROCERYK.Beer:
                return 5;
        }

        return 0;
    }

    static List<DESIGNATIONK> LDeskFromGrock(GROCERYK grock)
    {
        switch (grock)
        {
            case GROCERYK.Milk:
                return new List<DESIGNATIONK> { DESIGNATIONK.None, DESIGNATIONK.BuildHomes, DESIGNATIONK.CollectFood, DESIGNATIONK.StoreFood};
            case GROCERYK.Beer:
                return new List<DESIGNATIONK> { DESIGNATIONK.None, DESIGNATIONK.BuildHomes, DESIGNATIONK.StoreFood };
        }

        return new List<DESIGNATIONK> { DESIGNATIONK.None };
    }

    void DebugDrawText()
    {
        Text text = GetComponent<Text>();
        if (text == null)
            return;

        text.text = _grock.ToString() + "\n\n";
        text.text += "    Designation: " + _desk.ToString() + "\n";
        text.text += "    Current Job: ";
        if (_job != null)
        {
            text.text += _job._jobk.ToString() + "\n";

            switch (_job._jobk)
            {
                case JOBK.Build:
                    text.text += string.Format("    Work: {0} / {1}", _job._mpReskCRes[RESOURCEK.Work], CWorkRequiredFromGrock(_grock));
                    break;
                case JOBK.WarmHome:
                    text.text += string.Format("    Available Beds: {0}", _job._mpReskCRes[RESOURCEK.WarmBed]);
                    break;
                case JOBK.CollectFood:
                case JOBK.StoreFood:
                    text.text += string.Format("    Food: {0}", _job._mpReskCRes[RESOURCEK.Food]);
                    break;
            }
        }
        else
            text.text += "None\n";
    }

    // Update is called once per frame
    void Update()
    {
        if (_jobm == null)
            _jobm = GameObject.Find("Game Manager").GetComponent<Game>()._jobManager;


        DebugDrawText();

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

    public void OnButtonClick()
    {
        RectTransform rtrans = GetComponent<RectTransform>(); 

        List<DESIGNATIONK> lDesk = LDeskFromGrock(_grock);
        for (int iDesk = 0; iDesk < lDesk.Count; ++iDesk)
        {
            DESIGNATIONK desk = lDesk[iDesk];
            float u = (float)iDesk / (float)lDesk.Count;

            Vector3 vecOffset = new Vector3(Mathf.Sin(u * Mathf.PI * 2), Mathf.Cos(u * Mathf.PI * 2), 0.0f) * 150;
            Debug.Log(_prefabDesignationButton.GetComponent<DesignationButton>()._desk.ToString());
            GameObject objButton = Instantiate(_prefabDesignationButton, rtrans.position + vecOffset, new Quaternion(), GetComponentInParent<Transform>());
            DesignationButton desbtn = objButton.GetComponent<DesignationButton>();
            desbtn._desk = desk;
            desbtn._grocTarget = this;

            _lObjButton.Add(objButton);
        }

        GetComponent<Button>().interactable = false;
    }

    public void SetDesignation(DESIGNATIONK desk)
    {
        _desk = desk;
        foreach (GameObject obj in _lObjButton)
        {
            Destroy(obj);
        }
     
        GetComponent<Button>().interactable = true;

        if (_job != null)
        {
            _jobm.RemoveJob(_job);
            _job = null;
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
}
