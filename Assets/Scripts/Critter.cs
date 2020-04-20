﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class ShelfPosY
{
    public const float Shelf1 = -7.0f;
    public const float Shelf2 = -2.0f;
    public static float Shelf3 = 3.0f;
}

enum CritterStatType
{
    Hunger,
    Warmth,
    Lubrication,
    Count
}

class CritterStats
{
    public float[] _values = new float [(int)CritterStatType.Count];
    public CritterStats(CritterSettings settings) 
    {
        for (int i = 0; i < (int)CritterStatType.Count; ++i)
        {
            _values[i] = settings.statValueMax;
        }
    }
}

class Critter
{
    public int _id;
    public float _sleepTimer;
    public float _sleepDuration;
    public List<Task> _tasks;
    public GameObject _sprite;
    public GameObject _emote;

    public CritterStats _stats;

    public int _foodHeld = 0;

    public CritterSettings _settings;

    public Critter(int id, Vector3 pos, CritterSettings settings)
    {
        _id = id;
        _sleepTimer = 0.0f;
        _sleepDuration = UnityEngine.Random.value * 3f + 1f;
        _tasks = new List<Task>();
        _sprite = settings.InstantiateCritterBehaviour(pos, Quaternion.identity);
        _emote = _sprite.transform.Find("emote").gameObject;
        _stats = new CritterStats(settings);
        _settings = settings;
    }

    public void cancelTasks()
    {
        Task.CancelTasks(_tasks);
        _tasks.Clear();
    }

    public bool isSatiated()
    {
        return _stats._values[(int)CritterStatType.Hunger] > _settings.statValueMin;
    }
    public bool isLubricated()
    {
        return _stats._values[(int)CritterStatType.Lubrication] > _settings.statValueMin;
    }
    public bool isWarm()
    {
        return _stats._values[(int)CritterStatType.Warmth] > _settings.statValueMin;
    }

    public void tick(float deltaTime)
    {
        tickStats(deltaTime);

        // tick position change
        if (_tasks.Count > 0)
        {
            Vector3 pos = _sprite.transform.position;
            Task task = _tasks[0];
            if (task._job._location == null)
            {
                Task.CancelTasks(_tasks);
                _tasks.Clear();
            }
            else
            {
                float xGoal;
                
                Vector2 posTask = task._job._location.position + task._job._vecOffsetDoor;

                if (Mathf.Approximately(pos.y, posTask.y))
                {
                    xGoal = posTask.x;
                }
                else
                {
                    if (pos.x > 0)
                        xGoal = 10.0f;
                    else
                        xGoal = -9.0f;
                }

                float dirX = xGoal - pos.x;
                float dX = Mathf.Sign(dirX) * deltaTime * 2.0f;
                if (dX > Mathf.Abs(pos.x - xGoal))
                {
                    dX = Mathf.Sign(dX) * Mathf.Abs(pos.x - xGoal);
                }

                _sprite.GetComponent<SpriteRenderer>().flipX = dirX > 0.0f;

                pos.x += dX;

                if (Mathf.Approximately(pos.x, -9.0f) || Mathf.Approximately(pos.x, 10.0f))
                {
                    pos.y = posTask.y;
                }

                _sprite.transform.position = pos;

                if (!Mathf.Approximately(pos.x, posTask.x) && !Mathf.Approximately(pos.x, posTask.x))
                {
                    return; // guard against the task being completed until critter gets to site
                }
            }
        }

        _sleepTimer += deltaTime;
        if (_sleepTimer >= _sleepDuration)
        {
            if (_tasks.Count > 0)
            {
                Task task = _tasks[0];
                _tasks.RemoveAt(0);
                task.CompleteTask();

                Debug.Log(_id + ": Completing " + task._taskk.ToString());
            }
            _sleepTimer = 0;
        }
    }

    private void completeTask(Task task)
    {
        switch (task._taskk)
        {
            case TASKK.EatFood:
            {
                if (task._job.FTryClaimResource(RESOURCEK.Food))
                {
                    _stats._values[(int)CritterStatType.Hunger] += 1;
                    Debug.Log("Eating food: " + _stats._values[(int)CritterStatType.Hunger]);
                }
                break;
            }
            case TASKK.CollectFood:
            {
                if (task._job.FTryClaimResource(RESOURCEK.Food))
                {
                    _foodHeld += 1;
                }
                break;
            }

            case TASKK.GetWarm:
            {
                break;
            }
            case TASKK.StoreFood:
            case TASKK.Work:
                break;
        }
    }

    private void tickHunger(float deltaTime)
    {
        float hunger = _stats._values[(int)CritterStatType.Hunger];
        _stats._values[(int)CritterStatType.Hunger] = Mathf.Clamp(hunger - (deltaTime * _settings.hungerPerSecond), 0, _settings.statValueMax);
        Debug.Log("Hunger: " + _stats._values[(int)CritterStatType.Hunger]);
    }
    private void tickLubrication(float deltaTime)
    {
        float lube = _stats._values[(int)CritterStatType.Lubrication];
        _stats._values[(int)CritterStatType.Lubrication] = Mathf.Clamp(lube - (deltaTime * _settings.LubricationDecayPerSecond), 0, _settings.statValueMax);
        Debug.Log("Lube: " + _stats._values[(int)CritterStatType.Lubrication]);
    }
    private void tickWarmth(float deltaTime)
    {
        float warmth = _stats._values[(int)CritterStatType.Warmth];
        _stats._values[(int)CritterStatType.Warmth] = Mathf.Clamp(warmth - (deltaTime * _settings.WarmthDecayPerSecond), 0, _settings.statValueMax);
        Debug.Log("Warmth: " + _stats._values[(int)CritterStatType.Warmth]);
    }

    private void setEmoteSprite(Sprite emote)
    {
        _emote.GetComponent<SpriteRenderer>().sprite = emote;
    }

    private void tickStats(float deltaTime)
    {
        tickHunger(deltaTime);
        tickLubrication(deltaTime);
        tickWarmth(deltaTime);

        if (!isSatiated())
        {
            Debug.Log("Settings Srprite");
            setEmoteSprite(_settings.spriteEmoteHungry);
        }
        else if (!isLubricated())
        {
            setEmoteSprite(_settings.spriteEmoteDry);
        }
        else if (!isWarm()) 
        {
            setEmoteSprite(_settings.spriteEmoteCold);
        }
    }
}

public class Critters
{
    public Critters(CritterSettings critterSettings, JobManager jobManager)
    {
        m_critterSettings = critterSettings;
        m_jobManager = jobManager;
        m_critters = new List<Critter>();
        for (int i = 0; i < 15; ++i)
        {
            AddCritter(new Vector3(-8.0f + i * 1.5f, ShelfPosY.Shelf1, 0));
        }
    }

    // returns an integer handle that maps to the critter you farted out
    public int AddCritter(Vector3 pos)
    {
        m_critters.Add(new Critter(++m_nextCritterId, pos, m_critterSettings));
        return m_nextCritterId;
    }

    public void tick(float deltaTime)
    {
        foreach (Critter critter in m_critters)
        {
            critter.tick(Time.deltaTime);

            bool hasTasks = critter._tasks.Count > 0;
            if (hasTasks)
            {
                Task task = critter._tasks[0];

                // if we're starving and we're not actively eating food, drop what we're doing and try and find food
                if (!critter.isSatiated() && task._taskk != TASKK.EatFood)
                {
                    critter.cancelTasks(); 
                    m_jobManager.FTryFulfillNeed(NEEDK.Food, ref critter._tasks);
                }
            }
            else
            {
                m_jobManager.FTryGetWork(ref critter._tasks, critter._sprite.transform.position.y);
            }
        }
    }

    private List<Critter> m_critters;
    private int m_nextCritterId = 0;
    private JobManager m_jobManager;
    private CritterSettings m_critterSettings;
}
