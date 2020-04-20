using UnityEngine;

public class OutlineOnMouseHover : MonoBehaviour
{
    [Tooltip("Outlined variant of the root object's Sprite")]
    public Sprite mOutlinedSprite;

    private SpriteRenderer sr;
    private Sprite mOriginalSprite;

    // Start is called before the first frame update
    void Start()
    {
        sr = transform.root.gameObject.GetComponent<SpriteRenderer>();
        mOriginalSprite = sr.sprite;

        Debug.Assert(mOutlinedSprite);
    }

    // Update is called once per frame
    private void OnMouseOver()
    {
        sr.sprite = mOutlinedSprite;
    }

    private void OnMouseExit()
    {
        sr.sprite = mOriginalSprite;
    }
}
