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

public class JobManager : MonoBehaviour
{
    List<JobSite> _lJob;
    Critters _critters;

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



    void Start()
    {
        _critters = new Critters(this);

        _lJob = new List<JobSite>();
        JobSite jobCollectFood = new JobSite(JOBK.CollectFood);
        jobCollectFood._mpReskCRes[RESOURCEK.Food] = 20;
        _lJob.Add(jobCollectFood);

        JobSite jobStoreFood = new JobSite(JOBK.StoreFood);
        _lJob.Add(jobStoreFood);

        JobSite jobBuildHousing = new JobSite(JOBK.Build);
        _lJob.Add(jobBuildHousing);
    }

    void UpdateUiText()
    {
        Text text = _objText.GetComponent<Text>();
        text.text = "";
        foreach (JOBK jobk in Enum.GetValues(typeof(JOBK)))
        {
            foreach (JobSite job in _lJob)
            {
                if (job._jobk == jobk)
                {
                    switch (job._jobk)
                    {
                        case JOBK.CollectFood:
                            text.text += "Collect Food:  Food: " + job._mpReskCRes[RESOURCEK.Food] + "\n";
                            break;

                        case JOBK.StoreFood:
                            text.text += "Store Food:    Food: " + job._mpReskCRes[RESOURCEK.Food] + "\n";
                            break;

                        case JOBK.WarmHome:
                            text.text += "Warm Home:     Beds:  " + job._mpReskCRes[RESOURCEK.WarmBed] + "\n";
                            break;

                        case JOBK.Build:
                            text.text += "Building:      Work:  " + job._mpReskCRes[RESOURCEK.Work] + "\n";
                            break;
                    }
                }
            }
        }
    }

    void Update()
    {
        _critters.tick(Time.deltaTime);
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
