using System;
using UnityEngine;

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
