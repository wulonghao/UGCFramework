using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollRectCircle : MonoBehaviour
{
    /// <summary> 切换循环的最小和最大触发点，值基于scrollRect.normalizedPosition </summary>
    public Vector2 limitPosition = new Vector2(0.2f, 0.8f);
    ScrollRect scrollRect;
    [HideInInspector]
    public bool isDraging;

    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
    }

    void Start()
    {
        UGUIEventListenerContainDrag.Get(scrollRect.gameObject).onDragStart += delegate { isDraging = true; };
        UGUIEventListenerContainDrag.Get(scrollRect.gameObject).onDragEnd += delegate { isDraging = false; };
        scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
    }

    void OnScrollValueChanged(Vector2 v2)
    {
        if (scrollRect.content.childCount < 4)
            return;
        if (isDraging)
            return;
        ValueChange(v2);
    }

    void ValueChange(Vector2 v2)
    {
        RectTransform rtfFirst = scrollRect.content.GetChild(0).GetComponent<RectTransform>();
        RectTransform rtfLast = scrollRect.content.GetChild(scrollRect.content.childCount - 1).GetComponent<RectTransform>();
        if (scrollRect.horizontal)
        {
            if (scrollRect.content.rect.width < scrollRect.viewport.rect.width * 2)
                return;
            if (v2.x.CompareTo(limitPosition.y) > 0)
            {
                rtfFirst.SetAsLastSibling();
                scrollRect.content.localPosition += Vector3.right * rtfFirst.rect.width;
            }
            else if (v2.x.CompareTo(limitPosition.x) < 0)
            {
                rtfLast.SetAsFirstSibling();
                scrollRect.content.localPosition += Vector3.left * rtfLast.rect.width;
            }
        }
        else if (scrollRect.vertical)
        {
            if (scrollRect.content.rect.height < scrollRect.viewport.rect.height * 2)
                return;
            if (v2.y.CompareTo(limitPosition.x) < 0)
            {
                rtfFirst.SetAsLastSibling();
                scrollRect.content.localPosition += Vector3.down * rtfLast.rect.height;
            }
            else if (v2.y.CompareTo(limitPosition.y) > 0)
            {
                rtfLast.SetAsFirstSibling();
                scrollRect.content.localPosition += Vector3.up * rtfFirst.rect.height;
            }
        }
    }
}
