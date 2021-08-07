using UnityEngine;
using UnityEngine.EventSystems;

public class UGUIEventListenerContainDrag : UGUIEventListener,
                                IBeginDragHandler,
                                IDragHandler,
                                IEndDragHandler,
                                IDropHandler,
                                IScrollHandler
{
    public UGUIDelegateData onDragStart;
    public UGUIDelegateData onDrag;
    public UGUIDelegateData onDragEnd;
    public UGUIDelegateData onDrop;
    public UGUIDelegateData onScrollData;

    public void OnBeginDrag(PointerEventData eventData) { onDragStart?.Invoke(gameObject, eventData); }

    public void OnDrag(PointerEventData eventData) { onDrag?.Invoke(gameObject, eventData); }

    public void OnEndDrag(PointerEventData eventData) { onDragEnd?.Invoke(gameObject, eventData); }

    public void OnDrop(PointerEventData eventData) { onDrop?.Invoke(gameObject, eventData); }

    public void OnScroll(PointerEventData eventData) { onScrollData?.Invoke(gameObject, eventData); }

    public new static UGUIEventListenerContainDrag Get(GameObject go)
    {
        if (!go) return null;
        UGUIEventListenerContainDrag listener = go.GetComponent<UGUIEventListenerContainDrag>();
        if (listener == null) listener = go.AddComponent<UGUIEventListenerContainDrag>();
        return listener;
    }
}