using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public Transform _location; // where this job will take place
    public Dictionary<RESOURCEK, int> _mpReskCRes; // a mapping of resource type to how many resources this jobsite has available
    public Dictionary<RESOURCEK, int> _mpReskCResPending; // 
    public Dictionary<RESOURCEK, int> _mpReskCResLimit; // maximum amount of resources
    public JOBK _jobk;

    public JobSite(JOBK jobk, Transform location)
    {
        _mpReskCRes = new Dictionary<RESOURCEK, int>();
        _mpReskCRes[RESOURCEK.Food] = 0;
        _mpReskCRes[RESOURCEK.WarmBed] = 0;
        _mpReskCRes[RESOURCEK.Work] = 0;

        _mpReskCResPending = new Dictionary<RESOURCEK, int>();
        _mpReskCResPending[RESOURCEK.Food] = 0;
        _mpReskCResPending[RESOURCEK.WarmBed] = 0;
        _mpReskCResPending[RESOURCEK.Work] = 0;

        _mpReskCResLimit = new Dictionary<RESOURCEK, int>();
        _mpReskCResLimit[RESOURCEK.Food] = 0;
        _mpReskCResLimit[RESOURCEK.WarmBed] = 0;
        _mpReskCResLimit[RESOURCEK.Work] = 0;

        _jobk = jobk;
        _location = location;
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
        _mpReskCResPending[resk]--;
    }
}

public class JobManager
{
    List<JobSite> _lJob;

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
    public bool FTryGetWork(ref List<Task> pLTask, float posY)
    {
        pLTask.Clear();

        JobSite jobPerformed = null;
        foreach (JobSite job in _lJob)
        {
            if (job._location.position.y != posY)
            {
                continue;
            }
            switch (job._jobk)
            {
                case JOBK.CollectFood:
                    break;

                case JOBK.WarmHome:
                    break;

                case JOBK.StoreFood:
                    if (job._mpReskCRes[RESOURCEK.Food] + job._mpReskCResPending[RESOURCEK.Food] < job._mpReskCResLimit[RESOURCEK.Food])
                    {
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
                            job._mpReskCResPending[RESOURCEK.Food]++;
                        }
                    }

                    break;

                case JOBK.Build:
                    if (job._mpReskCRes[RESOURCEK.Work] + job._mpReskCResPending[RESOURCEK.Work] < job._mpReskCResLimit[RESOURCEK.Work])
                    {
                        pLTask.Add(new Task(job, TASKK.Work));
                        job._mpReskCResPending[RESOURCEK.Work]++;
                    }
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


    public JobManager()
    {
        _lJob = new List<JobSite>();
    }

    public void tick(float deltaTime)
    {
    }
}
