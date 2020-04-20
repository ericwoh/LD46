using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "LD46/Building Module")]
public class BuildingModule : ScriptableObject
{
    [Tooltip("Which set does this module belong to")]
    public BuildingModuleSet mModuleSet = null;

    [Tooltip("Identifies what neighboring modules are compatible per cardinal edge")]
    public List<BuildingModule> mLeftNeightborModules;
    public List<BuildingModule> mRightNeightborModules;
    public List<BuildingModule> mUpperNeightborModules;
    public List<BuildingModule> mLowerNeightborModules;

    public Sprite mSprite;

    private void OnEnable()
    {
        //if (mModuleSet)
        //   mModuleSet.Add(this);
    }

    private void OnDisable()
    {
        //if (mModuleSet)
        //    mModuleSet.Remove(this);
    }
}