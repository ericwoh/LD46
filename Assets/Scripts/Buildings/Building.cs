using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public GameObject mEmptySlotPrefab;

    [Tooltip("This is the set of modules (tiles) that are used to build this building")]
    public BuildingModuleSet mModuleSet;

    [Tooltip("Width of the building in module units (e.g., 2 is 2 modules wide)")]
    public int mBuildingWidth = 2;
    [Tooltip("Height of the building in module units (e.g., 2 is 2 modules high)")]
    public int mBuildingHeight = 2;

    private List<GameObject> mSlots;
    private float mSlotWidth = 1.0f;
    private float mSlotHeight = 1.0f;

    private void OnEnable()
    {
        mSlots = new List<GameObject>(mBuildingWidth * mBuildingHeight);

        // iterate through each slot and create a Slot prefab there
        for (int i = 0; i < mBuildingWidth; i++)
        {
            for (int j = 0; j < mBuildingHeight; ++j)
            {
                Vector3 slotPosition = transform.position;
                slotPosition.x += i * mSlotWidth;
                slotPosition.y += j * mSlotHeight;

                // instantiate a BuildingSlot object at this position as a child of this object
                mSlots.Add(Instantiate(mEmptySlotPrefab, slotPosition, Quaternion.identity, transform));
            }
        }
    }
}
