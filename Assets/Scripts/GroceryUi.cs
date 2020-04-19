﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroceryUi : MonoBehaviour
{

    public GameObject _prefabDesignationButton;
    public bool _fDisableDebugText = true;
    Grocery _groc;

    List<GameObject> _lObjButton;
    GroceryManager _grocm;

    bool _fHovered = false;
    float _tLastHover = -1000.0f;

    // Start is called before the first frame update
    void Start()
    {
        _lObjButton = new List<GameObject>();
        _grocm = GameObject.Find("Game Manager").GetComponent<Game>()._grocm;
    }

    private void OnDestroy()
    {
        if (_lObjButton.Count > 0)
            _grocm._fButtonsDisabled = false;

        foreach (GameObject obj in _lObjButton)
        {
            Destroy(obj);
        }
    }

    public void SetGrocery(Grocery groc)
    {
        _groc = groc;

        // spawn ui for buildings
        Vector3 posScreenLL = Camera.main.WorldToScreenPoint(_groc.transform.position);
        Vector3 posScreenUR = Camera.main.WorldToScreenPoint(_groc.transform.position + new Vector3(_groc._width, _groc._height, 0));

        RectTransform rtrans = GetComponent<RectTransform>();
        rtrans.position = Vector3.Lerp(posScreenLL, posScreenUR, 0.5f);
        rtrans.sizeDelta = new Vector2(posScreenUR.x - posScreenLL.x, posScreenUR.y - posScreenLL.y);

        GameObject objBg = transform.Find("ResourceMeterBg").gameObject;
        RectTransform rtransBg = objBg.GetComponent<RectTransform>();
        rtransBg.position = new Vector2(Mathf.Lerp(posScreenLL.x, posScreenUR.x, 0.5f), posScreenLL.y - 20.0f);
        rtransBg.sizeDelta = new Vector2(posScreenUR.x - posScreenLL.x - 10.0f, rtransBg.sizeDelta.y);
    }

    // Update is called once per frame
    void Update()
    {
        DebugDrawText();

        GetComponent<Button>().interactable = !_grocm._fButtonsDisabled;

        if (_fHovered && !_grocm._fButtonsDisabled && _groc._desk == DESIGNATIONK.None)
            _tLastHover = Time.time;

        _groc.SetFade(Mathf.Clamp01((Time.time - _tLastHover) * 10.0f));

        if (_groc._job != null)
        {
            Color color = Color.white;
            float uFill = 1.0f;
            switch (_groc._job._jobk)
            {
                case JOBK.Build:
                    uFill = (float)_groc._job._mpReskCRes[RESOURCEK.Work] / (float)_groc._job._mpReskCResLimit[RESOURCEK.Work];
                    color = Color.HSVToRGB(18.0f / 360.0f, 0.6f, 0.6f);
                    break;
                case JOBK.CollectFood:
                case JOBK.StoreFood:
                    uFill = (float)_groc._job._mpReskCRes[RESOURCEK.Food] / (float)_groc._job._mpReskCResLimit[RESOURCEK.Food];
                    color = new Color(0.4f, 0.7f, 0.2f);
                    break;
                case JOBK.WarmHome:
                    uFill = (float)_groc._job._mpReskCRes[RESOURCEK.WarmBed] / (float)_groc._job._mpReskCResLimit[RESOURCEK.WarmBed];
                    color = Color.HSVToRGB(33.0f / 360.0f, 0.87f, 0.89f);
                    break;
            }

            GameObject objFill = transform.Find("ResourceMeterFill").gameObject;
            RectTransform rtransFill = objFill.GetComponent<RectTransform>();

            GameObject objBg = transform.Find("ResourceMeterBg").gameObject;
            RectTransform rtransBg = objBg.GetComponent<RectTransform>();
            
            objFill.GetComponent<Image>().enabled = true;
            objFill.GetComponent<Image>().color = color;
            objBg.GetComponent<Image>().enabled = true;

            rtransFill.offsetMin = rtransBg.offsetMin;
            rtransFill.offsetMax = new Vector3(Mathf.Lerp(rtransBg.offsetMin.x, rtransBg.offsetMax.x, uFill), rtransBg.offsetMax.y);
        }
        else
        {
            GameObject objFill = transform.Find("ResourceMeterFill").gameObject;
            objFill.GetComponent<Image>().enabled = false;

            GameObject objBg = transform.Find("ResourceMeterBg").gameObject;
            objBg.GetComponent<Image>().enabled = false;
        }
    }

    void DebugDrawText()
    {
        Text text = GetComponent<Text>();
        if (text == null)
            return;

        if (_fDisableDebugText)
        {
            text.text = "";
            return;
        }

        text.text = _groc._grock.ToString() + "\n";
        text.text += "Des: " + _groc._desk.ToString() + "\n";
        text.text += "Job: ";
        if (_groc._job != null)
        {
            text.text += _groc._job._jobk.ToString() + "\n";

            switch (_groc._job._jobk)
            {
                case JOBK.Build:
                    text.text += string.Format("Work: {0}", _groc._job._mpReskCRes[RESOURCEK.Work]);
                    break;
                case JOBK.WarmHome:
                    text.text += string.Format("Available Beds: {0}", _groc._job._mpReskCRes[RESOURCEK.WarmBed]);
                    break;
                case JOBK.CollectFood:
                case JOBK.StoreFood:
                    text.text += string.Format("Food: {0}", _groc._job._mpReskCRes[RESOURCEK.Food]);
                    break;
            }
        }
        else
            text.text += "None\n";
    }

    public void OnButtonClick()
    {
        if (_groc._desk != DESIGNATIONK.None)
            return;

        RectTransform rtrans = GetComponent<RectTransform>();

        List<DESIGNATIONK> lDesk = Grocery.LDeskFromGrock(_groc._grock);

        // filter invalid designationks...
        if (lDesk.Contains(DESIGNATIONK.CollectFood))
        {
            if (_groc._mpReskCRes[RESOURCEK.Food] <= 0)
                lDesk.Remove(DESIGNATIONK.CollectFood);
        }

        for (int iDesk = 0; iDesk < lDesk.Count; ++iDesk)
        {
            DESIGNATIONK desk = lDesk[iDesk];
            float u = (float)iDesk / (float)lDesk.Count;

            Vector3 vecOffset = new Vector3(Mathf.Sin(u * Mathf.PI * 2) * 100, Mathf.Cos(u * Mathf.PI * 2) * 50, 10.0f);
            Debug.Log(_prefabDesignationButton.GetComponent<DesignationButton>()._desk.ToString());
            
            GameObject canvas = GameObject.Find("Canvas");
            GameObject objButton = Instantiate(_prefabDesignationButton, rtrans.position + vecOffset, new Quaternion(), canvas.transform);
            DesignationButton desbtn = objButton.GetComponent<DesignationButton>();
            desbtn._desk = desk;
            desbtn._grocTarget = _groc;
            desbtn._grocui = this;

            _lObjButton.Add(objButton);
        }

        _grocm._fButtonsDisabled = true;
    }

    public void OnStartHover()
    {
        _fHovered = true;
    }

    public void OnEndHover()
    {
        _fHovered = false;
    }

    public void Clear()
    {
        foreach (GameObject obj in _lObjButton)
        {
            Destroy(obj);
        }
        _grocm._fButtonsDisabled = false;
    }
}
