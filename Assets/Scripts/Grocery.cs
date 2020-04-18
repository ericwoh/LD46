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
    public GROCERYK _grock;
    
    // Player will set this through UI, which will then control what JOBKs get set
    public DESIGNATIONK _desk;
    JobManager _jobm;
    JobSite _job;

    // Start is called before the first frame update
    void Start()
    {
        //_desk = DESIGNATIONK.None;
        _jobm = GameObject.Find("JobManager").GetComponent<JobManager>();
        _job = null;
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

        switch(_desk)
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
}
