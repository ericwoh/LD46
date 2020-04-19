using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enum that represents a slot's type based on the occupation of neighboring slots
/// Cardinal direction only, and assumes the below is always occupied for a valid option
/// </summary>
public enum  NeighborState
{
    type0 = 0,  // Type0: No neighbors
    type1 = 1,  // Type1: Right edge (only neighbor is to the right)
    type2 = 2,  // Type2: Middle with roof (only neighbor is above)
    type3 = 3,  // Type3: L-shape. Neighbors to the right and top
    type4 = 4,  // Type4: Left edge (only neighbor is to the left)
    type5 = 5,  // Type5: Middle tile, no top (neighbors on right & left)\
    type6 = 6,  // Type6: Reverse-L. Neighbors to the left and top
    type7 = 7,  // Type7: Middle tile, neighbors on left, right, & top

    empty = 99, // invalid value
}

/// <summary>
/// Building slot represents a tile space within a building that a BuildingModule can be built
/// It also handles showing the Sprite for the current BuildingModule in this slot.
/// </summary>
public class BuildingSlot : MonoBehaviour
{
    [Tooltip("Represents what module type is used here based on the neighbors")]
    public NeighborState mModuleType;

    private BuildingModule mModule = null;
    public bool mIsDoor = false;

    public void SetBuildingModule(BuildingModule module)
    {
        mModule = module;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sprite = module.mSprite;

        Debug.Log("Setting module to " + module);
    }

    /// <summary>
    /// Simple public method to check whether this slot has been filled by a module
    /// </summary>
    /// <returns>Returns true if no module exists, this slot is empty</returns>
    public bool IsSlotEmpty()
    {
        return (mModule == null) ? true : false;
    }

    /// <summary>
    /// Public method used to reset this building slot, reinitializing to blank state
    /// </summary>
    public void ClearSlot()
    {
        mModule = null;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sprite = null;
    }
}