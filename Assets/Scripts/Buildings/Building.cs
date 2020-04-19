using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                slotPosition.x += j * mSlotWidth + (mSlotWidth * 0.5f);
                slotPosition.y += i * mSlotHeight + (mSlotHeight * 0.5f);

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
        int doorSlot = (int)(((mBuildingWidth * 0.5f) - 1));
        mSlots[doorSlot].GetComponent<BuildingSlot>().SetBuildingModule(mModuleSet.mDoorModule);
        mLastModuleBuiltIndex = doorSlot;
        mEmptySlots -= 1;
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

        Debug.Log("Building a new module in building " + gameObject);
        // select which slot to fill next based on what module was built last
        int nextSlot = SelectNextSlotToBuild();
        Debug.Assert(nextSlot >= 0);
        mSlots[nextSlot].GetComponent<BuildingSlot>().SetBuildingModule(mModuleSet.mDoorModule);
        mEmptySlots -= 1;
    }

    public void ClearBuildingModules()
    {
        foreach (GameObject go in mSlots)
        {
            BuildingSlot slot = go.GetComponent<BuildingSlot>();
            slot.ClearSlot();
        }
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
            if (++iteration > 30)
            {
                Debug.LogError("Took over 30 iterations to find an empty slot, something is broken.");
                return -1;
            }    
        }
        return -1;
    }

    #endregion

}
