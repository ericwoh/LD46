using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEngine;

public enum NeighborDir
{
    left = 0,
    right = 1,
    upper = 2,
    lower = 3
}

/// <summary>
/// Buildings are containers with a tile-based width and height. Buildings contain a grid of 
/// BuildingSlots, which are containers that can be filled by building BuildintModules in a slot.
/// Building is the primary object in the building concept that interacts wiith other components
/// </summary>
public class Building : MonoBehaviour
{
    #region Properties
    /// ------------------------------------ PUBLIC PROPERTIES --------------------------------- ///
    
    [Tooltip("GameObject prefab for empty BuildingSlots. DEPRECATE when this is generated at runtime")]
    public GameObject mEmptySlotPrefab;

    [Tooltip("This is the set of modules (tiles) that are used to build this building")]
    public BuildingModuleSet mModuleSet;

    [Tooltip("Width of the building in module units (e.g., 2 is 2 modules wide)")]
    public int mBuildingWidth = 2;
    [Tooltip("Height of the building in module units (e.g., 2 is 2 modules high)")]
    public int mBuildingHeight = 2;

    [Tooltip("UI prefab to spawn on top of this object in UI space")]
    public GameObject mUiPrefab;

    #endregion

    #region Internal Members
    /// ------------------------------------ INTERNAL MEMBERS ---------------------------------- ///

    private List<GameObject> mSlots;                    // List of BuildingSlots for this building
    private float mSlotWidth = 1.0f;                    // Width (world space units) of a Slot
    private float mSlotHeight = 1.0f;                   // Height (world space units) of a Slot

    private int mLastModuleBuiltIndex = 0;              // index in mSlots of the last module built      
    private int mEmptySlots = 0;                        // number of current empty slots in building

    #endregion

    #region Internal Unity Methods
    
    /// <summary>
    /// OnEnable is a Unity method called whever the root GameObject of this script is first enabled
    /// This can be used for startup or initialization logic, and if relevant, cleanup logic can use
    /// OnDisable which is guaranteed to be called on object deactivate/disable or Destruction
    /// </summary>
    private void OnEnable()
    {
        mEmptySlots = mBuildingWidth * mBuildingHeight;

        if (mSlots == null)
            mSlots = new List<GameObject>(mBuildingWidth * mBuildingHeight);

        // iterate through, instantiating slots floot by floor moving upward, e.g. list indeices visually:
        // 8 9
        // 4 5 6 7
        // 0 1 2 3
        for (int i = 0; i < mBuildingHeight; i++)
        {
            for (int j = 0; j < mBuildingWidth; ++j)
            {
                Vector3 slotPosition = transform.position;
                slotPosition.x += j * mSlotWidth;// + (mSlotWidth * 0.5f);
                slotPosition.y += i * mSlotHeight;// + (mSlotHeight * 0.5f);

                // instantiate a BuildingSlot object at this position as a child of this object
                //mSlots.Add(Instantiate(mEmptySlotPrefab, slotPosition, Quaternion.identity, transform));

                GameObject emptySlotObject = new GameObject("emptySlot" + (i + j));
                emptySlotObject.transform.parent = transform;
                emptySlotObject.transform.position = slotPosition;
                emptySlotObject.AddComponent<SpriteRenderer>();
                SpriteRenderer sr = emptySlotObject.GetComponent<SpriteRenderer>();
                sr.sprite = null;
                sr.sortingLayerName = "BuildingSlots";
                emptySlotObject.AddComponent<BuildingSlot>();

                mSlots.Add(emptySlotObject);

            }
        }

        // initialize build order for this building
        // start each building with a front door module

        BuildNewModule();
    }
    #endregion

    #region Public Methods
    /// ------------------------------------- PUBLIC METHODS ----------------------------------- ///

    /// <summary>
    /// Public method calls to have this building build a new module if not already full
    /// </summary>
    public void BuildNewModule()
    {
        if (mEmptySlots == 0)
        {
            Debug.Log("This building is full and can't build anymore modules");
            return;
        }

        CameraShaker camShake = GetComponent<CameraShaker>();
        AudioSource audio = GetComponent<AudioSource>();

        // special case - if it's the first module, make it a door.
        if (mEmptySlots == mBuildingWidth * mBuildingHeight)
        {
            int doorSlot = (int)(((mBuildingWidth * 0.5f) - 1));
            mSlots[doorSlot].GetComponent<BuildingSlot>().SetBuildingModule(mModuleSet.mDoorModules[0]);
            mSlots[doorSlot].GetComponent<BuildingSlot>().mIsDoor = true;
            mLastModuleBuiltIndex = doorSlot;
            mEmptySlots -= 1;

            // add in little camera shake when building a new module
            if (camShake)
                camShake.ShakeCamera();

            if (audio)
                audio.Play();

            return;
        }


        //Debug.Log("Building a new module in building " + gameObject);
        // select which slot to fill next based on what module was built last
        int nextSlot = SelectNextSlotToBuild();
        if (nextSlot < 0)
        {
            Debug.LogError("Took too long choosing a random slot, didn't build anything. Try again.");
            return;
        }

        NeighborState neighbors = CheckNeighborSlots(nextSlot);
        BuildingModule firstModule = mModuleSet.GetModuleOfType(neighbors);
        mSlots[nextSlot].GetComponent<BuildingSlot>().SetBuildingModule(firstModule);
        mEmptySlots -= 1;

        // add in little camera shake when building a new module
        if (camShake)
            camShake.ShakeCamera();

        if (audio)
            audio.Play();

        // update neighbor state for all slots to adjust module types if needed
        for (int i = 0; i < mSlots.Count; i++)
        {
            BuildingSlot slot = mSlots[i].GetComponent<BuildingSlot>();

            // if this slot is still empty, don't care about its neighbors yet
            if (slot.IsSlotEmpty())
                continue;

            NeighborState nState = CheckNeighborSlots(i);

            // if this slot's neighbors haven't changed, no need to update
            if (nState == slot.mModuleType)
                continue;

            if (slot.mModuleType != NeighborState.empty)
            {
                slot.mModuleType = nState;
                BuildingModule module = mModuleSet.GetModuleOfType(slot.mModuleType);
                if (slot.mIsDoor)
                    slot.SetBuildingModule(mModuleSet.mDoorModules[(int)slot.mModuleType]);
                else    
                    slot.SetBuildingModule(module);
            }            
        }
    }

    public void ClearBuildingModules()
    {
        foreach (GameObject go in mSlots)
        {
            BuildingSlot slot = go.GetComponent<BuildingSlot>();
            slot.ClearSlot();
        }
        mEmptySlots = mBuildingWidth * mBuildingHeight; // reinit to empty
    }

    public int NumSlotsMax()
    {
        return mBuildingWidth * mBuildingHeight;
    }

    public int NumSlots()
    {
        return NumSlotsMax() - mEmptySlots;
    }

    public void ClearAndRefresh()
    {
        ClearBuildingModules();
        mSlots.Clear();
        OnEnable();
    }

    #endregion

    #region Internal Helper Methods
    /// ------------------------------------ INTERNAL METHODS ---------------------------------- ///

    /// <summary>
    /// Randomly select a slot to build in as long as it's not empty below
    /// </summary>
    /// <returns></returns>
    private int SelectNextSlotToBuild()
    {
        if (mEmptySlots == 0)
            Debug.LogError("Attempted to build module in building that's already full.");

        bool foundNextSlot = false;
        int nextbuildSlot = mLastModuleBuiltIndex;

        int iteration = 0;
        while (!foundNextSlot)
        {
            nextbuildSlot = Random.Range(0, mSlots.Count);

            BuildingSlot nextSlot = mSlots[nextbuildSlot].GetComponent<BuildingSlot>();
            if (nextSlot.IsSlotEmpty())
            {
                if (nextbuildSlot < mBuildingWidth)
                    return nextbuildSlot;   // ground floor, good to build.

                // otherwise, make sure the slot below this isn't empty
                int slotBelow = nextbuildSlot - mBuildingWidth;
                if (mSlots[slotBelow].GetComponent<BuildingSlot>().IsSlotEmpty() == false)
                    return nextbuildSlot;
            }                

            // watchdog timer
            if (++iteration > 50)
            {
                Debug.LogError("Took over 30 iterations to find an empty slot, something is broken.");
                return -1;
            }    
        }
        return -1;
    }

    /// <summary>
    /// Simple helper used to check whether a neighbor slot is empty or occupied
    /// </summary>
    /// <param name="slotIndex">Index in mSlots of this slot</param>
    /// <param name="dir">Cardinal direction of neighbor to be checked</param>
    /// <returns></returns>
    private NeighborState CheckNeighborSlots(int slotIndex)
    {
        bool leftN = false;
        bool rightN = false;
        bool upperN = false;

        // check left neighbor
        // if on the left edge, consider left neighbor empty, otherwise check
        if (slotIndex % mBuildingWidth != 0)
        {
            BuildingSlot leftNeighborSlot = mSlots[slotIndex - 1].GetComponent<BuildingSlot>();
            if (!leftNeighborSlot.IsSlotEmpty())
                leftN = true;
        }

        // if on the right edge, consider right neighbor empty, otherwise check
        if ((slotIndex) % mBuildingWidth != mBuildingWidth - 1)
        {
            BuildingSlot rightNeighborSlot = mSlots[slotIndex + 1].GetComponent<BuildingSlot>();
            if (!rightNeighborSlot.IsSlotEmpty())
                rightN = true;
        }

        // if on the top edge, consider upper neighbor empty
        if (slotIndex < mBuildingWidth * (mBuildingHeight - 1))
        {
            BuildingSlot upperNeighborSlot = mSlots[slotIndex + mBuildingWidth].GetComponent<BuildingSlot>();
            if (!upperNeighborSlot.IsSlotEmpty())
                upperN = true;
        }

        if (!leftN && !rightN && !upperN)
            return NeighborState.type0; // no neighbors
        if (leftN && !rightN && !upperN)
            return NeighborState.type1; // right edge
        if (!leftN && !rightN && upperN)
            return NeighborState.type2;
        if (!leftN && rightN && upperN)
            return NeighborState.type3;
        if (!leftN && rightN && !upperN)
            return NeighborState.type4; // left edge
        if (leftN && rightN && !upperN)
            return NeighborState.type5;
        if (leftN && !rightN && upperN)
            return NeighborState.type6;
        if (leftN && rightN && upperN)
            return NeighborState.type7;


        return NeighborState.empty; // error if we got here.
    }

    #endregion

}
