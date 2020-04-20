using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple data container class for grouping sets of BuildingModules like "tilesets" for generating
/// WFC-generated buildings
/// </summary>
[CreateAssetMenu(menuName = "LD46/Building Module Set")]
public class BuildingModuleSet : ScriptableObject
{
    #if UNITY_EDITOR
    [Tooltip("Description of each type of module")]
    [Multiline (lines:8)]
    public string mModuleTypeDescription = "Module types:"
        + "\nType0: No neighbors"
        + "\nType1: Left edge (only neighbor is to the right)"
        + "\nType2: Middle with roof (only neighbor is above)"
        + "\nType3: L-shape. Neighbors to the right and top"
        + "\nType4: Right edge (only neighbor is to the left)"
        + "\nType5: Middle tile, no top (neighbors on right & left)"
        + "\nType6: Reverse-L. Neighbors to the left and top"
        + "\nType7: Middle tile, neighbors on left, right, & top";
    #endif

    [Tooltip("Door module is special since all buildings should start with a door")]
    public List<BuildingModule> mDoorModules;  // door is a special module

    public List<BuildingModule> mType0Modules;
    public List<BuildingModule> mType1Modules;
    public List<BuildingModule> mType2Modules;
    public List<BuildingModule> mType3Modules;
    public List<BuildingModule> mType4Modules;
    public List<BuildingModule> mType5Modules;
    public List<BuildingModule> mType6Modules;
    public List<BuildingModule> mType7Modules;

    public BuildingModule GetModuleOfType(NeighborState type)
    {
        int moduleIndex;
        switch (type)
        {
            case NeighborState.type0:
                moduleIndex = Random.Range(0, mType0Modules.Count);
                return mType0Modules[moduleIndex];

            case NeighborState.type1:
                moduleIndex = Random.Range(0, mType1Modules.Count);
                return mType1Modules[moduleIndex];

            case NeighborState.type2:
                moduleIndex = Random.Range(0, mType2Modules.Count);
                return mType2Modules[moduleIndex];

            case NeighborState.type3:
                moduleIndex = Random.Range(0, mType3Modules.Count);
                return mType3Modules[moduleIndex];

            case NeighborState.type4:
                moduleIndex = Random.Range(0, mType4Modules.Count);
                return mType4Modules[moduleIndex];

            case NeighborState.type5:
                moduleIndex = Random.Range(0, mType5Modules.Count);
                return mType5Modules[moduleIndex];

            case NeighborState.type6:
                moduleIndex = Random.Range(0, mType6Modules.Count);
                return mType6Modules[moduleIndex];

            case NeighborState.type7:
                moduleIndex = Random.Range(0, mType7Modules.Count);
                return mType7Modules[moduleIndex];

            default:
                Debug.LogError("Provided invalid input to GetModuleOfTpe in BuildingModuleSet. Look at this");
                return mDoorModules[0];
        }
    }
}