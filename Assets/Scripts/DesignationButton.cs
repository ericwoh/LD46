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
        GetComponentInChildren<Text>().text = _desk.ToString();
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
