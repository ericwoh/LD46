using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DesignationButton : MonoBehaviour
{
    public Grocery _grocTarget;
    public GroceryUi _grocui;
    public DESIGNATIONK _desk;

    // Start is called before the first frame update
    void Start()
    {

        switch (_desk)
        {
            case DESIGNATIONK.None:
                GetComponentInChildren<Text>().text = "Cancel";
                break;

            case DESIGNATIONK.BuildHomes:
                GetComponentInChildren<Text>().text = "Make Homes";
                break;

            case DESIGNATIONK.CollectFood:
                GetComponentInChildren<Text>().text = "Get Food";
                break;

            case DESIGNATIONK.StoreFood:
                GetComponentInChildren<Text>().text = "Store Food";
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnButtonClick()
    {
        _grocTarget.SetDesignation(_desk);
        _grocui.Clear();
    }
}
