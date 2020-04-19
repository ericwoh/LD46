using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class Critter
{
    public int _id;
    public float _sleepTimer;
    public float _sleepDuration;
    public List<Task> _tasks;
    public GameObject _sprite;
    public GameObject _emote;

    public Critter(int id, GameObject sprite)
    {
        _id = id;
        _sleepTimer = 0.0f;
        _sleepDuration = UnityEngine.Random.value * 3f + 1f;
        _tasks = new List<Task>();
        _sprite = sprite;
        _emote = sprite.transform.Find("emote").gameObject;
    }

    public void tick(float deltaTime)
    {

        if (_tasks.Count > 0)
        {
            Task task = _tasks[0];
            if (task._job._location == null)
            {
                foreach (Task taskOther in _tasks)
                    taskOther.CancelTask();
                _tasks.Clear();
            }
            else
            {
                Vector3 pos = _sprite.transform.position;
                float taskX = task._job._location.position.x;
                float critterX = pos.x;
                float newCritterX = Mathf.Lerp(critterX, taskX, deltaTime * 2.0f);
                _sprite.transform.position = new Vector3(newCritterX, pos.y, pos.z);
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

    public void setEmoteSprite(Sprite emote)
    {
        _emote.GetComponent<SpriteRenderer>().sprite = emote;
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
            AddCritter(new Vector3(i * 1.5f, 3, 0), Quaternion.identity);
        }
    }

    // returns an integer handle that maps to the critter you farted out
    public int AddCritter(Vector3 pos, Quaternion rot)
    {
        GameObject Sprite = m_critterSettings.InstantiateCritterBehaviour(pos, rot);
        m_critters.Add(new Critter(++m_nextCritterId, Sprite));
        m_critters[m_critters.Count - 1].setEmoteSprite(m_critterSettings.spriteEmoteHappy);
        return m_nextCritterId;
    }

    public void tick(float deltaTime)
    {
        foreach (Critter critter in m_critters)
        {
            critter.tick(Time.deltaTime);

            if (critter._tasks.Count == 0)
            {
                m_jobManager.FTryGetWork(ref critter._tasks);
            }
        }
    }

    private List<Critter> m_critters;
    private int m_nextCritterId = 0;
    private JobManager m_jobManager;
    private CritterSettings m_critterSettings;
}
