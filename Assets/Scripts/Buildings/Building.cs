using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    [Tooltip("This is the set of modules (tiles) that are used to build this building")]
    public BuildingModuleSet mModuleSet;

    [Tooltip("Width of the building in module units (e.g., 2 is 2 modules wide)")]
    public int mBuildingWidth = 2;
    [Tooltip("Height of the building in module units (e.g., 2 is 2 modules high)")]
    public int mBuildingHeight = 2;

    private List<BuildingSlot> mSlots;

    private void OnEnable()
    {
        mSlots = new List<BuildingSlot>(mBuildingWidth * mBuildingHeight);
    }
}
