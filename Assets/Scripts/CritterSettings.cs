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

    public GameObject InstantiateCritterBehaviour(Vector3 pos, Quaternion rot)
    {
        return Instantiate(_critterPrefab, pos, rot);
    }
}
