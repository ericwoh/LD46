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

    [Tooltip("UI prefab to spawn on top of this object in UI space")]
    public GameObject mUiPrefab;

    #region InternalMethods
    private void OnEnable()
    {
        // spawn ui for buildings
        GameObject canvas = GameObject.Find("Canvas");
        Vector3 posScreen = Camera.main.WorldToScreenPoint(transform.position);
        Instantiate(mUiPrefab, posScreen, new Quaternion(), canvas.GetComponent<Transform>());

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
    }
    #endregion

    #region Public Methods

    public void BuildNewModule()
    {
        Debug.Log("Building a new module in building " + gameObject);
        // select which slot to fill next based on what module was built last
        int nextSlot = SelectNextSlotToBuild();
        if (nextSlot >= 0)
        {

        }
    }

    private int SelectNextSlotToBuild()
    {
        bool foundNextSlot = false;
        int nextbuildSlot = mLastModuleBuilt;

        while (!foundNextSlot)
        {
            if (mSlots[nextbuildSlot].GetComponent<BuildingSlot>().IsSlotEmpty())
                return nextbuildSlot;

            nextbuildSlot = Random.Range(0, mSlots.Count - 1);
        }




        // FALLBACK PLAN - built out from door
        //// first try to build horizontally - check left and right
        //int potentialModule = mLastModuleBuilt;
        
        //// check to its left if that's an option
        //if ((potentialModule % mBuildingWidth) - 1 >= 0 &&
        //    mSlots[potentialModule - 1].GetComponent<BuildingSlot>().IsSlotEmpty())
        //{
        //    return potentialModule - 1;
        //}
        //else if ((potentialModule % mBuildingWidth) + 1 <= mBuildingWidth &&
        //    mSlots[potentialModule + 1].GetComponent<BuildingSlot>().IsSlotEmpty())
        //{
        //    return potentialModule + 1;
        //}

        // check above to see if that's an option

        // -1 is error value, returned when no valid spot is found
        return -1;
    }

    #endregion

}
