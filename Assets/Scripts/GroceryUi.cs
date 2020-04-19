using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroceryUi : MonoBehaviour
{

    public GameObject _prefabDesignationButton;
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
    }

    // Update is called once per frame
    void Update()
    {
        DebugDrawText();

        GetComponent<Button>().interactable = !_grocm._fButtonsDisabled;

        if (_fHovered && !_grocm._fButtonsDisabled)
            _tLastHover = Time.time;

        float u = Mathf.Lerp(0.6f, 1.0f, Mathf.Clamp01((Time.time - _tLastHover) * 10.0f));
        _groc.GetComponent<SpriteRenderer>().color = new Color(u, u, u);
    }

    void DebugDrawText()
    {
        Text text = GetComponent<Text>();
        if (text == null)
            return;

        text.text = _groc._grock.ToString() + "\n";
        text.text += "Designation: " + _groc._desk.ToString() + "\n";
        text.text += "Current Job: ";
        if (_groc._job != null)
        {
            text.text += _groc._job._jobk.ToString() + "\n";

            switch (_groc._job._jobk)
            {
                case JOBK.Build:
                    text.text += string.Format("Work: {0} / {1}", _groc._job._mpReskCRes[RESOURCEK.Work], Grocery.CWorkRequiredFromGrock(_groc._grock));
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
        RectTransform rtrans = GetComponent<RectTransform>();

        List<DESIGNATIONK> lDesk = Grocery.LDeskFromGrock(_groc._grock);
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
