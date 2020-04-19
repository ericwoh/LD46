using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The 'physical/heavy' representation of a critter in our game'
[CreateAssetMenu(menuName = "LD46/CritterSettings")]
public class CritterSettings : ScriptableObject
{
    public GameObject _critterPrefab;

    public Sprite spriteEmoteHappy;
    public Sprite spriteEmoteSad;
    public Sprite spriteEmoteHungry;
    public Sprite spriteEmoteDry;
    public Sprite spriteEmoteCold;

    public GameObject InstantiateCritterBehaviour(Vector3 pos, Quaternion rot)
    {
        return Instantiate(_critterPrefab, pos, rot);
    }

    public float statValueMax = 100.0f;
    public float statValueMin = 0.0f;
    public float hungerPerSecond = 1;
    public float LubricationDecayPerSecond = 1;
    public float WarmthDecayPerSecond = 1;
}
