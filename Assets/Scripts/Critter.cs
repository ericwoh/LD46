using System;
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

enum CritterState
{
    idleOrWorking,
    
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
        foreach (Task task in _tasks)
        {
            task.CancelTask();
        }
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
            float taskX = task._job._location.position.x;
            float critterX = pos.x;
            float dirX = taskX - critterX;
            critterX += (dirX / Mathf.Abs(dirX)) * deltaTime * 2.0f;
            _sprite.transform.position = new Vector3(critterX, pos.y, pos.z);
            if (Mathf.Approximately(critterX, taskX) == false)
            {
                return; // guard against the task being completed until critter gets to site
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

    private void tickHunger(float deltaTime)
    {
        _stats._values[(int)CritterStatType.Hunger] -= deltaTime * _settings.hungerPerSecond;
        //Debug.Log("Warmth: " + _stats._values[(int)CritterStatType.Hunger]);
    }
    private void tickLubrication(float deltaTime)
    {
        _stats._values[(int)CritterStatType.Lubrication] -= deltaTime * _settings.LubricationDecayPerSecond;
    }
    private void tickWarmth(float deltaTime)
    {
        _stats._values[(int)CritterStatType.Warmth] -= deltaTime * _settings.WarmthDecayPerSecond;
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
        for (int i = 0; i < 1; ++i)
        {
            AddCritter(new Vector3(i * 1.5f, ShelfPosY.Shelf1, 0));
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
                    if (m_jobManager.FTryFulfillNeed(NEEDK.Food, ref critter._tasks))
                    {
                        Debug.Log("EATING FOOD");
                    }
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
