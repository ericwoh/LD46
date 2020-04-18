using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShelfPos // tag = spos
{
    public int _iShelf; // index of shelf, 0 being the bottom shelf
    public float _uPos; // 0..1 value, 0 being max left of shelf, 1 being full right
}

public enum JOBK // tag = jobk
{
    WarmHome,
    StoreFood,
    CollectFood,
    Build,
}

public enum RESOURCEK // tag = resk
{
    Food,
    WarmBed,
    Work, // time?
}

public enum NEEDK // tag = needk
{
    Food,
    Warmth,
    // lubrication?
}



// Jobs are how the critters interact with groceries
public class JobSite // tag = job
{
    //public Grocery _grocery; // the grocery that created this job
    public ShelfPos _spos; // where this job will take place
    public Dictionary<RESOURCEK, int> _mpReskCRes; // a mapping of resource type to how many resources this jobsite has available
    public JOBK _jobk;

    public JobSite(JOBK jobk)
    {
        _mpReskCRes = new Dictionary<RESOURCEK, int>();
        _mpReskCRes[RESOURCEK.Food] = 0;
        _mpReskCRes[RESOURCEK.WarmBed] = 0;
        _mpReskCRes[RESOURCEK.Work] = 0;

        _jobk = jobk;
    }

    public bool FCanFulfillNeed(NEEDK needk) 
    {
        switch (_jobk)
        {
            case JOBK.CollectFood:
            case JOBK.StoreFood:
                if (needk == NEEDK.Food)
                    return _mpReskCRes[RESOURCEK.Food] > 0;
                return false;

            case JOBK.WarmHome:
                if (needk == NEEDK.Warmth)
                    return _mpReskCRes[RESOURCEK.WarmBed] > 0;
                return false;

            case JOBK.Build:
                return false;
        }

        Debug.LogError("Unexpected JOBK!");
        return false;
    }

    public bool FHasResource(RESOURCEK resk)
    {
        return _mpReskCRes[resk] > 0;
    }

    public bool FTryClaimResource(RESOURCEK resk)
    {
        if (_mpReskCRes[resk] > 0)
        {
            _mpReskCRes[resk]--;
            return true;
        }

        return false;
    }

    public void GiveResource(RESOURCEK resk)
    {
        _mpReskCRes[resk]++;
    }
}



// a discrete unit of work that can be complete by a critter
public enum TASKK
{
    // basic needs
    EatFood,
    GetWarm,

    // food management
    CollectFood,
    StoreFood,
    
    // construction
    Work,
}

public class Task // tag = task
{
    public TASKK _taskk;
    public JobSite _job; 

    public Task(JobSite job, TASKK taskk)
    {
        _taskk = taskk;
        _job = job;

        // Claim the resources now so that I'm guaranteed to get it
        switch (_taskk)
        {
            case TASKK.EatFood:
            case TASKK.CollectFood:
                {
                    bool fClaimed = _job.FTryClaimResource(RESOURCEK.Food);
                    Debug.Assert(fClaimed);
                    break;
                }

            case TASKK.GetWarm:
                {
                    bool fClaimed = _job.FTryClaimResource(RESOURCEK.WarmBed);
                    Debug.Assert(fClaimed);
                    break;
                }

            case TASKK.StoreFood:
            case TASKK.Work:
                break;
        }
    }

    public void CancelTask()
    {
        // unclaim resources
        switch (_taskk)
        {
            case TASKK.EatFood:
            case TASKK.CollectFood:
                _job.GiveResource(RESOURCEK.Food);
                break;

            case TASKK.GetWarm:
                _job.GiveResource(RESOURCEK.WarmBed);
                break;

            case TASKK.StoreFood:
            case TASKK.Work:
                break;
        }
    }

    public void CompleteTask()
    {
        // the critter will need to manage its own meters, this just handles resources

        switch (_taskk)
        {
            case TASKK.EatFood:
            case TASKK.CollectFood:
                break;

            case TASKK.StoreFood:
                _job.GiveResource(RESOURCEK.Food);
                break;

            case TASKK.GetWarm:
                _job.GiveResource(RESOURCEK.WarmBed);
                break;

            case TASKK.Work:
                _job.GiveResource(RESOURCEK.Work);
                break;
        }
    }
}

public class TestCritter
{
    public int _iCritter;
    public float _tSleep;
    public float _tSleepDur;

    public List<Task> _lTaskToDo;

    public TestCritter(int iCritter)
    {
        _iCritter = iCritter;

        _tSleep = 0.0f;
        _tSleepDur = UnityEngine.Random.value * 3f + 1f;

        _lTaskToDo = new List<Task>();
    }
}

public class JobManager : MonoBehaviour
{
    List<JobSite> _lJob;
    public GameObject _objText;

    public void AddJob(JobSite job)
    {
        _lJob.Add(job);
    }

    public void RemoveJob(JobSite job)
    {
        _lJob.Remove(job);
    }



    // If this returns true, the list of tasks is filled out with instructions on how to fulfill the need

    public bool FTryFulfillNeed(NEEDK needk, ref List<Task> pLTask)
    {
        foreach (JobSite job in _lJob)
        {
            if (job.FCanFulfillNeed(needk))
            {
                switch (needk)
                {
                    case NEEDK.Food:
                        pLTask.Add(new Task(job, TASKK.EatFood));
                        break;

                    case NEEDK.Warmth:
                        pLTask.Add(new Task(job, TASKK.GetWarm));
                        break;
                }
                return true;
            }
        }

        return false;
    }

    // If this returns true, the list of tasks is filled with instructions on work to do for the player

    public bool FTryGetWork(ref List<Task> pLTask)
    {
        pLTask.Clear();

        JobSite jobPerformed = null;
        foreach (JobSite job in _lJob)
        {
            switch (job._jobk)
            {
                case JOBK.CollectFood:
                    break;
                
                case JOBK.WarmHome:
                    break;

                case JOBK.StoreFood:
                    foreach (JobSite jobOther in _lJob)
                    {
                        if (jobOther._jobk == JOBK.CollectFood && jobOther.FHasResource(RESOURCEK.Food))
                        {
                            pLTask.Add(new Task(jobOther, TASKK.CollectFood));
                            break;
                        }
                    }

                    if (pLTask.Count > 0)
                    {
                        pLTask.Add(new Task(job, TASKK.StoreFood));
                    }

                    break;

                case JOBK.Build:
                    pLTask.Add(new Task(job, TASKK.Work));
                    break;
            }

            if (pLTask.Count != 0)
            {
                jobPerformed = job;
                break;
            }
        }

        if (jobPerformed != null)
        {
            // move the performed job to the back of the list to do shitty load balancing

            _lJob.Remove(jobPerformed);
            _lJob.Add(jobPerformed);
            return true;
        }

        return false;
    }

    public int _cFoodTotal
    {
        get
        {
            int cFood = 0;
            foreach (JobSite job in _lJob)
            {
                cFood += job._mpReskCRes[RESOURCEK.Food];
            }
            return cFood;
        }
    }

    public int _cBedsAvailable
    {
        get
        {
            int cBed = 0;
            foreach (JobSite job in _lJob)
            {
                cBed += job._mpReskCRes[RESOURCEK.WarmBed];
            }
            return cBed;
        }
    }



    List<TestCritter> _lCritter; // debug only

    void Start()
    {
        _lCritter = new List<TestCritter>();
        for (int i = 0; i < 10; ++i)
            _lCritter.Add(new TestCritter(i));

        _lJob = new List<JobSite>();
        //JobSite jobCollectFood = new JobSite(JOBK.CollectFood);
        //jobCollectFood._mpReskCRes[RESOURCEK.Food] = 20;
        //_lJob.Add(jobCollectFood);

        //JobSite jobStoreFood = new JobSite(JOBK.StoreFood);
        //_lJob.Add(jobStoreFood);

        //JobSite jobBuildHousing = new JobSite(JOBK.Build);
        //_lJob.Add(jobBuildHousing);
    }

    void UpdateUiText()
    {
        Text text = _objText.GetComponent<Text>();
        text.text = "";
        //foreach (JOBK jobk in Enum.GetValues(typeof(JOBK)))
        //{
        //    foreach (JobSite job in _lJob)
        //    {
        //        if (job._jobk == jobk)
        //        {
        //            switch (job._jobk)
        //            {
        //                case JOBK.CollectFood:
        //                    text.text += "Collect Food:  Food: " + job._mpReskCRes[RESOURCEK.Food] + "\n";
        //                    break;

        //                case JOBK.StoreFood:
        //                    text.text += "Store Food:    Food: " + job._mpReskCRes[RESOURCEK.Food] + "\n";
        //                    break;

        //                case JOBK.WarmHome:
        //                    text.text += "Warm Home:     Beds:  " + job._mpReskCRes[RESOURCEK.WarmBed] + "\n";
        //                    break;

        //                case JOBK.Build:
        //                    text.text += "Building:      Work:  " + job._mpReskCRes[RESOURCEK.Work] + "\n";
        //                    break;
        //            }
        //        }
        //    }
        //}
    }

    void Update()
    {
        foreach (TestCritter critter in _lCritter)
        {
            critter._tSleep += Time.deltaTime;
            if (critter._tSleep >= critter._tSleepDur)
            {
                if (critter._lTaskToDo.Count > 0)
                {
                    Task task = critter._lTaskToDo[0];
                    critter._lTaskToDo.RemoveAt(0);
                    task.CompleteTask();

                    Debug.Log(critter._iCritter + ": Completing " + task._taskk.ToString());
                }

                if (critter._lTaskToDo.Count == 0)
                {
                    FTryGetWork(ref critter._lTaskToDo);
                }

                critter._tSleep = 0;
            }
        }

        UpdateUiText();

        JobSite jobDel = null;
        foreach (JobSite job in _lJob)
        {
            if (job._jobk == JOBK.Build)
            {
                if (job._mpReskCRes[RESOURCEK.Work] >= 20)
                {
                    jobDel = job;
                    break;
                }
            }
        }

        if (jobDel != null)
        {
            _lJob.Remove(jobDel);
            
            JobSite jobHousing = new JobSite(JOBK.WarmHome);
            jobHousing._mpReskCRes[RESOURCEK.WarmBed] = 5;
            _lJob.Add(jobHousing);
        }
    }
}
