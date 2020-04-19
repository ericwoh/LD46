using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = ("LD46/Camera Shake Config"))]
public class CameraShakeConfig : ScriptableObject
{
    [Tooltip("How long the object should shake for")]
    [Range(0.0f, 2.0f)]
    public float mShakeDuration = 0.0f;

    [Tooltip("Amplitude: larger value shakes harder")]
    [Range(0.0f, 0.5f)]
    public float mShakeAmount = 0.0f;

    [Tooltip("How quickly shaking slows to zero. Larger number returns to zero slower.")]
    [Range(0.0f, 6.0f)]
    public float mDecreaseFactor = 0.0f;
}

