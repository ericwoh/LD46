﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Critter
{
    public int _id;
    public float _sleepTimer;
    public float _sleepDuration;

    public List<Task> _tasks;

    public Critter(int id)
    {
        _id = id;

        _sleepTimer = 0.0f;
        _sleepDuration = UnityEngine.Random.value * 3f + 1f;

        _tasks = new List<Task>();
    }

    public void tick(float deltaTime)
    {
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
}

public class Critters
{
    public Critters(CritterSettings critterSettings, JobManager jobManager)
    {
        m_critterSettings = critterSettings;
        m_jobManager = jobManager;
        m_critters = new List<Critter>();
        for (int i = 0; i < 10; ++i)
        {
            m_critters.Add(new Critter(i));
        }
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

    public List<Critter> Get
    {
        get => m_critters;
    }

    private List<Critter> m_critters;
    private JobManager m_jobManager;
    private CritterSettings m_critterSettings;
}
