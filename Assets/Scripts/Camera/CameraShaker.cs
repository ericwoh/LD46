using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    public CameraShakeConfig mData;

    private Transform mCamTransform;
    private Vector3 mOriginalPos;

    private float mShakeDuration = 0.0f;        // How long the object should shake for 
    private float mShakeAmount = 0.0f;          // Amplitude: larger value shakes harder
    private float mDecreaseFactor = 0.0f;       // How quickly shaking slows to zero    

    /// <summary>
    /// On Awake, initialize the camera and origin position
    /// </summary>
    private void Start()
    {
        mCamTransform = Camera.main.transform;
        mOriginalPos = mCamTransform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (mCamTransform == null)
        {
            mCamTransform = Camera.main.transform;
        }

        if (mShakeDuration > 0)
        {
            mCamTransform.localPosition = mOriginalPos + (Vector3)Random.insideUnitCircle * mShakeAmount;

            mShakeDuration -= Time.deltaTime * mDecreaseFactor;
        }
        else
        {
            mShakeDuration = 0f;
            mCamTransform.localPosition = mOriginalPos;
        }
    }

    public void ShakeCamera()
    {
        // mark the new camera origin position 
        //mOriginalPos = mCamTransform.position;

        // start shake based on configured data
        mShakeDuration = mData.mShakeDuration;
        mShakeAmount = mData.mShakeAmount;
        mDecreaseFactor = mData.mDecreaseFactor;
    }
}