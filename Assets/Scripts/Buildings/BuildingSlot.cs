using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSlot : MonoBehaviour
{
    private BuildingModule mModule = null;
    public void SetBuildingModule(BuildingModule module)
    {
        mModule = module;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sprite = module.mSprite;
    }

    /// <summary>
    /// Simple public method to check whether this slot has been filled by a module
    /// </summary>
    /// <returns>Returns true if no module exists, this slot is empty</returns>
    public bool IsSlotEmpty()
    {
        return (mModule == null) ? true : false;
    }
}