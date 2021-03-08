using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UIListBox : MonoBehaviour
{
    new RectTransform transform;
    // children of the object are the objects that will be arranged in a list. each will be positioned based on its own height within the rect transform
    float initHeight;

    void Awake()
    {
        transform = GetComponent<RectTransform>();
        initHeight = transform.rect.height;
        
    }

    void Update()
    {
        SetItemsPositions();
    }

    /// <summary>
    /// offset of the list elements (they will be moved by this amount when scrolling)
    /// </summary>
	[SerializeField]
	[HideInInspector]
    float offsetY = 0;

    void SetItemsPositions()
    {
        float tempOffset = 0;
        for (int i = 0; i < transform.childCount; i++) {
            RectTransform t = (RectTransform)transform.GetChild(i);
            // get transform height
            var h = t.rect.height;
            t.anchoredPosition = new Vector2(t.anchoredPosition.x, tempOffset - offsetY);
            tempOffset += h;
        }

    }

    public void SetScroll(float value)
    {
        // count max height
        var totalH = 0f;
        for (int i = 0; i < transform.childCount; i++) {
            RectTransform t = (RectTransform)transform.GetChild(i);
            var h = t.rect.height;
            totalH += h;  
        }
        offsetY = value;//Mathf.Clamp(value, 0, totalH - initHeight);

    }

    public void SetScrollToItem(int itemIndex)
    {
        // calculate height to that item
        // count heights
        var partialH = 0f;
        int childcount = 0;
        for (int i = 0; i < transform.childCount; i++) {
            RectTransform t = (RectTransform)transform.GetChild(i);
            var h = t.rect.height;
            if (childcount == itemIndex) break;
             childcount++;
            partialH += h;
        }
        SetScroll(partialH);
    }

}
