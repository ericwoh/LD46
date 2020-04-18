using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu (menuName = "LD46/BuildingModule")]
public class BuildingModule : ScriptableObject
{
    [Tooltip("Identifies what neighboring modules are compatible per cardinal edge")]
    public List<BuildingModule> mLeftNeightborModules;
    public List<BuildingModule> mRightNeightborModules;
    public List<BuildingModule> mUpperNeightborModules;
    public List<BuildingModule> mLowerNeightborModules;


}
