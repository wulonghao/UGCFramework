using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

[Hotfix]
public class ClickAnimation : MonoBehaviour
{
    public bool isZoom = true;
    public float zoomScale = 0.9f;
    Vector3 defaultVt;

    void Start()
    {
        defaultVt = transform.localScale;
        UGUIEventListener.Get(gameObject).onDown = delegate
        {
            if (isZoom)
                transform.localScale = Vector3.one * zoomScale;
        };
        UGUIEventListener.Get(gameObject).onUp = delegate
        {
            if (isZoom)
                transform.localScale = defaultVt;
        };
    }
}
