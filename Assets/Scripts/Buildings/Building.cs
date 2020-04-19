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

    public List<GameObject> mSlots;
    private List<int> mModuleBuildOrder;
    private float mSlotWidth = 1.0f;
    private float mSlotHeight = 1.0f;

    private int mLastModuleBuilt = 0;
    private int mEmptySlots;

    [Tooltip("UI prefab to spawn on top of this object in UI space")]
    public GameObject mUiPrefab;

    #region InternalMethods
    private void OnEnable()
    {
        mEmptySlots = mBuildingWidth * mBuildingHeight;

        // spawn ui for buildings
        GameObject canvas = GameObject.Find("Canvas");
        Vector3 posScreen = Camera.main.WorldToScreenPoint(transform.position);
        Instantiate(mUiPrefab, posScreen + new Vector3(0, 125, 0), new Quaternion(), canvas.GetComponent<Transform>());

        if (mSlots == null)
            mSlots = new List<GameObject>(mBuildingWidth * mBuildingHeight);
        mModuleBuildOrder = new List<int>(mSlots.Count);

        // iterate through, instantiating slots floot by floor moving upward, e.g. visually:
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
                mSlots.Add(Instantiate(mEmptySlotPrefab, slotPosition, Quaternion.identity, transform));
            }
        }

        // initialize build order for this building
        // start each building with a front door module
        int doorSlot = (int)(((mBuildingWidth * 0.5f) - 1));
        mSlots[doorSlot].GetComponent<BuildingSlot>().SetBuildingModule(mModuleSet.mDoorModule);
        mLastModuleBuilt = doorSlot;
        mEmptySlots -= 1;
    }
    #endregion

    #region Public Methods

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

    /// <summary>
    /// Randomly select a slot to build in as long as it's not empty below
    /// </summary>
    /// <returns></returns>
    private int SelectNextSlotToBuild()
    {
        if (mEmptySlots == 0)
            Debug.LogError("Attempted to build module in building that's already full.");

        bool foundNextSlot = false;
        int nextbuildSlot = mLastModuleBuilt;

        float timeStarted = Time.time;
        while (!foundNextSlot)
        {
            if (mSlots[nextbuildSlot].GetComponent<BuildingSlot>().IsSlotEmpty())
            {
                if (nextbuildSlot < mBuildingWidth)
                    return nextbuildSlot;   // ground floor, good to build.

                // otherwise, make sure the slot below this isn't empty
                int slotBelow = nextbuildSlot - mBuildingWidth;
                if (mSlots[slotBelow].GetComponent<BuildingSlot>().IsSlotEmpty() == false)
                    return nextbuildSlot;
            }                

            // watchdog timer
            if (Time.time - timeStarted > 3.0f)
            {
                Debug.LogError("Took over 3 seconds to find an empty slot, something is broken.");
                return -1;
            }    
        }
        return -1;
    }

    #endregion

}
