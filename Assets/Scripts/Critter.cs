using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Critter
{
    public int _iCritter;
    public float _tSleep;
    public float _tSleepDur;

    public List<Task> _lTaskToDo;

    public Critter(int iCritter)
    {
        _iCritter = iCritter;

        _tSleep = 0.0f;
        _tSleepDur = UnityEngine.Random.value * 3f + 1f;

        _lTaskToDo = new List<Task>();
    }
}
